# Armali.Horizon
C# framework for management and structure of the Armali project

## Release

Current workflow:

1. Active work is kept on the "dev" branch.
2. When a new feature is ready, a pull request is made to merge it into the "main" branch.
3. Run the CI/CD pipeline of the corresponding app to build and push the Docker image to the registry.

The GitHub repository needs the following secrets to be set up for the CI/CD pipelines to work:
- **ACR_REGISTRY**: The login server of the Azure Container Registry (e.g., "myregistry.azurecr.io")
- **ACR_USERNAME**: The username for the Azure Container Registry (e.g., "myregistry")
- **ACR_PASSWORD**: The password for the Azure Container Registry (e.g., "myregistrypassword")

### Version management

Versions are tagged by running the release pipelines. This will keep in sync the Docker image tag
and the commit tag. The versioning scheme follows semantic versioning (e.g., "1.0.0").

## Deployment

Most images are designed to run via Docker Compose on Portainer.
Some of them could require environment variables to be set up in the Portainer stack configuration.

### Armali.Horizon.Segaris

Requires the following env variables:

- **DATALAKE_ACCOUNT_KEY**: The key for the Azure Data Lake Storage account
