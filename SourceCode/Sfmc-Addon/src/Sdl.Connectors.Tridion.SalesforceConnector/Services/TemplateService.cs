namespace Sdl.Connectors.Tridion.SalesforceConnector.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web;
    using global::Tridion.ConnectorFramework.Contracts;
    using global::Tridion.ConnectorFramework.Contracts.ContentManager.Capabilities;
    using global::Tridion.ConnectorFramework.Contracts.Logging;

    public class TemplateService : ITemplateCapability
    {
        private readonly IConnectorLogger logger;

        public TemplateService(IConnectorLogger logger)
        {
            this.logger = logger;
        }

        // TODO: Should we really return something here? I guess we can return NULL if there is no template fragments available
        public Task<string> GetTemplateFragment(IEntityIdentity identity, IDictionary<string, string> attributes, IConnectorContext context = null)
        {
            string identityIdString = HttpUtility.UrlEncode(identity.Id);

            this.logger.LogDebug($"Entered: {nameof(TemplateService)}.GetTemplateFragment");

            return Task.FromResult(identityIdString);
        }

        public Task<string> GetDirectLinkToPublished(IEntityIdentity identity, IDictionary<string, string> attributes, IConnectorContext context = null)
        {
            string identityIdString = HttpUtility.UrlEncode(identity.Id);
            this.logger.LogDebug($"Entered: {nameof(TemplateService)}.GetDirectLinkToPublished");

            return Task.FromResult(identityIdString);
        }
    }
}
