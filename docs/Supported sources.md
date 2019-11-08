# Supported sources

Currently these services are supported as (git) sources to be mirrored:

## Github User (public repositories only)

Currently only public repositories are supported as a source:

``` json
{
  "source": "https://github.com/<user name>"
}
```

## Azure DevOps Project (public repositories only):

This feature only works if you have [made your project public](https://docs.microsoft.com/azure/devops/organizations/public/make-project-public) in which case all its repositories automatically become public can be mirrored at once.

``` json
{
  "source": "https://dev.azure.com/<account name>/<project name>"
}
```

## Azure DevOps Project (private repositories):

By default Azure DevOps projects are private. To use a private DevOps project as a mirror source you must provide an access token along with the url.

``` json
{
  "source": {
    "source": "https://dev.azure.com/<account name>/<project name>",
    "accessToken": {
      ..
    }
  }
}
```

Required permissions on the Azure DevOps access token: **Code (read)**

## Entire Azure DevOps organization:

You can also clone an entire Azure DevOps organization (all projects). To use a DevOps organization as a mirror source you must provide an access token along with the url.

``` json
{
  "source": {
    "source": "https://dev.azure.com/<account name>/*",
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