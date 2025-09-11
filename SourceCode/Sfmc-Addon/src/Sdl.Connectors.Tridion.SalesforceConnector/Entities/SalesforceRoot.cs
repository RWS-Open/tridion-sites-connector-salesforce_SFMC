namespace Sdl.Connectors.Tridion.SalesforceConnector.Entities
{
    using global::Tridion.ConnectorFramework.Connector.SDK;
    using global::Tridion.ConnectorFramework.Connector.SDK.Content;

    public class SalesforceRoot : RootContentFolder
    {
        public SalesforceRoot(string namespaceId, string localeId)
            : base(namespaceId, localeId)
        {
            this.Icon = new BinaryReference
            {
                NamespaceId = namespaceId,
                Id = "salesforce.png",
                Type = nameof(ContextType.Icon)
            };
        }
    }
}
