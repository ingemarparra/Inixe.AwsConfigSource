# Samples

In order to run the you will need either an AWS account with permissions to use and access to Secrets Manager service, or
setup an instance of [localstack](https://localstack.cloud)

The easiest way is by using the docker version with the following commands.

```pwsh
docker pull localstack/localstack
docker run --rm -d --name localstack -p 4566:4566 -p 4510-4559:4510-4559 localstack/localstack
```

To create a secret using the AWS CLI first you will have to add a profile to the credentials file by entering *test* as ACCESS_KEY and SECRET_KEY in the wizard.
Name the profile *localstack*.

```pwsh
aws configure wizard
```

Use the profile to create a new sample secret.
```pwsh
aws secretsmanager create-secret --name Confidential --secret-string '"{\"UserName\":\"Super\", \"Password\":\"Secret\"}"' --profile localstack --endpoint-url http://localhost
```