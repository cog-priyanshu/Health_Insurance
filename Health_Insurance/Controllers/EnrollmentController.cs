// Controllers/EnrollmentController.cs
using Health_Insurance.Data; // Ensure namespace is correct for your DbContext
using Health_Insurance.Models; // Ensure namespace is correct for your Models
using Health_Insurance.Services; // Ensure namespace is correct for your Services
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Needed for .ToListAsync() on Employees
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering; // Needed for SelectList

namespace Health_Insurance.Controllers // Ensure this namespace is correct
{
    // Controller to handle requests related to Policy Enrollments
    public class EnrollmentController : Controller
    {
        // Inject the Enrollment Service interface
        private readonly IEnrollmentService _enrollmentService;
        private readonly ApplicationDbContext _context; // Inject DbContext to get Employees for dropdown

        // Constructor: Inject the Enrollment Service and DbContext
        public EnrollmentController(IEnrollmentService enrollmentService, ApplicationDbContext context)
        {
            _enrollmentService = enrollmentService;
            _context = context; // Assign injected DbContext
        }

        // GET: /Enrollment/Index
        // This action will list available policies for enrollment and allow employee selection.
        public async Task<IActionResult> Index()
        {
            // Use the service to get all available policies
            var policies = await _enrollmentService.GetAllPoliciesAsync();

            // Fetch all employees to populate the dropdown
            var employees = await _context.Employees.ToListAsync();

            // Create a SelectList for the employee dropdown
            // Value: EmployeeId, Text: Employee Name
            ViewBag.EmployeeList = new SelectList(employees, "EmployeeId", "Name");

            // Pass the list of policies to the Index view
            return View(policies);
        }

        // GET: /Enrollment/Enroll?policyId=X&employeeId=Y
        // This action handles the request to enroll an employee in a policy.
        // employeeId is now passed dynamically from the view.
        public async Task<IActionResult> Enroll(int policyId, int employeeId)
        {
            // In a real application, you'd get the employeeId from the logged-in user
            // For now, we'll pass it as a route parameter or query string for testing.

            // Use the service to perform the enrollment logic
            var success = await _enrollmentService.EnrollEmployeeInPolicyAsync(employeeId, policyId);

            if (success)
            {
                // Redirect to the page showing the employee's enrolled policies
                return RedirectToAction("EnrolledPolicies", new { employeeId = employeeId });
            }
            else
            {
                // Handle enrollment failure (e.g., already enrolled, policy/employee not found)
                // You could redirect to an error page or back to the policy list with an error message
                ViewBag.ErrorMessage = "Enrollment failed. Please check if you are already enrolled or if the policy/employee exists.";
                // Redirect back to the policy list
                return RedirectToAction("Index"); // Or RedirectToAction("Details", "Employee", new { id = employeeId })
            }
        }


        // GET: /Enrollment/EnrolledPolicies/5
        // This action lists the policies a specific employee is enrolled in.
        public async Task<IActionResult> EnrolledPolicies(int employeeId) // Need employee ID
        {
            // In a real application, you'd get the employeeId from the logged-in user
            // For now, we'll pass it as a route parameter or query string for testing.

            var enrollments = await _enrollmentService.GetEnrolledPoliciesByEmployeeIdAsync(employeeId);

            // Fetch the employee's name to display on the page
            var employee = await _context.Employees.FindAsync(employeeId);
            ViewBag.EmployeeName = employee?.Name ?? "Unknown Employee"; // Handle case where employee is not found

            return View(enrollments); // Pass the list of enrollments to the EnrolledPolicies view
        }

        // POST: /Enrollment/CancelEnrollment/5 - Handle cancellation request
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelEnrollment(int enrollmentId, int employeeId) // Need enrollment ID and employee ID to redirect back
        {
            var success = await _enrollmentService.CancelEnrollmentAsync(enrollmentId);

            if (success)
            {
                // Redirect back to the employee's enrolled policies page
                return RedirectToAction("EnrolledPolicies", new { employeeId = employeeId });
            }
            else
            {
                // Handle failure (e.g., enrollment not found, cannot cancel)
                ViewBag.ErrorMessage = "Cancellation failed. Enrollment not found or cannot be cancelled.";
                // Redirect back to the employee's enrolled policies page
                return RedirectToAction("EnrolledPolicies", new { employeeId = employeeId }); // Stay on the same page with an error
            }
        }

        // You will add other actions as needed, e.g., for enrollment confirmation form (GET/POST)
    }
}


