using DataExporter.Dtos;
using DataExporter.Exceptions;
using DataExporter.Services;
using Microsoft.AspNetCore.Mvc;

namespace DataExporter.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PoliciesController : ControllerBase
    {
        private PolicyService _policyService;

        public PoliciesController(PolicyService policyService) 
        { 
            _policyService = policyService;
        }

        [HttpPost]
        public async Task<IActionResult> PostPolicies([FromBody]CreatePolicyDto createPolicyDto)
        {         
            try
            {
                var newItem = await _policyService.CreatePolicyAsync(createPolicyDto);
                return Ok(newItem);
            }
            catch (PolicyValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetPolicies()
        {
            //return Ok();
            var results = await _policyService.ReadPoliciesAsync();
            return Ok(results);
        }

        [HttpGet("{policyId}")]
        public async Task<IActionResult> GetPolicy(int policyId)
        {
            //return Ok(_policyService.ReadPolicyAsync(id));
            var item = await _policyService.ReadPolicyAsync(policyId);
            return item is null ? NotFound() : Ok(item);
        }


        [HttpPost("export")]
        public async Task<IActionResult> ExportData([FromQuery]DateTime startDate, [FromQuery] DateTime endDate)
        {
            //todo - bring in data matching date ranges, check date ranges if invalid, use mins / maxs etc.
            //if (startDate.)
            var results = await _policyService.ReadPoliciesFilteredByStartDateEndDateAsync(startDate, endDate);
            return Ok(results);
        }
    }
}
