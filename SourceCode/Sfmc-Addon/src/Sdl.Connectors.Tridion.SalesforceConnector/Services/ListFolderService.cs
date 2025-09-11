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

    public class ListFolderService : IListEntitiesCapability
    {
        private readonly ISalesforceContext context;
        private readonly IConnectorLogger connectorLogger;

        public ListFolderService(ISalesforceContext context, IConnectorLogger logger)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.connectorLogger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEntityPaginatedList> ListEntities(IEntityIdentity parentIdentity, IPaginationData paginationData, StructureTypeSelector structureTypeSelector = StructureTypeSelector.All, IConnectorContext context = null)
        {
            if (parentIdentity is null)
            {
                this.connectorLogger.LogError($"{this.GetType().Name}.{nameof(this.ListEntities)}. Parent identity id is null");
                throw new ArgumentNullException(nameof(parentIdentity));
            }

            this.connectorLogger.LogInfo($"{this.GetType().Name}.{nameof(this.ListEntities)}. Parent identity id: {parentIdentity.Id}");

            await this.context.EnsureAuthentication().ConfigureAwait(false);

            IList<IEntity> items = await this.context.GetContentList(parentIdentity).ConfigureAwait(false);

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
