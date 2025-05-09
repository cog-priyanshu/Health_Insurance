// Controllers/EnrollmentController.cs
using Health_Insurance.Models; // Ensure namespace is correct
using Health_Insurance.Services; // Ensure namespace is correct
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering; // Needed for SelectList if you add dropdowns

namespace Health_Insurance.Controllers // Ensure this namespace is correct
{
    // Controller to handle requests related to Policy Enrollments
    public class EnrollmentController : Controller
    {
        // Inject the Enrollment Service interface
        private readonly IEnrollmentService _enrollmentService;
        // You might still need DbContext for dropdowns in views like Create/Edit,
        // but business logic should go through the service.
        // private readonly ApplicationDbContext _context;

        // Constructor: Inject the Enrollment Service
        public EnrollmentController(IEnrollmentService enrollmentService /*, ApplicationDbContext context */)
        {
            _enrollmentService = enrollmentService;
            // _context = context; // Uncomment if you need DbContext for SelectLists
        }

        // GET: /Enrollment/Index
        // This action will list available policies for enrollment, as per our manual design.
        public async Task<IActionResult> Index()
        {
            // Use the service to get all available policies
            var policies = await _enrollmentService.GetAllPoliciesAsync();
            // Pass the list of policies to the Index view

            return View(policies);
        }

        // GET: /Enrollment/Enroll?policyId=X&employeeId=Y
        // This action handles the request to enroll an employee in a policy.
        // In a real app, employeeId would come from the logged-in user.
        public async Task<IActionResult> Enroll(int policyId, int employeeId)
        {
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
                return RedirectToAction("Index");
            }
        }

        // GET: /Enrollment/EnrolledPolicies/5
        // This action lists the policies a specific employee is enrolled in.
        public async Task<IActionResult> EnrolledPolicies(int employeeId)
        {
            // In a real application, you'd get the employeeId from the logged-in user
            // For now, we'll pass it as a route parameter or query string for testing.

            // Use the service to get the employee's enrolled policies
            var enrollments = await _enrollmentService.GetEnrolledPoliciesByEmployeeIdAsync(employeeId);

            // Pass the list of enrollments to the EnrolledPolicies view
            return View(enrollments);
        }

        // POST: /Enrollment/CancelEnrollment/5
        // This action handles the request to cancel an enrollment.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelEnrollment(int enrollmentId, int employeeId) // Need employeeId to redirect back
        {
            // Use the service to perform the cancellation logic
            var success = await _enrollmentService.CancelEnrollmentAsync(enrollmentId);

            if (success)
            {
                // Redirect back to the employee's enrolled policies page
                return RedirectToAction("EnrolledPolicies", new { employeeId = employeeId });
            }
            else
            {
                // Handle cancellation failure (e.g., enrollment not found, cannot cancel)
                ViewBag.ErrorMessage = "Cancellation failed. Enrollment not found or cannot be cancelled.";
                // Redirect back to the employee's enrolled policies page with an error
                return RedirectToAction("EnrolledPolicies", new { employeeId = employeeId });
            }
        }

        // Note: The scaffolded Details, Create, Edit, Delete actions for Enrollment
        // are not directly needed for the document's specified Policy Enrollment workflow.
        // You might keep them for admin purposes or remove them if not required.
        // If keeping Create/Edit, you would need to inject ApplicationDbContext
        // back into the controller's constructor to populate SelectLists.
    }
}

