// Controllers/PremiumCalculatorController.cs
using Health_Insurance.Services; // Ensure namespace is correct for your Services
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Health_Insurance.Controllers // Ensure this namespace is correct for your Controllers folder
{
    // Controller to handle requests related to Premium Calculation
    public class PremiumCalculatorController : Controller
    {
        // Inject the Premium Calculator Service interface
        private readonly IPremiumCalculatorService _premiumCalculatorService;

        // Constructor: Inject the Premium Calculator Service
        public PremiumCalculatorController(IPremiumCalculatorService premiumCalculatorService)
        {
            _premiumCalculatorService = premiumCalculatorService;
        }

        // GET: /PremiumCalculator/CalculatePremium
        // This action could display a form for premium calculation input
        public IActionResult CalculatePremium()
        {
            // This view could contain input fields for Employee ID and Policy ID
            // or other criteria needed for calculation.
            return View();
        }

        // POST: /PremiumCalculator/CalculatePremium
        // This action handles the premium calculation request, likely via AJAX or a form submission.
        [HttpPost]
        // You might want to add [ValidateAntiForgeryToken] if this is a form post
        // [ValidateAntiForgeryToken]
        public async Task<IActionResult> CalculatePremium(int employeeId, int policyId)
        {
            // Use the injected Premium Calculator Service to perform the calculation
            var calculatedPremium = await _premiumCalculatorService.CalculatePremiumAsync(employeeId, policyId);

            // Return the calculated premium.
            // If this is an AJAX call, return as JSON.
            // If this is a form post, you might pass the result to a confirmation view.

            // For now, let's assume it's designed to be called via AJAX and return JSON.
            // If you need a form-based approach, we can adjust this.
            return Json(new { premium = calculatedPremium });
        }

        // You might add other actions here as needed for premium calculation features
        // e.g., GetCalculationForm, DisplayCalculationResult
    }
}

