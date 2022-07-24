// -----------------------------------------------------------------------
// <copyright file="ConfigurationBuilderExtensionsTests.cs" company="Inixe S.A.">
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
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Xunit;

    public class ConfigurationBuilderExtensionsTests
    {
        [Fact]
        public void Should_ReturnConfigurationBuilderInstance_When_NoOptionsIsProvided()
        {
            // Arrange
            var builderMock = new Mock<IConfigurationBuilder>(MockBehavior.Strict);
            builderMock.Setup(x => x.Add(It.IsAny<IConfigurationSource>()))
                .Returns(() => builderMock.Object);

            var builder = builderMock.Object;

            // Act
            var res = builder.AddAwsConfiguration();

            // Assert
            Assert.NotNull(res);
        }

        [Fact]
        public void Should_ReturnConfigurationBuilderInstance_When_OptionsIsNotNull()
        {
            // Arrange
            var builderMock = new Mock<IConfigurationBuilder>(MockBehavior.Strict);
            builderMock.Setup(x => x.Add(It.IsAny<IConfigurationSource>()))
                .Returns(() => builderMock.Object);

            var builder = builderMock.Object;
            var options = new AwsConfigurationSourceOptions();

            // Act
            var res = builder.AddAwsConfiguration(options);

            // Assert
            Assert.NotNull(res);
        }

        [Fact]
        public void Should_ThrowArgumentNullException_When_ConfigurationBuilderIsNullAndOptionsIsNotNull()
        {
            // Arrange
            IConfigurationBuilder builder = null;
            var options = new AwsConfigurationSourceOptions();

            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => builder.AddAwsConfiguration(options));
        }

        [Fact]
        public void Should_ThrowArgumentNullException_When_ConfigurationBuilderIsNullAndConfigSectionNameIsNotEmpty()
        {
            // Arrange
            IConfigurationBuilder builder = null;
            var configSectionName = "Sample";

            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => builder.AddAwsConfiguration(configSectionName));
        }
    }
}
