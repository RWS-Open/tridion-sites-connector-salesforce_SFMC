namespace Sdl.Connectors.Tridion.SalesforceDeployerConnector
{
    using System;
    using System.Threading.Tasks;
    using Configuration;
    using Data;
    using Entities;
    using global::Tridion.ConnectorFramework.Connector.SDK;
    using global::Tridion.ConnectorFramework.Contracts;
    using global::Tridion.ConnectorFramework.Contracts.Exception;
    using Services;

    public class SalesforceDeployerConnector : ConnectorBase
    {
        private ISalesforceConfiguration config;

        private UploadFileService uploadFileService;
        private FileService fileService;
        private QueryService queryService;
        private DeleteService deleteService;

        public override string Name => nameof(SalesforceDeployerConnector);

        protected override Type ConnectorConfigurationType => typeof(ISalesforceConfiguration);

        public override Task<IContainerEntity> GetRoot(string localeId = null)
        {
            this.Logger.LogInfo("Entered: GetRoot");
            return Task.FromResult<IContainerEntity>(new SalesforceRoot(this.NamespaceId, localeId));
        }

        protected override Task OnInitialize()
        {
            this.Logger.LogDebug("Entered: OnInitilized");

            this.config = this.Configuration as ISalesforceConfiguration;

            this.ValidateConfiguration(this.config);

            return Task.CompletedTask;
        }

        public override Task Start()
        {
            this.Logger.LogInfo("Entered: Start");

            var context = new SalesforceContext(this.config, this.Logger);

            this.uploadFileService = new UploadFileService(context, this.Logger);
            this.fileService = new FileService(context, this.Logger);
            this.queryService = new QueryService(context, this.Logger);
            this.deleteService = new DeleteService(context, this.Logger);

            this.DefineConnectorWideCapabilityMetadata(this.uploadFileService, this.fileService, this.queryService, this.deleteService);

            return Task.CompletedTask;
        }

        public override Task Stop()
        {
            this.ClearCapabilityMetadata();
            this.config = null;

            this.uploadFileService = null;
            this.fileService = null;
            this.queryService = null;
            this.deleteService = null;

            return Task.CompletedTask;
        }

        private void ValidateConfiguration(ISalesforceConfiguration configuration)
        {
            this.Logger.LogDebug("Validating Configuration.");

            if (string.IsNullOrWhiteSpace(configuration.ClientId))
            {
                throw new ConfigurationException($"{nameof(configuration.ClientId)} is missing from the configuration.");
            }

            if (string.IsNullOrWhiteSpace(configuration.ClientSecret))
            {
                throw new ConfigurationException($"{nameof(configuration.ClientSecret)} is missing from the configuration.");
            }

            if (string.IsNullOrWhiteSpace(configuration.HostUrl))
            {
                throw new ConfigurationException($"{nameof(configuration.HostUrl)} is missing from the configuration.");
            }

            if (string.IsNullOrWhiteSpace(configuration.AccountId))
            {
                throw new ConfigurationException($"{nameof(configuration.AccountId)} is missing from the configuration.");
            }

            if (string.IsNullOrWhiteSpace(configuration.MarkupUploadCategoryId))
            {
                throw new ConfigurationException($"{nameof(configuration.MarkupUploadCategoryId)} is missing from the configuration.");
            }

            if (string.IsNullOrWhiteSpace(configuration.ImagesUploadCategoryId))
            {
                throw new ConfigurationException($"{nameof(configuration.ImagesUploadCategoryId)} is missing from the configuration.");
            }
        }
    }
}
