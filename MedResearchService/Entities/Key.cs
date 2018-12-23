using Newtonsoft.Json;

namespace MedResearchService.Entities
{
    public class Key
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "pub")]
        public string PublicKey { get; set; }

        [JsonProperty(PropertyName = "priv")]
        public string PrivateKey { get; set; }
    }
}