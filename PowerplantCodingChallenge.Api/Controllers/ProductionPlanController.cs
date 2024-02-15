using Microsoft.AspNetCore.Mvc;
using PowerplantCodingChallenge.Api.Models;
using PowerplantCodingChallenge.Api.Services;

namespace PowerplantCodingChallenge.Api.Controllers
{
    [ApiController]
    [Route("productionplan")]
    public class ProductionPlanController(ProductionPlanService productionPlanService) : ControllerBase
    {
        private readonly ProductionPlanService productionPlanService = productionPlanService;

        [HttpPost]
        public async Task<ActionResult<IEnumerable<ProductionPlanItem>>> GetProductionPlan(PowerPlanRequirement payload)
        {
            return Ok(await Task.FromResult(productionPlanService.GetProductionPlan(payload)));
        }
    }
}
