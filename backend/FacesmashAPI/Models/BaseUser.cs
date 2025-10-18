using System.ComponentModel.DataAnnotations;

namespace FacesmashAPI.Models
{
    /// <summary>
    /// Base class for all user types in the Facesmash application.
    /// Demonstrates polymorphism through inheritance and virtual method overriding.
    /// </summary>
    public abstract class BaseUser
    {
        /// <summary>
        /// Unique identifier for the user.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// User's display name. Must be between 2 and 100 characters.
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// User's email address. Must be a valid email format and unique.
        /// </summary>
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters.")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Hashed password using BCrypt. Never store plain text passwords.
        /// </summary>
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// User's gender. Must be 'M' for Male or 'F' for Female.
        /// </summary>
        [Required]
        [StringLength(1)]
        [RegularExpression("^[MF]$", ErrorMessage = "Gender must be 'M' or 'F'.")]
        public string Gender { get; set; } = string.Empty;

        /// <summary>
        /// URL to the user's profile photo. Can be null if no photo is uploaded.
        /// </summary>
        public string? PhotoUrl { get; set; }

        /// <summary>
        /// User's biography or description. Optional field for additional user information.
        /// </summary>
        [StringLength(500, ErrorMessage = "Bio cannot exceed 500 characters.")]
        public string? Bio { get; set; }

        /// <summary>
        /// User's current rating in the ELO rating system. 
        /// Starts at 1200 (average) and changes based on voting results.
        /// </summary>
        [Range(0, 3000, ErrorMessage = "Rating must be between 0 and 3000.")]
        public int Rating { get; set; } = 1200;

        /// <summary>
        /// Timestamp when the user account was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Virtual method to get user type description.
        /// Can be overridden by derived classes to provide specific user type information.
        /// </summary>
        /// <returns>String description of the user type.</returns>
        public virtual string GetUserType()
        {
            return "Standard User";
        }

        /// <summary>
        /// Virtual method to calculate rating multiplier based on user type.
        /// Different user types may have different rating calculation rules.
        /// </summary>
        /// <returns>Rating multiplier for this user type.</returns>
        public virtual double GetRatingMultiplier()
        {
            return 1.0; // Standard users have no multiplier
        }

        /// <summary>
        /// Virtual method to get user display information.
        /// Can be overridden to provide custom display formatting.
        /// </summary>
        /// <returns>Formatted string with user information.</returns>
        public virtual string GetDisplayInfo()
        {
            return $"{Name} ({GetUserType()}) - Rating: {Rating}";
        }

        /// <summary>
        /// Abstract method that must be implemented by derived classes.
        /// Forces each user type to define its own validation rules.
        /// </summary>
        /// <returns>True if the user meets all validation criteria for their type.</returns>
        public abstract bool ValidateUserType();
    }
}
