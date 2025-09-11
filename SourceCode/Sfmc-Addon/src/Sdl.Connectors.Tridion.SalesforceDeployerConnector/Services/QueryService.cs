namespace Sdl.Connectors.Tridion.SalesforceDeployerConnector.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Data;
    using global::Tridion.ConnectorFramework.Connector.SDK;
    using global::Tridion.ConnectorFramework.Contracts;
    using global::Tridion.ConnectorFramework.Contracts.Capabilities;
    using global::Tridion.ConnectorFramework.Contracts.Logging;

    public class QueryService : IQueryEntitiesCapability
    {
        private readonly IConnectorLogger logger;
        private readonly ISalesforceContext context;

        public QueryService(ISalesforceContext context, IConnectorLogger logger)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEntityPaginatedList> QueryEntities(IEntityFilter filter, IPaginationData paginationData, IConnectorContext context = null)
        {
            this.logger.LogDebug($"Entered: {nameof(QueryService)}.QueryEntities");

            if (string.IsNullOrWhiteSpace(filter?.SearchText))
            {
                this.logger.LogInfo("No search text provided. Returning empty list");

                return new EntityPaginatedList { Entities = new List<IEntity>(), TotalCount = 0 };
            }

            await this.context.EnsureAuthentication().ConfigureAwait(false);

            this.logger.LogDebug($"Search by: {filter.SearchText}");
            List<IEntity> items = await this.context.SearchByDataProperty(filter).ConfigureAwait(false);

            IEntityPaginatedList list = new EntityPaginatedList
            {
                PageIndex = paginationData?.PageIndex ?? 0,
                PageSize = paginationData?.PageSize ?? 0,
                Entities = items,
                TotalCount = items.Count
            };

            return list;
        }
    }
}
