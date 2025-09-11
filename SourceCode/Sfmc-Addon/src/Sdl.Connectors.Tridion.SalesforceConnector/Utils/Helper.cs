namespace Sdl.Connectors.Tridion.SalesforceConnector.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Web;
    using Common.Contract.ConnectedInfo;
    using ContentService.Contract;
    using Entities;
    using Newtonsoft.Json;

    public static class Helper
    {
        public const string JourneysId = "EventDefinitions";
        public const string EventDefinitionForlderName = "Event Definitions";

        public static string EncodeBinaryReference(BinaryReferenceInfo binaryReferenceInfo)
        {
            if (binaryReferenceInfo is null)
            {
                throw new ArgumentNullException(nameof(binaryReferenceInfo));
            }

            string jsonEncodedRepresentation = JsonConvert.SerializeObject(binaryReferenceInfo);

            byte[] charArrayRepresentation = Encoding.UTF8.GetBytes(jsonEncodedRepresentation);

            string base64Representation = Convert.ToBase64String(charArrayRepresentation);

            string urlSafeRepresentation = HttpUtility.UrlEncode(base64Representation);

            return urlSafeRepresentation;
        }

        public static BinaryReferenceInfo DecodeBinaryReference(string encodedBinaryRefrenceInfo)
        {
            if (encodedBinaryRefrenceInfo is null)
            {
                throw new ArgumentNullException(nameof(encodedBinaryRefrenceInfo));
            }

            string base64Representation = HttpUtility.UrlDecode(encodedBinaryRefrenceInfo);
            byte[] charArrayRepresentation = Convert.FromBase64String(base64Representation);
            string jsonEncodedRepresentation = Encoding.UTF8.GetString(charArrayRepresentation);
            var binaryReference = JsonConvert.DeserializeObject<BinaryReferenceInfo>(jsonEncodedRepresentation);

            return binaryReference;
        }

        public static List<ConnectedFileSystemEntry> EncodeFolders(List<ConnectedFileSystemEntry> dataFolders, string contentType)
        {
            List<ConnectedFileSystemEntry> encodedEntries = new List<ConnectedFileSystemEntry>();

            foreach (ConnectedFileSystemEntry dataFolder in dataFolders)
            {
                dataFolder.Id = CreateComposedId(contentType, dataFolder.Id);
                encodedEntries.Add(dataFolder);
            }

            return encodedEntries;
        }

        public static string ComposeDataExtensionId(string customerKey, string dataExtensionName)
        {
            StringBuilder privateComposedId = new StringBuilder();

            privateComposedId.Append(customerKey);
            privateComposedId.Append('.');
            privateComposedId.Append(dataExtensionName);

            return privateComposedId.ToString();
        }

        public static string[] DecodeDataExtensionId(string itemId)
        {
            return itemId.Split('.');
        }

        public static string CreateComposedId(string type, string entryId)
        {
            StringBuilder privateComposedId = new StringBuilder();

            privateComposedId.Append(entryId);
            privateComposedId.Append('.');
            privateComposedId.Append(type);

            return privateComposedId.ToString();
        }

        public static ContentIdentity DecodeIdentityId(string entityId)
        {
            if (!string.IsNullOrWhiteSpace(entityId))
            {
                ContentIdentity identity = new ContentIdentity();
                string[] values = entityId.Split('.');
                identity.Id = values[0];
                if (values.Length > 1)
                {
                    identity.Context = values[1];
                }

                return identity;
            }

            return null;
        }
    }
}
