using Microsoft.AspNetCore.Routing;
using PowerplantCodingChallenge.Api.Exceptions;
using PowerplantCodingChallenge.Api.Models;
using PowerplantCodingChallenge.Api.Services;

namespace PowerplantCodingChallenge.Test
{
    [TestClass]
    public class UnitTest1
    {
        private ProductionPlanService _service;
        private Fuels _baseFuels;

        [TestInitialize]
        public void Init()
        {
            _service = new PowerplantCodingChallenge.Api.Services.ProductionPlanService();
            _baseFuels = new Fuels() { Co2PricePerTon = 20, KerosinePricePerMWh = 50, GasPricePerMWh = 15, WindPercentage = 50 };
        }

        //[TestMethod]
        //public void ComputeBestPowerUsage_CannotProvideLoad_NotEnough()
        //{
        //    // arrange + act
        //    var productionPlan = new PowerPlanRequirement()
        //    {
        //        Fuels = _baseFuels,
        //        Load = 500,
        //        Powerplants = new List<Powerplant>
        //        {
        //            new Powerplant
        //            {
        //                Name = "Gas1",
        //                Type = PowerPlantType.Gasfired,
        //                Efficiency = 50,
        //                Pmin = 50,
        //                Pmax = 100
        //            },
        //            new Powerplant
        //            {
        //                Name = "Gas2",
        //                Type = PowerPlantType.Gasfired,
        //                Efficiency = 50,
        //                Pmin = 50,
        //                Pmax = 100
        //            }
        //        }
        //    };

        //    // assert
        //    Assert.ThrowsException<InvalidLoadException>(() => _planner.ComputeBestPowerUsage(productionPlan));
        //}

        [TestMethod]
        public void ComputeBestPowerUsage_Wind_Enough()
        {
            // arrange
            PowerPlanRequirement productionPlan = new PowerPlanRequirement
            {
                Fuels = _baseFuels,
                Load = 25,
                Powerplants = new List<Powerplant>
                {
                    new Powerplant
                    {
                        Name = "Gas1",
                        Type = PowerPlantType.Gasfired,
                        Efficiency = 0.5m,
                        Pmin = 10,
                        Pmax = 100
                    },
                    new Powerplant
                    {
                        Name = "Wind1",
                        Type = PowerPlantType.Windturbine,
                        Efficiency = 1,
                        Pmin = 0,
                        Pmax = 50
                    }
                }
            };

            // act
            var result = _service.GetProductionPlan(productionPlan);

            // assert
            Assert.AreEqual(25, result.First(x => x.Name == "Wind1").Power);
            Assert.AreEqual(0, result.First(x => x.Name == "Gas1").Power);
        }

        [TestMethod]
        public void ComputeBestPowerUsage_Wind_NotEnough()
        {
            // arrange
            PowerPlanRequirement productionPlan = new PowerPlanRequirement
            {
                Fuels = _baseFuels,
                Load = 50,
                Powerplants = new List<Powerplant>
                {
                    new Powerplant
                    {
                        Name = "Gas1",
                        Type = PowerPlantType.Gasfired,
                        Efficiency = 0.5m,
                        Pmin = 10,
                        Pmax = 100
                    },
                    new Powerplant
                    {
                        Name = "Wind1",
                        Type = PowerPlantType.Windturbine,
                        Efficiency = 1,
                        Pmin = 0,
                        Pmax = 50
                    }
                }
            };

            // act
            var result = _service.GetProductionPlan(productionPlan);

            // assert
            Assert.AreEqual(25, result.First(x => x.Name == "Wind1").Power);
            Assert.AreEqual(25, result.First(x => x.Name == "Gas1").Power);
        }

        [TestMethod]
        public void ComputeBestPowerUsage_Wind_TooMuch()
        {
            PowerPlanRequirement productionPlan = new PowerPlanRequirement
            {
                Fuels = _baseFuels,
                Load = 20,
                Powerplants = new List<Powerplant>
                {
                    new Powerplant
                    {
                        Name = "Gas1",
                        Type = PowerPlantType.Gasfired,
                        Efficiency = 0.5m,
                        Pmin = 10,
                        Pmax = 100
                    },
                    new Powerplant
                    {
                        Name = "Wind1",
                        Type = PowerPlantType.Windturbine,
                        Efficiency = 1,
                        Pmin = 0,
                        Pmax = 50
                    }
                }
            };

            // act
            var result = _service.GetProductionPlan(productionPlan);

            // assert
            Assert.AreEqual(0, result.First(x => x.Name == "Wind1").Power);
            Assert.AreEqual(20, result.First(x => x.Name == "Gas1").Power);
        }

