# Supported targets

Currently these services are supported as (git) targets:

## Gitlab User (public repositories only)

Currently only public repositories are supported as a Gitlab target (authentication is required to create a Gitlab repository and on creating a repository it will also copy the description from Github automatically):

``` json
{
    "target": "https://gitlab.com/MarcStan",
    "accessToken": {
        "source": "https://mykeyvault.vault.azure.net",
        "secretName": "MyGitlabPAT"
    }
}
```

Required Gitlab scopes on the access token: **api**

## Azure DevOps Project

Azure DevOps projects can be private (default) or public. Either way authentication is required to create repositories.

The access token must have at least permission to read repositories in the project.
``` json
{
    "target": "https://dev.azure.com/marcstanlive/Opensource",
    "accessToken": {
        "source": "https://mykeyvault.vault.azure.net",
        "secretName": "MyDevOpsGitPAT"
    }
}
```

Required permissions on the Azure DevOps access token: **Code (read & write)**

## Access token

See [access token](./docs/Access%20token.md) for documentation on the access token syntax.