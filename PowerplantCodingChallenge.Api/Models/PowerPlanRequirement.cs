using System.Text.Json.Serialization;

namespace PowerplantCodingChallenge.Api.Models
{
    public class PowerPlanRequirement
    {
        public decimal Load { get; set; }
        public Fuels Fuels { get; set; }
        public List<Powerplant> Powerplants { get; set; }
    }

    public class Powerplant
    {
        public string Name { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PowerPlantType Type { get; set; }
        public decimal Efficiency { get; set; }
        public decimal Pmin { get; set; }
        public decimal Pmax { get; set; }
        public decimal UsedLoad { get; set; }
    }

    public class Fuels
    {
        [JsonPropertyName("gas(euro/MWh)")]
        public decimal GasPricePerMWh { get; set; }
        [JsonPropertyName("kerosine(euro/MWh)")]
        public decimal KerosinePricePerMWh { get; set; }
        [JsonPropertyName("co2(euro/ton)")]
        public decimal Co2PricePerTon { get; set; }
        [JsonPropertyName("wind(%)")]
        public decimal WindPercentage { get; set; }
    }

    public enum PowerPlantType
    {
        Windturbine,
        Gasfired,
        Turbojet
    }
}
