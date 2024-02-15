using System.Text.Json;

namespace ProductionPlan
{
    class Program
    {
        static void Main(string[] args)
        {
            // Deserialize payload
            string payloadJson = @"{
              ""load"": 910,
              ""fuels"": {
                ""gas(euro/MWh)"": 13.4,
                ""kerosine(euro/MWh)"": 50.8,
                ""co2(euro/ton)"": 20,
                ""wind(%)"": 60
              },
              ""powerplants"": [
                {
                  ""name"": ""gasfiredbig1"",
                  ""type"": ""gasfired"",
                  ""efficiency"": 0.53,
                  ""pmin"": 100,
                  ""pmax"": 460
                },
                {
                  ""name"": ""gasfiredbig2"",
                  ""type"": ""gasfired"",
                  ""efficiency"": 0.53,
                  ""pmin"": 100,
                  ""pmax"": 460
                },
                {
                  ""name"": ""gasfiredsomewhatsmaller"",
                  ""type"": ""gasfired"",
                  ""efficiency"": 0.37,
                  ""pmin"": 40,
                  ""pmax"": 210
                },
                {
                  ""name"": ""tj1"",
                  ""type"": ""turbojet"",
                  ""efficiency"": 0.3,
                  ""pmin"": 0,
                  ""pmax"": 16
                },
                {
                  ""name"": ""windpark1"",
                  ""type"": ""windturbine"",
                  ""efficiency"": 1,
                  ""pmin"": 0,
                  ""pmax"": 150
                },
                {
                  ""name"": ""windpark2"",
                  ""type"": ""windturbine"",
                  ""efficiency"": 1,
                  ""pmin"": 0,
                  ""pmax"": 36
                }
              ]
            }";

            var payload = JsonSerializer.Deserialize<Payload>(payloadJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Calculate production plan
            var productionPlan = CalculateProductionPlan(payload);

            // Serialize and print the production plan
            string productionPlanJson = JsonSerializer.Serialize(productionPlan, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            });
            Console.WriteLine(productionPlanJson);
        }

        static List<ProductionPlanItem> CalculateProductionPlan(Payload payload)
        {
            var productionPlan = new List<ProductionPlanItem>();

            // Sort powerplants based on their type and efficiency
            payload.Powerplants.Sort((x, y) => PowerplantBusinessCost(y, payload) <= PowerplantBusinessCost(x, payload) ? 0 : -1);

            double remainingLoad = payload.Load;

            foreach (var powerplant in payload.Powerplants)
            {
                double powerToProduce = 0;

                // If remaining load is less than or equal to 0, break the loop
                if (remainingLoad <= 0)
                {
                    productionPlan.Add(new ProductionPlanItem { Name = powerplant.Name, Power = 0 });
                    continue;
                }

                // Calculate power to produce based on powerplant type
                switch (powerplant.Type)
                {
                    case "gasfired":
                    case "turbojet":
                        powerToProduce = Math.Min(remainingLoad, powerplant.Pmax);
                        powerToProduce = Math.Max(powerToProduce, powerplant.Pmin); // Ensure the power produced meets the minimum requirement
                        break;
                    case "windturbine":
                        powerToProduce = Math.Min(remainingLoad, powerplant.Pmax * powerplant.Efficiency * payload.Fuels["wind(%)"] / 100d);
                        powerToProduce = Math.Max(powerToProduce, powerplant.Pmin); // Ensure the power produced meets the minimum requirement
                        break;
                }

                // Update remaining load and add to production plan
                remainingLoad -= powerToProduce;
                productionPlan.Add(new ProductionPlanItem { Name = powerplant.Name, Power = powerToProduce });
            }

            return productionPlan;
        }

        static double PowerplantBusinessCost(Powerplant powerplant, Payload payload)
        {
            double gasCostPerMWh = payload.Fuels["gas(euro/MWh)"];
            double kerosineCostPerMWh = payload.Fuels["kerosine(euro/MWh)"];

            return powerplant.Type switch
            {
                "windturbine" => powerplant.Efficiency,
                "gasfired" => gasCostPerMWh / powerplant.Efficiency,
                _ => kerosineCostPerMWh / powerplant.Efficiency
            };
        }
    }

    public class Payload
    {
        public double Load { get; set; }
        public Dictionary<string, double> Fuels { get; set; }
        public List<Powerplant> Powerplants { get; set; }
    }

    public class Powerplant
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public double Efficiency { get; set; }
        public double Pmin { get; set; }
        public double Pmax { get; set; }
    }

    public class ProductionPlanItem
    {
        public string Name { get; set; }
        public double Power { get; set; }
    }
}
