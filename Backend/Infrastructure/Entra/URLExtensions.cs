using System;
using System.Collections.Generic;

namespace Infrastructure.Entra
{
    public static class URLExtensions
    {
        public const string TenantIdSecretName = "ENTRA_TENANT";
        public const string ClientIdSecretName = "ENTRA_ID";
        public const string ClientSecretName = "ENTRA_SECRET";

        public const string TokenEndpointTemplate =
            "https://login.microsoftonline.com/{0}/oauth2/v2.0/token";
        public const string Scope =
            "https://graph.microsoft.com/.default";

        private const string GraphBaseUrl = "https://graph.microsoft.com/v1.0";
        private const string UsersPath = "/users";

        public static readonly string[] UserSelectFields =
        {
            "id", "accountEnabled", "ageGroup", "businessPhones", "city", "companyName", "country",
            "createdDateTime", "creationType", "department", "displayName", "employeeHireDate",
            "employeeId", "employeeLeaveDateTime", "employeeType", "givenName", "jobTitle",
            "mail", "mobilePhone", "postalCode", "surname", "userPrincipalName", "userType", "externalUserState"
        };

        public static readonly string[] UserFilters =
        {
            "userType eq 'Member'",
            "accountEnabled eq true",
            "country eq 'Denmark'"
        };

        public const bool IncludeCount = true;
        private const string ExpandClause = "$expand=manager($select=id,displayName)";


        public static string GetTokenEndpoint(string tenantId) =>
            string.Format(TokenEndpointTemplate, tenantId);

        public static Dictionary<string, string> CreateTokenRequestPayload(
            string clientId,
            string clientSecret)
        {
            return new Dictionary<string, string>
            {
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["scope"] = Scope,
                ["grant_type"] = "client_credentials"
            };
        }

        public static string BuildUsersEndpoint()
        {
            var query = new List<string>();

            query.Add(ExpandClause);

            if (UserSelectFields.Length > 0)
                query.Add("$select=" + string.Join(",", UserSelectFields));

            if (UserFilters.Length > 0)
                query.Add("$filter=" + string.Join(" and ", UserFilters));

            if (IncludeCount)
                query.Add("$count=true");

            return $"{GraphBaseUrl}{UsersPath}?{string.Join("&", query)}";
        }
    }
}
