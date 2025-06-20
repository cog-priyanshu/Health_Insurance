﻿// Controllers/ClaimController.cs
using Health_Insurance.Data; // Ensure namespace is correct for your DbContext
using Health_Insurance.Models; // Ensure namespace is correct for your Models
using Health_Insurance.Services; // Ensure namespace is correct for your Services
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Needed for .ToListAsync()
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims; // Still needed for ClaimTypes, ClaimsPrincipal, etc.

namespace Health_Insurance.Controllers
{
    // All actions in this controller require authentication by default.
    [Authorize]
    public class ClaimController : Controller
    {
        private readonly IClaimService _claimService;
        private readonly ApplicationDbContext _context;

        public ClaimController(IClaimService claimService, ApplicationDbContext context)
        {
            _claimService = claimService;
            _context = context;
        }

        // GET: /Claim/ListAllClaims or /Claim
        // Accessible to all authenticated users.
        // Admins see all claims. Employees see only their own claims.
        public async Task<IActionResult> ListAllClaims(int? employeeId = null) // Make employeeId nullable
        {
            IEnumerable<Health_Insurance.Models.Claim> claims; // Explicitly use your model's Claim

            if (User.IsInRole("Admin"))
            {
                // Admin sees all claims
                claims = await _claimService.ListAllClaimsAsync();
            }
            else if (User.IsInRole("Employee"))
            {
                // Explicitly qualify System.Security.Claims.Claim
                System.Security.Claims.Claim employeeIdClaim = User.FindFirst("EmployeeId");
                if (employeeIdClaim == null || !int.TryParse(employeeIdClaim.Value, out int actualEmployeeId))
                {
                    return Forbid(); // EmployeeId claim missing or invalid
                }

                // Employee sees only their own claims, ensure the parameter matches logged-in user
                if (employeeId == null || employeeId.Value != actualEmployeeId)
                {
                    // If employeeId is not provided or doesn't match, use the logged-in employeeId
                    employeeId = actualEmployeeId;
                }
                else if (employeeId.Value != actualEmployeeId)
                {
                    // Attempt by an Employee to view someone else's claims by passing a different employeeId
                    return Forbid();
                }

                // Filter claims by the authenticated employee's ID
                claims = (await _claimService.ListAllClaimsAsync()).Where(c => c.Enrollment.EmployeeId == employeeId.Value);
            }
            else
            {
                // Should not happen with [Authorize] but as a fallback
                return Forbid();
            }

            return View(claims);
        }

        // GET: /Claim/SubmitClaim
        // Accessible to all authenticated users.
        public async Task<IActionResult> SubmitClaim()
        {
            // Only allow employees to submit claims for themselves.
            // Admin can submit for anyone, but this form is typically for employees.
            if (User.IsInRole("Employee"))
            {
                // Explicitly qualify System.Security.Claims.Claim
                System.Security.Claims.Claim employeeIdClaim = User.FindFirst("EmployeeId");
                if (employeeIdClaim == null || !int.TryParse(employeeIdClaim.Value, out int actualEmployeeId))
                {
                    return Forbid(); // EmployeeId claim missing or invalid
                }

                // Fetch enrollments only for the logged-in employee
                var enrollments = await _context.Enrollments
                                                .Include(e => e.Policy)
                                                .Where(e => e.EmployeeId == actualEmployeeId) // Filter by logged-in employee
                                                .ToListAsync();

                ViewBag.EnrollmentList = new SelectList(enrollments.Select(e => new {
                    EnrollmentId = e.EnrollmentId,
                    DisplayText = $"Enrollment #{e.EnrollmentId} - {e.Policy?.PolicyName ?? "Unknown Policy"}"
                }), "EnrollmentId", "DisplayText");
            }
            else if (User.IsInRole("Admin"))
            {
                // Admin can see all enrollments
                var enrollments = await _context.Enrollments
                                                .Include(e => e.Employee)
                                                .Include(e => e.Policy)
                                                .ToListAsync();
                ViewBag.EnrollmentList = new SelectList(enrollments.Select(e => new {
                    EnrollmentId = e.EnrollmentId,
                    DisplayText = $"Enrollment #{e.EnrollmentId} - {e.Employee?.Name ?? "Unknown Employee"} ({e.Policy?.PolicyName ?? "Unknown Policy"})"
                }), "EnrollmentId", "DisplayText");
            }
            else
            {
                return Forbid(); // Not an Admin or Employee
            }

            return View();
        }

