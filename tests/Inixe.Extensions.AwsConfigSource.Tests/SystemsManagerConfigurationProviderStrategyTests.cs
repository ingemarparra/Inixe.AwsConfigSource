// -----------------------------------------------------------------------
// <copyright file="SystemsManagerConfigurationProviderStrategyTests.cs" company="Inixe S.A.">
// Copyright All Rights reserved. Inixe S.A. 2021
// </copyright>
// -----------------------------------------------------------------------

namespace Inixe.Extensions.AwsConfigSource.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon.SimpleSystemsManagement;
    using Amazon.SimpleSystemsManagement.Model;
    using Inixe.Extensions.AwsConfigSource;
    using Moq;
    using Xunit;

    public class SystemsManagerConfigurationProviderStrategyTests
    {
        private const string ParameterName = "my-parameter";
        private const string ParameterValue = "example";

        private Mock<IAmazonSimpleSystemsManagement> systemsManagementMock;
        private Mock<IValuePairStrategy> valuePairStrategyMock;

        public SystemsManagerConfigurationProviderStrategyTests()
        {
            this.valuePairStrategyMock = new Mock<IValuePairStrategy>(MockBehavior.Strict);
            this.valuePairStrategyMock.Setup(x => x.GetConfigurationPairs(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((k, v) => new Dictionary<string, string>() { { k, v } });

            this.systemsManagementMock = new Mock<IAmazonSimpleSystemsManagement>(MockBehavior.Strict);
            this.systemsManagementMock.Setup(x => x.DescribeParametersAsync(It.IsAny<DescribeParametersRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new DescribeParametersResponse() { NextToken = string.Empty }));
        }

        public static IEnumerable<object[]> SystemsManagementAccessExceptions
        {
            get
            {
                return new List<object[]> { new object[] { new UnauthorizedAccessException() }, new object[] { new AmazonSimpleSystemsManagementException(new UnauthorizedAccessException()) } };
            }
        }

        [Fact]
        public void Should_ThrowArgumentNullException_When_OptionsIsNull()
        {
            // Arrange
            AwsConfigurationSourceOptions options = null;

            // Assert
            Assert.Throws<ArgumentNullException>(() => new SystemsManagerConfigurationProviderStrategy(options));
        }

        [Fact]
        public void Should_ThrowArgumentNullException_When_OptionsIsNullAndSystemsManagementFactoryIsNullAndValuePairStrategyIsNull()
        {
            // Arrange
            AwsConfigurationSourceOptions options = null;
            Func<AwsConfigurationSourceOptions, IAmazonSimpleSystemsManagement> factory = null;
            IValuePairStrategy valuePairStrategy = null;

            // Assert
            Assert.Throws<ArgumentNullException>(() => new SystemsManagerConfigurationProviderStrategy(factory, options, valuePairStrategy));
        }

        [Fact]
        public void Should_ThrowArgumentNullException_When_OptionsIsNotNullAndSystemsManagementFactoryIsNullAndValuePairStrategyIsNull()
        {
            // Arrange
            var options = new AwsConfigurationSourceOptions();
            Func<AwsConfigurationSourceOptions, IAmazonSimpleSystemsManagement> factory = null;
            IValuePairStrategy valuePairStrategy = null;

            // Assert
            Assert.Throws<ArgumentNullException>(() => new SystemsManagerConfigurationProviderStrategy(factory, options, valuePairStrategy));
        }

        [Fact]
        public void Should_ThrowArgumentNullException_When_OptionsIsNotNullAndSystemsManagementFactoryIsNotNullAndValuePairStrategyIsNull()
        {
            // Arrange
            var options = new AwsConfigurationSourceOptions();
            Func<AwsConfigurationSourceOptions, IAmazonSimpleSystemsManagement> factory = options => null;
            IValuePairStrategy valuePairStrategy = null;

            // Assert
            Assert.Throws<ArgumentNullException>(() => new SystemsManagerConfigurationProviderStrategy(factory, options, valuePairStrategy));
        }

        [Fact]
        public void Should_CreateSystemsManagerConfigurationProviderStrategy_When_OptionsIsNotNullAndSystemsManagementFactoryIsNotNull()
        {
            // Arrange
            var options = new AwsConfigurationSourceOptions();
            Func<AwsConfigurationSourceOptions, IAmazonSimpleSystemsManagement> factory = options => null;
            IValuePairStrategy valuePairStrategy = this.valuePairStrategyMock.Object;

            // Act
            var sut = new SystemsManagerConfigurationProviderStrategy(factory, options, valuePairStrategy);

            // Assert
            Assert.NotNull(sut);
        }

        [Fact]
        public void Should_DescribeParameters_When_LoadingSettings()
        {
            // Arrange
            var options = new AwsConfigurationSourceOptions();
            var sut = this.CreateInstance(options);

            // Act
            sut.LoadValues();

            // Assert
            this.systemsManagementMock.Verify(x => x.DescribeParametersAsync(It.IsAny<DescribeParametersRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Theory]
        [MemberData(nameof(SystemsManagementAccessExceptions))]
        public void Should_InvokeExceptionHandler_When_LoadingSettingsAndUserCantDescribeParameters(Exception ex)
        {
            // Arrange
            var wasCalled = false;

            this.systemsManagementMock.Setup(x => x.DescribeParametersAsync(It.IsAny<DescribeParametersRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(ex);

            var options = new AwsConfigurationSourceOptions();
            options.BuildExceptionHandler = x => wasCalled = true;

            var sut = this.CreateInstance(options);

            // Act
            sut.LoadValues();

            // Assert
            Assert.True(wasCalled);
        }

        [Theory]
        [MemberData(nameof(SystemsManagementAccessExceptions))]
        public void Should_CallExceptionHandler_When_LoadingSettingsAndAccessToParameterIsDenied(Exception ex)
        {
            // Arrange
            var wasCalled = false;

            this.SetupSystemsManagerParameter(ParameterName, ParameterValue);
            this.systemsManagementMock.Setup(x => x.GetParametersAsync(It.IsAny<GetParametersRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(ex);

            var options = new AwsConfigurationSourceOptions();
            options.BuildExceptionHandler = e => wasCalled = true;

            var sut = this.CreateInstance(options);

            // Act
            sut.LoadValues();

            // Assert
            Assert.True(wasCalled);
        }

        [Fact]
        public void Should_GetListedParameters_When_LoadingSettingsAndParameterNameAsPathIsTrue()
        {
            // Arrange
            this.SetupSystemsManagerParameter(ParameterName, ParameterValue);

            var options = new AwsConfigurationSourceOptions();

            var sut = this.CreateInstance(options);

            // Act
            sut.LoadValues();

            // Assert
            this.systemsManagementMock.Verify(x => x.GetParametersAsync(It.IsAny<GetParametersRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void Should_GetListedParametersByName_When_LoadingSettingsAndBaseParameterNamePathIsTrue()
        {
            // Arrange
            var hasParameter = false;

            var entry = new ParameterMetadata();
            entry.Name = ParameterName;

            var describeParametersResponse = new DescribeParametersResponse();
            describeParametersResponse.NextToken = string.Empty;
            describeParametersResponse.Parameters = new List<ParameterMetadata>();
            describeParametersResponse.Parameters.Add(entry);

            this.systemsManagementMock.Setup(x => x.DescribeParametersAsync(It.IsAny<DescribeParametersRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(describeParametersResponse));

            this.systemsManagementMock.Setup(x => x.GetParametersAsync(It.IsAny<GetParametersRequest>(), It.IsAny<CancellationToken>()))
                .Callback<GetParametersRequest, CancellationToken>((r, t) => hasParameter = r.Names.Contains(ParameterName))
                .Returns(Task.FromResult(new GetParametersResponse { Parameters = new List<Parameter> { new Parameter { Name = ParameterName, Value = ParameterValue } } }));

            var options = new AwsConfigurationSourceOptions();

            var sut = this.CreateInstance(options);

            // Act
            sut.LoadValues();

            // Assert
            Assert.True(hasParameter);
        }

        [Theory]
        [InlineData("my-parameter", "example", false, "/my-company/my-app", "my-parameter")]
        [InlineData("my-parameter", "example", false, "", "my-parameter")]
        [InlineData("my-parameter", "example", true, "/my-company/my-app", "my-parameter")]
        [InlineData("my-parameter", "example", true, "", "my-parameter")]
        [InlineData("/my-company/my-app/my-parameter", "example", true, "/my-company/my-app", "my-parameter")]
        public void Should_CreateConfiguartionEntryFromParameter_When_LoadingSettingsAndBaseParameterNamePathIsTrue(string parameterName, string parameterValue, bool parameterNameAsPath, string basePath, string expectedName)
        {
            // Arrange
            this.SetupSystemsManagerParameter(parameterName, parameterValue);

            var options = new AwsConfigurationSourceOptions();
            options.ParameterNameAsPath = parameterNameAsPath;
            options.BaseParameterNamePath = basePath;

            var sut = this.CreateInstance(options);

            // Act
            var values = sut.LoadValues();

            // Assert
            Assert.True(values.ContainsKey(expectedName));
        }

        private void SetupSystemsManagerParameter(string parameterName, string parameterValue)
        {
            var entry = new ParameterMetadata();
            entry.Name = parameterName;

            var describeParametersResponse = new DescribeParametersResponse();
            describeParametersResponse.NextToken = string.Empty;
            describeParametersResponse.Parameters = new List<ParameterMetadata>();
            describeParametersResponse.Parameters.Add(entry);

            this.systemsManagementMock.Setup(x => x.DescribeParametersAsync(It.IsAny<DescribeParametersRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(describeParametersResponse));

            var parameter = new Parameter();
            parameter.Name = parameterName;
            parameter.Value = parameterValue;

            var getParametersResponse = new GetParametersResponse();
            getParametersResponse.Parameters = new List<Parameter>();
            getParametersResponse.Parameters.Add(parameter);

            this.systemsManagementMock.Setup(x => x.GetParametersAsync(It.IsAny<GetParametersRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(getParametersResponse));
        }

        private SystemsManagerConfigurationProviderStrategy CreateInstance(AwsConfigurationSourceOptions options)
        {
            Func<AwsConfigurationSourceOptions, IAmazonSimpleSystemsManagement> factory = options => this.systemsManagementMock.Object;
            IValuePairStrategy valuePairStrategy = this.valuePairStrategyMock.Object;

            return new SystemsManagerConfigurationProviderStrategy(factory, options, valuePairStrategy);
        }
    }
}