        [TestMethod]
        public void ComputeBestPowerUsage_Gas_Efficiency()
        {
            // arrange
            PowerPlanRequirement productionPlan = new PowerPlanRequirement
            {
                Fuels = _baseFuels,
                Load = 20,
                Powerplants = new List<Powerplant>
                {
                    new Powerplant
                    {
                        Name = "Gas1",
                        Type = PowerPlantType.Gasfired,
                        Efficiency = 0.5m,
                        Pmin = 10,
                        Pmax = 100
                    },
                    new Powerplant
                    {
                        Name = "Gas2",
                        Type = PowerPlantType.Gasfired,
                        Efficiency = 0.6m,
                        Pmin = 10,
                        Pmax = 100
                    },
                    new Powerplant
                    {
                        Name = "Gas3",
                        Type = PowerPlantType.Gasfired,
                        Efficiency = 0.8m,
                        Pmin = 10,
                        Pmax = 100
                    },
                    new Powerplant
                    {
                        Name = "Gas4",
                        Type = PowerPlantType.Gasfired,
                        Efficiency = 0.3m,
                        Pmin = 10,
                        Pmax = 100
                    },
                    new Powerplant
                    {
                        Name = "Gas5",
                        Type = PowerPlantType.Gasfired,
                        Efficiency = 0.45m,
                        Pmin = 10,
                        Pmax = 100
                    }
                }
            };

            // act
            var result = _service.GetProductionPlan(productionPlan);

            // assert
            Assert.AreEqual(20, result.First(x => x.Name == "Gas3").Power);
            Assert.AreEqual(0, result.Where(x => x.Name != "Gas3").Select(x => x.Power).Sum());
        }

        [TestMethod]
        public void ComputeBestPowerUsage_Gas_AllNeeded()
        {
            // arrange
            PowerPlanRequirement productionPlan = new PowerPlanRequirement
            {
                Fuels = _baseFuels,
                Load = 490,
                Powerplants = new List<Powerplant>
                {
                    new Powerplant
                    {
                        Name = "Gas1",
                        Type = PowerPlantType.Gasfired,
                        Efficiency = 0.5m,
                        Pmin = 10,
                        Pmax = 100
                    },
                    new Powerplant
                    {
                        Name = "Gas2",
                        Type = PowerPlantType.Gasfired,
                        Efficiency = 0.6m,
                        Pmin = 10,
                        Pmax = 100
                    },
                    new Powerplant
                    {
                        Name = "Gas3",
                        Type = PowerPlantType.Gasfired,
                        Efficiency = 0.8m,
                        Pmin = 10,
                        Pmax = 100
                    },
                    new Powerplant
                    {
                        Name = "Gas4",
                        Type = PowerPlantType.Gasfired,
                        Efficiency = 0.3m,
                        Pmin = 10,
                        Pmax = 100
                    },
                    new Powerplant
                    {
                        Name = "Gas5",
                        Type = PowerPlantType.Gasfired,
                        Efficiency = 0.45m,
                        Pmin = 10,
                        Pmax = 100
                    }
                }
            };

            // act
            var result = _service.GetProductionPlan(productionPlan);

            // assert
            Assert.AreEqual(100, result.First(x => x.Name == "Gas1").Power);
            Assert.AreEqual(100, result.First(x => x.Name == "Gas2").Power);
            Assert.AreEqual(100, result.First(x => x.Name == "Gas3").Power);
            Assert.AreEqual(90, result.First(x => x.Name == "Gas4").Power);
            Assert.AreEqual(100, result.First(x => x.Name == "Gas5").Power);
        }

