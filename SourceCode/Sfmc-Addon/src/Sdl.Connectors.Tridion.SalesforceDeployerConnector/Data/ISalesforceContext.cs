namespace Sdl.Connectors.Tridion.SalesforceDeployerConnector.Data
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using global::Tridion.ConnectorFramework.Contracts;
    using global::Tridion.ConnectorFramework.Contracts.Content;

    public interface ISalesforceContext
    {
        Task EnsureAuthentication(CancellationToken cancellationToken = default);

        Task<IEntity> UploadAsset(IMultimedia multimedia, string entityType, IBinary binary, CancellationToken cancellationToken = default);

        Task<IEntity> GetEntity(IEntityIdentity parentIdentity, CancellationToken cancellationToken = default);

        Task<List<IEntity>> SearchByDataProperty(IEntityFilter filter, CancellationToken cancellationToken = default);

        Task DeleteEntity(IEntityIdentity identity, CancellationToken cancellationToken = default);
    }
}
