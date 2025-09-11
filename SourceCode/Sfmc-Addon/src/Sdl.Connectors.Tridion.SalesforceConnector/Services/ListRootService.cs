namespace Sdl.Connectors.Tridion.SalesforceConnector.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Data;
    using global::Tridion.ConnectorFramework.Connector.SDK;
    using global::Tridion.ConnectorFramework.Contracts;
    using global::Tridion.ConnectorFramework.Contracts.Capabilities;
    using global::Tridion.ConnectorFramework.Contracts.Logging;

    public class ListRootService : IListEntitiesCapability
    {
        private readonly ISalesforceContext context;
        private readonly IConnectorLogger logger;

        public ListRootService(ISalesforceContext context, IConnectorLogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEntityPaginatedList> ListEntities(IEntityIdentity parentIdentity, IPaginationData paginationData, StructureTypeSelector structureTypeSelector = StructureTypeSelector.All, IConnectorContext context = null)
        {
            if (parentIdentity is null)
            {
                this.logger.LogError($"{this.GetType().Name}.{nameof(this.ListEntities)}. Parent identity id is null");
                throw new ArgumentNullException(nameof(parentIdentity));
            }

            this.logger.LogInfo($"{this.GetType().Name}.{nameof(this.ListEntities)}. parent identity id: {parentIdentity.Id}");

            await this.context.EnsureAuthentication().ConfigureAwait(false);

            IList<IEntity> folders = await this.context.GetRootContent(parentIdentity).ConfigureAwait(false);

            IEntityPaginatedList list = new EntityPaginatedList
            {
                PageIndex = paginationData?.PageIndex ?? 0,
                PageSize = paginationData?.PageSize ?? 0,
                Entities = folders,
                TotalCount = folders.Count
            };

            return list;
        }
    }
}
