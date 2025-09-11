namespace Sdl.Connectors.Tridion.SalesforceConnector.Data
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using global::Tridion.ConnectorFramework.Contracts;

    public interface ISalesforceContext
    {
        Task EnsureAuthentication(CancellationToken cancellationToken = default);

        Task<IList<IEntity>> GetRootContent(IEntityIdentity parentIdentity, CancellationToken cancellationToken = default);

        Task<IEntity> GetFolderInfo(IEntityIdentity identity, CancellationToken cancellationToken = default);

        Task<IList<IEntity>> GetContentList(IEntityIdentity parentIdentity, CancellationToken cancellationToken = default);

        Task<IEntity> GetDataExtensionFieldInfo(IEntityIdentity parentIdentity, CancellationToken cancellationToken = default);

        //Task<EventDefinitionList> GetEventDefinitionList(IEntityIdentity identity, CancellationToken cancellationToken = default);

        //Task<DataExtensionItem> GetDataExtensionItemInfo(IEntityIdentity identity, CancellationToken cancellationToken = default);

        Task<string> FireEventJourney(IEntityIdentity identity, IEntity entity, CancellationToken cancellationToken = default);

        Task<List<IEntity>> GetActiveEventDefinitions(IEntityIdentity parentIdentity, CancellationToken cancellationToken = default);

        Task<IEntity> GetEventDefinition(IEntityIdentity identity, CancellationToken cancellationToken = default);

        Task<IEntity> GetContactInfo(IEntityIdentity identity, CancellationToken cancellationToken = default);
    }
}
