// -----------------------------------------------------------------------
// <copyright file="AwsConfigurationSourceTests.cs" company="Inixe S.A.">
// Copyright All Rights reserved. Inixe S.A. 2021
// </copyright>
// -----------------------------------------------------------------------

namespace Inixe.Extensions.AwsConfigSource.Tests
{
    using System;
    using Inixe.Extensions.AwsConfigSource;
    using Microsoft.Extensions.Configuration;
    using Xunit;
    using Moq;

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
            var sut = this.CreateInstance();

            IConfigurationBuilder builder = null;

            // Assert
            Assert.Throws<ArgumentNullException>(() => sut.Build(builder));
        }

        [Fact]
        public void Should_ReturnConfigurationProvider_When_BuildingAndBuilderIsNotNull()
        {
            // Arrange
            var builderMock = new Mock<IConfigurationBuilder>(MockBehavior.Strict);
            var sut = this.CreateInstance();

            IConfigurationBuilder builder = builderMock.Object;

            // Act
            var res = sut.Build(builder);

            // Assert
            Assert.IsType<SecretsManagerConfigurationProvider>(res);
        }

        private AwsConfigurationSource CreateInstance()
        {
            var options = new AwsConfigurationSourceOptions();
            return new AwsConfigurationSource(options);
        }
    }
}
