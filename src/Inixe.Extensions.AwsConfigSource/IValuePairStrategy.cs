// -----------------------------------------------------------------------
// <copyright file="IValuePairStrategy.cs" company="Inixe S.A.">
// Copyright All Rights reserved. Inixe S.A. 2021
// </copyright>
// -----------------------------------------------------------------------

namespace Inixe.Extensions.AwsConfigSource
{
    using System.Collections.Generic;

    /// <summary>
    /// Strategy for transforming formats into <see cref="KeyValuePair{TKey, TValue}"/>.
    /// </summary>
    internal interface IValuePairStrategy
    {
        /// <summary>
        /// Gets the configuration pairs from simple string and key values.
        /// </summary>
        /// <param name="key">The key name that will be used as base or as the name itself of an entry.</param>
        /// <param name="secret">The secret value.</param>
        /// <returns>A list of Key value pairs with the secret entry data.</returns>
        IList<KeyValuePair<string, string>> GetConfigurationPairs(string key, string secret);
    }
}
