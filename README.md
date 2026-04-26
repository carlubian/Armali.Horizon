# Armali.Horizon
C# framework for management and structure of the Armali project

## Release

Current workflow:

1. Active work is kept on the "feature/*" branch.
2. When a new feature is ready, a pull request is made to merge it into the "main" branch.
3. Run the CI/CD pipeline of the corresponding app to build and push the Docker image to the registry.

The GitHub repository needs the following secrets to be set up for the CI/CD pipelines to work:
- **ACR_REGISTRY**: The login server of the Azure Container Registry (e.g., "myregistry.azurecr.io")
- **ACR_USERNAME**: The username for the Azure Container Registry (e.g., "myregistry")
- **ACR_PASSWORD**: The password for the Azure Container Registry (e.g., "myregistrypassword")

### Version management

Versions are tagged by running the release pipelines. This will keep in sync the Docker image tag
and the commit tag. The versioning scheme follows semantic versioning (e.g., "1.0.0").

## Local Deployment

The current version is intended to be run via Docker Compose, as apps depend on each other and need to be run together. 
The Docker Compose file for local tests is located in the root of the repository and is named "docker-compose.local.yml".

It cn be run with this command:

```
docker compose -f docker-compose.local.yml up --build
```

This process will also build the images locally including the latest changes.

## Production Deployment

This is deployed on Portainer using the "docker-compose.yml" file.

Check the "environment" section of each service and replace the required variables.
