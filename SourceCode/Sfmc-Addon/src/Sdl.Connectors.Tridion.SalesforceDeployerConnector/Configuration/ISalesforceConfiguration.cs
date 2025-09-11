namespace Sdl.Connectors.Tridion.SalesforceDeployerConnector.Configuration
{
    using global::Tridion.ConnectorFramework.Contracts.Connector;

    public interface ISalesforceConfiguration : IConnectorConfiguration
    {
        string ClientId { get; set; }

        string ClientSecret { get; set; }

        string HostUrl { get; set; }

        string AccountId { get; set; }

        LocaleMapping[] LocaleMapping { get; set; }

        string MarkupUploadCategoryId { get; set; }

        string ImagesUploadCategoryId { get; set; }
    }
}
