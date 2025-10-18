using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using FacesmashAPI.Data;
using FacesmashAPI.Models;
using FacesmashAPI.Services;
using FacesmashAPI.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace FacesmashAPI.Tests
{
    /// <summary>
    /// Unit tests for AuthenticationService.
    /// Demonstrates NUnit testing framework usage.
    /// </summary>
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private AppDbContext _context;
        private AuthenticationService _authService;
        private Mock<ILogger<AuthenticationService>> _mockLogger;

        /// <summary>
        /// Sets up the test environment before each test.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            // Create in-memory database for testing
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _mockLogger = new Mock<ILogger<AuthenticationService>>();
            _authService = new AuthenticationService(_context, _mockLogger.Object);

            // Seed test data
            SeedTestData();
        }

        /// <summary>
        /// Cleans up after each test.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        /// <summary>
        /// Seeds test data for unit tests.
        /// </summary>
        private void SeedTestData()
        {
            var testUser = new User
            {
                Id = 1,
                Name = "Test User",
                Email = "test@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                Gender = "M",
                Rating = 1200,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(testUser);
            _context.SaveChanges();
        }

        /// <summary>
        /// Tests successful user authentication.
        /// </summary>
        [Test]
        public async Task AuthenticateAsync_ValidCredentials_ReturnsUser()
        {
            // Arrange
            var email = "test@example.com";
            var password = "password123";

            // Act
            var result = await _authService.AuthenticateAsync(email, password);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(email, result.Email);
            Assert.AreEqual("Test User", result.Name);
        }

        /// <summary>
        /// Tests authentication with invalid email.
        /// </summary>
        [Test]
        public async Task AuthenticateAsync_InvalidEmail_ReturnsNull()
        {
            // Arrange
            var email = "nonexistent@example.com";
            var password = "password123";

            // Act
            var result = await _authService.AuthenticateAsync(email, password);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests authentication with invalid password.
        /// </summary>
        [Test]
        public async Task AuthenticateAsync_InvalidPassword_ReturnsNull()
        {
            // Arrange
            var email = "test@example.com";
            var password = "wrongpassword";

            // Act
            var result = await _authService.AuthenticateAsync(email, password);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests successful user registration.
        /// </summary>
        [Test]
        public async Task RegisterAsync_ValidData_ReturnsUser()
        {
            // Arrange
            var name = "New User";
            var email = "newuser@example.com";
            var password = "newpassword123";
            var gender = "F";

            // Act
            var result = await _authService.RegisterAsync(name, email, password, gender);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(name, result.Name);
            Assert.AreEqual(email, result.Email);
            Assert.AreEqual(gender, result.Gender);
            Assert.AreEqual(1200, result.Rating); // Default starting rating
        }

        /// <summary>
        /// Tests registration with duplicate email.
        /// </summary>
        [Test]
        public async Task RegisterAsync_DuplicateEmail_ReturnsNull()
        {
            // Arrange
            var name = "Another User";
            var email = "test@example.com"; // Already exists
            var password = "password123";
            var gender = "M";

            // Act
            var result = await _authService.RegisterAsync(name, email, password, gender);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests registration with invalid data.
        /// </summary>
        [Test]
        public async Task RegisterAsync_InvalidData_ReturnsNull()
        {
            // Arrange
            var name = ""; // Invalid empty name
            var email = "invalid-email"; // Invalid email format
            var password = ""; // Invalid empty password
            var gender = "X"; // Invalid gender

            // Act
            var result = await _authService.RegisterAsync(name, email, password, gender);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests email registration check for existing email.
        /// </summary>
        [Test]
        public async Task IsEmailRegisteredAsync_ExistingEmail_ReturnsTrue()
        {
            // Arrange
            var email = "test@example.com";

            // Act
            var result = await _authService.IsEmailRegisteredAsync(email);

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Tests email registration check for non-existing email.
        /// </summary>
        [Test]
        public async Task IsEmailRegisteredAsync_NonExistingEmail_ReturnsFalse()
        {
            // Arrange
            var email = "nonexistent@example.com";

            // Act
            var result = await _authService.IsEmailRegisteredAsync(email);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Tests password hashing functionality.
        /// </summary>
        [Test]
        public void HashPassword_ValidPassword_ReturnsHash()
        {
            // Arrange
            var password = "testpassword123";

            // Act
            var hash = _authService.HashPassword(password);

            // Assert
            Assert.IsNotNull(hash);
            Assert.IsNotEmpty(hash);
            Assert.AreNotEqual(password, hash); // Hash should be different from original
        }

        /// <summary>
        /// Tests password verification with correct password.
        /// </summary>
        [Test]
        public void VerifyPassword_CorrectPassword_ReturnsTrue()
        {
            // Arrange
            var password = "testpassword123";
            var hash = _authService.HashPassword(password);

            // Act
            var result = _authService.VerifyPassword(password, hash);

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Tests password verification with incorrect password.
        /// </summary>
        [Test]
        public void VerifyPassword_IncorrectPassword_ReturnsFalse()
        {
            // Arrange
            var password = "testpassword123";
            var wrongPassword = "wrongpassword";
            var hash = _authService.HashPassword(password);

            // Act
            var result = _authService.VerifyPassword(wrongPassword, hash);

            // Assert
            Assert.IsFalse(result);
        }
    }

    /// <summary>
    /// Unit tests for User model polymorphism.
    /// Demonstrates testing polymorphic behavior.
    /// </summary>
    [TestFixture]
    public class UserPolymorphismTests
    {
        /// <summary>
        /// Tests User class polymorphic methods.
        /// </summary>
        [Test]
        public void User_GetUserType_ReturnsStandardUser()
        {
            // Arrange
            var user = new User
            {
                Name = "Test User",
                Email = "test@example.com",
                Gender = "M",
                Rating = 1200
            };

            // Act
            var userType = user.GetUserType();

            // Assert
            Assert.AreEqual("Standard User", userType);
        }

        /// <summary>
        /// Tests User class rating multiplier.
        /// </summary>
        [Test]
        public void User_GetRatingMultiplier_ReturnsOne()
        {
            // Arrange
            var user = new User
            {
                Name = "Test User",
                Email = "test@example.com",
                Gender = "M",
                Rating = 1200
            };

            // Act
            var multiplier = user.GetRatingMultiplier();

            // Assert
            Assert.AreEqual(1.0, multiplier);
        }

        /// <summary>
        /// Tests User class validation.
        /// </summary>
        [Test]
        public void User_ValidateUserType_ValidUser_ReturnsTrue()
        {
            // Arrange
            var user = new User
            {
                Name = "Test User",
                Email = "test@example.com",
                Gender = "M",
                Rating = 1200
            };

            // Act
            var isValid = user.ValidateUserType();

            // Assert
            Assert.IsTrue(isValid);
        }

        /// <summary>
        /// Tests User class validation with invalid data.
        /// </summary>
        [Test]
        public void User_ValidateUserType_InvalidUser_ReturnsFalse()
        {
            // Arrange
            var user = new User
            {
                Name = "", // Invalid empty name
                Email = "invalid-email", // Invalid email
                Gender = "X", // Invalid gender
                Rating = -100 // Invalid rating
            };

            // Act
            var isValid = user.ValidateUserType();

            // Assert
            Assert.IsFalse(isValid);
        }
    }

    /// <summary>
    /// Unit tests for VipUser model polymorphism.
    /// Demonstrates testing polymorphic behavior with inheritance.
    /// </summary>
    [TestFixture]
    public class VipUserPolymorphismTests
    {
        /// <summary>
        /// Tests VipUser class polymorphic methods.
        /// </summary>
        [Test]
        public void VipUser_GetUserType_ReturnsVipUser()
        {
            // Arrange
            var vipUser = new VipUser
            {
                Name = "VIP User",
                Email = "vip@example.com",
                Gender = "F",
                Rating = 1500,
                VipLevel = "Gold"
            };

            // Act
            var userType = vipUser.GetUserType();

            // Assert
            Assert.AreEqual("VIP User (Gold)", userType);
        }

        /// <summary>
        /// Tests VipUser class rating multiplier for Gold level.
        /// </summary>
        [Test]
        public void VipUser_GetRatingMultiplier_GoldLevel_ReturnsCorrectMultiplier()
        {
            // Arrange
            var vipUser = new VipUser
            {
                Name = "VIP User",
                Email = "vip@example.com",
                Gender = "F",
                Rating = 1500,
                VipLevel = "Gold"
            };

            // Act
            var multiplier = vipUser.GetRatingMultiplier();

            // Assert
            Assert.AreEqual(1.3, multiplier); // Gold level has 30% bonus
        }

        /// <summary>
        /// Tests VipUser class validation.
        /// </summary>
        [Test]
        public void VipUser_ValidateUserType_ValidVipUser_ReturnsTrue()
        {
            // Arrange
            var vipUser = new VipUser
            {
                Name = "VIP User",
                Email = "vip@example.com",
                Gender = "F",
                Rating = 1500,
                VipLevel = "Gold",
                VipExpiryDate = DateTime.UtcNow.AddMonths(1)
            };

            // Act
            var isValid = vipUser.ValidateUserType();

            // Assert
            Assert.IsTrue(isValid);
        }

        /// <summary>
        /// Tests VipUser VIP membership expiry check.
        /// </summary>
        [Test]
        public void VipUser_IsVipActive_ActiveMembership_ReturnsTrue()
        {
            // Arrange
            var vipUser = new VipUser
            {
                Name = "VIP User",
                Email = "vip@example.com",
                Gender = "F",
                Rating = 1500,
                VipLevel = "Gold",
                VipExpiryDate = DateTime.UtcNow.AddMonths(1) // Future date
            };

            // Act
            var isActive = vipUser.IsVipActive();

            // Assert
            Assert.IsTrue(isActive);
        }

        /// <summary>
        /// Tests VipUser VIP membership expiry check with expired membership.
        /// </summary>
        [Test]
        public void VipUser_IsVipActive_ExpiredMembership_ReturnsFalse()
        {
            // Arrange
            var vipUser = new VipUser
            {
                Name = "VIP User",
                Email = "vip@example.com",
                Gender = "F",
                Rating = 1500,
                VipLevel = "Gold",
                VipExpiryDate = DateTime.UtcNow.AddDays(-1) // Past date
            };

            // Act
            var isActive = vipUser.IsVipActive();

            // Assert
            Assert.IsFalse(isActive);
        }
    }
}
