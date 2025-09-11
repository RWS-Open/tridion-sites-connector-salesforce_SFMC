namespace Sdl.Connectors.Tridion.SalesforceConnector.Services
{
    using System;
    using System.Threading.Tasks;
    using Data;
    using global::Tridion.ConnectorFramework.Contracts;
    using global::Tridion.ConnectorFramework.Contracts.Capabilities;
    using global::Tridion.ConnectorFramework.Contracts.Logging;

    // TODO: Rename to DataExtensionFieldService instead? So it clear what the service handles. FileService is too vague name.
    public class DataExtensionFieldService : IGetEntityCapability
    {
        private readonly ISalesforceContext context;
        private readonly IConnectorLogger logger;

        public DataExtensionFieldService(ISalesforceContext context, IConnectorLogger logger)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEntity> GetEntity(IEntityIdentity identity, IConnectorContext context = null)
        {
            if (identity is null)
            {
                this.logger.LogError($"{this.GetType().Name}.{nameof(this.GetEntity)}. Identity id is null");
                throw new ArgumentNullException(nameof(identity));
            }

            this.logger.LogInfo($"{this.GetType().Name}.{nameof(this.GetEntity)}. parent identity id: {identity.Id}");

            await this.context.EnsureAuthentication().ConfigureAwait(false);

            IEntity file = await this.context.GetDataExtensionFieldInfo(identity).ConfigureAwait(false);

            return file;
        }
    }
}
