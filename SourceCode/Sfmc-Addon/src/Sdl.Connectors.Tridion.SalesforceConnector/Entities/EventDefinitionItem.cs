namespace Sdl.Connectors.Tridion.SalesforceConnector.Entities
{
    using Castle.Core.Resource;
    using global::Tridion.ConnectorFramework.Connector.SDK.Attribute;
    using global::Tridion.ConnectorFramework.Connector.SDK.Content;
    using global::Tridion.ConnectorFramework.Contracts;

    [Schema("EventDefinitionItem", "Event Definition Item")]
    public class EventDefinitionItem : MultimediaBase
    {
        public EventDefinitionItem()
        { }

        public EventDefinitionItem(IEntityIdentity identity, IEntityIdentity parentIdentity)
        {
            base.Identity = identity;

            base.ParentIdentity = parentIdentity;
        }

        [SchemaField(false, true, "Name")]
        public virtual string Name { get; set; }
        
        [SchemaField(false, true, "Event Definition Key")]
        public virtual string EventDefinitionKey { get; set; }

        [SchemaField(false, true, "Data Extension Id")]
        public virtual string DataExtensionId { get; set; }

        [SchemaField(false, true, "Data Extension Name")]
        public virtual string DataExtensionName { get; set; }
    }
}
