// -----------------------------------------------------------------------
// <copyright file="AwsConfigurationSourceOptions.cs" company="Inixe S.A.">
// Copyright All Rights reserved. Inixe S.A. 2021
// </copyright>
// -----------------------------------------------------------------------

namespace Inixe.Extensions.AwsConfigSource
{
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
        }

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
