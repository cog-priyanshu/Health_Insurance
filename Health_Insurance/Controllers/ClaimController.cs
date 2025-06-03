// Controllers/ClaimController.cs
using Health_Insurance.Data; // Ensure namespace is correct for your DbContext
using Health_Insurance.Models; // Ensure namespace is correct for your Models
using Health_Insurance.Services; // Ensure namespace is correct for your Services
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Needed for .ToListAsync()
using System.Threading.Tasks;
using System.Collections.Generic; // Needed for IEnumerable and List
using Microsoft.AspNetCore.Mvc.Rendering; // Needed for SelectList
using System.Linq; // Needed for .ToList()

namespace Health_Insurance.Controllers // Ensure this namespace is correct for your Controllers folder
{
    // Controller to handle requests related to Claims Management
    public class ClaimController : Controller
    {
        // Inject the Claim Service interface
        private readonly IClaimService _claimService;
        // Inject the ApplicationDbContext to fetch data for dropdowns
        private readonly ApplicationDbContext _context; // Inject DbContext

        // Constructor: Inject the Claim Service and DbContext
        public ClaimController(IClaimService claimService, ApplicationDbContext context) // Add DbContext to constructor
        {
            _claimService = claimService;
            _context = context; // Assign injected DbContext
        }

        // GET: /Claim/ListAllClaims or /Claim
        // Action to list all claims
        public async Task<IActionResult> ListAllClaims() // Or Index()
        {
            // Use the Claim Service to get all claims
            var claims = await _claimService.ListAllClaimsAsync();
            // Pass the list of claims to the view
            return View(claims); // You will need to create a ListAllClaims.cshtml view
        }

        // GET: /Claim/SubmitClaim
        // Action to display the form for submitting a new claim
        public async Task<IActionResult> SubmitClaim() // Made async to fetch enrollments
        {
            // Fetch all Enrollments to populate the dropdown
            // Include Employee and Policy to display helpful info in the dropdown text
            var enrollments = await _context.Enrollments
                                            .Include(e => e.Employee)
                                            .Include(e => e.Policy)
                                            .ToListAsync();

            // Create a SelectList for the Enrollment dropdown
            // Value: EnrollmentId, Text: A combination of Employee Name and Policy Name
            // Using a more descriptive text for the dropdown options
            ViewBag.EnrollmentList = new SelectList(enrollments.Select(e => new {
                EnrollmentId = e.EnrollmentId,
                DisplayText = $"Enrollment #{e.EnrollmentId} - {e.Employee?.Name ?? "Unknown Employee"} ({e.Policy?.PolicyName ?? "Unknown Policy"})" // Handle potential nulls
            }), "EnrollmentId", "DisplayText");


            return View(); // Return the SubmitClaim.cshtml view
        }

        // POST: /Claim/SubmitClaim
        // Action to handle the submission of the new claim form
        [HttpPost]
        [ValidateAntiForgeryToken] // Prevents CSRF attacks
        // The [Bind] attribute explicitly lists the properties allowed to be bound from the form.
        // ClaimStatus and Enrollment are intentionally excluded as they are not directly submitted by the form.
        public async Task<IActionResult> SubmitClaim([Bind("EnrollmentId,ClaimAmount,ClaimReason,ClaimDate")] Claim claim)
        {
            // ModelState.IsValid will be true ONLY if all [Required] fields included in [Bind] are provided
            // and other validation rules (like DataType, StringLength) for those bound fields are met.
            if (ModelState.IsValid)
            {
                // Use the Claim Service to submit the claim
                // The service should set the initial status (e.g., "SUBMITTED")
                var success = await _claimService.SubmitClaimAsync(claim);

                if (success)
                {
                    // Redirect to a page showing claim details or a success message
                    // For now, redirect to the list of all claims
                    return RedirectToAction(nameof(ListAllClaims));
                }
                else
                {
                    // Handle submission failure (e.g., invalid enrollment ID, service logic failure)
                    ViewBag.ErrorMessage = "Claim submission failed. Please check the Enrollment ID.";
                    // Repopulate dropdowns if needed and return the view with submitted data
                    var enrollments = await _context.Enrollments.Include(e => e.Employee).Include(e => e.Policy).ToListAsync();
                    ViewBag.EnrollmentList = new SelectList(enrollments.Select(e => new {
                        EnrollmentId = e.EnrollmentId,
                        DisplayText = $"Enrollment #{e.EnrollmentId} - {e.Employee?.Name ?? "Unknown Employee"} ({e.Policy?.PolicyName ?? "Unknown Policy"})"
                    }), "EnrollmentId", "DisplayText", claim.EnrollmentId);

                    return View(claim); // Return the view with errors
                }
            }
            // If model state is not valid (due to missing required fields or invalid data types for bound fields),
            // return the view with submitted data to show validation errors.
            // Repopulate dropdowns so the user doesn't lose their selection context.
            var enrollmentsInvalid = await _context.Enrollments.Include(e => e.Employee).Include(e => e.Policy).ToListAsync();
            ViewBag.EnrollmentList = new SelectList(enrollmentsInvalid.Select(e => new {
                EnrollmentId = e.EnrollmentId,
                DisplayText = $"Enrollment #{e.EnrollmentId} - {e.Employee?.Name ?? "Unknown Employee"} ({e.Policy?.PolicyName ?? "Unknown Policy"})"
            }), "EnrollmentId", "DisplayText", claim.EnrollmentId);

            return View(claim);
        }

