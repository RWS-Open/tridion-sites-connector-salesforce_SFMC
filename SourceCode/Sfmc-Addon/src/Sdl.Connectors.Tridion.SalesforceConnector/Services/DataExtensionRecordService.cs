namespace Sdl.Connectors.Tridion.SalesforceConnector.Services
{
    using System;
    using System.Threading.Tasks;
    using Data;
    using Entities;
    using global::Tridion.ConnectorFramework.Connector.SDK;
    using global::Tridion.ConnectorFramework.Contracts;
    using global::Tridion.ConnectorFramework.Contracts.Capabilities;
    using global::Tridion.ConnectorFramework.Contracts.Logging;

    public class DataExtensionRecordService : IGetEntityCapability
    {
        private readonly ISalesforceContext context;
        private readonly IConnectorLogger logger;

        public DataExtensionRecordService(ISalesforceContext context, IConnectorLogger logger)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEntity> GetEntity(IEntityIdentity identity, IConnectorContext context = null)
        {
            // TODO: Implement
            // TODO: How to handle if identity is event ID? Resolve to data record ID? Or should we even return event ID when doing create?
            // For now just echo back the identity to get GraphQL create request to work nicely. Right now when doing create requests
            // a get request is also done under hood in the Content Service to return the created entity.
            //

            return new JourneyEventTrigger
            {
                Identity = new EntityIdentity(identity.Id) {NamespaceId = identity.NamespaceId, Type = identity.Type}
            };
        }
    }
}
