// -----------------------------------------------------------------------
// <copyright file="AwsConfigurationProvider.cs" company="Inixe S.A.">
// Copyright All Rights reserved. Inixe S.A. 2021
// </copyright>
// -----------------------------------------------------------------------

namespace Inixe.Extensions.AwsConfigSource
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Configuration Provider based on AWS services.
    /// </summary>
    /// <seealso cref="Microsoft.Extensions.Configuration.ConfigurationProvider" />
    internal class AwsConfigurationProvider : ConfigurationProvider
    {
        private readonly List<IConfigurationProviderStrategy> strategies;

        /// <summary>
        /// Initializes a new instance of the <see cref="AwsConfigurationProvider"/> class.
        /// </summary>
        /// <param name="strategies">The strategies.</param>
        /// <exception cref="ArgumentNullException">strategies.</exception>
        public AwsConfigurationProvider(List<IConfigurationProviderStrategy> strategies)
        {
            this.strategies = strategies ?? throw new ArgumentNullException(nameof(strategies));
        }

        /// <summary>
        /// Gets a list of the strategies used to access AWS services to fetch configuration data.
        /// </summary>
        /// <value>
        /// The strategies to access configration data.
        /// </value>
        public IReadOnlyList<IConfigurationProviderStrategy> Strategies
        {
            get
            {
                return this.strategies;
            }
        }

        /// <summary>
        /// Loads (or reloads) the data for this provider.
        /// </summary>
        public override void Load()
        {
            var data = new Dictionary<string, string>();
            foreach (var strategy in this.Strategies)
            {
                var values = strategy.LoadValues();
                foreach (var item in values)
                {
                    if (!data.TryAdd(item.Key, item.Value))
                    {
                        data[item.Key] = item.Value;
                    }
                }
            }

            this.Data = data;
        }
    }
}
