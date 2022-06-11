// -----------------------------------------------------------------------
// <copyright file="JsonValuePairStrategyTests.cs" company="Inixe S.A.">
// Copyright All Rights reserved. Inixe S.A. 2021
// </copyright>
// -----------------------------------------------------------------------

namespace Inixe.Extensions.AwsConfigSource.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Xunit;

    public class JsonValuePairStrategyTests
    {
        [Fact]
        public void Should_ReturnSingleKeyValuePair_When_SecretValueIsNotJson()
        {
            const string SecretName = "top-secret";
            const string SecretValue = "don't tell";

            // Arrange
            var strategy = new JsonValuePairStrategy();

            // Act
            var res = strategy.GetConfigurationPairs(SecretName, SecretValue);

            // Assert
            Assert.Contains(res, x => x.Key == SecretName && x.Value == SecretValue);
        }

        [Fact]
        public void Should_ReturnValuePairs_When_SecretValueIsJson()
        {
            const string SecretName = "top-secret";
            const string SecretValue = "{\"Value1\":\"don't tell\"}";

            // Arrange
            var strategy = new JsonValuePairStrategy();

            // Act
            var res = strategy.GetConfigurationPairs(SecretName, SecretValue);

            // Assert
            Assert.True(res.Count() == 2);
        }

        [Theory]
        [InlineData("{\"Value1\":[\"don't tell\"]}", 0)]
        [InlineData("{\"Value1\":[\"don't tell\",\"this is\"]}", 1)]
        [InlineData("{\"Value1\":[\"don't tell\",\"this is\",\"a secret string\"]}", 2)]
        public void Should_FormatArrayItemKeys_When_SecretValueIsJson(string secretValue, int expectedIndex)
        {
            const string SecretName = "top-secret";
            var expected = $"{SecretName}:Value1:{expectedIndex}";

            // Arrange
            var strategy = new JsonValuePairStrategy();

            // Act
            var res = strategy.GetConfigurationPairs(SecretName, secretValue);

            // Assert
            Assert.Contains(res, x => x.Key == expected);
        }
    }
}
