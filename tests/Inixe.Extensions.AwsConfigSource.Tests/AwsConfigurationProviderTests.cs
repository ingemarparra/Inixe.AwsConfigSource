// -----------------------------------------------------------------------
// <copyright file="AwsConfigurationProviderTests.cs" company="Inixe S.A.">
// Copyright All Rights reserved. Inixe S.A. 2021
// </copyright>
// -----------------------------------------------------------------------

namespace Inixe.Extensions.AwsConfigSource.Tests
{
    using System;
    using System.Collections.Generic;
    using Inixe.Extensions.AwsConfigSource;
    using Moq;
    using Xunit;

    public class AwsConfigurationProviderTests
    {
        private Mock<IConfigurationProviderStrategy> providerStrategyMock1;
        private Mock<IConfigurationProviderStrategy> providerStrategyMock2;

        public AwsConfigurationProviderTests()
        {
            this.providerStrategyMock1 = new Mock<IConfigurationProviderStrategy>(MockBehavior.Strict);
            this.providerStrategyMock2 = new Mock<IConfigurationProviderStrategy>(MockBehavior.Strict);
        }

        [Fact]
        public void Should_ThrowArgumentNullException_When_StrategiesListIsNull()
        {
            // Arrange
            List<IConfigurationProviderStrategy> strategies = null;

            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => new AwsConfigurationProvider(strategies));
        }

        [Fact]
        public void Should_LoadValuesFromStrategies_When_CallingLoad()
        {
            // Arrange
            this.providerStrategyMock1.Setup(x => x.LoadValues())
                .Returns(new Dictionary<string, string>());

            IConfigurationProviderStrategy mockStrategy = this.providerStrategyMock1.Object;

            var strategies = new List<IConfigurationProviderStrategy>();
            strategies.Add(mockStrategy);

            var sut = new AwsConfigurationProvider(strategies);

            // Act
            sut.Load();

            // Assert
            this.providerStrategyMock1.Verify(x => x.LoadValues(), Times.Once);
        }

        [Fact]
        public void Should_ReplaceValues_When_PreviousStrategyHasSameKey()
        {
            // Arrange
            const string Key = "mykey";
            const string Expected = "value2";

            this.providerStrategyMock1.Setup(x => x.LoadValues())
                .Returns(new Dictionary<string, string>() { { Key, "value1" } });

            this.providerStrategyMock2.Setup(x => x.LoadValues())
                .Returns(new Dictionary<string, string>() { { Key, "value2" } });

            IConfigurationProviderStrategy mockStrategy1 = this.providerStrategyMock1.Object;
            IConfigurationProviderStrategy mockStrategy2 = this.providerStrategyMock2.Object;

            var strategies = new List<IConfigurationProviderStrategy>();
            strategies.Add(mockStrategy1);
            strategies.Add(mockStrategy2);

            var sut = new AwsConfigurationProvider(strategies);

            // Act
            sut.Load();

            // Assert
            if (sut.TryGet(Key, out var res))
            {
                Assert.Equal(Expected, res);
            }
            else
            {
                throw new KeyNotFoundException("No key was found");
            }
        }
    }
}
