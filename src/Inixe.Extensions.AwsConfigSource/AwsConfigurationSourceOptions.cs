// -----------------------------------------------------------------------
// <copyright file="AwsConfigurationSourceOptions.cs" company="Inixe S.A.">
// Copyright All Rights reserved. Inixe S.A. 2021
// </copyright>
// -----------------------------------------------------------------------

namespace Inixe.Extensions.AwsConfigSource
{
    using System;

    /// <summary>
    /// Options for The Configuration Source.
    /// </summary>
    public class AwsConfigurationSourceOptions
    {
        private string basePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="AwsConfigurationSourceOptions"/> class.
        /// </summary>
        public AwsConfigurationSourceOptions()
        {
            this.PathSeparator = '/';
            this.SecretNameAsPath = false;
            this.BuildExceptionHandler = s => { };
            this.basePath = string.Empty;

            // This will default the SDK file to the SDK Default path.
            this.AwsCredentialsProfilePath = string.Empty;
        }

        /// <summary>
        /// Gets or sets the build exception handler. The exception handler is used as a helper for access permissions issues. If an exception is thrown from the underlying AWS client it will be passed to the handler.
        /// </summary>
        /// <value>
        /// The build exception handler.
        /// </value>
        public Action<Exception> BuildExceptionHandler { get; set; }

        /// <summary>
        /// Gets or sets the name of the AWS region the system is going to be accessing.
        /// </summary>
        /// <value>
        /// The name of the AWS region the system is going to be accessing.
        /// </value>
        public string AwsRegionName { get; set; }

        /// <summary>
        /// Gets or sets the AWS credentials profile path.
        /// </summary>
        /// <value>
        /// The AWS credentials profile path.
        /// </value>
        /// <remarks>The AWS SDK supports loading profiles from paths other than the default path specified in the documentation. This can also help on testing scenarios.</remarks>
        public string AwsCredentialsProfilePath { get; set; }

        /// <summary>
        /// Gets or sets the secrets manager service URL. If you are using a VPC endpoint or a compatible API, this can override the SDK selected endpoint.
        /// </summary>
        /// <value>
        /// The service URL.
        /// </value>
        public string SecretsManagerServiceUrl { get; set; }

        /// <summary>
        /// Gets or sets the name of the profile. This is useful for on-premises deployments where a credential file or a AWS CLI profile is available.
        /// </summary>
        /// <value>
        /// The name of the profile.
        /// </value>
        public string ProfileName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether use secret name paths. For accounts that host several services and have secrets for each service, this will filter out only the <see cref="BaseSecretNamePath"/> from the secret listing.
        /// </summary>
        /// <value>
        ///   <c>true</c> if use secret name paths; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// When a secret contains path separator characters after the <see cref="BaseSecretNamePath"/> has been removed, those instances will become colons in order to state the hierarchical configuration object notation.
        /// <para>
        /// Let secret name be: /my-company/service1/section1/subsection1/subsection2
        /// Let <see cref="BaseSecretNamePath"/> be: /my-company/service1/
        ///
        /// This will create a setting with the following path in the configuration dictionary: section1:subsection1:subsection2
        /// This will be the equivalent of the appsettings.json
        /// {
        ///    "section1":{
        ///      "subsection1":{
        ///        "subsection2": "xxx"
        ///      }
        ///    }
        /// }.
        /// </para>
        /// </remarks>
        public bool SecretNameAsPath { get; set; }

        /// <summary>
        /// Gets or sets the secrets name base path.
        /// </summary>
        /// <value>
        /// The secrets base name path.
        /// </value>
        public string BaseSecretNamePath
        {
            get
            {
                return !this.basePath.EndsWith(this.PathSeparator) && this.SecretNameAsPath ? this.basePath + this.PathSeparator : this.basePath;
            }

            set
            {
                this.basePath = value;
            }
        }

        /// <summary>
        /// Gets or sets the path separator. It defaults to /.
        /// </summary>
        /// <value>
        /// The path separator.
        /// </value>
        public char PathSeparator { get; set; }
    }
}
