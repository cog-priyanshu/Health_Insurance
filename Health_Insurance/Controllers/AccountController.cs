// Controllers/AccountController.cs
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks; // Needed for async methods

namespace Health_Insurance.Controllers // Ensure this namespace is correct based on your project name
{
    // Controller to handle Account related actions like Login, Logout, etc.
    public class AccountController : Controller
    {
        // GET: /Account/Login
        // Action to display the login page
        [HttpGet] // Specifies that this action handles GET requests
        public IActionResult Login()
        {
            // Return the Login view (Views/Account/Login.cshtml)
            return View();
        }

        // POST: /Account/Login
        // Action to handle the login form submission
        [HttpPost] // Specifies that this action handles POST requests
        [ValidateAntiForgeryToken] // Prevents Cross-Site Request Forgery attacks (important for forms)
        public async Task<IActionResult> Login(string adminUsername, string adminPassword, string employeeEmail, string employeePassword)
        {
            // This is where your backend login logic would go.
            // Based on the submitted data (adminUsername/adminPassword or employeeEmail/employeePassword),
            // you would authenticate the user against your database or identity system.

            // For this frontend-focused demonstration, we'll just simulate success/failure
            // and redirect. You will replace this with actual authentication logic later.

            bool loginSuccessful = false;
            string userRole = null; // To determine where to redirect

            // --- Simulated Backend Authentication Logic (Replace with real code) ---
            if (!string.IsNullOrEmpty(adminUsername) && !string.IsNullOrEmpty(adminPassword))
            {
                // Simulate Admin login check
                if (adminUsername == "admin" && adminPassword == "password") // Replace with actual admin credentials check
                {
                    loginSuccessful = true;
                    userRole = "Admin";
                    // In a real app, you'd sign in the user here using ASP.NET Core Identity or similar
                    // await HttpContext.SignInAsync(...)
                }
            }
            else if (!string.IsNullOrEmpty(employeeEmail) && !string.IsNullOrEmpty(employeePassword))
            {
                // Simulate Employee login check
                // You would query your database to find an employee with this email and password
                // var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == employeeEmail && e.Password == employeePassword); // Assuming Employee model has Password field
                // if (employee != null)
                // {
                //     loginSuccessful = true;
                //     userRole = "Employee";
                //     // In a real app, you'd sign in the user here
                //     // await HttpContext.SignInAsync(...)
                // }

                // For demo, just check a dummy employee
                if (employeeEmail == "employee@example.com" && employeePassword == "password") // Replace with actual employee credentials check
                {
                    loginSuccessful = true;
                    userRole = "Employee";
                    // In a real app, you'd sign in the user here
                    // await HttpContext.SignInAsync(...)
                }
            }
            // --- End Simulated Backend Authentication Logic ---


            if (loginSuccessful)
            {
                // Redirect based on role (replace with your actual dashboard/landing pages)
                if (userRole == "Admin")
                {
                    return RedirectToAction("Index", "Employee"); // Redirect Admin to Employee list (example)
                }
                else if (userRole == "Employee")
                {
                    // Redirect Employee to their enrolled policies or a dashboard
                    // You'll need to get the employeeId after successful login
                    // For demo, redirect to a placeholder or their enrolled policies (if you can get the ID)
                    // return RedirectToAction("EnrolledPolicies", "Enrollment", new { employeeId = /* get employee ID */ });
                    return RedirectToAction("Index", "Enrollment"); // Redirect Employee to available policies (example)
                }
                else
                {
                    // Default redirect if role is not explicitly handled
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                // If login fails, add an error message to ModelState
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");

                // Return the view with the error message
                // You might also repopulate the form fields if needed, but for passwords, it's usually not done.
                return View();
            }
        }

        // You would add a Logout action here later
        // [HttpPost]
        // public async Task<IActionResult> Logout() { ... }
    }
}

