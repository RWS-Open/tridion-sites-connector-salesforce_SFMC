namespace Sdl.Connectors.Tridion.SalesforceDeployerConnector.Services
{
    using System;
    using System.Threading.Tasks;
    using Data;
    using global::Tridion.ConnectorFramework.Contracts;
    using global::Tridion.ConnectorFramework.Contracts.Capabilities;
    using global::Tridion.ConnectorFramework.Contracts.Logging;

    public class DeleteService : IDeleteEntityCapability
    {
        private readonly IConnectorLogger logger;
        private readonly ISalesforceContext context;

        public DeleteService(ISalesforceContext context, IConnectorLogger logger)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task DeleteEntity(IEntityIdentity identity, IConnectorContext context = null)
        {
            this.logger.LogDebug($"Entered: {nameof(QueryService)}.DeleteEntity");
            this.logger.LogDebug($"Delete entity: {identity.Id}");
            await this.context.DeleteEntity(identity).ConfigureAwait(false);
        }
    }
}
