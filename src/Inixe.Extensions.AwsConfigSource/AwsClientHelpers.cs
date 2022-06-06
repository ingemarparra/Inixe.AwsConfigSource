// -----------------------------------------------------------------------
// <copyright file="AwsClientHelpers.cs" company="Inixe S.A.">
// Copyright All Rights reserved. Inixe S.A. 2021
// </copyright>
// -----------------------------------------------------------------------

namespace Inixe.Extensions.AwsConfigSource
{
    using System;
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
        /// <exception cref="System.ArgumentNullException">options.</exception>
        /// <exception cref="System.ArgumentException">Invalid Profile name supplied - options.</exception>
        internal static IAmazonSecretsManager CreateSecretsManagerClient(AwsConfigurationSourceOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var clientOptions = new AmazonSecretsManagerConfig();

            if (!string.IsNullOrEmpty(options.SecretsManagerServiceUrl))
            {
                clientOptions.ServiceURL = options.SecretsManagerServiceUrl;
            }

            if (!string.IsNullOrEmpty(options.ProfileName))
            {
                var credentialsFile = new Amazon.Runtime.CredentialManagement.SharedCredentialsFile();
                if (credentialsFile.TryGetProfile(options.ProfileName, out var credentialProfile))
                {
                    var credentials = credentialProfile.GetAWSCredentials(null);
                    return new AmazonSecretsManagerClient(credentials, credentialProfile.Region);
                }
                else
                {
                    throw new ArgumentException("Invalid Profile name supplied", nameof(options));
                }
            }

            return new AmazonSecretsManagerClient();
        }
    }
}
