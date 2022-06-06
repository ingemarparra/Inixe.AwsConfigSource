// -----------------------------------------------------------------------
// <copyright file="ConfigurationBuilderExtensions.cs" company="Inixe S.A.">
// Copyright All Rights reserved. Inixe S.A. 2021
// </copyright>
// -----------------------------------------------------------------------

namespace Inixe.Extensions.AwsConfigSource
{
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// <see cref="IConfigurationBuilder"/> extensions for adding AWS resources.
    /// </summary>
    public static class ConfigurationBuilderExtensions
    {
        /// <summary>
        /// Adds the AWS configuration source.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>A new instance of <see cref="IConfigurationBuilder"/> that contains a new configuration AWS configuration source.</returns>
        public static IConfigurationBuilder AddAwsConfiguration(this IConfigurationBuilder builder)
        {
            var options = new AwsConfigurationSourceOptions();
            return AddAwsConfiguration(builder, options);
        }

        /// <summary>
        /// Adds the AWS configuration source with the specified options.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="options">The options.</param>
        /// <returns>A new instance of <see cref="IConfigurationBuilder"/> that contains a new configuration AWS configuration source.</returns>
        public static IConfigurationBuilder AddAwsConfiguration(this IConfigurationBuilder builder, AwsConfigurationSourceOptions options)
        {
            var source = new AwsConfigurationSource(options);
            return builder.Add(source);
        }
    }
}
