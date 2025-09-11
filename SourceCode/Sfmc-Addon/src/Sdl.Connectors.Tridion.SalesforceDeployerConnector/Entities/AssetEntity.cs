namespace Sdl.Connectors.Tridion.SalesforceDeployerConnector.Entities
{
    using global::Tridion.ConnectorFramework.Connector.SDK.Attribute;
    using global::Tridion.ConnectorFramework.Connector.SDK.Content;

    [Schema("AssetEntity", "Asset Entity")]
    public class AssetEntity : MultimediaBase
    {
        [SchemaField(Required: false, Readonly: true, Description: "tridionId")]
        public virtual string tridionId { get; set; }
    }
}
