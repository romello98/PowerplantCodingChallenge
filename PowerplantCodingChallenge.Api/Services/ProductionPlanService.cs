using PowerplantCodingChallenge.Api.Models;

namespace PowerplantCodingChallenge.Api.Services
{
    public class ProductionPlanService
    {
        public IEnumerable<ProductionPlanItem> GetProductionPlan(PowerPlanRequirement requirement)
        {
            requirement.Powerplants.Sort((x, y) => 
                PowerplantBusinessCostMetric(y, requirement) <= PowerplantBusinessCostMetric(x, requirement) ? 1 : -1);

            decimal remainingLoad = requirement.Load;
            return requirement.Powerplants.Select(powerplant => CreateOrderedProductionPlanItem(ref remainingLoad, ref powerplant, requirement)).ToList();
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
                PowerPlantType.Windturbine => -powerplant.Efficiency,
                PowerPlantType.Gasfired => gasCostPerMWh / powerplant.Efficiency,
                _ => kerosineCostPerMWh / powerplant.Efficiency
            };
        }

        private static ProductionPlanItem CreateOrderedProductionPlanItem(ref decimal remainingLoad, ref Powerplant powerplant, PowerPlanRequirement requirement)
        {
            decimal powerToProduce;

            if (remainingLoad <= 0)
            {
                return new ProductionPlanItem { Name = powerplant.Name, Power = 0 };
            }

            powerToProduce = UseOrderedPower(remainingLoad, powerplant, requirement);
            remainingLoad -= powerToProduce;
            return new ProductionPlanItem { Name = powerplant.Name, Power = powerToProduce };
        }
        
        private static decimal UseOrderedPower(decimal remainingLoad, Powerplant powerplant, PowerPlanRequirement requirement)
        {
            decimal proportion = 1;
            if(powerplant.Type is PowerPlantType.Windturbine)
            {
                proportion = powerplant.Efficiency * requirement.Fuels.WindPercentage / 100m;
            }

            var powerToProduce = Math.Min(remainingLoad, powerplant.Pmax * proportion);
            return GetOrderedOptimizedPowerFromPowerplant(powerToProduce, powerplant, requirement);
        }

        private static decimal GetOrderedOptimizedPowerFromPowerplant(decimal powerToProduce, 
            Powerplant powerplant, PowerPlanRequirement requirement)
        {
            var potentialCandidates = requirement.Powerplants.SkipWhile(p => p != powerplant);
            var bestCandidate = powerplant;
            var actualCost = decimal.MaxValue;
            var actualLoad = powerToProduce;
            var actualPowerToProduce = powerToProduce;

            foreach (var pc in potentialCandidates)
            {
                var minimumLoad = pc.Pmin * pc.Efficiency;
                var maximumLoad = pc.Pmax * pc.Efficiency;
                var load = Math.Max(minimumLoad, powerToProduce);

                decimal cost;

                switch (pc.Type)
                {
                    case PowerPlantType.Windturbine:
                        if(maximumLoad * requirement.Fuels.WindPercentage / 100m > powerToProduce)
                        {
                            continue;
                        }
                        cost = 0;
                        break;
                    case PowerPlantType.Gasfired:
                        if(minimumLoad > powerToProduce)
                        {
                            continue;
                        }
                        cost = load * (requirement.Fuels.GasPricePerMWh / pc.Efficiency + Constants.CarbonTonsPerMWh * requirement.Fuels.Co2PricePerTon);
                        break;
                    default:
                        cost = load * requirement.Fuels.KerosinePricePerMWh / pc.Efficiency;
                        break;
                }

                if (cost < actualCost)
                {
                    bestCandidate = pc;
                    actualCost = cost;
                    actualLoad = load;
                }
            }

            if (potentialCandidates.FirstOrDefault() != bestCandidate)
            {
                return 0;
            }

            return actualLoad;
        }
    }
}
