// Models/Claim.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding; // Needed for [BindNever]

namespace Health_Insurance.Models // Ensure this namespace is correct based on your project name
{
    // Represents a Claim entity, mapping to the Claim table in the database.
    public class Claim
    {
        [Key] // Specifies this property is the primary key
        public int ClaimId { get; set; }

        // Foreign Key to the Enrollment table
        // A Claim is associated with a specific Enrollment
        [Required] // EnrollmentId should be required for a claim
        public int EnrollmentId { get; set; }

        // Navigation property to the related Enrollment
        [ForeignKey("EnrollmentId")] // Specifies the foreign key property
        [BindNever] // Add this attribute to prevent model binding for this property
        public virtual Enrollment Enrollment { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")] // Specify SQL Server data type for precision
        public decimal ClaimAmount { get; set; }

        // Reason for the claim
        [StringLength(500)] // Added a StringLength for ClaimReason
        public string ClaimReason { get; set; }

        [Required]
        [DataType(DataType.Date)] // Specify the data type for date input
        public DateTime ClaimDate { get; set; }

        // Status of the claim as described in the document
        // Removed [Required] attribute here as the status is set by the system on submission.
        [StringLength(20)]
        // Validation for allowed status values ("SUBMITTED", "APPROVED", "REJECTED") should be handled in the service layer or controller.
        public string ClaimStatus { get; set; } // e.g., "SUBMITTED", "APPROVED", "REJECTED"
    }
}




