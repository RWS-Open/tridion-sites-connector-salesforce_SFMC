namespace Sdl.Connectors.Tridion.SalesforceConnector.Services
{
    using System;
    using System.Threading.Tasks;
    using Data;
    using global::Tridion.ConnectorFramework.Connector.SDK;
    using global::Tridion.ConnectorFramework.Contracts;
    using global::Tridion.ConnectorFramework.Contracts.Capabilities;
    using global::Tridion.ConnectorFramework.Contracts.Logging;

    // TODO: Move to DataExtensionRecordService??

    public class CreateEntityService : ICreateEntityCapability
    {
        private readonly ISalesforceContext context;
        private readonly IConnectorLogger logger;

        public CreateEntityService(ISalesforceContext context, IConnectorLogger logger)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEntityIdentity> CreateEntity(IEntityIdentity parentId, string type, IEntity entity, IConnectorContext context = null)
        {
            await this.context.EnsureAuthentication().ConfigureAwait(false);

            string instanceId = await this.context.FireEventJourney(parentId, entity).ConfigureAwait(false);

            // TODO: Throw EntityAlreadyExistsException if data extension record is already defined

            IEntityIdentity identity = new EntityIdentity(instanceId)
            {
                LocaleId = parentId.LocaleId,
                NamespaceId = parentId.NamespaceId,
                Type = type
            };

            return identity;
        }
    }
}
