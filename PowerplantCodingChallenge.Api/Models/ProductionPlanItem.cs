using System.Text.Json.Serialization;

namespace PowerplantCodingChallenge.Api.Models
{
    public class ProductionPlanItem
    {
        public string Name { get; set; }
        [JsonPropertyName("p")]
        public decimal Power { get; set; }
    }
}
