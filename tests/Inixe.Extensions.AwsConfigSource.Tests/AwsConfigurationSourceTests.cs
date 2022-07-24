// -----------------------------------------------------------------------
// <copyright file="AwsConfigurationSourceTests.cs" company="Inixe S.A.">
// Copyright All Rights reserved. Inixe S.A. 2021
// </copyright>
// -----------------------------------------------------------------------

namespace Inixe.Extensions.AwsConfigSource.Tests
{
    using System;
    using System.Collections.Generic;
    using Inixe.Extensions.AwsConfigSource;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Xunit;

    public class AwsConfigurationSourceTests
    {
        [Fact]
        public void Should_ThrowArgumentNullException_When_OptionsIsNull()
        {
            AwsConfigurationSourceOptions options = null;

            // Arrange
            // Assert
            Assert.Throws<ArgumentNullException>(() => new AwsConfigurationSource(options));
        }

        [Fact]
        public void Should_ThrowArgumentException_When_SectionNameIsEmpty()
        {
            var sectionName = string.Empty;

            // Arrange
            // Assert
            Assert.Throws<ArgumentException>(() => new AwsConfigurationSource(sectionName));
        }

        [Fact]
        public void Should_CreateAwsConfigurationSource_When_OptionsIsNotNull()
        {
            // Arrange
            var options = new AwsConfigurationSourceOptions();

            // Act
            var sut = new AwsConfigurationSource(options);

            // Assert
            Assert.NotNull(sut);
        }

        [Fact]
        public void Should_ThrowArgumentNullException_When_BuildingAndBuilderIsNull()
        {
            // Arrange
            IConfigurationBuilder builder = null;
            var sut = this.CreateInstanceFromOptions();

            // Assert
            Assert.Throws<ArgumentNullException>(() => sut.Build(builder));
        }

        [Fact]
        public void Should_ReturnConfigurationProvider_When_BuildingAndBuilderIsNotNull()
        {
            // Arrange
            var builderMock = new Mock<IConfigurationBuilder>(MockBehavior.Strict);
            IConfigurationBuilder builder = builderMock.Object;

            var sut = this.CreateInstanceFromOptions();

            // Act
            var res = sut.Build(builder);

            // Assert
            Assert.IsType<AwsConfigurationProvider>(res);
        }

        [Fact]
        public void Should_RenderOptionsFromConfiguration_When_BuildingAndBuilderIsNotNull()
        {
            // Arrange
            var builderMock = SetupConfigurationBuilder(Amazon.RegionEndpoint.SAEast1.SystemName);
            IConfigurationBuilder builder = builderMock.Object;

            var sut = this.CreateInstanceFromSection();

            // Act
            var res = (AwsConfigurationProvider)sut.Build(builder);

            // Assert
            Assert.Equal(Amazon.RegionEndpoint.SAEast1.SystemName, res.Strategies[0].Options.AwsRegionName);
        }

        private static Mock<IConfigurationBuilder> SetupConfigurationBuilder(string regionName)
        {
            // Will create a single property in the configuration chain
            var configurationChildMock = new Mock<IConfigurationSection>(MockBehavior.Strict);

            configurationChildMock.Setup(x => x.GetChildren())
                .Returns(new List<IConfigurationSection>());

            configurationChildMock.Setup(x => x.Path)
                .Returns(nameof(AwsConfigurationSourceOptions.AwsRegionName));

            configurationChildMock.Setup(x => x.Value)
                .Returns(regionName);

            var configurationMock = new Mock<IConfigurationRoot>();

            configurationMock.Setup(x => x.GetSection(AwsConfigurationSource.DefaultSectionName))
                .Returns(configurationChildMock.Object);

            var builderMock = new Mock<IConfigurationBuilder>(MockBehavior.Strict);

            builderMock.Setup(x => x.Build())
                .Returns(configurationMock.Object);

            return builderMock;
        }

        private AwsConfigurationSource CreateInstanceFromSection()
        {
            return new AwsConfigurationSource(AwsConfigurationSource.DefaultSectionName);
        }

        private AwsConfigurationSource CreateInstanceFromOptions()
        {
            var options = new AwsConfigurationSourceOptions();
            return new AwsConfigurationSource(options);
        }
    }
}
