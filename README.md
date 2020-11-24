# DevOpsArtifactDownloader
Tool to download the Latest Build Artifact from Azure DevOps.

## Why?
With this tool you can use other tools developed in Azure DevOps on a private Jenkins behind a VPN connection.

Use it to download your latest DevOps build artifacts through a Jenkins job. Which can be triggered on a successfull build by a webhook. The webhook can be sent to a public or private [smee.io](https://smee.io/) url which a local smee-client then passes to a jenkins server. Jenkins can trigger a job using the [Generic Webhook Trigger](https://plugins.jenkins.io/generic-webhook-trigger/) plugin. You could then make your downloaded DevOps build artifacts into local Jenkins artifacts, so other Jenkins jobs can use the latest DevOps artifacts.

This keeps your development and building of tools fully in Azure DevOps. While being still able to use them in a private Jekins behind a VPN connection.

Ofcourse it can also be deployed in other use-cases.

## Usage
Run via the commandline with arguments;

	-o, --organization    Required.
  	-t, --pat             Required.
  	-p, --project         Required.
  	-d, --definition      Required.
  	-b, --branch
  	-a, --artifact        Required.
  	-r, --result          (Default: Build.zip)
  	--help                Display this help screen.
  	--version             Display version information.