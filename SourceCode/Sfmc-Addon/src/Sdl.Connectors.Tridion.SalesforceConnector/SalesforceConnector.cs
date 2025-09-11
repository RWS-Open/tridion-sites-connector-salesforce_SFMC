namespace Sdl.Connectors.Tridion.SalesforceConnector
{
    using System;
    using System.Threading.Tasks;
    using Configuration;
    using Data;
    using Entities;
    using global::Tridion.ConnectorFramework.Connector.SDK;
    using global::Tridion.ConnectorFramework.Contracts;
    using global::Tridion.ConnectorFramework.Contracts.Content.Capabilities;
    using global::Tridion.ConnectorFramework.Contracts.Exception;
    using Services;

    public class SalesforceConnector : ConnectorBase
    {
        private ISalesforceConfiguration config;

        private DownloadBinaryService downloadBinaryService;
        private ListRootService listRootService;
        private FolderService folderService;
        private ListFolderService listFolderService;
        private DataExtensionFieldService dataExtensionFieldService;
        private TemplateService templateService;
        private CreateEntityService createEntityService;
        private DataExtensionRecordService dataExtensionRecordService;
        private EventDefinitionService eventDefinitionService;
        private ContactService contactService;

        public override string Name => nameof(SalesforceConnector);

        protected override Type ConnectorConfigurationType => typeof(ISalesforceConfiguration);

        public override Task<IContainerEntity> GetRoot(string localeId = null)
        {
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
            var context = new SalesforceContext(this.config, this.Logger);

            var createThumbnailCapability = this.GetConnectorManagerCapability<ICreateThumbnailCapability>();

            this.downloadBinaryService = new DownloadBinaryService(context, this.Logger, createThumbnailCapability);
            this.listRootService = new ListRootService(context, this.Logger);
            this.folderService = new FolderService(context, this.Logger);
            this.listFolderService = new ListFolderService(context, this.Logger);
            this.dataExtensionFieldService = new DataExtensionFieldService(context, this.Logger);
            this.templateService = new TemplateService(this.Logger);
            this.createEntityService = new CreateEntityService(context, this.Logger);
            this.dataExtensionRecordService = new DataExtensionRecordService(context, this.Logger);
            this.eventDefinitionService = new EventDefinitionService(context, this.Logger);
            this.contactService = new ContactService(context, this.Logger);

            this.DefineConnectorWideCapabilityMetadata(this.listRootService, this.downloadBinaryService, this.templateService);

            this.DefineEntityCapabilityMetadata(nameof(SalesforceFolder),
                typeof(SalesforceFolder), this.listFolderService, this.folderService);

            this.DefineEntityCapabilityMetadata(nameof(EventDefinitionItem),
                typeof(EventDefinitionItem), this.eventDefinitionService);

            this.DefineEntityCapabilityMetadata(nameof(DataExtensionFolder),
                typeof(DataExtensionFolder), this.listFolderService, this.folderService);

            this.DefineEntityCapabilityMetadata(nameof(DataExtensionField),
                typeof(DataExtensionField), this.dataExtensionFieldService);

            this.DefineEntityCapabilityMetadata(nameof(JourneyEventTrigger),
                typeof(JourneyEventTrigger), this.createEntityService, this.dataExtensionRecordService);

            this.DefineEntityCapabilityMetadata(nameof(Contact),
                typeof(Contact), this.contactService);

            return Task.CompletedTask;
        }

        public override Task Stop()
        {
            this.ClearCapabilityMetadata();

            this.downloadBinaryService = null;
            this.listRootService = null;
            this.folderService = null;
            this.dataExtensionFieldService = null;
            this.templateService = null;
            this.createEntityService = null;
            this.dataExtensionRecordService = null;
            this.eventDefinitionService = null;
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
        }
    }
}
