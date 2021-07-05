---
Title: "Azure DevOps: Replacing Releases with a Multi-Stage pipeline"
Lead: Example of how to replace your existing Azure DevOps releases with Multistage pipelines
Published: 08/02/2020
Tags:
- Azure DevOps
- Pipelines
---

Recently I've discovered that most of my "Release" definitions can be converted into a Multi-Stage pipeline, including having the ability to gate deployments. This provides the added benefit of having my deployment defined in the same yaml file as my build. Given that Microsoft now refer to "Release" definitions as "Classic", this suggests to me that they aren't going to get much love in the future.

My pipline yaml initally looked like this. In this examle the code being build & deployed is a very simple react.js site.
```yaml
trigger:
- master

pool:
  vmImage: 'ubuntu-latest'

steps:
- task: NodeTool@0
  inputs:
    versionSpec: '11.x'
  displayName: 'Install Node.js'
- script: npm install
  displayName: 'Restore dependancies'
- script: npm run build:prod
  displayName: 'Build site'
- task: CopyFiles@2
  displayName: 'Copy scripts to $(Build.ArtifactStagingDirectory)'
  inputs:
    SourceFolder: scripts
    Contents: '*.sh'
    TargetFolder: '$(Build.ArtifactStagingDirectory)/scripts'
- task: PublishPipelineArtifact@1
  displayName: Publish Built Site
  inputs:
    targetPath: 'public/'
    artifact: 'site'
- task: PublishPipelineArtifact@1
  displayName: Publish Scripts
  inputs:
    targetPath: 'scripts/'
    artifact: 'scripts'
```

This restored any dependancies, built the site and published it as a build artifact along with some scripts used to deploy the site.

The release pipeline that then deployed the built site was very simple. It ran the publish powershell script.

![Image of the Release Definition](/assets/images/replacing-releases-with-azure-devops-multistage-pipelines/release.png)

The first step is to refactor the existing build definition to be part of a stage, rather than a standalone build. To do this I wrapped the "pool" & "steps" in a new stage. Leaving the actual steps the same.

```yaml
stages:
- stage: 'Build'
  jobs:
  - job:
    pool:
      vmImage: 'ubuntu-latest'
    workspace:
      clean: 'all'
    steps:
    - task: NodeTool@0
      inputs:
        versionSpec: '11.x'
      displayName: 'Install Node.js'
    - script: npm install
      displayName: 'Restore dependancies'
    - script: npm run build:prod
      displayName: 'Build site'
    - task: CopyFiles@2
      displayName: 'Copy scripts to $(Build.ArtifactStagingDirectory)'
      inputs:
        SourceFolder: scripts
        Contents: '*.sh'
        TargetFolder: '$(Build.ArtifactStagingDirectory)/scripts'
    - task: PublishPipelineArtifact@1
      displayName: Publish Built Site
      inputs:
        targetPath: 'public/'
        artifact: 'site'
    - task: PublishPipelineArtifact@1
      displayName: Publish Scripts
      inputs:
        targetPath: 'scripts/'
        artifact: 'scripts'
```

Once I had given this a test, I moved on to including the powershell task as part of a "Deployment" stage. First off I created a new "Environment" in Azure DevOps, and created a new approval requirement.

![Image of the Environment with the approval requirement](/assets/images/replacing-releases-with-azure-devops-multistage-pipelines/approvals-and-checks.png)

Once this was done, I added another stage to the yaml. Specifying that the previous stage must have succeeded and linking the job to the environment that I created earlier. Finally I copied the yaml of the PowerShell script task into the new stage.

```yaml
- stage: 'DeployToGitHubPages'
  displayName: 'Deploy to GitHub Pages'
  dependsOn: 'Build'
  condition: 'succeeded()'
  variables:
  - group: 'GitHub Pages Deployment Variables'
  jobs:
  - deployment:
    pool:
      vmImage: 'ubuntu-latest'
    environment: 'Blog Github Pages'
    strategy:
      runOnce:
        deploy:
          steps:
            - task: PowerShell@2
              displayName: 'Run Publish Script'
              inputs:
                filePath: '$(Pipeline.Workspace)/scripts/PublishBlog.ps1'
                arguments: '-gitToken $(Git.Token)'
```

This results in one pipeline, one simplified view & no loss of functionality.

![End Result](/assets/images/replacing-releases-with-azure-devops-multistage-pipelines/end-result.png)

I hope this was informative, any questions please feel free to reach out.

Cheers,
Alex
