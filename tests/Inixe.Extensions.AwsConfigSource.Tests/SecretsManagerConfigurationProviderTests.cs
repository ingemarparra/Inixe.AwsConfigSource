// -----------------------------------------------------------------------
// <copyright file="SecretsManagerConfigurationProviderTests.cs" company="Inixe S.A.">
// Copyright All Rights reserved. Inixe S.A. 2021
// </copyright>
// -----------------------------------------------------------------------

namespace Inixe.Extensions.AwsConfigSource.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon.SecretsManager;
    using Amazon.SecretsManager.Model;
    using Inixe.Extensions.AwsConfigSource;
    using Moq;
    using Xunit;

    public class SecretsManagerConfigurationProviderTests
    {
        private const string SecretName = "top-secret";
        private const string SecretValue = "don't tell";

        private Mock<IAmazonSecretsManager> secretsManagerMock;
        private Mock<IValuePairStrategy> valuePairStrategyMock;

        public SecretsManagerConfigurationProviderTests()
        {
            this.valuePairStrategyMock = new Mock<IValuePairStrategy>(MockBehavior.Strict);
            this.valuePairStrategyMock.Setup(x => x.GetConfigurationPairs(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new List<KeyValuePair<string, string>>());

            this.secretsManagerMock = new Mock<IAmazonSecretsManager>(MockBehavior.Strict);
            this.secretsManagerMock.Setup(x => x.ListSecretsAsync(It.IsAny<ListSecretsRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new ListSecretsResponse() { NextToken = string.Empty }));
        }

        [Fact]
        public void Should_ThrowArgumentNullException_When_OptionsIsNull()
        {
            // Arrange
            AwsConfigurationSourceOptions options = null;

            // Assert
            Assert.Throws<ArgumentNullException>(() => new SecretsManagerConfigurationProvider(options));
        }

        [Fact]
        public void Should_ThrowArgumentNullException_When_OptionsIsNullAndSecretsManagerFactoryIsNullAndValuePairStrategyIsNull()
        {
            // Arrange
            AwsConfigurationSourceOptions options = null;
            Func<AwsConfigurationSourceOptions, IAmazonSecretsManager> factory = null;
            IValuePairStrategy valuePairStrategy = null;

            // Assert
            Assert.Throws<ArgumentNullException>(() => new SecretsManagerConfigurationProvider(factory, options, valuePairStrategy));
        }

        [Fact]
        public void Should_ThrowArgumentNullException_When_OptionsIsNotNullAndSecretsManagerFactoryIsNullAndValuePairStrategyIsNull()
        {
            // Arrange
            var options = new AwsConfigurationSourceOptions();
            Func<AwsConfigurationSourceOptions, IAmazonSecretsManager> factory = null;
            IValuePairStrategy valuePairStrategy = null;

            // Assert
            Assert.Throws<ArgumentNullException>(() => new SecretsManagerConfigurationProvider(factory, options, valuePairStrategy));
        }

        [Fact]
        public void Should_CreateSecretsManagerConfigurationProvider_When_OptionsIsNotNullAndSecretsManagerFactoryIsNotNull()
        {
            // Arrange
            var options = new AwsConfigurationSourceOptions();
            Func<AwsConfigurationSourceOptions, IAmazonSecretsManager> factory = options => null;
            IValuePairStrategy valuePairStrategy = this.valuePairStrategyMock.Object;

            // Act
            var sut = new SecretsManagerConfigurationProvider(factory, options, valuePairStrategy);

            // Assert
            Assert.NotNull(sut);
        }

        [Fact]
        public void Should_ListSecrets_When_LoadingSettings()
        {
            // Arrange
            var options = new AwsConfigurationSourceOptions();
            var sut = this.CreateInstance(options);

            // Act
            sut.Load();

            // Assert
            this.secretsManagerMock.Verify(x => x.ListSecretsAsync(It.IsAny<ListSecretsRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void Should_PassSecrestsFilter_When_LoadingSettingsAndSecretNameAsPathIsTrue()
        {
            // Arrange
            const string BasePath = "/inixe/packages";

            var isUsingFilters = false;
            var listSecretsResponse = new ListSecretsResponse() { NextToken = string.Empty };
            this.secretsManagerMock.Setup(x => x.ListSecretsAsync(It.IsAny<ListSecretsRequest>(), It.IsAny<CancellationToken>()))
                .Callback<ListSecretsRequest, CancellationToken>((r, t) => isUsingFilters = r.Filters.Any(x => x.Key == FilterNameStringType.Name && x.Values.Any(y => y.StartsWith(BasePath))))
                .Returns(Task.FromResult(listSecretsResponse));

            var options = new AwsConfigurationSourceOptions();
            options.BaseSecretNamePath = BasePath;
            options.SecretNameAsPath = true;

            var sut = this.CreateInstance(options);

            // Act
            sut.Load();

            // Assert
            Assert.True(isUsingFilters);
        }

        [Fact]
        public void Should_GetListedSecrests_When_LoadingSettingsAndSecretNameAsPathIsTrue()
        {
            // Arrange
            this.SetupSecretsManagerSecret(SecretName, SecretValue);

            var options = new AwsConfigurationSourceOptions();

            var sut = this.CreateInstance(options);

            // Act
            sut.Load();

            // Assert
            this.secretsManagerMock.Verify(x => x.GetSecretValueAsync(It.IsAny<GetSecretValueRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void Should_GetListedSecrestsByArn_When_LoadingSettingsAndSecretNameAsPathIsTrue()
        {
            // Arrange
            var hasArn = false;
            var arn = $"arn:aws:::secrets/{SecretName}";

            var entry = new SecretListEntry();
            entry.Name = SecretName;
            entry.ARN = arn;

            var listSecretsResponse = new ListSecretsResponse();
            listSecretsResponse.NextToken = string.Empty;
            listSecretsResponse.SecretList = new List<SecretListEntry>();
            listSecretsResponse.SecretList.Add(entry);

            this.secretsManagerMock.Setup(x => x.ListSecretsAsync(It.IsAny<ListSecretsRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(listSecretsResponse));

            this.secretsManagerMock.Setup(x => x.GetSecretValueAsync(It.IsAny<GetSecretValueRequest>(), It.IsAny<CancellationToken>()))
                .Callback<GetSecretValueRequest, CancellationToken>((r, t) => hasArn = r.SecretId == arn)
                .Returns(Task.FromResult(new GetSecretValueResponse { Name = SecretName, SecretString = SecretValue }));

            var options = new AwsConfigurationSourceOptions();

            var sut = this.CreateInstance(options);

            // Act
            sut.Load();

            // Assert
            Assert.True(hasArn);
        }

        [Theory]
        [InlineData("top-secret", "don't tell", false, "/my-company/my-app", "top-secret")]
        [InlineData("top-secret", "don't tell", false, "", "top-secret")]
        [InlineData("top-secret", "don't tell", true, "/my-company/my-app", "top-secret")]
        [InlineData("top-secret", "don't tell", true, "", "top-secret")]
        [InlineData("/my-company/my-app/top-secret", "don't tell", true, "/my-company/my-app", "top-secret")]
        public void Should_CreateConfiguartionEntryFromSecret_When_LoadingSettingsAndSecretNameAsPathIsTrue(string secretName, string secretValue, bool secretNameAsPath, string basePath, string expectedName)
        {
            // Arrange
            this.valuePairStrategyMock.Setup(x => x.GetConfigurationPairs(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((k, v) => new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>(k, v) });

            this.SetupSecretsManagerSecret(secretName, secretValue);

            var options = new AwsConfigurationSourceOptions();
            options.SecretNameAsPath = secretNameAsPath;
            options.BaseSecretNamePath = basePath;

            var sut = this.CreateInstance(options);

            // Act
            sut.Load();

            // Assert
            Assert.True(sut.TryGet(expectedName, out _));
        }

        private void SetupSecretsManagerSecret(string SecretName, string SecretValue)
        {
            var entry = new SecretListEntry();
            entry.Name = SecretName;

            var listSecretsResponse = new ListSecretsResponse();
            listSecretsResponse.NextToken = string.Empty;
            listSecretsResponse.SecretList = new List<SecretListEntry>();
            listSecretsResponse.SecretList.Add(entry);

            this.secretsManagerMock.Setup(x => x.ListSecretsAsync(It.IsAny<ListSecretsRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(listSecretsResponse));

            this.secretsManagerMock.Setup(x => x.GetSecretValueAsync(It.IsAny<GetSecretValueRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new GetSecretValueResponse { Name = SecretName, SecretString = SecretValue }));
        }

        private SecretsManagerConfigurationProvider CreateInstance(AwsConfigurationSourceOptions options)
        {
            Func<AwsConfigurationSourceOptions, IAmazonSecretsManager> factory = options => this.secretsManagerMock.Object;
            IValuePairStrategy valuePairStrategy = this.valuePairStrategyMock.Object;

            return new SecretsManagerConfigurationProvider(factory, options, valuePairStrategy);
        }
    }
}
