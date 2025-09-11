namespace Sdl.Connectors.Tridion.SalesforceConnector.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Contract.Authorization;
    using Common.Contract.Configuration;
    using Common.Contract.ConnectedInfo;
    using Configuration;
    using ContentService.Contract;
    using Entities;
    using global::Tridion.ConnectorFramework.Connector.SDK;
    using global::Tridion.ConnectorFramework.Contracts;
    using global::Tridion.ConnectorFramework.Contracts.Exception;
    using global::Tridion.ConnectorFramework.Contracts.Logging;
    using Newtonsoft.Json;
    using SalesforceContentService;
    using SalesforceContentService.Contract;
    using SalesforceContentService.Models;
    using Utils;
    using ConnectorException = Common.Contract.Exception.ConnectorException;
    using SystemException = System.SystemException;

    public class SalesforceContext : ISalesforceContext
    {
        #region Private Fields

        private readonly ISalesforceContentService contentService;
        private readonly ISalesforceConfiguration config;
        private readonly IConnectorLogger logger;

        private AuthenticationEntity AuthenticationEntity { get; }

        #endregion Private Fields

        #region Constructor

        public SalesforceContext(ISalesforceConfiguration configuration, IConnectorLogger logger)
        {
            this.config = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this.AuthenticationEntity = CreateAuthEntity(configuration);

            this.contentService = new SalesforceContentService(this.AuthenticationEntity.Configuration);
        }

        #endregion Constructor

        #region Auth

        public async Task EnsureAuthentication(CancellationToken cancellationToken = default)
        {
            try
            {
                bool isAuthenticated = await this.contentService.IsAuthenticated(this.AuthenticationEntity.Token, cancellationToken)
                    .ConfigureAwait(false);

                if (isAuthenticated)
                {
                    return;
                }

                OAuthToken newToken = await this.contentService.Authenticate(cancellationToken).ConfigureAwait(false);
                this.AuthenticationEntity.Token = newToken;
            }
            catch (Exception ex)
            {
                throw new SystemException("Authentication failed.", ex);
            }
        }

        #endregion Auth

        #region Root

        public async Task<IList<IEntity>> GetRootContent(IEntityIdentity parentIdentity, CancellationToken cancellationToken = default)
        {
            try
            {
                List<ConnectedFileSystemEntry> dataFolders = await this.contentService.GetDataFolders(this.AuthenticationEntity.Token, new ContentIdentity { Id = "0" }, cancellationToken).ConfigureAwait(false);

                List<ConnectedFileSystemEntry> sharedDataFolders = await this.contentService.GetSharedDataFolders(this.AuthenticationEntity.Token, new ContentIdentity { Path = "shared_dataextension_default" }, cancellationToken).ConfigureAwait(false);

                dataFolders = Helper.EncodeFolders(dataFolders, ContentType.DataFolder.ToString());
                sharedDataFolders = Helper.EncodeFolders(sharedDataFolders, ContentType.SharedDataFolder.ToString());

                List<IEntity> entities = dataFolders.Select(connectedFileSystemEntry => connectedFileSystemEntry.MapSaleforceFolder(parentIdentity, true)).Cast<IEntity>().ToList();
                entities.AddRange(sharedDataFolders.Select(connectedFileSystemEntry => connectedFileSystemEntry.MapSaleforceFolder(parentIdentity, true)));

                ConnectedFolder journeyFolder = new ConnectedFolder(Helper.EventDefinitionForlderName)
                {
                    Id = Helper.JourneysId
                };

                entities.Add(journeyFolder.MapSaleforceFolder(parentIdentity, true));

                return entities;
            }
            catch (Exception ex)
            {
                throw new SystemException("GetRootContent failed.", ex);
            }
        }

        #endregion Root

        #region Folder Info

        public async Task<IEntity> GetFolderInfo(IEntityIdentity identity, CancellationToken cancellation = default)
        {
            try
            {
                if (identity.Id.Equals(Helper.JourneysId))
                {
                    ConnectedFolder journeyFolder = new ConnectedFolder(Helper.EventDefinitionForlderName)
                    {
                        Id = Helper.JourneysId
                    };

                    return journeyFolder.MapSaleforceFolder(identity, true);
                }
                else
                {
                    ContentIdentity contentIdentity = Helper.DecodeIdentityId(identity.Id);

                    if (contentIdentity.Context == ContentType.DataFolder.ToString() || contentIdentity.Context == ContentType.SharedDataFolder.ToString())
                    {
                        ConnectedFolder folder = await this.contentService.GetFolderInfo(this.AuthenticationEntity.Token, contentIdentity, cancellation).ConfigureAwait(false);

                        folder.Id = Helper.CreateComposedId(contentIdentity.Context, folder.Id);

                        IEntity entity;
                        //TODO: fix when find a solution to get the base shared items folder
                        if (folder.ParentId == "0" || folder.ParentId == "6022")
                        {
                            entity = folder.MapSaleforceFolder(identity, true);
                        }
                        else
                        {
                            ContentIdentity parentContentIdentity = new ContentIdentity { Context = this.SetParentIdentityContext(contentIdentity), Id = folder.ParentId };

                            ConnectedFolder parentFolder = await this.contentService.GetFolderInfo(this.AuthenticationEntity.Token, parentContentIdentity, cancellation).ConfigureAwait(false);
                            entity = folder.MapSaleforceFolder(identity, false, parentFolder);
                        }

                        return entity;
                    }
                    else
                    {
                        ExtendedConnectedFile file =
                            await this.contentService.GetFileInfo(this.AuthenticationEntity.Token, contentIdentity, cancellation).ConfigureAwait(false) as ExtendedConnectedFile;

                        ContentIdentity parentContentIdentity = new ContentIdentity { Context = this.SetParentIdentityContext(contentIdentity), Id = file?.ParentId };

                        ConnectedFolder folder = new ConnectedFolder(file?.FullName, file?.Id, file?.ParentId);

                        folder.Id = Helper.CreateComposedId(contentIdentity.Context, folder.Id);

                        return folder.MapDataExtensionFolder(identity, parentContentIdentity);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new SystemException("GetFolderInfo failed.", ex);
            }
        }

        #endregion Folder Info

        #region Content List

        public async Task<IList<IEntity>> GetContentList(IEntityIdentity parentIdentity, CancellationToken cancellationToken = default)
        {
            try
            {
                if (parentIdentity != null && parentIdentity.Id == Helper.JourneysId)
                {
                    return await this.GetActiveEventDefinitions(parentIdentity, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    ContentIdentity contentIdentity = Helper.DecodeIdentityId(parentIdentity?.Id);

                    switch (contentIdentity.Context)
                    {
                        case nameof(ContentType.DataFolder):
                            {
                                List<ConnectedFileSystemEntry> entities = new List<ConnectedFileSystemEntry>();

                                List<ConnectedFileSystemEntry> folders = await this.contentService
                                    .GetDataFolders(this.AuthenticationEntity.Token, new ContentIdentity { Id = contentIdentity.Id }, cancellationToken).ConfigureAwait(false);

                                entities.AddRange(Helper.EncodeFolders(folders, ContentType.DataFolder.ToString()));

                                List<ConnectedFileSystemEntry> dataExtensions = await this.contentService
                                    .GetDataExtensions(this.AuthenticationEntity.Token, new ContentIdentity { Id = contentIdentity.Id }, cancellationToken).ConfigureAwait(false);

                                entities.AddRange(Helper.EncodeFolders(dataExtensions, ContentType.DataExtensionFolder.ToString()));

                                return entities.ToFrameWorkEntities(parentIdentity);
                            }

                        case nameof(ContentType.SharedDataFolder):
                            {
                                List<ConnectedFileSystemEntry> entities = new List<ConnectedFileSystemEntry>();

                                List<ConnectedFileSystemEntry> folders = await this.contentService
                                    .GetSharedDataFolders(this.AuthenticationEntity.Token, new ContentIdentity { Id = contentIdentity.Id }, cancellationToken).ConfigureAwait(false);

                                entities.AddRange(Helper.EncodeFolders(folders, ContentType.SharedDataFolder.ToString()));

                                List<ConnectedFileSystemEntry> files = await this.contentService
                                    .GetSharedDataExtensions(this.AuthenticationEntity.Token, new ContentIdentity { Id = contentIdentity.Id }, cancellationToken).ConfigureAwait(false);

                                entities.AddRange(Helper.EncodeFolders(files, ContentType.SharedDataExtensionFolder.ToString()));

                                return entities.ToFrameWorkEntities(parentIdentity);
                            }

                        case nameof(ContentType.SharedDataExtensionFolder):
                        case nameof(ContentType.DataExtensionFolder):
                            {
                                List<DataExtensionFieldsModel> files = await this.contentService.GetDataExtensionsFields(this.AuthenticationEntity.Token, contentIdentity, cancellationToken)
                                    .ConfigureAwait(false);

                                files = files.SetDataExtensionListIds(parentIdentity);

                                return files.ToDataExtensionFields(parentIdentity);
                            }
                    }

                    throw new ConnectorException($"Error encountered while navigating, no appropriate context found {contentIdentity.Info()}");
                }
            }
            catch (Exception ex)
            {
                throw new SystemException("GetContentList failed.", ex);
            }
        }

        #endregion Content List

        #region Get Data Extension Fields

        public async Task<IEntity> GetDataExtensionFieldInfo(IEntityIdentity parentIdentity, CancellationToken cancellationToken = default)
        {
            try
            {
                string[] customerKeyInfos = Helper.DecodeDataExtensionId(parentIdentity.Id);
                ContentIdentity identity = new ContentIdentity
                {
                    Id = customerKeyInfos[0],
                    Path = customerKeyInfos[1]
                };

                List<DataExtensionFieldsModel> files = await this.contentService.GetDataExtensionsFields(this.AuthenticationEntity.Token, identity, cancellationToken).ConfigureAwait(false);

                DataExtensionFieldsModel field = files.FirstOrDefault(x => x.Name == identity.Path);

                field = field.SetDataExtensionIdProperty(parentIdentity);

                return field.MapDataExtensionField(parentIdentity);
            }
            catch (Exception ex)
            {
                throw new SystemException("GetDataExtensionFieldInfo failed.", ex);
            }
        }

        #endregion Get Data Extension Fields

        #region Fire Event

        public async Task<string> FireEventJourney(IEntityIdentity identity, IEntity entity, CancellationToken cancellationToken = default)
        {
            try
            {
                JourneyEventTrigger record = entity.Cast<JourneyEventTrigger>();

                this.logger.LogDebug($"FireJourneyMethod. Record content: {JsonConvert.SerializeObject(record)}");

                EventDefinitionModel eventDefinitionModel = await this.contentService
                    .GetEventDefinition(this.AuthenticationEntity.Token, new ContentIdentity { Id = record.EventDefinitionKey }, cancellationToken).ConfigureAwait(false);//eventDefinitionList.Events.FirstOrDefault(x => x.DataExtensionId == item.ObjectId);

                string contactKey = record.ContactKey;
                if (contactKey == null)
                {
                    DataExtensionModel dataExtensionModel = await this.contentService
                        .GetDataExtensionModel(this.AuthenticationEntity.Token, new ContentIdentity { Id = eventDefinitionModel.DataExtensionId }, cancellationToken).ConfigureAwait(false);

                    DataExtensionFieldsModel primaryKey = await this.GetPrimaryKey(dataExtensionModel.CustomerKey, cancellationToken).ConfigureAwait(false);

                    contactKey = record.Values.FirstOrDefault(x => x.Name == primaryKey.Name)?.Value;

                    if (string.IsNullOrWhiteSpace(contactKey))
                    {
                        throw new ArgumentException($"Primary key is empty: {nameof(contactKey)}");
                    }
                }

                Dictionary<string, string> dataContent = record.Values.ToDictionary(y => y.Name, y => y.Value);

                FireEventBody body = new FireEventBody();
                body.ContactKey = contactKey;
                body.EventDefinitionKey = eventDefinitionModel.EventDefinitionKey;
                body.Data = dataContent;

                string jsonBody = JsonConvert.SerializeObject(body);

                this.logger.LogDebug($"Firing journey event on event definition key: {eventDefinitionModel.EventDefinitionKey}");

                EventInstance eventInstance = await this.contentService.FireJourneyEvent(this.AuthenticationEntity.Token, jsonBody, cancellationToken).ConfigureAwait(false);

                // TODO: Return record here instead??

                return eventInstance.Id;
            }
            catch (Exception ex)
            {
                throw new SystemException("GetDataExtensionItemInfo failed.", ex);
            }
        }

        #endregion Fire Event

        #region GetEventDefinition

        public async Task<IEntity> GetEventDefinition(IEntityIdentity identity, CancellationToken cancellationToken = default)
        {
            try
            {
                ContentIdentity contentIdentity = new ContentIdentity
                {
                    Id = identity.Id,
                };

                EventDefinitionModel eventDefinition = await this.contentService.GetEventDefinition(this.AuthenticationEntity.Token, contentIdentity, cancellationToken).ConfigureAwait(false);

                return eventDefinition.MapToEventDefinitiontem(identity);
            }
            catch (Exception ex)
            {
                throw new SystemException("GetEventDefinition failed.", ex);
            }
        }

        #endregion GetEventDefinition

        #region GetActiveEventDefinitions

        public async Task<List<IEntity>> GetActiveEventDefinitions(IEntityIdentity parentIdentity, CancellationToken cancellationToken = default)
        {
            try
            {
                List<IEntity> entities = new List<IEntity>();
                List<Interaction> fullInfoInteractions = new List<Interaction>();

                EventDefinitionList eventDefinitionList = await this.GetEventDefinitionList(cancellationToken).ConfigureAwait(false);
                Interactions interactions = await this.contentService.GetInteractions(this.AuthenticationEntity.Token, cancellationToken).ConfigureAwait(false);

                foreach (Interaction interaction in interactions.Items)
                {
                    Interaction tempInteraction = await this.contentService.GetInteraction(this.AuthenticationEntity.Token, new ContentIdentity { Id = interaction.Id }, false, cancellationToken)
                        .ConfigureAwait(false);

                    fullInfoInteractions.Add(tempInteraction);
                }

                foreach (EventDefinitionModel eventDefinitionModel in eventDefinitionList.Events)
                {
                    Interaction interaction = fullInfoInteractions.FirstOrDefault(x => x.Activities.Any(y => y.ConfigurationArguments.EventDefinitionKey != null && y.ConfigurationArguments.EventDefinitionKey == eventDefinitionModel.EventDefinitionKey) || x.Defaults.ToString().Contains(eventDefinitionModel.EventDefinitionKey));
                    if (interaction != null)
                    {
                        eventDefinitionModel.Status = interaction.Status;
                        entities.Add(eventDefinitionModel.MapToEventDefinitiontem(parentIdentity));
                    }
                }

                return entities;
            }
            catch (Exception ex)
            {
                throw new SystemException("GetActiveEventDefinitions failed.", ex);
            }
        }

        #endregion GetActiveEventDefinitions

        #region GetContactInfo

        public async Task<IEntity> GetContactInfo(IEntityIdentity identity, CancellationToken cancellationToken = default)
        {
            try
            {
                Contact contact = Mapper.CreateContactEntity(identity, identity.Id);

                ContactMembershipResults contactMembershipResults =
                    await this.contentService.GetContactMembership(this.AuthenticationEntity.Token, identity?.Id, cancellationToken).ConfigureAwait(false);

                if (contactMembershipResults.Results.ContactMembershipList.Count != 0)
                {
                    EventDefinitionList eventDefinitionList = await this.GetEventDefinitionList(cancellationToken).ConfigureAwait(false);

                    foreach (ContactInfo contactInfo in contactMembershipResults.Results.ContactMembershipList)
                    {
                        ContactJourney contactJourney = new ContactJourney();
                        contactJourney.EventDefinitions = new List<EventDefinition>();

                        Interaction interaction = await this.contentService
                               .GetInteraction(this.AuthenticationEntity.Token, new ContentIdentity { Id = contactInfo.DefinitionKey }, true, cancellationToken).ConfigureAwait(false);

                        contactJourney.Name = interaction.Name;

                        EventDefinitionModel eventDefinitionModel = eventDefinitionList.Events.FirstOrDefault(x => interaction.Defaults.ToString().Contains(x.EventDefinitionKey) && interaction.Name == x.Name);

                        EventDefinitionModel apiEventDefinitionModel =
                            eventDefinitionList.Events.FirstOrDefault(x => interaction.Activities.Any(y => y.ConfigurationArguments.EventDefinitionKey == x.EventDefinitionKey));

                        if (apiEventDefinitionModel != null)
                        {
                            EventDefinition eventDefinition = new EventDefinition();
                            eventDefinition.Name = apiEventDefinitionModel.Name;
                            eventDefinition.Key = apiEventDefinitionModel.EventDefinitionKey;

                            contactJourney.EventDefinitions.Add(eventDefinition);
                        }

                        if (eventDefinitionModel != null)
                        {
                            DataExtensionRowListModel rowListModel = await this.contentService
                                        .GetDataExtensionRowList(this.AuthenticationEntity.Token, new ContentIdentity { Id = eventDefinitionModel.DataExtensionId }, cancellationToken).ConfigureAwait(false);
                            Item item = rowListModel.items.FirstOrDefault(x => x.keys.ToString().Contains(identity?.Id) || x.values.ToString().Contains(identity?.Id));

                            EventDefinition eventDefinition = new EventDefinition();
                            eventDefinition.Name = eventDefinitionModel.Name;
                            eventDefinition.Key = eventDefinitionModel.EventDefinitionKey;

                            contactJourney.EventDefinitions.Add(eventDefinition);
                            if (item != null)
                            {
                                Dictionary<string, string> keys = JsonConvert.DeserializeObject<Dictionary<string, string>>(item.keys.ToString());
                                Dictionary<string, string> values = JsonConvert.DeserializeObject<Dictionary<string, string>>(item.values.ToString());

                                contactJourney.ContactData = this.SetContactData(keys, values);
                            }

                            contact.Journeys.Add(contactJourney);
                        }
                    }
                }

                return contact;
            }
            catch (Exception ex)
            {
                throw new EntityNotFoundException("Contact not found.", ex);
            }
        }

        #endregion GetContactInfo

        #region Private Methods

        private List<ContactData> SetContactData(Dictionary<string, string> keys, Dictionary<string, string> values)
        {
            List<ContactData> contactDatas = new List<ContactData>();

            foreach (KeyValuePair<string, string> keyValuePair in keys)
            {
                contactDatas.Add(new ContactData
                {
                    Name = keyValuePair.Key,
                    Value = keyValuePair.Value,
                });
            }

            foreach (KeyValuePair<string, string> keyValuePair in values)
            {
                contactDatas.Add(new ContactData
                {
                    Name = keyValuePair.Key,
                    Value = keyValuePair.Value,
                });
            }

            return contactDatas;
        }

        private async Task<DataExtensionItem> GetDataExtensionItemInfo(IEntityIdentity identity, CancellationToken cancellationToken = default)
        {
            try
            {
                ContentIdentity contentIdentity = new ContentIdentity
                {
                    Id = identity.Id
                };

                ExtendedConnectedFile file = await this.contentService.GetFileInfo(this.AuthenticationEntity.Token, contentIdentity, cancellationToken).ConfigureAwait(false) as ExtendedConnectedFile;

                DataExtensionItem extensionItem = new DataExtensionItem
                {
                    CustomerKey = file?.Id,
                    Name = file?.FullName,
                    ObjectId = file?.ObjectId
                };

                return extensionItem;
            }
            catch (Exception ex)
            {
                throw new SystemException("GetDataExtensionItemInfo failed.", ex);
            }
        }

        private async Task<EventDefinitionList> GetEventDefinitionList(CancellationToken cancellationToken = default)
        {
            try
            {
                EventDefinitionList eventDefinitionList = await this.contentService.GetJourneys(this.AuthenticationEntity.Token, cancellationToken).ConfigureAwait(false);

                return eventDefinitionList;
            }
            catch (Exception ex)
            {
                throw new SystemException("GetEventDefinitionList failed.", ex);
            }
        }

        private async Task<DataExtensionFieldsModel> GetPrimaryKey(string dataExtensionId, CancellationToken cancellationToken = default)
        {
            try
            {
                ContentIdentity contentIdentity = new ContentIdentity
                {
                    Id = dataExtensionId
                };

                List<DataExtensionFieldsModel> fields = await this.contentService.GetDataExtensionsFields(this.AuthenticationEntity.Token, contentIdentity, cancellationToken).ConfigureAwait(false);

                return fields.FirstOrDefault(x => bool.Parse(x.IsPrimaryKey));
            }
            catch (Exception ex)
            {
                throw new SystemException("GetPrimaryKey failed.", ex);
            }
        }

        private string SetParentIdentityContext(ContentIdentity identity)
        {
            if (identity.Context == nameof(ContentType.SharedDataExtensionFolder) || identity.Context == nameof(ContentType.SharedDataFolder))
            {
                return nameof(ContentType.SharedDataFolder);
            }
            else
            {
                return nameof(ContentType.DataFolder);
            }
        }

        private static AuthenticationEntity CreateAuthEntity(ISalesforceConfiguration configuration)
        {
            ConfigurationItem config = new ConfigurationItem
            {
                ClientId = configuration.ClientId,
                ClientSecret = configuration.ClientSecret,
                HostUrl = configuration.HostUrl,
                Username = configuration.AccountId
            };

            AuthenticationEntity authenticationEntity = new AuthenticationEntity
            {
                Configuration = config,
                Token = new OAuthToken()
            };

            return authenticationEntity;
        }

        #endregion Private Methods
    }
}
