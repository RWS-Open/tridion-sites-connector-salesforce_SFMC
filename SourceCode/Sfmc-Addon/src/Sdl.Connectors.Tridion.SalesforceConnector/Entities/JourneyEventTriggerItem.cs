namespace Sdl.Connectors.Tridion.SalesforceConnector.Entities
{
    using global::Tridion.ConnectorFramework.Connector.SDK.Attribute;
    using global::Tridion.Remoting.Contracts;

    [Schema("JourneyEventTriggerItem", "Journey Event Trigger Item")]
    public class JourneyEventTriggerItem : IValueObject
    {
        [SchemaField(Required: false, Readonly: false, Description: "Name")]
        public virtual string Name { get; set; }

        [SchemaField(Required: false, Readonly: false, Description: "Value")]
        public virtual string Value { get; set; }
    }
}
