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
    using Amazon.Runtime;
    using Amazon.SecretsManager;
    using Amazon.SimpleSystemsManagement;

    /// <summary>
    /// Helpers for creating Amazon Clients With the specified options.
    /// </summary>
    internal static class AwsClientHelpers
    {
        /// <summary>
        /// Normalizes a path string by appending a path separator to the string if the path separator is not present.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="normalize">if set to <c>true</c> string provided in path should be normalized by appending a path separator to the string if the path separator is not present.</param>
        /// <param name="separator">The path separator character.</param>
        /// <returns>The normalized path if <paramref name="normalize"/> is <c>true</c> otherwise <c>false</c> is returned.</returns>
        internal static string NormalizePath(string path, bool normalize, char separator)
        {
            return !path.EndsWith(separator) && normalize ? path + separator : path;
        }

        /// <summary>
        /// Creates the systems manager client.
        /// </summary>
        /// <param name="options">The Configuration Source options.</param>
        /// <returns>A Secrets Manager Client based on the supplied options.</returns>
        /// <exception cref="System.ArgumentNullException">When options is null.</exception>
        /// <exception cref="System.ArgumentException">Invalid Profile name supplied - options.</exception>
        internal static IAmazonSimpleSystemsManagement CreateSystemsManagementClient(AwsConfigurationSourceOptions options)
        {
            return CreateAwsClient<IAmazonSimpleSystemsManagement, AmazonSimpleSystemsManagementConfig>(options, options?.SystemsManagementServiceUrl, CreateSystemsManagementClient);
        }

        /// <summary>
        /// Creates the secrets manager client.
        /// </summary>
        /// <param name="options">The Configuration Source options.</param>
        /// <returns>A Secrets Manager Client based on the supplied options.</returns>
        /// <exception cref="System.ArgumentNullException">When options is null.</exception>
        /// <exception cref="System.ArgumentException">Invalid Profile name supplied - options.</exception>
        internal static IAmazonSecretsManager CreateSecretsManagerClient(AwsConfigurationSourceOptions options)
        {
            return CreateAwsClient<IAmazonSecretsManager, AmazonSecretsManagerConfig>(options, options?.SecretsManagerServiceUrl, CreateSecretsManagerClient);
        }

        private static TClient CreateAwsClient<TClient, TConfig>(AwsConfigurationSourceOptions options, string serviceUrlOverride, Func<AWSCredentials, TConfig, TClient> clientFactory)
            where TConfig : ClientConfig, new()
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (!string.IsNullOrEmpty(options.ProfileName))
            {
                return CreateClientFromProfile<TClient, TConfig>(options, serviceUrlOverride, clientFactory);
            }

            var clientOptions = ConfigureOptions<TConfig>(options, serviceUrlOverride);
            return clientFactory(null, clientOptions);
        }

        private static IAmazonSimpleSystemsManagement CreateSystemsManagementClient(AWSCredentials credentials, AmazonSimpleSystemsManagementConfig config)
        {
            return credentials == null ? new AmazonSimpleSystemsManagementClient(config) : new AmazonSimpleSystemsManagementClient(credentials, config);
        }

        private static IAmazonSecretsManager CreateSecretsManagerClient(AWSCredentials credentials, AmazonSecretsManagerConfig config)
        {
            return credentials == null ? new AmazonSecretsManagerClient(config) : new AmazonSecretsManagerClient(credentials, config);
        }

        private static T ConfigureOptions<T>(AwsConfigurationSourceOptions options, string serviceUrlOverride)
            where T : ClientConfig, new()
        {
            var clientOptions = new T();
            if (!string.IsNullOrWhiteSpace(options.AwsRegionName))
            {
                clientOptions.RegionEndpoint = FindRegionEndpoint(options.AwsRegionName);
            }

            if (!string.IsNullOrEmpty(serviceUrlOverride))
            {
                clientOptions.ServiceURL = serviceUrlOverride;
            }

            return clientOptions;
        }

        private static RegionEndpoint FindRegionEndpoint(string awsRegionName)
        {
            const bool IgnoreCase = true;

            Func<RegionEndpoint, bool> predicate = region => string.Compare(region.SystemName, awsRegionName, IgnoreCase) == 0;
            return Amazon.RegionEndpoint.EnumerableAllRegions.SingleOrDefault(predicate);
        }

        private static TClient CreateClientFromProfile<TClient, TConfig>(AwsConfigurationSourceOptions options, string serviceUrlOverride, Func<AWSCredentials, TConfig, TClient> clientFactory)
            where TConfig : ClientConfig, new()
        {
            var clientOptions = new TConfig();
            var credentialsFile = new Amazon.Runtime.CredentialManagement.SharedCredentialsFile(options.AwsCredentialsProfilePath);
            if (credentialsFile.TryGetProfile(options.ProfileName, out var credentialProfile))
            {
                var credentials = credentialProfile.GetAWSCredentials(null);
                clientOptions.RegionEndpoint = credentialProfile.Region;

                // Setting the Region endpoint will reset the Service URL to null.
                if (!string.IsNullOrEmpty(serviceUrlOverride))
                {
                    clientOptions.ServiceURL = serviceUrlOverride;
                }

                return clientFactory(credentials, clientOptions);
            }
            else
            {
                throw new ArgumentException("Invalid Profile name supplied", nameof(options));
            }
        }
    }
}
