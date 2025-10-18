using System.ComponentModel.DataAnnotations;

namespace FacesmashAPI.Models
{
    /// <summary>
    /// Represents a standard user in the Facesmash application.
    /// Inherits from BaseUser and demonstrates polymorphism through method overriding.
    /// </summary>
    public class User : BaseUser
    {
        /// <summary>
        /// Overrides the base method to provide specific user type information.
        /// Demonstrates polymorphism through method overriding.
        /// </summary>
        /// <returns>String description of the standard user type.</returns>
        public override string GetUserType()
        {
            return "Standard User";
        }

        /// <summary>
        /// Overrides the base method to provide standard user rating calculation.
        /// Standard users have no special rating multiplier.
        /// </summary>
        /// <returns>Rating multiplier for standard users (1.0).</returns>
        public override double GetRatingMultiplier()
        {
            return 1.0; // Standard users have no multiplier
        }

        /// <summary>
        /// Overrides the base method to provide custom display formatting for standard users.
        /// </summary>
        /// <returns>Formatted string with standard user information.</returns>
        public override string GetDisplayInfo()
        {
            return $"{Name} (Standard) - Rating: {Rating} - {Bio ?? "No bio"}";
        }

        /// <summary>
        /// Implements the abstract method from BaseUser.
        /// Validates that a standard user meets all required criteria.
        /// </summary>
        /// <returns>True if the user meets standard user validation criteria.</returns>
        public override bool ValidateUserType()
        {
            // Standard users must have a name, email, and valid gender
            return !string.IsNullOrWhiteSpace(Name) && 
                   !string.IsNullOrWhiteSpace(Email) && 
                   (Gender == "M" || Gender == "F") &&
                   Rating >= 0 && Rating <= 3000;
        }
    }
}
