namespace Sdl.Connectors.Tridion.SalesforceConnector.Entities
{
    using global::Tridion.ConnectorFramework.Connector.SDK.Attribute;
    using global::Tridion.ConnectorFramework.Connector.SDK.Content;
    using global::Tridion.ConnectorFramework.Contracts;

    [Schema("DataExtensionField", "Data Extension Field")]
    public class DataExtensionField : MultimediaBase
    {
        public DataExtensionField()
        { }

        public DataExtensionField(IEntityIdentity identity, IEntityIdentity parentIdentity)
        {
            base.Identity = identity;

            base.ParentIdentity = parentIdentity;
        }

        [SchemaField(false, true, "Customer Key")]
        public virtual string CustomerKey { get; set; }

        [SchemaField(false, true, "Field Type")]
        public virtual string FieldType { get; set; }

        [SchemaField(false, true, "Max Length")]
        public virtual string MaxLength { get; set; }

        [SchemaField(false, true, "Is Primary Key")]
        public virtual string IsPrimaryKey { get; set; }
    }
}
