// Services/UserService.cs
using Health_Insurance.Data; // Ensure this namespace is correct for your DbContext
using Health_Insurance.Models; // Ensure this namespace is correct for your Models
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using BCrypt.Net; // For password hashing

namespace Health_Insurance.Services // Ensure this namespace is correct
{
    // Implementation of the User (Authentication) Service
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context; // DbContext for database interaction

        // Constructor: Inject the ApplicationDbContext
        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hashes a plain-text password using BCrypt
        public string HashPassword(string password)
        {
            // BCrypt.HashPassword generates a salt and hashes the password
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Verifies a plain-text password against a stored hashed password
        public bool VerifyPassword(string password, string hashedPassword)
        {
            // BCrypt.Verify compares the plain-text password with the hash
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

        // Authenticates a user by username, password, and enforces the expected login type.
        // Returns the authenticated Admin or Employee object, or null if authentication fails
        // or if the user's actual role doesn't match the expected loginType.
        public async Task<object> AuthenticateUserAsync(string username, string password, string loginType) // Added loginType parameter
        {
            if (loginType == "Admin")
            {
                var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Username == username);
                if (admin != null)
                {
                    if (VerifyPassword(password, admin.PasswordHash))
                    {
                        return admin; // Authentication successful for Admin
                    }
                }
            }
            else if (loginType == "Employee")
            {
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Username == username);
                if (employee != null)
                {
                    if (VerifyPassword(password, employee.PasswordHash))
                    {
                        return employee; // Authentication successful for Employee
                    }
                }
            }

            // If no user found for the specified loginType or password incorrect, return null
            return null;
        }
    }
}

