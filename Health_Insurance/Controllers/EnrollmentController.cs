﻿// Controllers/EnrollmentController.cs
using Health_Insurance.Data;
using Health_Insurance.Models;
using Health_Insurance.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization; // Add this using statement
using System.Security.Claims; // Needed for User.FindFirst

namespace Health_Insurance.Controllers
{
    // All actions in this controller require authentication.
    [Authorize]
    public class EnrollmentController : Controller
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly IPremiumCalculatorService _premiumCalculatorService;
        private readonly ApplicationDbContext _context;

        public EnrollmentController(IEnrollmentService enrollmentService, IPremiumCalculatorService premiumCalculatorService, ApplicationDbContext context)
        {
            _enrollmentService = enrollmentService;
            _premiumCalculatorService = premiumCalculatorService;
            _context = context;
        }

        // GET: /Enrollment/Index
        // Accessible to all authenticated users (Admin and Employee)
        public async Task<IActionResult> Index()
        {
            var policies = await _enrollmentService.GetAllPoliciesAsync();

            // Fetch all employees to populate the dropdown for Admin to select
            // For an Employee, this dropdown might be hidden or pre-selected.
            var employees = await _context.Employees.ToListAsync();
            ViewBag.EmployeeList = new SelectList(employees, "EmployeeId", "Name");

            return View(policies);
        }

        // GET: /Enrollment/Enroll?policyId=X&employeeId=Y
        // Accessible to all authenticated users (Admin and Employee)
        public async Task<IActionResult> Enroll(int policyId, int employeeId)
        {
            // IMPORTANT: In a real application, you would verify that the 'employeeId'
            // matches the logged-in user's EmployeeId if the user is an 'Employee'.
            // An Admin can enroll any employee, but an Employee can only enroll themselves.
            if (User.IsInRole("Employee"))
            {
                var loggedInEmployeeId = User.FindFirst("EmployeeId")?.Value;
                if (loggedInEmployeeId == null || !int.TryParse(loggedInEmployeeId, out int actualEmployeeId) || actualEmployeeId != employeeId)
                {
                    // Attempt by an Employee to enroll someone else or an invalid employeeId
                    return Forbid(); // Or RedirectToAction("AccessDenied", "Account");
                }
            }

            var success = await _enrollmentService.EnrollEmployeeInPolicyAsync(employeeId, policyId);

            if (success)
            {
                return RedirectToAction("EnrolledPolicies", new { employeeId = employeeId });
            }
            else
            {
                ViewBag.ErrorMessage = "Enrollment failed. Please check if you are already enrolled or if the policy/employee exists.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Enrollment/EnrolledPolicies/5
        // Accessible to all authenticated users.
        // An Admin can view any employee's enrollments. An Employee can only view their own.
        public async Task<IActionResult> EnrolledPolicies(int employeeId)
        {
            // Enforce that an Employee can only view their own enrollments
            if (User.IsInRole("Employee"))
            {
                var loggedInEmployeeId = User.FindFirst("EmployeeId")?.Value;
                if (loggedInEmployeeId == null || !int.TryParse(loggedInEmployeeId, out int actualEmployeeId) || actualEmployeeId != employeeId)
                {
                    // Attempt by an Employee to view someone else's enrollments
                    return Forbid(); // Or RedirectToAction("AccessDenied", "Account");
                }
            }

            var enrollments = await _enrollmentService.GetEnrolledPoliciesByEmployeeIdAsync(employeeId);

            var employee = await _context.Employees.FindAsync(employeeId);
            ViewBag.EmployeeName = employee?.Name ?? "Unknown Employee";

            return View(enrollments);
        }

        // POST: /Enrollment/CancelEnrollment/5
        // Accessible to all authenticated users.
        // An Admin can cancel any enrollment. An Employee can only cancel their own.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelEnrollment(int enrollmentId, int employeeId)
        {
            // Enforce that an Employee can only cancel their own enrollments
            if (User.IsInRole("Employee"))
            {
                var loggedInEmployeeId = User.FindFirst("EmployeeId")?.Value;
                if (loggedInEmployeeId == null || !int.TryParse(loggedInEmployeeId, out int actualEmployeeId) || actualEmployeeId != employeeId)
                {
                    // Attempt by an Employee to cancel someone else's enrollment
                    return Forbid(); // Or RedirectToAction("AccessDenied", "Account");
                }
            }

            var success = await _enrollmentService.CancelEnrollmentAsync(enrollmentId);

            if (success)
            {
                return RedirectToAction("EnrolledPolicies", new { employeeId = employeeId });
            }
            else
            {
                ViewBag.ErrorMessage = "Cancellation failed. Enrollment not found or cannot be cancelled.";
                return RedirectToAction("EnrolledPolicies", new { employeeId = employeeId });
            }
        }

        // POST: /Enrollment/CalculatePremium - Action to calculate premium via AJAX
        // Accessible to all authenticated users
        [HttpPost]
        public async Task<IActionResult> CalculatePremium(int employeeId, int policyId)
        {
            // In a real application, you might verify if the employeeId belongs to the logged-in user
            // if the user is an 'Employee' and not an 'Admin'.
            if (User.IsInRole("Employee"))
            {
                var loggedInEmployeeId = User.FindFirst("EmployeeId")?.Value;
                if (loggedInEmployeeId == null || !int.TryParse(loggedInEmployeeId, out int actualEmployeeId) || actualEmployeeId != employeeId)
                {
                    // Attempt by an Employee to calculate premium for someone else
                    return Forbid();
                }
            }

            var calculatedPremium = await _premiumCalculatorService.CalculatePremiumAsync(employeeId, policyId);
            return Json(new { premium = calculatedPremium });
        }
    }
}

