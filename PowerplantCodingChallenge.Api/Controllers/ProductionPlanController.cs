using Microsoft.AspNetCore.Mvc;
using PowerplantCodingChallenge.Api.Models;
using PowerplantCodingChallenge.Api.Services;

namespace PowerplantCodingChallenge.Api.Controllers
{
    [ApiController]
    [Route("productionplan")]
    public class ProductionPlanController : ControllerBase
    {
        private readonly ProductionPlanService _productionPlanService;
        private readonly ILogger<ProductionPlanController> _logger;

        public ProductionPlanController(
            ProductionPlanService productionPlanService,
            ILogger<ProductionPlanController> logger)
        {
            _productionPlanService = productionPlanService;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        public async Task<ActionResult<IEnumerable<ProductionPlanItem>>> GetProductionPlan(PowerPlanRequirement payload)
        {
            try
            {
                return Ok(await Task.FromResult(_productionPlanService.GetProductionPlan(payload)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting production plan");
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
