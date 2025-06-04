// Controllers/AccountController.cs
using Health_Insurance.Models; // Ensure this namespace is correct for LoginViewModel, Employee, Admin
using Health_Insurance.Services; // Ensure this namespace is correct for IUserService
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Security.Claims; // Needed for ClaimsIdentity, ClaimsPrincipal
using Microsoft.AspNetCore.Authentication; // Needed for SignInAsync, SignOutAsync
using Microsoft.AspNetCore.Authentication.Cookies; // Needed for CookieAuthenticationDefaults
using System.Collections.Generic; // Needed for List<Claim>

namespace Health_Insurance.Controllers // Ensure this namespace is correct
{
    // Controller to handle user authentication (login, logout, access denied)
    public class AccountController : Controller
    {
        private readonly IUserService _userService; // Inject the UserService

        // Constructor: Inject the UserService
        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: /Account/Login
        // Displays the login page.
        public IActionResult Login()
        {
            // If a user is already authenticated, redirect them to their respective dashboard
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index", "Employee"); // Admin goes to Employee management
                }
                else if (User.IsInRole("Employee"))
                {
                    // Get the EmployeeId from the claims to redirect to their specific enrolled policies
                    var employeeIdClaim = User.FindFirst("EmployeeId");
                    if (employeeIdClaim != null && int.TryParse(employeeIdClaim.Value, out int employeeId))
                    {
                        return RedirectToAction("EnrolledPolicies", "Enrollment", new { employeeId = employeeId });
                    }
                    // Fallback if employeeId claim is missing for some reason
                    return RedirectToAction("Index", "Home");
                }
                // Fallback for authenticated users without specific roles
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: /Account/Login
        // Handles the login form submission.
        [HttpPost]
        [ValidateAntiForgeryToken] // Protects against CSRF attacks
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // Check if the submitted model is valid based on data annotations
            if (!ModelState.IsValid)
            {
                return View(model); // Return the view with validation errors
            }

            // Authenticate user using the UserService, passing the LoginType
            var user = await _userService.AuthenticateUserAsync(model.Username, model.Password, model.LoginType); // Pass model.LoginType here

            if (user == null)
            {
                // Authentication failed (either credentials incorrect or user type mismatch)
                ModelState.AddModelError(string.Empty, "Invalid username or password, or incorrect login type selected.");
                return View(model); // Return the view with an error message
            }

            // Determine user role and create claims
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(ClaimTypes.Name, model.Username), // Store username
            };

            string userRole = string.Empty;
            int userId = 0; // To store AdminId or EmployeeId

            if (user is Admin adminUser)
            {
                claims.Add(new System.Security.Claims.Claim(ClaimTypes.Role, "Admin")); // Add Admin role claim
                claims.Add(new System.Security.Claims.Claim("AdminId", adminUser.AdminId.ToString())); // Custom claim for AdminId
                userRole = "Admin";
                userId = adminUser.AdminId;
            }
            else if (user is Employee employeeUser)
            {
                claims.Add(new System.Security.Claims.Claim(ClaimTypes.Role, "Employee")); // Add Employee role claim
                claims.Add(new System.Security.Claims.Claim("EmployeeId", employeeUser.EmployeeId.ToString())); // Custom claim for EmployeeId
                userRole = "Employee";
                userId = employeeUser.EmployeeId;
            }

            // Create ClaimsIdentity and ClaimsPrincipal
            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                // Allow refreshing the authentication session cookie
                AllowRefresh = true,
                // IsPersistent = model.RememberMe, // Use if you implement "Remember Me"
                ExpiresUtc = System.DateTimeOffset.UtcNow.AddMinutes(30) // Set cookie expiration
            };

            // Sign in the user
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Redirect based on role after successful login
            if (userRole == "Admin")
            {
                return RedirectToAction("Index", "Employee"); // Admin goes to Employee management
            }
            else if (userRole == "Employee")
            {
                // Employee goes to their enrolled policies
                return RedirectToAction("EnrolledPolicies", "Enrollment", new { employeeId = userId });
            }

            // Fallback redirect for unexpected scenarios
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Logout
        // Handles user logout.
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account"); // Redirect to login page after logout
        }

        // GET: /Account/AccessDenied
        // Displays a message when a user tries to access a restricted resource.
        public IActionResult AccessDenied()
        {
            return View(); // You will create an AccessDenied.cshtml view
        }
    }
}

