// Controllers/EmployeeController.cs
using Health_Insurance.Data; // Ensure namespace is correct for your DbContext
using Health_Insurance.Models; // Ensure namespace is correct for your Models
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering; // Needed for SelectList in Create/Edit views

namespace Health_Insurance.Controllers // Ensure this namespace is correct for your Controllers folder
{
    // Controller to handle requests related to Employees
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context; // DbContext instance for database interaction

        // Constructor: Dependency Injection of the DbContext
        public EmployeeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Employee/Index
        // Action to display a list of all employees
        public async Task<IActionResult> Index()
        {
            // Retrieve all employees from the database, including their related Organization
            // .Include(e => e.Organization) is needed to load the Organization data
            // If Organization is null when displaying, check if you included it here.
            var employees = await _context.Employees.Include(e => e.Organization).ToListAsync();
            // Pass the list of employees to the view (Views/Employee/Index.cshtml)
            return View(employees);
        }

        // GET: /Employee/Details/5
        // Action to display details of a specific employee
        public async Task<IActionResult> Details(int? id)
        {
            // Check if id is null
            if (id == null)
            {
                return NotFound(); // Return 404 if id is not provided
            }

            // Find the employee by id, including their related Organization
            var employee = await _context.Employees
                .Include(e => e.Organization)
                .FirstOrDefaultAsync(m => m.EmployeeId == id);

            // Check if employee was found
            if (employee == null)
            {
                return NotFound(); // Return 404 if employee is not found
            }

            // Pass the employee object to the view (Views/Employee/Details.cshtml)
            return View(employee);
        }

        // GET: /Employee/Create
        // Action to display the form for creating a new employee
        public IActionResult Create()
        {
            // To populate a dropdown for selecting Organization, pass data using SelectList
            ViewData["OrganizationId"] = new SelectList(_context.Organizations, "OrganizationId", "OrganizationName");
            // Return the Create view (Views/Employee/Create.cshtml)
            return View();
        }

        // POST: /Employee/Create
        // Action to handle the submission of the new employee form
        [HttpPost] // Specifies that this action handles POST requests
        [ValidateAntiForgeryToken] // Prevents Cross-Site Request Forgery attacks
        public async Task<IActionResult> Create([Bind("Name,Email,Phone,Address,Designation,OrganizationId")] Employee employee)
        {
            // Check if the submitted model is valid based on data annotations (e.g., [Required])
            if (ModelState.IsValid)
            {
                // Add the new employee to the DbContext
                _context.Add(employee);
                // Save changes to the database asynchronously
                await _context.SaveChangesAsync();
                // Redirect to the Index action to display the list of employees
                return RedirectToAction(nameof(Index));
            }
            // If the model is not valid, repopulate the dropdown and return the view with the submitted data to show validation errors
            ViewData["OrganizationId"] = new SelectList(_context.Organizations, "OrganizationId", "OrganizationName", employee.OrganizationId);
            return View(employee);
        }

        // GET: /Employee/Edit/5
        // Action to display the form for editing an existing employee
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Find the employee to edit
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            // Populate the dropdown for Organizations, pre-selecting the employee's current organization
            ViewData["OrganizationId"] = new SelectList(_context.Organizations, "OrganizationId", "OrganizationName", employee.OrganizationId);
            // Return the Edit view (Views/Employee/Edit.cshtml)
            return View(employee);
        }

        // POST: /Employee/Edit/5
        // Action to handle the submission of the edited employee form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EmployeeId,Name,Email,Phone,Address,Designation,OrganizationId")] Employee employee)
        {
            // Check if the id in the URL matches the id in the submitted form data
            if (id != employee.EmployeeId)
            {
                return NotFound();
            }

            // Check if the submitted model is valid
            if (ModelState.IsValid)
            {
                try
                {
                    // Update the employee in the DbContext
                    _context.Update(employee);
                    // Save changes to the database
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Handle concurrency conflicts (e.g., another user updated the same record)
                    if (!EmployeeExists(employee.EmployeeId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw; // Re-throw the exception if it's not a "not found" issue
                    }
                }
                // Redirect to the Index action after successful update
                return RedirectToAction(nameof(Index));
            }
            // If the model is not valid, repopulate the dropdown and return the view with the submitted data
            ViewData["OrganizationId"] = new SelectList(_context.Organizations, "OrganizationId", "OrganizationName", employee.OrganizationId);
            return View(employee);
        }

        // GET: /Employee/Delete/5
        // Action to display the confirmation page for deleting an employee
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Find the employee to delete, including their related Organization
            var employee = await _context.Employees
                .Include(e => e.Organization)
                .FirstOrDefaultAsync(m => m.EmployeeId == id);
            if (employee == null)
            {
                return NotFound();
            }

            // Pass the employee object to the view (Views/Employee/Delete.cshtml)
            return View(employee);
        }

        // POST: /Employee/Delete/5
        // Action to handle the deletion of an employee
        [HttpPost, ActionName("Delete")] // Specifies that this action handles POST requests and is named "Delete" (overloading the GET Delete)
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Find the employee to delete
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                // Remove the employee from the DbContext
                _context.Employees.Remove(employee);
            }

            // Save changes to the database
            await _context.SaveChangesAsync();
            // Redirect to the Index action after successful deletion
            return RedirectToAction(nameof(Index));
        }

        // Helper method to check if an employee exists
        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.EmployeeId == id);
        }
    }
}



