namespace Sdl.Connectors.Tridion.SalesforceDeployerConnector.Data
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Contract.Authorization;
    using Common.Contract.Configuration;
    using Common.Contract.Exception;
    using Configuration;
    using ContentService.Contract;
    using Entities;
    using global::Tridion.ConnectorFramework.Connector.SDK;
    using global::Tridion.ConnectorFramework.Contracts;
    using global::Tridion.ConnectorFramework.Contracts.Content;
    using global::Tridion.ConnectorFramework.Contracts.Logging;
    using Newtonsoft.Json;
    using SalesforceContentService;
    using SalesforceContentService.Contract;
    using SalesforceContentService.Models;
    using Utils;

    public class SalesforceContext : ISalesforceContext
    {
        #region Private Fields

        private const string DefaultLanguageId = "source";

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

        #region Upload

        public async Task<IEntity> UploadAsset(IMultimedia multimedia, string entityType, IBinary binary, CancellationToken cancellationToken = default)
        {
            try
            {
                AssetCollection collection = await this.SearchDuplicates(entityType, multimedia, cancellationToken).ConfigureAwait(false);
                if (collection.Count == 0)
                {
                    switch (entityType)
                    {
                        case nameof(DownloadContextType.CodeSnippet):
                            {
                                SalesforceMarketingCloudAsset asset = this.CreateSnippetAsset(this.config.MarkupUploadCategoryId, multimedia, binary);
                                this.logger.LogDebug($"Asset Title: {asset.Name}");
                                SalesforceMarketingCloudAsset savedAsset = await this.contentService
                                    .UploadSalesforceContent(this.AuthenticationEntity.Token,
                                        JsonConvert.SerializeObject(asset), cancellationToken).ConfigureAwait(false);

                                return savedAsset.ToSalesforceAsset(entityType, multimedia.ParentIdentity);
                            }
                        case nameof(DownloadContextType.Image):
                            {
                                SalesforceMarketingCloudAsset asset = this.CreateImageAsset(this.config.ImagesUploadCategoryId, multimedia, binary);
                                SalesforceMarketingCloudAsset savedAsset = await this.contentService.UploadSalesforceContent(this.AuthenticationEntity.Token,
                                    JsonConvert.SerializeObject(asset), cancellationToken).ConfigureAwait(false);
                                return savedAsset.ToSalesforceAsset(entityType, multimedia.ParentIdentity);
                            }
                        default:
                            throw new ConnectorException($"UploadAsset error: {multimedia.Identity}");
                    }
                }
                else
                {
                    switch (entityType)
                    {
                        case nameof(DownloadContextType.CodeSnippet):
                            {
                                ContentIdentity identity = new ContentIdentity { Id = collection.Items.FirstOrDefault()?.Id };
                                Asset updateAsset = this.CreateAssetForUpdate(this.config.MarkupUploadCategoryId, entityType, multimedia, binary);
                                SalesforceMarketingCloudAsset salesforceMarketing =
                                    await this.contentService.Update(this.AuthenticationEntity.Token, JsonConvert.SerializeObject(updateAsset), identity, cancellationToken).ConfigureAwait(false);
                                return salesforceMarketing.ToSalesforceAsset(entityType, multimedia.ParentIdentity);
                            }
                        case nameof(DownloadContextType.Image):
                            {
                                ContentIdentity identity = new ContentIdentity { Id = collection.Items.FirstOrDefault()?.Id };
                                Asset updateAsset = this.CreateAssetForUpdate(this.config.ImagesUploadCategoryId, entityType, multimedia, binary);
                                SalesforceMarketingCloudAsset salesforceMarketing =
                                    await this.contentService.Update(this.AuthenticationEntity.Token, JsonConvert.SerializeObject(updateAsset), identity, cancellationToken).ConfigureAwait(false);
                                return salesforceMarketing.ToSalesforceAsset(entityType, multimedia.ParentIdentity);
                            }
                        default:
                            throw new ConnectorException($"Update error: {multimedia.Identity}");
                    }
                }
            }
            catch (ConnectorException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ConnectorException($"UploadAsset failed.", ex);
            }
        }

        #endregion Upload

        #region Get

        public async Task<IEntity> GetEntity(IEntityIdentity parentIdentity, CancellationToken cancellationToken = default)
        {
            try
            {
                this.logger.LogInfo($"Get Entity. Parent id: {parentIdentity.Id}");

                ContentIdentity identity = new ContentIdentity()
                {
                    Id = parentIdentity.Id
                };

                SalesforceMarketingCloudAsset asset = await this.contentService
                    .GetAsset(this.AuthenticationEntity.Token, identity, cancellationToken).ConfigureAwait(false);

                return asset.ToAsset(parentIdentity, parentIdentity.Type);
            }
            catch (ConnectorException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ConnectorException($"GetEntity failed.", ex);
            }
        }

        #endregion Get

        #region Delete

        public async Task DeleteEntity(IEntityIdentity identity, CancellationToken cancellationToken = default)
        {
            try
            {
                ContentIdentity contentIdentity = new ContentIdentity
                {
                    Id = identity.Id
                };

                await this.contentService.DeleteAsset(this.AuthenticationEntity.Token, contentIdentity, cancellationToken).ConfigureAwait(false);
            }
            catch (ConnectorException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ConnectorException($"DeleteEntity failed.", ex);
            }
        }

        #endregion Delete

        #region Search

        public async Task<List<IEntity>> SearchByDataProperty(IEntityFilter filter, CancellationToken cancellationToken = default)
        {
            try
            {
                SimpleQueryItem queryString = new SimpleQueryItem()
                {
                    Operand = new Operand
                    {
                        Property = Helper.DataQuery,
                        SimpleOperator = Helper.MustContainOperator,
                        Value = filter.SearchText
                    }
                };

                this.logger.LogDebug($"Search text: {JsonConvert.SerializeObject(queryString)}");
                AssetCollection asset = await this.contentService.Search(this.AuthenticationEntity.Token, JsonConvert.SerializeObject(queryString), cancellationToken)
                    .ConfigureAwait(false);

                List<IEntity> entities = new List<IEntity>();

                if (asset.Count != 0)
                {
                    foreach (SalesforceMarketingCloudAsset salesforceMarketingCloudAsset in asset.Items)
                    {
                        entities.Add(salesforceMarketingCloudAsset.ToAsset(filter.Context, filter.EntityType));
                    }
                }

                return entities;
            }
            catch (ConnectorException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ConnectorException($"Search failed.", ex);
            }
        }

        #endregion Search

        #region Private Methods

        private async Task<AssetCollection> SearchDuplicates(string entityType, IMultimedia multimedia, CancellationToken cancellationToken = default)
        {
            try
            {
                string multimediaName = string.Empty;

                switch (entityType)
                {
                    case nameof(DownloadContextType.CodeSnippet):
                        {
                            multimediaName = this.GetMultimediaName(multimedia);
                            break;
                        }
                    case nameof(DownloadContextType.Image):
                        {
                            multimediaName = multimedia.Title;
                            break;
                        }
                }

                AssetEntity entity = multimedia.Cast<AssetEntity>();
                ComplexQueryItem queryItem = new ComplexQueryItem
                {
                    Item = new ComplexQueryDefinition
                    {
                        LeftOperand = new Operand { Property = Helper.NameQuery, SimpleOperator = Helper.EqualOperator, Value = multimediaName },
                        LogicalOperator = Helper.AndLogicalOperator,
                        RightOperand = new Operand { Property = Helper.DataQuery, SimpleOperator = Helper.MustContainOperator, Value = entity.tridionId }
                    }
                };

                this.logger.LogDebug($"Search duplicates test: {JsonConvert.SerializeObject(queryItem)}");
                await this.EnsureAuthentication(cancellationToken).ConfigureAwait(false);

                AssetCollection collection = await this.contentService.Search(this.AuthenticationEntity.Token, JsonConvert.SerializeObject(queryItem), cancellationToken).ConfigureAwait(false);

                return collection;
            }
            catch (Exception ex)
            {
                throw new ConnectorException($"SearchDuplicates failed.", ex);
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

        private Asset CreateAssetForUpdate(string categoryId, string entityType, IMultimedia multimedia, IBinary binary)
        {
            Asset asset = new Asset();
            AssetEntity entity = multimedia.Cast<AssetEntity>();

            switch (entityType)
            {
                case nameof(DownloadContextType.CodeSnippet):
                    {
                        asset.Category = new Category
                        {
                            Id = categoryId
                        };

                        asset.Data = new AssetDataItem
                        {
                            TridionEclUri = entity.tridionId
                        };

                        asset.Name = this.GetMultimediaName(multimedia);
                        StreamReader reader = new StreamReader(binary.Stream);
                        asset.Content = reader.ReadToEnd();
                        break;
                    }
                case nameof(DownloadContextType.Image):
                    {
                        asset.Category = new Category
                        {
                            Id = categoryId
                        };

                        asset.Data = new AssetDataItem
                        {
                            TridionEclUri = entity.tridionId
                        };

                        asset.Name = multimedia.Title;
                        MemoryStream stream = new MemoryStream();
                        binary.Stream.CopyTo(stream);
                        byte[] imageBytes = stream.ToArray();
                        asset.File = Convert.ToBase64String(imageBytes);
                        break;
                    }
            }

            return asset;
        }

        private SalesforceMarketingCloudAsset CreateSnippetAsset(string categoryId, IMultimedia multimedia, IBinary binary)
        {
            SalesforceMarketingCloudAsset asset = new SalesforceMarketingCloudAsset();
            AssetEntity entity = multimedia.Cast<AssetEntity>();

            asset.AssetType = new AssetType()
            {
                Id = ((int)AssetTypeEnum.CODE_SNIPPET).ToString()
            };

            asset.Category = new Category
            {
                Id = categoryId
            };

            asset.Data = new AssetDataItem
            {
                TridionEclUri = entity.tridionId
            };

            asset.Name = this.GetMultimediaName(multimedia);
            StreamReader reader = new StreamReader(binary.Stream);
            asset.Content = reader.ReadToEnd();

            return asset;
        }

        private SalesforceMarketingCloudAsset CreateImageAsset(string categoryId, IMultimedia multimedia, IBinary binary)
        {
            SalesforceMarketingCloudAsset asset = new SalesforceMarketingCloudAsset();
            AssetEntity entity = multimedia.Cast<AssetEntity>();

            asset.ContentType = multimedia.ContentType;
            asset.AssetType = new AssetType
            {
                Id = SetImageType(multimedia.ContentType)
            };

            asset.Data = new AssetDataItem
            {
                TridionEclUri = entity.tridionId
            };

            asset.Category = new Category
            {
                Id = categoryId
            };

            asset.Name = multimedia.Title;

            MemoryStream stream = new MemoryStream();
            binary.Stream.CopyTo(stream);
            byte[] imageBytes = stream.ToArray();
            asset.File = Convert.ToBase64String(imageBytes);

            return asset;
        }

        private static string SetImageType(string type)
        {
            switch (type)
            {
                case "image/png":
                    {
                        return ((int)AssetTypeEnum.PNG).ToString();
                    }
                case "image/gif":
                    {
                        return ((int)AssetTypeEnum.GIF).ToString();
                    }
                case "image/jpeg":
                    {
                        return ((int)AssetTypeEnum.JPEG).ToString();
                    }
                case "image/jpg":
                    {
                        return ((int)AssetTypeEnum.JPG).ToString();
                    }
            }

            return String.Empty;
        }

        private string GetMultimediaName(IMultimedia entity)
        {
            string multimediaName;

            if (this.TryGetLanguageId(entity.ParentIdentity.LocaleId, out string languageCode) && languageCode != DefaultLanguageId)
            {
                multimediaName = $"{entity.Title}[{languageCode}]";
            }
            else
            {
                multimediaName = entity.Title;
            }

            return multimediaName;
        }

        private bool TryGetLanguageId(string publicationId, out string languageId)
        {
            languageId = null;

            LocaleMapping[] localeMappings = this.config.LocaleMapping;

            if (localeMappings is null || localeMappings.Length == 0)
            {
                return false;
            }

            foreach (LocaleMapping mappingPair in localeMappings)
            {
                if (!mappingPair.PublicationIds.Contains(publicationId))
                {
                    continue;
                }

                languageId = mappingPair.Locale;

                return true;
            }

            this.logger.LogWarning($"No appropiate locale code found for Tridion PublicationId: {publicationId}.");

            return false;
        }

        #endregion Private Methods
    }
}
