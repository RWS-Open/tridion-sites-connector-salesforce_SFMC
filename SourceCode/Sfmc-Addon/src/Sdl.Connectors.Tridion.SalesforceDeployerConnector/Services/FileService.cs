namespace Sdl.Connectors.Tridion.SalesforceDeployerConnector.Services
{
    using System;
    using System.Threading.Tasks;
    using Data;
    using global::Tridion.ConnectorFramework.Contracts;
    using global::Tridion.ConnectorFramework.Contracts.Capabilities;
    using global::Tridion.ConnectorFramework.Contracts.Logging;

    public class FileService : IGetEntityCapability
    {
        public readonly ISalesforceContext context;
        private readonly IConnectorLogger logger;

        public FileService(ISalesforceContext context, IConnectorLogger logger)
        {
            this.logger = logger;
            this.context = context;
        }

        public async Task<IEntity> GetEntity(IEntityIdentity identity, IConnectorContext context = null)
        {
            this.logger.LogDebug($"Entered: {nameof(QueryService)}.GetEntity");

            if (identity == null)
            {
                throw new ArgumentNullException(nameof(identity));
            }

            await this.context.EnsureAuthentication().ConfigureAwait(false);

            IEntity file = await this.context.GetEntity(identity).ConfigureAwait(false);

            return file;
        }
    }
}
