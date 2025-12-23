using System.Resources;
using DataExporter.Dtos;
using DataExporter.Exceptions;
using Microsoft.EntityFrameworkCore;


namespace DataExporter.Services
{
    public class PolicyService
    {
        private ExporterDbContext _dbContext;

        public PolicyService(ExporterDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbContext.Database.EnsureCreated();
        }

        /// <summary>
        /// Creates a new policy from the DTO.
        /// </summary>
        /// <param name="policy"></param>
        /// <returns>Returns a ReadPolicyDto representing the new policy, if succeded. Returns null, otherwise.</returns>
        public async Task<ReadPolicyDto?> CreatePolicyAsync(CreatePolicyDto createPolicyDto)
        {
            
            // Additional business validation (duplicate check)
            if (await _dbContext.Policies.AnyAsync(p => p.PolicyNumber == createPolicyDto.PolicyNumber))
                throw new PolicyValidationException("A policy with this policy number already exists.");
            
            // Validate input
            if (string.IsNullOrWhiteSpace(createPolicyDto.PolicyNumber))
                throw new PolicyValidationException("Policy number is required.");
            
            if (createPolicyDto.Premium <= 0)
                throw new PolicyValidationException("Premium must be greater than zero.");
            
            if (createPolicyDto.StartDate < DateTime.UtcNow.Date.AddYears(-10) || 
                createPolicyDto.StartDate > DateTime.UtcNow.Date.AddYears(10))
            {
                throw new PolicyValidationException("Start date must be within the last 10 years or next 10 years.");
            }
    
            var policy = new Model.Policy
            {
                PolicyNumber = createPolicyDto.PolicyNumber,
                Premium = createPolicyDto.Premium,
                StartDate = createPolicyDto.StartDate
            };
            
            _dbContext.Policies.Add(policy);
            await _dbContext.SaveChangesAsync();
            
            var newPolicyDto = new ReadPolicyDto
            {
                Id = policy.Id,
                PolicyNumber = policy.PolicyNumber,
                Premium = policy.Premium,
                StartDate = policy.StartDate
            };

            return newPolicyDto;
        }

        /// <summary>
        /// Retrives all policies.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Returns a list of ReadPoliciesDto.</returns>
        public async Task<IList<ReadPolicyDto>> ReadPoliciesAsync()
        {
            //return await Task.FromResult(new List<ReadPolicyDto>());
            var results = await _dbContext.Policies
                .AsNoTracking()
                .Include(p => p.Notes)
                .Select(p => new ReadPolicyDto
                {
                Id = p.Id,
                PolicyNumber = p.PolicyNumber,
                Premium = p.Premium,
                StartDate = p.StartDate,
                Notes = p.Notes
                })
                .ToListAsync();
                return results;
        }

        /// <summary>
        /// Retrieves a policy by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Returns a ReadPolicyDto.</returns>
        public async Task<ReadPolicyDto?> ReadPolicyAsync(int id)
        {
            var policy = await _dbContext.Policies.Include(p => p.Notes).SingleOrDefaultAsync(x => x.Id == id);
            if (policy == null)
            {
                return null;
            }

            var policyDto = new ReadPolicyDto()
            {
                Id = policy.Id,
                PolicyNumber = policy.PolicyNumber,
                Premium = policy.Premium,
                StartDate = policy.StartDate,
                Notes = policy.Notes
            };

            return policyDto;
        }

        /// <summary>
        /// Retrieves policies within specified date range
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Returns a ReadPolicyDto.</returns>
        public async Task<List<ReadPolicyDto>> ReadPoliciesFilteredByStartDateEndDateAsync(DateTime startDate, DateTime endDate)
        {
          
            var results = await _dbContext.Policies
            .Where(x => x.StartDate >= startDate && x.StartDate <= endDate)
            .Select(p => new ReadPolicyDto
                {
                Id = p.Id,
                PolicyNumber = p.PolicyNumber,
                Premium = p.Premium,
                StartDate = p.StartDate,
                Notes = p.Notes
                })
                .ToListAsync();
            return results;
        }
    }
}
