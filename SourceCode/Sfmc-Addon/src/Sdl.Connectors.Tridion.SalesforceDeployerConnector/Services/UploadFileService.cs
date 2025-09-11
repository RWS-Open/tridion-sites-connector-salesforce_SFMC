namespace Sdl.Connectors.Tridion.SalesforceDeployerConnector.Services
{
    using System.Threading.Tasks;
    using Data;
    using global::Tridion.ConnectorFramework.Contracts;
    using global::Tridion.ConnectorFramework.Contracts.Capabilities;
    using global::Tridion.ConnectorFramework.Contracts.Content;
    using global::Tridion.ConnectorFramework.Contracts.Logging;

    public class UploadFileService : ICreateMultimediaCapability
    {
        private readonly IConnectorLogger logger;
        private readonly ISalesforceContext context;

        public UploadFileService(ISalesforceContext context, IConnectorLogger logger)
        {
            this.logger = logger;
            this.context = context;
        }

        public async Task<IEntityIdentity> UploadAndCreate(IMultimedia entity, string type, IBinary binary, IConnectorContext context = null)
        {
            this.logger.LogDebug($"Entered: UploadFileService.UploadAndCreate");

            await this.context.EnsureAuthentication().ConfigureAwait(false);

            IEntity asset = await this.context.UploadAsset(entity, type, binary).ConfigureAwait(false);

            return asset.Identity;
        }
    }
}
