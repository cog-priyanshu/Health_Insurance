﻿// Models/Employee.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding; // Needed for [BindNever]

namespace Health_Insurance.Models // Ensure this namespace is correct based on your project name
{
    // Represents an Employee entity, mapping to the Employee table in the database.
    public class Employee
    {
        [Key] // Specifies this property is the primary key
        public int EmployeeId { get; set; }

        [Required] // Specifies that this property is required
        [StringLength(100)] // Specifies the maximum length of the string
        public string Name { get; set; }

        [StringLength(100)]
        [EmailAddress] // Provides validation for email format
        public string Email { get; set; }

        [StringLength(15)]
        public string Phone { get; set; }

        public string Address { get; set; }

        [StringLength(50)]
        public string Designation { get; set; }

        // Foreign Key to the Organization table
        public int OrganizationId { get; set; }

        // Navigation property to the related Organization
        [ForeignKey("OrganizationId")] // Specifies the foreign key property
        [BindNever] // Add this attribute to prevent model binding for this property
        public virtual Organizations Organization { get; set; }

        // --- Authentication Fields for Employee Login ---
        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [StringLength(256)] // Store hashed passwords, so a longer length is needed
        public string PasswordHash { get; set; }
        // --- End Authentication Fields ---

        // Navigation property for Enrollments (if needed later)
        // public virtual ICollection<Enrollment> Enrollments { get; set; }
    }
}


