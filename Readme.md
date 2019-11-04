# Git mirror automation

Automate the mirroring of public git repositories across multiple services (such as github, gitlab & Azure DevOps).

[![GitMirrorAutomation](https://dev.azure.com/marcstanlive/Opensource/_apis/build/status/48)](https://dev.azure.com/marcstanlive/Opensource/_build/definition?definitionId=44)

# Motivation

A long time ago I wrote about [mirroring github, gitlab and Azure DevOps repositories](https://marcstan.net/blog/2018/08/31/Mirror-github-gitlab-and-VSTS-repositories/).

I still use this workflow (with slight improvements) to mirror the repositories but it still requires a set of manual steps that I decided to [automate](https://xkcd.com/1319/) everything with this azure function.

# Manual steps this project solves

While using the automation steps described in my blog post there are still manual steps left after creating a repository in github:

* Sign into and create the same repository in Gitlab
* Sign into and create the same repository in Azure DevOps
* Create a new build in Azure DevOps (by cloning existing)

# How it works

The azure function runs on a schedule and scans the source for any new repositories of a user (github webhooks are not available on the user level).

If matched it then calls the APIs for Azure DevOps/Gitlab to create the necessary repository/build pipeline.

Once the build pipeline exists it is triggered on every push as well as a schedule to mirror the repository.

(I use Azure DevOps builds instead of github actions because Azure DevOps allows secret sharing across pipelines).

# Example config

This example would mirror all github repos using Azure DevOps Pipelines (one per repository) each being a clone of the existing build:
``` json
{
    "source": "https://github.com/MarcStan",
    "mirror-via": {
        "target": "https://dev.azure.com/marcstanlive/Opensource",
        "buildNameToClone": "Github clones\\[Github clone] GitMirrorAutomation"
    }
}
```

## Setting up the build pipeline

The build is responsible for the actual cloning. Here's the instructions to create a brand new build:

In Azure DevOps create a new build/pipeline. Don't use YAML, pick the old "classic editor".

Connect it to Github as the source (if you don't have a github connection set up yet, this is the time to do it).

Pick one of your repositories as the source (all others will be cloned when the automation duplicates the builds).

Add a powershell step to the build and paste this script:
``` powershell
# auto fetch from github based on source
# github always sends username/repoName
$repoName = "$env:BUILD_REPOSITORY_NAME".split('/')[1]

# suppress git warnings as they produce errors in powershell
# https://github.com/dahlbyk/posh-git/issues/109
$env:GIT_REDIRECT_STDERR = '2>&1'

function Mirror-To($target) {
  # http://blog.plataformatec.com.br/2013/05/how-to-properly-mirror-a-git-repository/
  Write-Output "Mirroring to $target"
  git push --prune $target +refs/remotes/origin/*:refs/heads/* +refs/tags/*:refs/tags/*
  if ($lastexitcode -ne 0) { 
    throw "Failed to mirror"
  }
}

# ensure we have the absolute latest without any stale branches
git fetch --prune

# push to these targets
$devOpsAccount = "<devops account name>"
$devOpsProject = "<project name in your devops account>"
$gitlabUser = "<your gitlab username>"

Mirror-To "https://$(DevOpsPAT)@dev.azure.com/$devOpsAccount/$devOpsProject/_git/$repoName"
Mirror-To ("https://$(GitlabPAT)@gitlab.com//$repoName")

```
Configure the variables at the end of the script (and adjust the targets if you have other git repos where you want to push to). Note that each url requires the PAT in the url as I haven't implemented header authentication yet.

Note the `$(DevOpsPAT)` and `$(GitlabPAT)` variables in the urls. These are [Azure DevOps variables](https://docs.microsoft.com/azure/devops/pipelines/process/variables) and we will inject them from a variable group now.

The variable group allows you to share variables across many builds without duplicating them. That way whenever the PAT expires and needs to be renewed you will only have to update it once.

In the build click on Variables -> Variable groups and click "manage variable groups".

Create a new variable group, give it a name such as "Git mirror secrets" and add variables `DevOpsPAT` and `GitlabPAT`.

Fill them with your private access tokens that you create in the respective services (note that they must have git push permissions) and be sure to **mark them as secret**. This will prevent them from showing up in cleartext in the logs!

Now that the group is created, link it into the build.

We only have one more tab to complete in the build: Triggers.

The default trigger is to run the build for every master build of the github repository.

Enter "\*" in the branch specification to tell it to run the build for every pushed change (not just the master branch).

(Optionally) set up a scheduled build to run either daily or weekly. Some changes in the repository are not detected (such as when you push tags without pushing commits). This scheduled build ensures that they are copied over eventually.

Now save the build with a name like "[Github clone] \<repo name>".

**Pro tip:** There is one further change I recommend you make to the build: Right click the powershell task and create a taskgroup from it. In the task group then configure the variables like this:

![parameters](screenshots/parameters.png)

The big advantage is that you will now have one source for the script (which all cloned builds will reference). If you ever need to make a change to the script it will be in only one place as opposed to all builds.