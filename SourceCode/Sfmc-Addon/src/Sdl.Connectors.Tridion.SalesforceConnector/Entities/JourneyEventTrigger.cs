namespace Sdl.Connectors.Tridion.SalesforceConnector.Entities
{
    using System.Collections.Generic;
    using global::Tridion.ConnectorFramework.Connector.SDK.Attribute;
    using global::Tridion.ConnectorFramework.Connector.SDK.Content;

    [Schema("JourneyEventTrigger", "Journey Event Trigger")]
    public class JourneyEventTrigger : ContentBase
    {
        [SchemaField(false, false, "EventDefinitionKey")]
        public virtual string EventDefinitionKey { get; set; }

        [SchemaField(false, false, "ContactKey")]
        public virtual string ContactKey { get; set; }

        [SchemaField(Required: false, Readonly: false, Description: "Values")]
        public virtual IList<JourneyEventTriggerItem> Values { get; set; }
    }
}