        [TestMethod]
        public void ComputeBestPowerUsage_Gas_Pmin()
        {
            // arrange
            PowerPlanRequirement productionPlan = new PowerPlanRequirement
            {
                Fuels = _baseFuels,
                Load = 125,
                Powerplants = new List<Powerplant>
                {
                    new Powerplant
                    {
                        Name = "Wind1",
                        Type = PowerPlantType.Windturbine,
                        Efficiency = 1,
                        Pmin = 0,
                        Pmax = 50
                    },
                    new Powerplant
                    {
                        Name = "Gas1",
                        Type = PowerPlantType.Gasfired,
                        Efficiency = 0.5m,
                        Pmin = 110,
                        Pmax = 200
                    },
                    new Powerplant
                    {
                        Name = "Gas2",
                        Type = PowerPlantType.Gasfired,
                        Efficiency = 0.8m,
                        Pmin = 80,
                        Pmax = 150
                    }
                }
            };

            // act
            var result = _service.GetProductionPlan(productionPlan);

            // assert
            Assert.AreEqual(100, result.First(x => x.Name == "Gas2").Power);
            Assert.AreEqual(0, result.First(x => x.Name == "Gas1").Power);
        }

        [TestMethod]
        public void ComputeBestPowerUsage_Kerosine()
        {
            // arrange
            PowerPlanRequirement productionPlan = new PowerPlanRequirement
            {
                Fuels = _baseFuels,
                Load = 100,
                Powerplants = new List<Powerplant>
                {
                    new Powerplant
                    {
                        Name = "Wind1",
                        Type = PowerPlantType.Windturbine,
                        Efficiency = 1,
                        Pmin = 0,
                        Pmax = 150
                    },
                    new Powerplant
                    {
                        Name = "Gas1",
                        Type = PowerPlantType.Gasfired,
                        Efficiency = 0.5m,
                        Pmin = 100,
                        Pmax = 200
                    },
                    new Powerplant
                    {
                        Name = "Kerosine1",
                        Type = PowerPlantType.Turbojet,
                        Efficiency = 0.5m,
                        Pmin = 0,
                        Pmax = 200
                    }
                }
            };

            // act
            var result = _service.GetProductionPlan(productionPlan);

            // assert
            Assert.AreEqual(0, result.First(x => x.Name == "Gas1").Power);
            Assert.AreEqual(25, result.First(x => x.Name == "Kerosine1").Power);
        }

        [TestMethod]
        public void ComputeBestPowerUsage_CO2Impact()
        {
            // arrange
            PowerPlanRequirement productionPlan = new PowerPlanRequirement
            {
                Fuels = _baseFuels,
                Load = 150,
                Powerplants = new List<Powerplant>
                {
                    new Powerplant
                    {
                        Name = "Gas1",
                        Type = PowerPlantType.Gasfired,
                        Efficiency = 0.3m,
                        Pmin = 100,
                        Pmax = 200
                    },
                    new Powerplant
                    {
                        Name = "Kerosine1",
                        Type = PowerPlantType.Turbojet,
                        Efficiency = 1,
                        Pmin = 0,
                        Pmax = 200
                    }
                }
            };

            // act
            var resultCO2 = _service.GetProductionPlan(productionPlan);

            // assert
            Assert.AreEqual(150, resultCO2.First(x => x.Name == "Kerosine1").Power);
        }

        [TestMethod]
        public void ComputeBestPowerUsage_TrickyTest1()
        {
            // arrange
            Fuels energyMetrics = new Fuels { Co2PricePerTon = 0, KerosinePricePerMWh = 50.8m, GasPricePerMWh = 20, WindPercentage = 100 };
            PowerPlanRequirement productionPlan = new PowerPlanRequirement
            {
                Fuels = energyMetrics,
                Load = 60,
                Powerplants = new List<Powerplant>
                {
                    new Powerplant
                    {
                        Name = "windpark1",
                        Type = PowerPlantType.Windturbine,
                        Efficiency = 1,
                        Pmin = 0,
                        Pmax = 20
                    },
                    new Powerplant
                    {
                        Name = "gasfired",
                        Type = PowerPlantType.Gasfired,
                        Efficiency = 0.9m,
                        Pmin = 50,
                        Pmax = 100
                    },
                    new Powerplant
                    {
                        Name = "gasfiredinefficient",
                        Type = PowerPlantType.Gasfired,
                        Efficiency = 0.1m,
                        Pmin = 0,
                        Pmax = 100
                    }
                }
            };

            // act
            var result = _service.GetProductionPlan(productionPlan);

            // assert
            Assert.AreEqual(60, result.Sum(x => x.Power));
            Assert.AreEqual(0, result.First(x => x.Name == "windpark1").Power);
            Assert.AreEqual(60, result.First(x => x.Name == "gasfired").Power);
            Assert.AreEqual(0, result.First(x => x.Name == "gasfiredinefficient").Power);
        }
    }
}