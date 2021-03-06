// -----------------------------------------------------------------------
// <copyright file="AwsClientHelpers.cs" company="Inixe S.A.">
// Copyright All Rights reserved. Inixe S.A. 2021
// </copyright>
// -----------------------------------------------------------------------

namespace Inixe.Extensions.AwsConfigSource
{
    using System;
    using System.Linq;
    using Amazon;
    using Amazon.SecretsManager;

    /// <summary>
    /// Helpers for creating Amazon Clients With the specified options.
    /// </summary>
    internal static class AwsClientHelpers
    {
        /// <summary>
        /// Creates the secrets manager client.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>A Secrets Manager Client based on the supplied options.</returns>
        /// <exception cref="System.ArgumentNullException">When options is null.</exception>
        /// <exception cref="System.ArgumentException">Invalid Profile name supplied - options.</exception>
        internal static IAmazonSecretsManager CreateSecretsManagerClient(AwsConfigurationSourceOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (!string.IsNullOrEmpty(options.ProfileName))
            {
                return CreateClientWithProfileCredentials(options);
            }

            var clientOptions = new AmazonSecretsManagerConfig();
            if (!string.IsNullOrWhiteSpace(options.AwsRegionName))
            {
                clientOptions.RegionEndpoint = FindRegionEndpoint(options.AwsRegionName);
            }

            if (!string.IsNullOrEmpty(options.SecretsManagerServiceUrl))
            {
                clientOptions.ServiceURL = options.SecretsManagerServiceUrl;
            }

            return new AmazonSecretsManagerClient(clientOptions);
        }

        private static RegionEndpoint FindRegionEndpoint(string awsRegionName)
        {
            const bool IgnoreCase = true;

            Func<RegionEndpoint, bool> predicate = region => string.Compare(region.SystemName, awsRegionName, IgnoreCase) == 0;
            return Amazon.RegionEndpoint.EnumerableAllRegions.SingleOrDefault(predicate);
        }

        private static IAmazonSecretsManager CreateClientWithProfileCredentials(AwsConfigurationSourceOptions options)
        {
            var clientOptions = new AmazonSecretsManagerConfig();
            var credentialsFile = new Amazon.Runtime.CredentialManagement.SharedCredentialsFile(options.AwsCredentialsProfilePath);
            if (credentialsFile.TryGetProfile(options.ProfileName, out var credentialProfile))
            {
                var credentials = credentialProfile.GetAWSCredentials(null);
                clientOptions.RegionEndpoint = credentialProfile.Region;

                // Setting the Region endpoint will reset the Service URL to null.
                if (!string.IsNullOrEmpty(options.SecretsManagerServiceUrl))
                {
                    clientOptions.ServiceURL = options.SecretsManagerServiceUrl;
                }

                return new AmazonSecretsManagerClient(credentials, clientOptions);
            }
            else
            {
                throw new ArgumentException("Invalid Profile name supplied", nameof(options));
            }
        }
    }
}