        // POST: /Claim/SubmitClaim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitClaim([Bind("EnrollmentId,ClaimAmount,ClaimReason,ClaimDate")] Health_Insurance.Models.Claim claim) // Explicitly qualify your model's Claim
        {
            // Validate that the submitted EnrollmentId belongs to the logged-in employee if they are an Employee
            if (User.IsInRole("Employee"))
            {
                // Explicitly qualify System.Security.Claims.Claim
                System.Security.Claims.Claim employeeIdClaim = User.FindFirst("EmployeeId");
                if (employeeIdClaim == null || !int.TryParse(employeeIdClaim.Value, out int actualEmployeeId))
                {
                    ModelState.AddModelError(string.Empty, "Employee ID not found in claims.");
                    return await RePopulateSubmitClaimDropdown(claim);
                }

                var enrollment = await _context.Enrollments.AsNoTracking().FirstOrDefaultAsync(e => e.EnrollmentId == claim.EnrollmentId);
                if (enrollment == null || enrollment.EmployeeId != actualEmployeeId)
                {
                    ModelState.AddModelError("EnrollmentId", "You can only submit claims for your own enrollments.");
                    return await RePopulateSubmitClaimDropdown(claim);
                }
            }

            if (ModelState.IsValid)
            {
                var success = await _claimService.SubmitClaimAsync(claim);

                if (success)
                {
                    return RedirectToAction(nameof(ListAllClaims));
                }
                else
                {
                    ViewBag.ErrorMessage = "Claim submission failed. Please check the Enrollment ID.";
                    return await RePopulateSubmitClaimDropdown(claim);
                }
            }
            return await RePopulateSubmitClaimDropdown(claim);
        }

        // Helper to repopulate dropdown for SubmitClaim view
        private async Task<IActionResult> RePopulateSubmitClaimDropdown(Health_Insurance.Models.Claim claim) // Explicitly qualify your model's Claim
        {
            if (User.IsInRole("Employee"))
            {
                // Explicitly qualify System.Security.Claims.Claim
                System.Security.Claims.Claim employeeIdClaim = User.FindFirst("EmployeeId");
                if (employeeIdClaim == null || !int.TryParse(employeeIdClaim.Value, out int actualEmployeeId))
                {
                    // This case should ideally be caught earlier by Forbid()
                    return Forbid();
                }
                var enrollments = await _context.Enrollments
                                                .Include(e => e.Policy)
                                                .Where(e => e.EmployeeId == actualEmployeeId)
                                                .ToListAsync();
                ViewBag.EnrollmentList = new SelectList(enrollments.Select(e => new {
                    EnrollmentId = e.EnrollmentId,
                    DisplayText = $"Enrollment #{e.EnrollmentId} - {e.Policy?.PolicyName ?? "Unknown Policy"}"
                }), "EnrollmentId", "DisplayText", claim.EnrollmentId);
            }
            else if (User.IsInRole("Admin"))
            {
                var enrollments = await _context.Enrollments
                                                .Include(e => e.Employee)
                                                .Include(e => e.Policy)
                                                .ToListAsync();
                ViewBag.EnrollmentList = new SelectList(enrollments.Select(e => new {
                    EnrollmentId = e.EnrollmentId,
                    DisplayText = $"Enrollment #{e.EnrollmentId} - {e.Employee?.Name ?? "Unknown Employee"} ({e.Policy?.PolicyName ?? "Unknown Policy"})"
                }), "EnrollmentId", "DisplayText", claim.EnrollmentId);
            }
            return View("SubmitClaim", claim);
        }


        // GET: /Claim/GetClaimDetails/5
        // Accessible to all authenticated users. Admin can view any. Employee can view their own.
        public async Task<IActionResult> GetClaimDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var claim = await _claimService.GetClaimDetailsAsync(id.Value);

            if (claim == null)
            {
                return NotFound();
            }

            // Enforce that an Employee can only view their own claims
            if (User.IsInRole("Employee"))
            {
                // Explicitly qualify System.Security.Claims.Claim
                System.Security.Claims.Claim employeeIdClaim = User.FindFirst("EmployeeId");
                if (employeeIdClaim == null || !int.TryParse(employeeIdClaim.Value, out int actualEmployeeId) || claim.Enrollment.EmployeeId != actualEmployeeId)
                {
                    return Forbid(); // Attempt by an Employee to view someone else's claim
                }
            }

            return View(claim);
        }

        // GET: /Claim/UpdateClaimStatus/5
        // Restricted to Admin users only.
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateClaimStatus(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var claim = await _claimService.GetClaimDetailsAsync(id.Value);

            if (claim == null)
            {
                return NotFound();
            }

            var statuses = new List<string> { "SUBMITTED", "APPROVED", "REJECTED" };
            ViewBag.Statuses = new SelectList(statuses, claim.ClaimStatus);

            return View(claim);
        }

        // POST: /Claim/UpdateClaimStatus/5
        // Restricted to Admin users only.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateClaimStatus(int id, [Bind("ClaimId,ClaimStatus")] Health_Insurance.Models.Claim claim) // Explicitly qualify your model's Claim
        {
            if (id != claim.ClaimId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var success = await _claimService.UpdateClaimStatusAsync(claim.ClaimId, claim.ClaimStatus);

                    if (success)
                    {
                        return RedirectToAction(nameof(GetClaimDetails), new { id = claim.ClaimId });
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Failed to update claim status. Please check the Claim ID or status.";
                        var statuses = new List<string> { "SUBMITTED", "APPROVED", "REJECTED" };
                        ViewBag.Statuses = new SelectList(statuses, claim.ClaimStatus);
                        return View(claim);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClaimExists(claim.ClaimId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            var statusesInvalid = new List<string> { "SUBMITTED", "APPROVED", "REJECTED" };
            ViewBag.Statuses = new SelectList(statusesInvalid, claim.ClaimStatus);
            return View(claim);
        }

        // Helper method to check if a claim exists
        private bool ClaimExists(int id)
        {
            return _context.Claims.Any(e => e.ClaimId == id);
        }
    }
}




