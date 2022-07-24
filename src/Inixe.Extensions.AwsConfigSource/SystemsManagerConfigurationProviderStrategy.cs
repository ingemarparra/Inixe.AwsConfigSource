// -----------------------------------------------------------------------
// <copyright file="SystemsManagerConfigurationProviderStrategy.cs" company="Inixe S.A.">
// Copyright All Rights reserved. Inixe S.A. 2021
// </copyright>
// -----------------------------------------------------------------------

namespace Inixe.Extensions.AwsConfigSource
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Amazon.SimpleSystemsManagement;
    using Amazon.SimpleSystemsManagement.Model;

    /// <summary>
    /// Strategy for providing values from systems manager.
    /// </summary>
    /// <seealso cref="Inixe.Extensions.AwsConfigSource.IConfigurationProviderStrategy" />
    internal class SystemsManagerConfigurationProviderStrategy : IConfigurationProviderStrategy
    {
        private readonly AwsConfigurationSourceOptions options;
        private readonly IValuePairStrategy valuePairStrategy;
        private readonly Func<AwsConfigurationSourceOptions, IAmazonSimpleSystemsManagement> systemsManagementFactory;
        private IAmazonSimpleSystemsManagement ssmClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemsManagerConfigurationProviderStrategy"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public SystemsManagerConfigurationProviderStrategy(AwsConfigurationSourceOptions options)
            : this(AwsClientHelpers.CreateSystemsManagementClient, options, new JsonValuePairStrategy())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemsManagerConfigurationProviderStrategy"/> class.
        /// </summary>
        /// <param name="systemsManagementFactory">The systems management factory.</param>
        /// <param name="options">The options.</param>
        /// <param name="valuePairStrategy">The value pair strategy.</param>
        /// <exception cref="ArgumentNullException">
        /// options
        /// or
        /// systemsManagermentFactory
        /// or
        /// valuePairStrategy.
        /// </exception>
        internal SystemsManagerConfigurationProviderStrategy(Func<AwsConfigurationSourceOptions, IAmazonSimpleSystemsManagement> systemsManagementFactory, AwsConfigurationSourceOptions options, IValuePairStrategy valuePairStrategy)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.systemsManagementFactory = systemsManagementFactory ?? throw new ArgumentNullException(nameof(systemsManagementFactory));
            this.valuePairStrategy = valuePairStrategy ?? throw new ArgumentNullException(nameof(valuePairStrategy));
        }

        /// <summary>
        /// Gets the options instance.
        /// </summary>
        /// <value>
        /// The options instance.
        /// </value>
        public AwsConfigurationSourceOptions Options
        {
            get
            {
                return this.options;
            }
        }

        /// <summary>
        /// Gets the implementation name.
        /// </summary>
        /// <value>
        /// The implementation name.
        /// </value>
        public string Name
        {
            get
            {
                return nameof(SystemsManagerConfigurationProviderStrategy);
            }
        }

        private IAmazonSimpleSystemsManagement Client
        {
            get
            {
                if (this.ssmClient == null)
                {
                    this.ssmClient = this.systemsManagementFactory(this.Options);
                }

                return this.ssmClient;
            }
        }

        /// <summary>
        /// Loads (or reloads) the data for this provider.
        /// </summary>
        /// <returns>
        /// A dictionary with all the values provided by the strategy.
        /// </returns>
        public IDictionary<string, string> LoadValues()
        {
            var data = Task.Run(async () => await this.LoadParametersAsync())
                .GetAwaiter()
                .GetResult();

            return data;
        }

        private async Task<IDictionary<string, string>> LoadParametersAsync()
        {
            var parametersInformation = await this.GetParametersListAsync();
            var parameterValues = await this.GetParameterValues(parametersInformation);

            return new Dictionary<string, string>(parameterValues);
        }

        private async Task<List<KeyValuePair<string, string>>> GetParameterValues(List<ParameterMetadata> parametersInformation)
        {
            var entries = new List<KeyValuePair<string, string>>();

            if (parametersInformation.Any())
            {
                var request = new GetParametersRequest();
                request.WithDecryption = true;
                request.Names = parametersInformation.Select(x => x.Name)
                    .ToList();

                try
                {
                    var response = await this.Client.GetParametersAsync(request);

                    foreach (var parameter in response.Parameters)
                    {
                        var keyName = this.FormatParameterName(parameter.Name);
                        var parameterEntries = this.valuePairStrategy.GetConfigurationPairs(keyName, parameter.Value);
                        entries.AddRange(parameterEntries);
                    }
                }
                catch (AmazonSimpleSystemsManagementException assmex)
                {
                    this.Options.BuildExceptionHandler?.Invoke(assmex);
                }
                catch (UnauthorizedAccessException uaex)
                {
                    this.Options.BuildExceptionHandler?.Invoke(uaex);
                }
            }

            return entries;
        }

        private async Task<List<ParameterMetadata>> GetParametersListAsync()
        {
            var parameters = new List<ParameterMetadata>();

            var token = string.Empty;
            try
            {
                do
                {
                    var request = this.CreateDescribeParametersRequest(token);
                    var response = await this.Client.DescribeParametersAsync(request);
                    parameters.AddRange(response.Parameters);

                    token = response.NextToken;
                }
                while (!string.IsNullOrEmpty(token));
            }
            catch (AmazonSimpleSystemsManagementException assmex)
            {
                this.Options.BuildExceptionHandler?.Invoke(assmex);
            }
            catch (UnauthorizedAccessException uaex)
            {
                this.Options.BuildExceptionHandler?.Invoke(uaex);
            }

            return parameters;
        }

        private DescribeParametersRequest CreateDescribeParametersRequest(string token)
        {
            var request = new DescribeParametersRequest();
            if (!string.IsNullOrEmpty(token))
            {
                request.NextToken = token;
            }

            if (this.Options.ParameterNameAsPath)
            {
                var filter = new ParameterStringFilter();
                filter.Key = ParametersFilterKey.Name;
                filter.Values = new List<string> { this.Options.BaseParameterNamePath };
                filter.Option = "BeginsWith";

                request.ParameterFilters = new List<ParameterStringFilter> { filter };
            }

            return request;
        }

        private string FormatParameterName(string parameterName)
        {
            var formattedName = parameterName;

            if (this.Options.ParameterNameAsPath)
            {
                var keyName = parameterName.Replace(this.Options.BaseParameterNamePath, string.Empty);

                formattedName = keyName.Replace('/', ':');
            }

            return formattedName;
        }
    }
}
