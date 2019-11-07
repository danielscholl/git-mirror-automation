## Examples

This example configuration will mirror all public repositories in my github account.

Each mirror target will then use its own access token to create a repository of the same name (in case of Gitlab, the repository will have the same description as Github and will use the set visibility (default: private)).

It uses Azure DevOps as the service to perform mirroring.

Specifically for every repository:
* the Azure Function will check if it is already mirrored by scanning existing builds
* if not yet mirrored, the build `[Github clone] MyBuild` will be cloned (and the source modified to be the new repository)
* the build will be named `buildNamePrefix + repository name`, so `[Github clone] ` + `<repo name>`
* because Azure DevOps needs authentication to let the Azure Function create a build an accessToken (in the form of a PAT stored in keyvault) is provided and will be used with every request

If a new repository is found and the build is created the repository will also be mirrored to all targets, in this case: Gitlab (as a public repository) and Azure DevOps.

For both these services access tokens are needed again to create the resources.

``` json
{
  "source": "https://github.com/MarcStan",
  "mirror-via": {
    "type": "https://dev.azure.com/marcstanlive/Opensource",
    "buildToClone": "[Github clone] MyBuild",
    "buildNamePrefix": "[Github clone] ",
    "accessToken": {
      "source": "https://mykeyvault.vault.azure.net",
      "secretName": "MyDevOpsBuildPAT"
    }
  },
  "mirror-to": [
    {
      "target": "https://gitlab.com/MarcStan",
      "accessToken": {
        "source": "https://mykeyvault.vault.azure.net",
        "secretName": "MyGitlabPAT"
      }
    },
    {
      "target": "https://dev.azure.com/marcstanlive/Opensource",
      "accessToken": {
        "source": "https://mykeyvault.vault.azure.net",
        "secretName": "MyDevOpsGitPAT"
      }
    }
  ]
}

```

See [working examples](./GitMirrorAutomation.Tests/WorkingExamples) for an additional set of working configurations.