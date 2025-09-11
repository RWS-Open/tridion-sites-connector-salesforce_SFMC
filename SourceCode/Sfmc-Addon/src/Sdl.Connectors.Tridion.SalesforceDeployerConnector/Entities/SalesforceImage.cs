namespace Sdl.Connectors.Tridion.SalesforceDeployerConnector.Entities
{
    using global::Tridion.ConnectorFramework.Connector.SDK.Attribute;
    using global::Tridion.ConnectorFramework.Contracts;

    [Schema("SalesforceImage", "Salesforce Image")]
    public class SalesforceImage : SalesforceAsset
    {
        public SalesforceImage(IEntityIdentity identity, IEntityIdentity parentIdentity) : base(identity,
            parentIdentity)
        {

        }

        [SchemaField(false, true, "PublishedUrl" )]
        public virtual string PublishedUrl { get; set; }
        
        public string FileContent { get; set; }
    }
}
