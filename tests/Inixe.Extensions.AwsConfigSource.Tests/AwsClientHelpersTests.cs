// -----------------------------------------------------------------------
// <copyright file="AwsClientHelpersTests.cs" company="Inixe S.A.">
// Copyright All Rights reserved. Inixe S.A. 2021
// </copyright>
// -----------------------------------------------------------------------

namespace Inixe.Extensions.AwsConfigSource.Tests
{
    using System;
    using Amazon.Runtime.CredentialManagement;
    using Xunit;

    public class AwsClientHelpersTests
    {
        [Fact]
        public void Should_ThrowArgumentNullException_When_CreatignSecretsManagerClientAndOptionsIsNull()
        {
            // Arrange
            AwsConfigurationSourceOptions options = null;

            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => AwsClientHelpers.CreateSecretsManagerClient(options));
        }

        [Fact]
        public void Should_ThrowArgumentNullException_When_CreatignSystemsManagementClientAndOptionsIsNull()
        {
            // Arrange
            AwsConfigurationSourceOptions options = null;

            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => AwsClientHelpers.CreateSystemsManagementClient(options));
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
        public void Should_ReturnConfiguredSecretsManagerInstanceToRegion_When_OptionsIsDefaultAndAwsSdkDeterminesTheRegion()
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
        public void Should_ReturnConfiguredSystemsManagementInstanceToRegion_When_OptionsIsDefaultAndAwsSdkDeterminesTheRegion()
        {
            // Arrange
            const string RegionName = "us-east-1";

            Environment.SetEnvironmentVariable("AWS_REGION", RegionName, EnvironmentVariableTarget.Process);
            var options = new AwsConfigurationSourceOptions();

            // Act
            var client = AwsClientHelpers.CreateSystemsManagementClient(options);

            // Assert
            Assert.Equal(RegionName, client.Config.RegionEndpoint.SystemName);
        }

        [Fact]
        public void Should_ThrowArgumentException_When_CreateSecretsManagerUsingNonExistingProfileName()
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
            var client = AwsClientHelpers.CreateSystemsManagementClient(options);

            // Assert
            var regionName = client.Config.ServiceURL;
            Assert.Equal(EndpointUrl, regionName);
        }

        [Fact]
        public void Should_AddPathSeparator_When_PathConainsTrailingPathSeparatorAndNormalizeIsTrue()
        {
            // Arrange
            const bool Normalize = true;
            const string TestPath = "/example";
            const string Expected = "/example/";
            const char PathSeparator = '/';

            // Act
            var res = AwsClientHelpers.NormalizePath(TestPath, Normalize, PathSeparator);

            // Assert
            Assert.Equal(Expected, res);
        }

        [Fact]
        public void Should_RemovePathSeparator_When_PathNotConainsTrailingPathSeparatorAndNormalizeIsTrue()
        {
            // Arrange
            const bool Normalize = true;
            const string TestPath = "/example/";
            const string Expected = "/example/";
            const char PathSeparator = '/';

            // Act
            var res = AwsClientHelpers.NormalizePath(TestPath, Normalize, PathSeparator);

            // Assert
            Assert.Equal(Expected, res);
        }

        [Fact]
        public void Should_NotRemovePathSeparator_When_PathConainsTrailingPathSeparatorAndNormalizeIsFalse()
        {
            // Arrange
            const bool Normalize = false;
            const string TestPath = "/example";
            const string Expected = "/example";
            const char PathSeparator = '/';

            // Act
            var res = AwsClientHelpers.NormalizePath(TestPath, Normalize, PathSeparator);

            // Assert
            Assert.Equal(Expected, res);
        }
    }
}
