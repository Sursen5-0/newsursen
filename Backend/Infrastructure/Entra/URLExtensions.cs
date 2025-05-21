using System;
using System.Collections.Generic;

namespace Infrastructure.Entra
{
    public static class URLExtensions
    {
        // ─── Vault secret names ───────────────────────────────────────────────────
        public const string TenantIdSecretName = "ENTRA_TENANT";
        public const string ClientIdSecretName = "ENTRA_ID";
        public const string ClientSecretName = "ENTRA_SECRET";

        // ─── Token endpoint & scope ───────────────────────────────────────────────
        public const string TokenEndpointTemplate =
            "https://login.microsoftonline.com/{0}/oauth2/v2.0/token";
        public const string Scope = "https://graph.microsoft.com/.default";

        // ─── Graph /users endpoint components ────────────────────────────────────
        private const string GraphBaseUrl = "https://graph.microsoft.com/v1.0";
        private const string UsersPath = "/users";

        /// <summary>Which fields to select in the $select query.</summary>
        public static readonly string[] UserSelectFields =
        {
            "id","accountEnabled","ageGroup","businessPhones","city","companyName","country",
            "createdDateTime","creationType","department","displayName","employeeHireDate",
            "employeeId","employeeLeaveDateTime","employeeType","givenName","jobTitle",
            "mail","mobilePhone","postalCode","surname","userPrincipalName","userType"
        };

        /// <summary>OData filter clauses that will be joined with " and ".</summary>
        public static readonly string[] UserFilters =
        {
            "accountEnabled eq true",
            "endswith(mail, '@twoday.com')",
            "jobTitle ne null"
        };

        /// <summary>Whether to append $count=true.</summary>
        public const bool IncludeCount = true;

        // ─── Helpers ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Build the full token‐endpoint URL by injecting the tenant ID.
        /// </summary>
        public static string GetTokenEndpoint(string tenantId) =>
            string.Format(TokenEndpointTemplate, tenantId);

        /// <summary>
        /// Build the form‐url‐encoded payload dictionary for client_credentials.
        /// </summary>
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

        /// <summary>
        /// Assemble the /users URL from BaseUrl, path, and query parts.
        /// </summary>
        public static string BuildUsersEndpoint()
        {
            var q = new List<string>();

            // $select=
            if (UserSelectFields.Length > 0)
                q.Add("$select=" + string.Join(",", UserSelectFields));

            // $filter=
            if (UserFilters.Length > 0)
                q.Add("$filter=" + string.Join(" and ", UserFilters));

            // $count=
            if (IncludeCount)
                q.Add("$count=true");

            return GraphBaseUrl
                 + UsersPath
                 + "?"
                 + string.Join("&", q);
        }
    }
}
