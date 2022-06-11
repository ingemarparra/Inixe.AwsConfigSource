// -----------------------------------------------------------------------
// <copyright file="JsonValuePairStrategy.cs" company="Inixe S.A.">
// Copyright All Rights reserved. Inixe S.A. 2021
// </copyright>
// -----------------------------------------------------------------------

namespace Inixe.Extensions.AwsConfigSource
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;

    /// <summary>
    /// Json Based IValuePairStrategy. If a secret is a json object it will flatten the value into multiple key value pairs.
    /// </summary>
    /// <seealso cref="Inixe.Extensions.AwsConfigSource.IValuePairStrategy" />
    internal class JsonValuePairStrategy : IValuePairStrategy
    {
        /// <summary>
        /// Gets the configuration pairs from simple string and key values.
        /// </summary>
        /// <param name="key">The key name that will be used as base or as the name itself of an entry.</param>
        /// <param name="secret">The secret value.</param>
        /// <returns>
        /// A list of Key value pairs with the secret entry data.
        /// </returns>
        public IEnumerable<KeyValuePair<string, string>> GetConfigurationPairs(string key, string secret)
        {
            var pairs = new List<KeyValuePair<string, string>>();

            if (IsJson(secret))
            {
                var jsonEntries = FlattenJson(key, secret);
                pairs.AddRange(jsonEntries);
            }

            pairs.Add(new KeyValuePair<string, string>(key, secret));
            return pairs;
        }

        private static bool IsJson(string input)
        {
            input = input.Trim();
            var isJsonObject = input.StartsWith("{") && input.EndsWith("}");
            var isJsonArray = input.StartsWith("[") && input.EndsWith("]");
            return isJsonObject || isJsonArray;
        }

        private static List<KeyValuePair<string, string>> FlattenJson(string key, string value)
        {
            var bytes = new ReadOnlySpan<byte>(System.Text.Encoding.UTF8.GetBytes(value));
            var reader = new Utf8JsonReader(bytes);
            if (JsonDocument.TryParseValue(ref reader, out var jsonDoc))
            {
                return FlattenJson(key, jsonDoc.RootElement);
            }

            return new List<KeyValuePair<string, string>>();
        }

        private static List<KeyValuePair<string, string>> FlattenJson(string key, JsonElement element)
        {
            var entries = new List<KeyValuePair<string, string>>();

            if (element.ValueKind == JsonValueKind.Array)
            {
                var itemIndex = 0;
                foreach (var item in element.EnumerateArray())
                {
                    var configurationKeyName = $"{key}:{itemIndex}";
                    var arrayEntries = FlattenJson(configurationKeyName, item);
                    entries.AddRange(arrayEntries);

                    itemIndex++;
                }
            }
            else if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in element.EnumerateObject())
                {
                    var configurationKeyName = $"{key}:{prop.Name}";
                    var objectEntries = FlattenJson(configurationKeyName, prop.Value);
                    entries.AddRange(objectEntries);
                }
            }
            else
            {
                var entryValue = element.GetString();
                entries.Add(new KeyValuePair<string, string>(key, entryValue));
            }

            return entries;
        }
    }
}
