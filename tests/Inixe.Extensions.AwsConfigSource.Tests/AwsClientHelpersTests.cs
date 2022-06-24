// -----------------------------------------------------------------------
// <copyright file="AwsClientHelpersTests.cs" company="Inixe S.A.">
// Copyright All Rights reserved. Inixe S.A. 2021
// </copyright>
// -----------------------------------------------------------------------

namespace Inixe.Extensions.AwsConfigSource.Tests
{
    using System;
    using Amazon.SecretsManager;
    using Amazon.Runtime.CredentialManagement;
    using Xunit;

    public class AwsClientHelpersTests
    {
        [Fact]
        public void Should_ThrowArgumentNullException_When_OptionsIsNull()
        {
            // Arrange
            AwsConfigurationSourceOptions options = null;

            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => AwsClientHelpers.CreateSecretsManagerClient(options));
        }

        [Fact]
        public void Should_ServiceEndpoint_When_SecretsManagerServiceUrlIsNotEmpty()
        {
            // Arrange
            const string EnpointOverrideUrl = "http://localhost/";

            var options = new AwsConfigurationSourceOptions();
            options.SecretsManagerServiceUrl = EnpointOverrideUrl;

            // Act
            var client = AwsClientHelpers.CreateSecretsManagerClient(options);

            // Assert
            var url = client.Config.ServiceURL;
            Assert.Equal(EnpointOverrideUrl, url);
        }

        [Fact]
        public void Should_AwsRegion_When_AwsRegionNameIsNotEmpty()
        {
            // Arrange
            const string RegionOverride = "us-west-2";

            var options = new AwsConfigurationSourceOptions();
            options.AwsRegionName = RegionOverride;

            // Act
            var client = AwsClientHelpers.CreateSecretsManagerClient(options);

            // Assert
            var region = client.Config.RegionEndpoint.SystemName;
            Assert.Equal(RegionOverride, region);
        }

        [Fact]
        public void Should_ReturnConfiguredInstanceToRegion_When_OptionsIsDefaultAndAwsSdkDeterminesTheRegion()
        {
            // Arrange
            const string RegionName = "us-east-1";

            Environment.SetEnvironmentVariable("AWS_REGION", RegionName, EnvironmentVariableTarget.Process);
            var options = new AwsConfigurationSourceOptions();

            // Act
            var client = AwsClientHelpers.CreateSecretsManagerClient(options);

            // Assert
            Assert.Equal(RegionName, client.Config.RegionEndpoint.SystemName);
        }

        [Fact]
        public void Should_ThrowArgumentException_When_UsingNonExistingProfileName()
        {
            // Arrange
            var options = new AwsConfigurationSourceOptions();
            options.ProfileName = "Sample";

            // Act
            // Assert
            Assert.Throws<ArgumentException>(() => AwsClientHelpers.CreateSecretsManagerClient(options));
        }

        [Fact]
        public void Should_UseProfileRegion_When_UsingExistingProfileName()
        {
            // Arrange
            const string ProfileName = "Sample";

            var profileOptions = new CredentialProfileOptions();
            profileOptions.AccessKey = "Anything";
            profileOptions.SecretKey = "Secret";

            var profile = new CredentialProfile(ProfileName, profileOptions);
            profile.Region = Amazon.RegionEndpoint.EUNorth1;

            var credentialsFilePath = System.IO.Path.GetTempFileName();
            var credentialsFile = new SharedCredentialsFile(credentialsFilePath);
            credentialsFile.RegisterProfile(profile);

            var options = new AwsConfigurationSourceOptions();
            options.ProfileName = ProfileName;
            options.AwsCredentialsProfilePath = credentialsFilePath;

            // Act
            var client = AwsClientHelpers.CreateSecretsManagerClient(options);

            // Assert
            var regionName = client.Config.RegionEndpoint.SystemName;
            Assert.Equal(Amazon.RegionEndpoint.EUNorth1.SystemName, regionName);
        }

        [Fact]
        public void Should_UseProvidedServiceUrl_When_UsingExistingProfileName()
        {
            // Arrange
            const string EndpointUrl = "http://localhost/";
            const string ProfileName = "Sample";

            var profileOptions = new CredentialProfileOptions();
            profileOptions.AccessKey = "Anything";
            profileOptions.SecretKey = "Secret";

            var profile = new CredentialProfile(ProfileName, profileOptions);
            profile.Region = Amazon.RegionEndpoint.EUNorth1;

            var credentialsFilePath = System.IO.Path.GetTempFileName();
            var credentialsFile = new SharedCredentialsFile(credentialsFilePath);
            credentialsFile.RegisterProfile(profile);

            var options = new AwsConfigurationSourceOptions();
            options.ProfileName = ProfileName;
            options.AwsCredentialsProfilePath = credentialsFilePath;
            options.SecretsManagerServiceUrl = EndpointUrl;

            // Act
            var client = AwsClientHelpers.CreateSecretsManagerClient(options);

            // Assert
            var regionName = client.Config.ServiceURL;
            Assert.Equal(EndpointUrl, regionName);
        }
    }
}
