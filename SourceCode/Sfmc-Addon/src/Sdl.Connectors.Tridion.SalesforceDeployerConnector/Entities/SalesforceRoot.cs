namespace Sdl.Connectors.Tridion.SalesforceDeployerConnector.Entities
{
    using global::Tridion.ConnectorFramework.Connector.SDK;
    using global::Tridion.ConnectorFramework.Connector.SDK.Content;

    // This is the root entity for the entire connector
    public class SalesforceRoot : RootContentFolder
    {
        public SalesforceRoot(string namespaceId, string localeId)
            : base(namespaceId, localeId)
        {
            // TODO: Add comments here
            Icon = new BinaryReference
            {
                NamespaceId = namespaceId,
                Id = "SFMC_icon_16.png",
                Type = nameof(DownloadContextType.Icon)
            };
        }
    }
}
