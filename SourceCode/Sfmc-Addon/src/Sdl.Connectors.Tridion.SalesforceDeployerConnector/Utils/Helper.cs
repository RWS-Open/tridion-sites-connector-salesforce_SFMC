namespace Sdl.Connectors.Tridion.SalesforceDeployerConnector.Utils
{
    using System;
    using System.Text;
    using System.Web;
    using Entities;
    using Newtonsoft.Json;

    public static class Helper
    {
        public const string DataQuery = "data.tridionEclUri";
        public const string NameQuery = "name";
        public const string MustContainOperator = "mustcontain";
        public const string EqualOperator = "equal";
        public const string AndLogicalOperator = "AND";
        public const int Name_Unique_Error = 118039;


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
    }
}
