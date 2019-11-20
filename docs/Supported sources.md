# Supported sources

Currently these services are supported as (git) sources to be mirrored:

## Github User (public repositories)

For public repositories no access token is needed.

You do however need to have write permissions to the repositories (as the build mirror works by installing default webhooks into the repo to receive notifications on updates):

``` json
{
  "source": "https://github.com/<user name>"
}
```

## Azure DevOps Project (public repositories):

This feature only works if you have [made your project public](https://docs.microsoft.com/azure/devops/organizations/public/make-project-public) in which case all its repositories automatically become public can be mirrored at once.

``` json
{
  "source": "https://dev.azure.com/<account name>/<project name>"
}
```

## Azure DevOps Project (private repositories):

By default Azure DevOps projects are private. To use a private DevOps project as a mirror source you must provide an access token along with the url.

:warning: When cloning private repositories make sure the target is also private or you will expose your private repositories!

``` json
{
  "source": {
    "source": "https://dev.azure.com/<account name>/<project name>",
    "repositoriesToIgnore": [
      "optional",
      "names-of-repos-not-to-clone"
    ],
    "accessToken": {
      ..
    }
  }
}
```

Required permissions on the Azure DevOps access token: **Code (read)**

## Entire Azure DevOps organization:

You can also clone an entire Azure DevOps organization (all projects). To use a DevOps organization as a mirror source you must provide an access token along with the url.

:warning: When cloning private repositories make sure the target is also private or you will expose your private repositories!

``` json
{
  "source": {
    "source": "https://dev.azure.com/<account name>/*",
    "repositoriesToIgnore": [
      "optional",
      "names-of-repos-not-to-clone"
    ],
    "projectsToIgnore": [
      "optional",
      "names-of-projects-not-to-clone"
    ],
    "accessToken": {
      ..
    }
  }
}
```

Required permissions on the Azure DevOps access token: **Code (read), Project and Team (Read)**

**Note:** When using this source then the only type of valid target is another DevOps organization!

## Access token

See [access token](./docs/Access%20token.md) for documentation on the access token syntax.