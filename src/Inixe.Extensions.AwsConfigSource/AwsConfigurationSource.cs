// -----------------------------------------------------------------------
// <copyright file="AwsConfigurationSource.cs" company="Inixe S.A.">
// Copyright All Rights reserved. Inixe S.A. 2021
// </copyright>
// -----------------------------------------------------------------------

namespace Inixe.Extensions.AwsConfigSource
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Configuration Source for AWS stored parameters.
    /// </summary>
    /// <seealso cref="Microsoft.Extensions.Configuration.IConfigurationSource" />
    internal class AwsConfigurationSource : IConfigurationSource
    {
        /// <summary>
        /// The default config section name.
        /// </summary>
        internal static readonly string DefaultSectionName = "AwsHostedSettings";

        private readonly AwsConfigurationSourceOptions options;
        private readonly string sectionName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AwsConfigurationSource"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <exception cref="System.ArgumentNullException">When options is null.</exception>
        public AwsConfigurationSource(AwsConfigurationSourceOptions options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.sectionName = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AwsConfigurationSource"/> class.
        /// </summary>
        /// <param name="sectionName">Name of the section.</param>
        /// <exception cref="System.ArgumentNullException">Invalid section name was supplied.</exception>
        public AwsConfigurationSource(string sectionName)
            : this(new AwsConfigurationSourceOptions())
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                throw new ArgumentException("Invalid section name was supplied", nameof(sectionName));
            }

            this.sectionName = sectionName;
        }

        /// <summary>
        /// Builds the <see cref="IConfigurationProvider" /> for this source.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder" />.</param>
        /// <returns>
        /// An <see cref="IConfigurationProvider" />.
        /// </returns>
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (!string.IsNullOrWhiteSpace(this.sectionName))
            {
                this.LoadOptionsFromConfiguration(builder);
            }

            var provider = new SecretsManagerConfigurationProvider(this.options);
            return provider;
        }

        private void LoadOptionsFromConfiguration(IConfigurationBuilder builder)
        {
            var valuePairs = this.GetConfigurationPairs(builder);

            var props = TypeDescriptor.GetProperties(typeof(AwsConfigurationSourceOptions))
                .Cast<PropertyDescriptor>()
                .ToList();

            foreach (var prop in props)
            {
                if (valuePairs.ContainsKey(prop.Name))
                {
                    var propertyValue = prop.Converter.ConvertFromString(valuePairs[prop.Name]);
                    prop.SetValue(this.options, propertyValue);
                }
            }
        }

        private Dictionary<string, string> GetConfigurationPairs(IConfigurationBuilder builder)
        {
            var temporaryConfiguration = builder.Build();
            var section = temporaryConfiguration.GetSection(this.sectionName);

            var valuePairs = section.AsEnumerable()
                .ToDictionary(x => x.Key, y => y.Value);

            return valuePairs;
        }
    }
}
