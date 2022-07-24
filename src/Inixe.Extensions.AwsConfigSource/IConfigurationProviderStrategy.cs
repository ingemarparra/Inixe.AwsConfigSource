// -----------------------------------------------------------------------
// <copyright file="IConfigurationProviderStrategy.cs" company="Inixe S.A.">
// Copyright All Rights reserved. Inixe S.A. 2021
// </copyright>
// -----------------------------------------------------------------------

namespace Inixe.Extensions.AwsConfigSource
{
    using System.Collections.Generic;

    /// <summary>
    /// Strategy for acquiring configuration data from different AWS services.
    /// </summary>
    internal interface IConfigurationProviderStrategy
    {
        /// <summary>
        /// Gets the options instance.
        /// </summary>
        /// <value>
        /// The options instance.
        /// </value>
        AwsConfigurationSourceOptions Options { get; }

        /// <summary>
        /// Gets the implementation name.
        /// </summary>
        /// <value>
        /// The implementation name.
        /// </value>
        string Name { get; }

        /// <summary>
        /// Loads (or reloads) the data for this provider.
        /// </summary>
        /// <returns>A dictionary with all the values provided by the strategy.</returns>
        IDictionary<string, string> LoadValues();
    }
}
