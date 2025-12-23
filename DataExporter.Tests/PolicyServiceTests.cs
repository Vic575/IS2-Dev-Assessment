using Xunit;
using Microsoft.EntityFrameworkCore;
using DataExporter.Services;
using DataExporter.Dtos;
using DataExporter.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace DataExporter.Tests
{
    // Test-specific DbContext that doesn't seed data
    public class TestExporterDbContext : ExporterDbContext
    {
        public TestExporterDbContext(DbContextOptions<ExporterDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // Don't call base.OnConfiguring - we want our test database name, not "ExporterDb"
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Don't call base.OnModelCreating to avoid seeding test data
            // Just configure the model structure without seeding
        }
    }

    public class PolicyServiceTests
    {
        // Helper: Create an in-memory DbContext for testing (unique database per test)
        private ExporterDbContext GetTestDbContext(bool seedData = false)
        {
            var options = new DbContextOptionsBuilder<ExporterDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .Options;
            
            var context = new TestExporterDbContext(options);
            
            // Seed test data if requested
            if (seedData)
            {
                context.Policies.AddRange(
                    new Policy { PolicyNumber = "TEST001", Premium = 100, StartDate = new DateTime(2024, 1, 1) },
                    new Policy { PolicyNumber = "TEST002", Premium = 200, StartDate = new DateTime(2024, 6, 1) }
                );
                context.SaveChanges();
                
                // Get the generated IDs
                var policy1 = context.Policies.First(p => p.PolicyNumber == "TEST001");
                var policy2 = context.Policies.First(p => p.PolicyNumber == "TEST002");
                
                context.Notes.AddRange(
                    new Note { Text = "Note 1", PolicyId = policy1.Id },
                    new Note { Text = "Note 2", PolicyId = policy1.Id }
                );
                context.SaveChanges();
            }
            
            return context;
        }

        // Test: CreatePolicyAsync - success
        [Fact]
        public async Task CreatePolicyAsync_WithValidData_ReturnsNewPolicy()
        {
            // Arrange
            var context = GetTestDbContext();
            var service = new PolicyService(context);
            var dto = new CreatePolicyDto 
            { 
                PolicyNumber = "NEW001", 
                Premium = 150, 
                StartDate = new DateTime(2024, 3, 1) 
            };

            // Act
            var result = await service.CreatePolicyAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("NEW001", result.PolicyNumber);
            Assert.Equal(150, result.Premium);
            Assert.True(result.Id > 0);
        }

        // Test: CreatePolicyAsync - duplicate policy number
        [Fact]
        public async Task CreatePolicyAsync_WithDuplicatePolicyNumber_ReturnsNull()
        {
            // Arrange
            var context = GetTestDbContext(seedData: true);
            var service = new PolicyService(context);
            var dto = new CreatePolicyDto 
            { 
                PolicyNumber = "TEST001", // Already exists
                Premium = 150, 
                StartDate = new DateTime(2024, 3, 1) 
            };

            // Act
            var result = await service.CreatePolicyAsync(dto);

            // Assert
            Assert.Null(result);
        }

        // Test: CreatePolicyAsync - invalid premium
        [Fact]
        public async Task CreatePolicyAsync_WithInvalidPremium_ReturnsNull()
        {
            // Arrange
            var context = GetTestDbContext();
            var service = new PolicyService(context);
            var dto = new CreatePolicyDto 
            { 
                PolicyNumber = "NEW001", 
                Premium = -50, // Invalid
                StartDate = new DateTime(2024, 3, 1) 
            };

            // Act
            var result = await service.CreatePolicyAsync(dto);

            // Assert
            Assert.Null(result);
        }

        // Test: CreatePolicyAsync - invalid date range
        [Fact]
        public async Task CreatePolicyAsync_WithUnrealisticDate_ReturnsNull()
        {
            // Arrange
            var context = GetTestDbContext();
            var service = new PolicyService(context);
            var dto = new CreatePolicyDto 
            { 
                PolicyNumber = "NEW001", 
                Premium = 150, 
                StartDate = new DateTime(1900, 1, 1) // Too far in past
            };

            // Act
            var result = await service.CreatePolicyAsync(dto);

            // Assert
            Assert.Null(result);
        }

        // Test: ReadPoliciesAsync - returns all policies with notes
        [Fact]
        public async Task ReadPoliciesAsync_ReturnsAllPoliciesWithNotes()
        {
            // Arrange
            var context = GetTestDbContext(seedData: true);
            var service = new PolicyService(context);

            // Act
            var result = await service.ReadPoliciesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("TEST001", result[0].PolicyNumber);
            Assert.NotEmpty(result[0].Notes); // Policy 1 has notes
        }

        // Test: ReadPoliciesAsync - empty result when no policies exist
        [Fact]
        public async Task ReadPoliciesAsync_WithNoPolicies_ReturnsEmptyList()
        {
            // Arrange
            var context = GetTestDbContext(seedData: false); // Empty database
            var service = new PolicyService(context);

            // Act
            var result = await service.ReadPoliciesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        // Test: ReadPolicyAsync - returns single policy by id
        [Fact]
        public async Task ReadPolicyAsync_WithValidId_ReturnsSinglePolicy()
        {
            // Arrange
            var context = GetTestDbContext(seedData: true);
            var service = new PolicyService(context);
            var policy1 = context.Policies.First(p => p.PolicyNumber == "TEST001");

            // Act
            var result = await service.ReadPolicyAsync(policy1.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TEST001", result.PolicyNumber);
            Assert.NotEmpty(result.Notes);
        }

        // Test: ReadPolicyAsync - returns null for invalid id
        [Fact]
        public async Task ReadPolicyAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var context = GetTestDbContext(seedData: true);
            var service = new PolicyService(context);

            // Act
            var result = await service.ReadPolicyAsync(999);

            // Assert
            Assert.Null(result);
        }

        // Test: ReadPoliciesFilteredByStartDateEndDateAsync - date range filter
        [Fact]
        public async Task ReadPoliciesFilteredByStartDateEndDateAsync_WithDateRange_ReturnsFiltredPolicies()
        {
            // Arrange
            var context = GetTestDbContext(seedData: true);
            var service = new PolicyService(context);
            var startDate = new DateTime(2024, 1, 1);
            var endDate = new DateTime(2024, 3, 31);

            // Act
            var result = await service.ReadPoliciesFilteredByStartDateEndDateAsync(startDate, endDate);

            // Assert
            Assert.Single(result); // Only TEST001 falls in this range
            Assert.Equal("TEST001", result[0].PolicyNumber);
        }

        // Test: ReadPoliciesFilteredByStartDateEndDateAsync - no matches
        [Fact]
        public async Task ReadPoliciesFilteredByStartDateEndDateAsync_WithNoMatches_ReturnsEmptyList()
        {
            // Arrange
            var context = GetTestDbContext(seedData: true);
            var service = new PolicyService(context);
            var startDate = new DateTime(2025, 1, 1);
            var endDate = new DateTime(2025, 12, 31);

            // Act
            var result = await service.ReadPoliciesFilteredByStartDateEndDateAsync(startDate, endDate);

            // Assert
            Assert.Empty(result);
        }

        // Test: ReadPoliciesFilteredByStartDateEndDateAsync - includes notes
        [Fact]
        public async Task ReadPoliciesFilteredByStartDateEndDateAsync_IncludesNotesInResult()
        {
            // Arrange
            var context = GetTestDbContext(seedData: true);
            var service = new PolicyService(context);
            var startDate = new DateTime(2024, 1, 1);
            var endDate = new DateTime(2024, 12, 31);

            // Act
            var result = await service.ReadPoliciesFilteredByStartDateEndDateAsync(startDate, endDate);

            // Assert
            Assert.NotEmpty(result);
            Assert.NotEmpty(result[0].Notes);
        }
    }
}
