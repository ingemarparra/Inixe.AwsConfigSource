// -----------------------------------------------------------------------
// <copyright file="SecretsManagerConfigurationProvider.cs" company="Inixe S.A.">
// Copyright All Rights reserved. Inixe S.A. 2021
// </copyright>
// -----------------------------------------------------------------------

namespace Inixe.Extensions.AwsConfigSource
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Amazon.SecretsManager;
    using Amazon.SecretsManager.Model;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Configuration Provider for Secrets Manager.
    /// </summary>
    /// <seealso cref="Microsoft.Extensions.Configuration.ConfigurationProvider" />
    internal class SecretsManagerConfigurationProvider : ConfigurationProvider
    {
        private readonly Func<AwsConfigurationSourceOptions, IAmazonSecretsManager> secretsManagerFactory;
        private readonly AwsConfigurationSourceOptions options;
        private readonly IValuePairStrategy valuePairStrategy;
        private IAmazonSecretsManager secretsManagerClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecretsManagerConfigurationProvider"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public SecretsManagerConfigurationProvider(AwsConfigurationSourceOptions options)
            : this(AwsClientHelpers.CreateSecretsManagerClient, options, new JsonValuePairStrategy())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecretsManagerConfigurationProvider"/> class.
        /// </summary>
        /// <param name="secretsManagerFactory">The secrets manager factory.</param>
        /// <param name="options">The options.</param>
        /// <param name="valuePairStrategy">The value pair transformer strategy that will be used for creating value configuration value pairs.</param>
        /// <exception cref="ArgumentNullException">
        /// options
        /// or
        /// secretsManagerFactory.
        /// </exception>
        internal SecretsManagerConfigurationProvider(Func<AwsConfigurationSourceOptions, IAmazonSecretsManager> secretsManagerFactory, AwsConfigurationSourceOptions options, IValuePairStrategy valuePairStrategy)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.secretsManagerFactory = secretsManagerFactory ?? throw new ArgumentNullException(nameof(secretsManagerFactory));
            this.valuePairStrategy = valuePairStrategy ?? throw new ArgumentNullException(nameof(valuePairStrategy));
        }

        private IAmazonSecretsManager Client
        {
            get
            {
                if (this.secretsManagerClient == null)
                {
                    this.secretsManagerClient = this.secretsManagerFactory(this.options);
                }

                return this.secretsManagerClient;
            }
        }

        /// <summary>
        /// Loads (or reloads) the data for this provider.
        /// </summary>
        public override void Load()
        {
            this.Data = Task.Run(async () => await this.LoadSecretsAsync())
                .GetAwaiter()
                .GetResult();
        }

        private async Task<IDictionary<string, string>> LoadSecretsAsync()
        {
            var secretsEntries = await this.GetSecretNamesAsync();
            var entries = await this.ReadSecretsAsync(secretsEntries);

            return entries;
        }

        private async Task<List<SecretListEntry>> GetSecretNamesAsync()
        {
            var secretsEntries = new List<SecretListEntry>();
            var token = string.Empty;
            try
            {
                do
                {
                    var request = this.CreateListSecretRequest(token);
                    var response = await this.Client.ListSecretsAsync(request);
                    secretsEntries.AddRange(response.SecretList);
                    token = response.NextToken;
                }
                while (!string.IsNullOrEmpty(token));
            }
            catch (AmazonSecretsManagerException asme)
            {
                this.options.BuildExceptionHandler?.Invoke(asme);
            }
            catch (UnauthorizedAccessException uaex)
            {
                this.options.BuildExceptionHandler?.Invoke(uaex);
            }

            return secretsEntries;
        }

        private async Task<Dictionary<string, string>> ReadSecretsAsync(List<SecretListEntry> secretList)
        {
            var entries = new List<KeyValuePair<string, string>>();

            foreach (var item in secretList)
            {
                var key = this.FormatSecretKey(item);

                try
                {
                    var request = new GetSecretValueRequest();
                    request.SecretId = item.ARN;
                    var response = await this.Client.GetSecretValueAsync(request);

                    var secretEntries = this.valuePairStrategy.GetConfigurationPairs(key, response.SecretString);
                    entries.AddRange(secretEntries);
                }
                catch (AmazonSecretsManagerException asme)
                {
                    this.options.BuildExceptionHandler?.Invoke(asme);
                    continue;
                }
                catch (UnauthorizedAccessException uaex)
                {
                    this.options.BuildExceptionHandler?.Invoke(uaex);
                    continue;
                }
            }

            return new Dictionary<string, string>(entries, StringComparer.InvariantCultureIgnoreCase);
        }

        private string FormatSecretKey(SecretListEntry item)
        {
            if (this.options.SecretNameAsPath)
            {
                var keyName = item.Name.Replace(this.options.BaseSecretNamePath, string.Empty);

                var configKeyName = keyName.Replace('/', ':');
                return configKeyName;
            }
            else
            {
                return item.Name;
            }
        }

        private ListSecretsRequest CreateListSecretRequest(string token)
        {
            var request = new ListSecretsRequest();
            if (!string.IsNullOrEmpty(token))
            {
                request.NextToken = token;
            }

            request.Filters = new List<Filter>();

            if (this.options.SecretNameAsPath)
            {
                var filter = new Filter();
                filter.Key = FilterNameStringType.Name;
                filter.Values = new List<string>();
                filter.Values.Add(this.options.BaseSecretNamePath);

                request.Filters.Add(filter);
            }

            return request;
        }
    }
}
