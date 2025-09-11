namespace Sdl.Connectors.Tridion.SalesforceConnector.Services
{
    using System;
    using System.Threading.Tasks;
    using Data;
    using global::Tridion.ConnectorFramework.Contracts;
    using global::Tridion.ConnectorFramework.Contracts.Capabilities;
    using global::Tridion.ConnectorFramework.Contracts.Logging;

    public class FolderService : IGetEntityCapability
    {
        private readonly ISalesforceContext context;
        private readonly IConnectorLogger connectorLogger;

        public FolderService(ISalesforceContext context, IConnectorLogger connectorLogger)
        {
            this.connectorLogger = connectorLogger ?? throw new ArgumentNullException(nameof(connectorLogger));
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEntity> GetEntity(IEntityIdentity identity, IConnectorContext context = null)
        {
            if (identity == null)
            {
                this.connectorLogger.LogError($"{this.GetType().Name}.{nameof(this.GetEntity)}. Identity id is null");
                throw new ArgumentNullException(nameof(identity));
            }

            this.connectorLogger.LogInfo($"{this.GetType().Name}.{nameof(this.GetEntity)}. parent identity id: {identity.Id}");

            await this.context.EnsureAuthentication().ConfigureAwait(false);

            IEntity folder = await this.context.GetFolderInfo(identity).ConfigureAwait(false);

            return folder;
        }
    }
}
