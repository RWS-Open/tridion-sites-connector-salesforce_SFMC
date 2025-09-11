namespace Sdl.Connectors.Tridion.SalesforceConnector.Entities
{
    using Newtonsoft.Json;

    public class FireEventBody
    {
        [JsonProperty("ContactKey")]
        public string ContactKey { get; set; }

        [JsonProperty("EventDefinitionKey")]
        public string EventDefinitionKey { get; set; }

        [JsonProperty("Data")]
        public object Data { get; set; }
    }
}
