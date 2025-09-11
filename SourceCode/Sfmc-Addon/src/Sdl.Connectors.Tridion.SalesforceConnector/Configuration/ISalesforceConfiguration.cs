namespace Sdl.Connectors.Tridion.SalesforceConnector.Configuration
{
    using global::Tridion.ConnectorFramework.Contracts.Connector;

    public interface ISalesforceConfiguration : IConnectorConfiguration
    {
        string ClientId { get; set; }

        string ClientSecret { get; set; }

        string HostUrl { get; set; }

        string AccountId { get; set; }
    }
}
