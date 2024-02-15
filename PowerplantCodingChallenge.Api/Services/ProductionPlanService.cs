using PowerplantCodingChallenge.Api.Models;

namespace PowerplantCodingChallenge.Api.Services
{
    public class ProductionPlanService
    {
        public IEnumerable<ProductionPlanItem> GetProductionPlan(PowerPlanRequirement requirement)
        {
            // Sort powerplants based on their type and efficiency
            requirement.Powerplants.Sort((x, y) => 
                PowerplantBusinessCostMetric(y, requirement) <= PowerplantBusinessCostMetric(x, requirement) ? 0 : -1);

            decimal remainingLoad = requirement.Load;
            return requirement.Powerplants.Select(powerplant => CreateOrderedProductionPlanItem(ref remainingLoad, ref powerplant, requirement));
        }

        private static decimal PowerplantBusinessCostMetric(Powerplant powerplant, PowerPlanRequirement requirement)
        {
            var gasCostPerMWh = requirement.Fuels.GasPricePerMWh;
            var kerosineCostPerMWh = requirement.Fuels.KerosinePricePerMWh;
            
            if(powerplant.Efficiency <= 0)
            {
                return decimal.MaxValue;
            }

            return powerplant.Type switch
            {
                "windturbine" => 0,
                "gasfired" => gasCostPerMWh / powerplant.Efficiency,
                _ => kerosineCostPerMWh / powerplant.Efficiency
            };
        }

        private static ProductionPlanItem CreateOrderedProductionPlanItem(ref decimal remainingLoad, ref Powerplant powerplant, PowerPlanRequirement requirement)
        {
            decimal powerToProduce;

            // If remaining load is less than or equal to 0, break the loop
            if (remainingLoad <= 0 || powerplant.UsedLoad == powerplant.Pmax)
            {
                return new ProductionPlanItem { Name = powerplant.Name, Power = 0 };
            }

            // Update remaining load and add to production plan
            powerToProduce = UseOrderedPower(remainingLoad, powerplant, requirement);
            remainingLoad -= powerToProduce;
            return new ProductionPlanItem { Name = powerplant.Name, Power = powerToProduce };
        }
        
        private static decimal UseOrderedPower(decimal remainingLoad, Powerplant powerplant, PowerPlanRequirement requirement)
        {
            decimal proportion = 1;
            if(powerplant.Type is "windturbine")
            {
                proportion *= powerplant.Efficiency * requirement.Fuels.WindPercentage / 100m;
            }

            var powerToProduce = Math.Min(remainingLoad, powerplant.Pmax * proportion);
            return GetOrderedOptimizedPowerFromPowerplant(powerToProduce, powerplant, requirement);
        }

        private static decimal GetOrderedOptimizedPowerFromPowerplant(decimal powerToProduce, 
            Powerplant powerplant, PowerPlanRequirement requirement)
        {
            var potentialCandidates = requirement.Powerplants.SkipWhile(p => p != powerplant || p.UsedLoad == p.Pmax);
            Powerplant bestCandidate = powerplant;
            decimal actualCost = decimal.MaxValue;
            decimal actualLoad = powerToProduce;
            decimal actualPowerToProduce = powerToProduce;

            foreach (var pc in potentialCandidates)
            {
                var gasCostPerMWh = requirement.Fuels.GasPricePerMWh;
                var kerosineCostPerMWh = requirement.Fuels.KerosinePricePerMWh;
                var load = Math.Max(Math.Max(pc.Pmin, powerToProduce), pc.UsedLoad);
                decimal cost;

                switch (pc.Type)
                {
                    case "gasfired":
                        cost = load * (gasCostPerMWh / pc.Efficiency + 0.3m * requirement.Fuels.Co2PricePerTon);
                        break;
                    default:
                        cost = load * kerosineCostPerMWh / pc.Efficiency;
                        break;
                }

                if (cost < actualCost)
                {
                    bestCandidate = pc;
                    actualCost = cost;
                    actualLoad = load;
                }
            }

            if (potentialCandidates.First() != bestCandidate)
            {
                return 0;
            }

            return actualLoad;
        }
    }
}
