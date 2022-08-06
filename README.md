# Inixe AWS Configuration Source

An implementation of a configuration source that consumes settings or key value pairs from AWS secrets manager/ System's Manager Parameter Store.

Inspired in [Kralizek AWS Secrets Manager Configuration Extensions](https://github.com/Kralizek/AWSSecretsManagerConfigurationExtensions), this extension aims to consume both
AWS Secrets Manager and System's Manager Parameter Store values using a Microsoft Configurations Extensions.

# Table of Contents

* [Prerequisites](#prerequisites)
* [Building from Source](#building-from-source)
* [How to use](#how-to-use)
* [License](#license)

# Prerequisites

* .NET 6.0 SDK version 6 or above
* Visual Studio Code, Visual Studio 2019 or above
* Powershell 7.0 or above
* AWS CLI v2

# Building from Source

```pwsh
$PackageVersion="1.0.0"
dotnet build Inixe.Extensions.AwsConfigSource.sln -c Release -p:Version=$PackageVersion
```

# Features

* Integrates Systems Manager Parameter store as a configuration source
* Integrates Secrets Manager as a configuration source

# How to use

Using the package is as simple as adding a new configuration source.

```csharp
using Inixe.Extensions.AwsConfigSource;
using System;
using Microsoft.Extensions.Configuration;

namespace Inixe.Extensions.AwsConfigSource
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configuration = SetupConfiguration()
                .Build();

            // Your code your secrets and parameters here...
        }

        private IConfigurationBuilder SetupConfiguration()
        {
            var builder = new ConfigurationBuilder();
            builder.AddAwsConfiguration();

            return builder;
        }
    }
}
```

The configuration source is optional by default, so it's recommended to add it last to the build chain.

```csharp
using Inixe.Extensions.AwsConfigSource;
using System;
using Microsoft.Extensions.Configuration;

namespace Inixe.Extensions.AwsConfigSource
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configuration = SetupConfiguration()
                .Build();

            // Your code your secrets and parameters here...
        }

        private IConfigurationBuilder SetupConfiguration()
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddInMemoryCollection()
                .AddAwsConfiguration();

            return builder;
        }
    }
}
```

Check the samples folder for more details and/or the [configuration options](doc/configuration.md) for further information.

# License

You can find the License terms [here](LICENSE)