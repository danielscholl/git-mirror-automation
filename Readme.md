# Git mirror automation

Automate the mirroring of public repositories across multiple services (github, gitlab & Azure DevOps).

# Motivation

A long time ago I wrote about [Mirroring github, gitlab and Azure DevOps repositories](https://marcstan.net/blog/2018/08/31/Mirror-github-gitlab-and-VSTS-repositories/).

I still use this workflow (with slight improvements) to mirror the repositories but it still requires a set of manual steps that I decided to [automate](https://xkcd.com/1319/) with this azure function.

# Manual steps this project solves

After creating a repository in github I still have to manually:

* Sign into and create the same repository in Gitlab
* Sign into and create the same repository in Azure DevOps
* Create a new build in Azure DevOps
    * Have it use the github repository as the source
    * Add the task that mirrors the repository to gitlab and Azure DevOps

# How it works

The azure function runs on a schedule and scans the source for any new repositories of a user (github webhooks are not available on the user level).

If matched it then calls the APIs for Azure DevOps/Gitlab to create the necessary repository/build pipeline.

Once the build pipeline exists it is triggered on every push as well as a schedule to mirror the repository.

(I use Azure DevOps builds instead of github actions because Azure DevOps allows secret sharing across pipelines).

# Example config

This example would mirror all github repos (excluding forks) to gitlab and Azure DevOps using Azure DevOps builds (one per repository) each named "[Github clone]":
``` json
{
    "source": {
        "type": "github",
        "config": {
            "userName": "MarcStan",
            "mirrorForks": false
        }
    },
    "mirroring": {
        "type": "Azure DevOps",
        "config": {
            "name": "[Github clone] %repo",
            "variableGroups": [
                "Opensource Secrets"
            ]
        }
    },
    "targets": [
        {
            "type": "gitlab",
            "config": {
                "userName": "MarcStan",
                "accessTokenName": "GitlabPAT"
            }
        },
        {
            "type": "Azure DevOps",
            "config": {
                "accountName": "marcstanlive",
                "projectName": "Opensource",
                "accessTokenName": "AzureDevOpsPAT"
            }
        }
    ]
}
```