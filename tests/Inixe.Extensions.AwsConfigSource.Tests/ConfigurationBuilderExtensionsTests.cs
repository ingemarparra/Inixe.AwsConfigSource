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
    }
}
