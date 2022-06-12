// -----------------------------------------------------------------------
// <copyright file="ConfigurationBuilderExtensions.cs" company="Inixe S.A.">
// Copyright All Rights reserved. Inixe S.A. 2021
// </copyright>
// -----------------------------------------------------------------------

namespace Inixe.Extensions.AwsConfigSource
{
    using System;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// <see cref="IConfigurationBuilder"/> extensions for adding AWS resources.
    /// </summary>
    public static class ConfigurationBuilderExtensions
    {
        /// <summary>
        /// The AWS configuration default section name.
        /// </summary>
        public static readonly string AwsConfigDefaultSectionName = AwsConfigurationSource.DefaultSectionName;

        /// <summary>
        /// Adds the AWS configuration source with default values.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>A new instance of <see cref="IConfigurationBuilder"/> that contains a new configuration AWS configuration source.</returns>
        /// <remarks>This method registers the default section name <see cref="AwsConfigDefaultSectionName"/> as the options source. If no section is found in the configuration chain then all default values are used.</remarks>
        public static IConfigurationBuilder AddAwsConfiguration(this IConfigurationBuilder builder)
        {
            return AddAwsConfiguration(builder, AwsConfigDefaultSectionName);
        }

        /// <summary>
        /// Adds the AWS configuration with the option to read the <see cref="AwsConfigurationSourceOptions"/> from a section of the configuration chain.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="sectionName">Name of the section.</param>
        /// <returns>A new instance of <see cref="IConfigurationBuilder"/> that contains a new configuration AWS configuration source.</returns>
        /// <exception cref="System.ArgumentNullException">When builder is null.</exception>
        public static IConfigurationBuilder AddAwsConfiguration(this IConfigurationBuilder builder, string sectionName)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var source = new AwsConfigurationSource(sectionName);
            return builder.Add(source);
        }

        /// <summary>
        /// Adds the AWS configuration source with the specified options.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="options">The options.</param>
        /// <returns>A new instance of <see cref="IConfigurationBuilder"/> that contains a new configuration AWS configuration source.</returns>
        /// <exception cref="System.ArgumentNullException">When builder is null.</exception>
        public static IConfigurationBuilder AddAwsConfiguration(this IConfigurationBuilder builder, AwsConfigurationSourceOptions options)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var source = new AwsConfigurationSource(options);
            return builder.Add(source);
        }
    }
}
