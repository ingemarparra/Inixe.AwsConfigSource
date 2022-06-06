// -----------------------------------------------------------------------
// <copyright file="AwsConfigurationSource.cs" company="Inixe S.A.">
// Copyright All Rights reserved. Inixe S.A. 2021
// </copyright>
// -----------------------------------------------------------------------

namespace Inixe.Extensions.AwsConfigSource
{
    using System;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Configuration Source for AWS stored parameters.
    /// </summary>
    /// <seealso cref="Microsoft.Extensions.Configuration.IConfigurationSource" />
    internal class AwsConfigurationSource : IConfigurationSource
    {
        private readonly AwsConfigurationSourceOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="AwsConfigurationSource"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <exception cref="System.ArgumentNullException">options.</exception>
        public AwsConfigurationSource(AwsConfigurationSourceOptions options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
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

            var provider = new SecretsManagerConfigurationProvider(this.options);
            return provider;
        }
    }
}
