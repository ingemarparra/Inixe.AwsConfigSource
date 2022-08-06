# Configuration Options

* [Configuration Parameters](#configuration-parameters)
    * [Treating resource names as paths](#treating-resource-names-as-paths)
    * [AWS configuration options](#aws-configuration-options)
* [Using another configuration source as the provider](#using-another-configuration-source-as-the-provider)

## Configuration Parameters

### Treating resource names as paths

Secrets and Parameters stored in AWS can be treated as paths so that complex objects or simple key/value pair relationships can be represented.
By default all secrets that are available to configured IAM entity within the AWS account are added as key/value pairs in the configuration.
If you want to segregate the access to certain resources, it is possible to load only those that belong to your application.

In order to do so, you need to name your resources representing a path like name.
```
/my-team/my-application/my-resource
team-application-resource
```

By configuring the __AwsConfigurationSourceOptions__ it's possible to set values as the base path used to look for your resources. It's possible to control
which is your path separator by setting the corresponding character in __PathSeparator__.

### BaseSecretNamePath

__Optional__. This is the base path used by your application to list secrets that belong to the application.

### BaseParameterNamePath

__Optional__. This is the base path used by your application to list parameters that belong to the application.

### SecretNameAsPath

__Optional__. Controls whether or not the Path segregation feature is enabled for secrets manager. Disabled by default.
When enabled all secrets which names begin with the value specified in [BaseSecretNamePath](#basesecretnamepath) will be included in the resulting configuration.

### ParameterNameAsPath

__Optional__. Controls whether or not the Path segregation feature is enabled for systems manager. Disabled by default.
When enabled all parameters which names begin with the value specified in [BaseSecretNamePath](#baseparameternamepath) will be included in the resulting configuration.

## AWS configuration options

### ProfileName

__Optional__. When developing or when the AWS CLI or any AWSSDK is installed it's possible to configure a profile that can have the security details to access AWS resources

### SecretsManagerServiceUrl and SystemsManagementServiceUrl
__Optional__. By default AWS services need access to internet even from within the AWS infrastructure itself. If your implementation requires the use of a compatible API or a VPC endpoint, it should be specified for the corresponding service.

## Using another configuration source as the provider

Setting the configuration values for the AWS configuration Source can be done in two ways.

* In code
* In configuration

For the configuration option it's possible to create a configuration section and add the corresponding properties to it. This gives system managers the ability to control the parameters for each environment.

For example a __appsettings.json__
```json
{
    "Inixe":{
        "Custom":{
            "ProfileName": "SampleProfile",
            "BaseParameterNamePath": "/Inixe/SampleApplication",
            "ParameterNameAsPath": false,
            "BaseSecretNamePath": "/Inixe/SampleApplication",
            "SecretNameAsPath": true
        }
    }
}
```

The name of the configuration section is customizable, You only need to follow the Configuration extensions conventions. For the preceding example the corresponding configuration would be:

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
            const string InixeSecretsManagerSectionName = "Inixe:Custom";

            var builder = new ConfigurationBuilder();
            builder.AddAwsConfiguration(InixeSecretsManagerSectionName);

            return builder;
        }
    }
}
```
