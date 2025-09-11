namespace Sdl.Connectors.Tridion.SalesforceDeployerConnector.Entities
{
    using global::Tridion.ConnectorFramework.Connector.SDK.Attribute;
    using global::Tridion.ConnectorFramework.Contracts;

    [Schema("SalesforceCodeSnippet", "Salesforce Code Snippet")]
    public class SalesforceCodeSnippet : SalesforceAsset
    {
        public SalesforceCodeSnippet(IEntityIdentity identity, IEntityIdentity parentIdentity) : base(identity,
            parentIdentity)
        {

        }

        [SchemaField(false, true, "Content")]
        public virtual string Content { get; set; } 
    }
}
