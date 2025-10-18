using System.ComponentModel.DataAnnotations;

namespace FacesmashAPI.Models
{
    /// <summary>
    /// Represents a VIP user in the Facesmash application.
    /// Inherits from BaseUser and demonstrates polymorphism through method overriding.
    /// VIP users have special privileges and different rating calculations.
    /// </summary>
    public class VipUser : BaseUser
    {
        /// <summary>
        /// VIP membership level (Bronze, Silver, Gold, Platinum).
        /// </summary>
        [Required]
        [StringLength(20)]
        public string VipLevel { get; set; } = "Bronze";

        /// <summary>
        /// Date when VIP membership expires.
        /// </summary>
        public DateTime VipExpiryDate { get; set; } = DateTime.UtcNow.AddMonths(1);

        /// <summary>
        /// Overrides the base method to provide VIP user type information.
        /// Demonstrates polymorphism through method overriding.
        /// </summary>
        /// <returns>String description of the VIP user type with level.</returns>
        public override string GetUserType()
        {
            return $"VIP User ({VipLevel})";
        }

        /// <summary>
        /// Overrides the base method to provide VIP user rating calculation.
        /// VIP users get different rating multipliers based on their VIP level.
        /// </summary>
        /// <returns>Rating multiplier for VIP users based on their level.</returns>
        public override double GetRatingMultiplier()
        {
            return VipLevel.ToLower() switch
            {
                "bronze" => 1.1,   // 10% bonus
                "silver" => 1.2,   // 20% bonus
                "gold" => 1.3,     // 30% bonus
                "platinum" => 1.5, // 50% bonus
                _ => 1.0            // Default to no bonus
            };
        }

        /// <summary>
        /// Overrides the base method to provide custom display formatting for VIP users.
        /// </summary>
        /// <returns>Formatted string with VIP user information including special badges.</returns>
        public override string GetDisplayInfo()
        {
            var vipBadge = VipLevel.ToLower() switch
            {
                "bronze" => "🥉",
                "silver" => "🥈",
                "gold" => "🥇",
                "platinum" => "💎",
                _ => "⭐"
            };

            return $"{vipBadge} {Name} (VIP {VipLevel}) - Rating: {Rating} - {Bio ?? "No bio"}";
        }

        /// <summary>
        /// Implements the abstract method from BaseUser.
        /// Validates that a VIP user meets all required criteria including VIP-specific rules.
        /// </summary>
        /// <returns>True if the user meets VIP user validation criteria.</returns>
        public override bool ValidateUserType()
        {
            // VIP users must meet standard criteria plus VIP-specific criteria
            var standardValidation = !string.IsNullOrWhiteSpace(Name) && 
                                   !string.IsNullOrWhiteSpace(Email) && 
                                   (Gender == "M" || Gender == "F") &&
                                   Rating >= 0 && Rating <= 3000;

            var vipValidation = !string.IsNullOrWhiteSpace(VipLevel) &&
                              VipExpiryDate > DateTime.UtcNow &&
                              new[] { "Bronze", "Silver", "Gold", "Platinum" }.Contains(VipLevel);

            return standardValidation && vipValidation;
        }

        /// <summary>
        /// Checks if the VIP membership is still active.
        /// </summary>
        /// <returns>True if VIP membership is active, false if expired.</returns>
        public bool IsVipActive()
        {
            return VipExpiryDate > DateTime.UtcNow;
        }

        /// <summary>
        /// Gets the number of days remaining in VIP membership.
        /// </summary>
        /// <returns>Number of days until VIP membership expires.</returns>
        public int GetDaysUntilExpiry()
        {
            if (!IsVipActive())
                return 0;

            return (VipExpiryDate - DateTime.UtcNow).Days;
        }
    }
}