        // GET: /Claim/GetClaimDetails/5
        // Action to display details of a specific claim
        public async Task<IActionResult> GetClaimDetails(int? id) // Using 'id' is standard for details
        {
            if (id == null)
            {
                return NotFound();
            }

            // Use the Claim Service to get claim details
            // The service should include Enrollment and Policy details
            var claim = await _claimService.GetClaimDetailsAsync(id.Value); // Use .Value for nullable int

            if (claim == null)
            {
                return NotFound(); // Claim not found
            }

            // Pass the claim object to the view
            return View(claim); // Return the GetClaimDetails.cshtml view
        }

        // GET: /Claim/UpdateClaimStatus/5
        // Action to display a form for updating claim status
        public async Task<IActionResult> UpdateClaimStatus(int? id) // Using 'id' for the claim to update
        {
            if (id == null)
            {
                return NotFound();
            }

            // Use the Claim Service to get claim details to display on the form
            var claim = await _claimService.GetClaimDetailsAsync(id.Value);

            if (claim == null)
            {
                return NotFound(); // Claim not found
            }

            // Create a list of possible statuses
            var statuses = new List<string> { "SUBMITTED", "APPROVED", "REJECTED" };
            // Create a SelectList for the Status dropdown, pre-selecting the current status
            ViewBag.Statuses = new SelectList(statuses, claim.ClaimStatus); // Use the overload that takes the selected value

            // Pass the claim object to the view
            return View(claim); // Return the UpdateClaimStatus.cshtml view
        }

        // POST: /Claim/UpdateClaimStatus/5
        // Action to handle the submission of the status update form
        [HttpPost]
        [ValidateAntiForgeryToken] // Prevents CSRF attacks
        public async Task<IActionResult> UpdateClaimStatus(int id, [Bind("ClaimId,ClaimStatus,ClaimReason")] Claim claim) // Bind only necessary fields
        {
            // Check if the id in the URL matches the id in the submitted form data
            if (id != claim.ClaimId)
            {
                return NotFound();
            }

            // Basic validation on the submitted model (only ClaimId and ClaimStatus are bound)
            if (ModelState.IsValid)
            {
                try
                {
                    // Use the Claim Service to update the status
                    var success = await _claimService.UpdateClaimStatusAsync(claim.ClaimId, claim.ClaimStatus);

                    if (success)
                    {
                        // Redirect to the claim details page or list of claims
                        return RedirectToAction(nameof(GetClaimDetails), new { id = claim.ClaimId }); // Redirect to details
                        // Or RedirectToAction(nameof(ListAllClaims));
                    }
                    else
                    {
                        // Handle update failure (e.g., claim not found, invalid status)
                        ViewBag.ErrorMessage = "Failed to update claim status. Please check the Claim ID or status.";
                        // Repopulate dropdowns and return the view
                        var statuses = new List<string> { "SUBMITTED", "APPROVED", "REJECTED" };
                        ViewBag.Statuses = new SelectList(statuses, claim.ClaimStatus); // Use the overload that takes the selected value
                        return View(claim); // Return the view with errors
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Handle concurrency conflicts (e.g., another user updated the same record)
                    if (!ClaimExists(claim.ClaimId)) // Use ClaimExists instead of EmployeeExists
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw; // Re-throw the exception if it's not a "not found" issue
                    }
                }
            }
            // If model state is not valid, return the view with submitted data
            // Repopulate dropdowns
            var statusesInvalid = new List<string> { "SUBMITTED", "APPROVED", "REJECTED" };
            ViewBag.Statuses = new SelectList(statusesInvalid, claim.ClaimStatus); // Use the overload that takes the selected value
            return View(claim);
        }

        // Helper method to check if a claim exists (Corrected from EmployeeExists)
        private bool ClaimExists(int id)
        {
            return _context.Claims.Any(e => e.ClaimId == id);
        }

        // You would add other actions as needed for claims management workflow
    }
}




