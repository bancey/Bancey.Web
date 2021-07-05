---
Title: "Flows imported as part of a Dynamics 365 solution are not enabled? 2"
Published: 02/02/2020
Tags:
- Dynamics 365
- Power Automate (Flow)
- PowerShell
---

We recently ran into an issue where our Flows (now called Power Automate) that are being imported to our downstream Dynamics 365 environments are not enabled as part of the deployment, unlike traditional workflows. Having to manually enabled around 20 flows every morning was not something we wanted to do for very long. The solution that we've used to combat this is a PowerShell script that uses [preview cmdlets](https://docs.microsoft.com/en-gb/power-platform/admin/powerapps-powershell) to enable all of the flows in a particular environment.

Our PowerShell script is pretty crude but it does the trick;

```yaml
Set-PSRepository -Name "PSGallery" -InstallationPolicy Trusted
Install-Module -Name Microsoft.PowerApps.Administration.PowerShell

$password = ConvertTo-SecureString $Env:password -AsPlainText -Force
Add-PowerAppsAccount -Username $Env:username -Password $password

$flows = Get-AdminFlow -EnvironmentName $Env:FlowEnvironmentName
$count = $flows.Count;
$results = @();

Write-Output "Enabling $count flows...";
for ($index = 0; $index -lt $flows.Count; $index++) {
    $flowName = ($flows[$index] | Select-Object -ExpandProperty "FlowName");
    $result = Enable-AdminFlow -EnvironmentName $Env:FlowEnvironmentName -FlowName $flowName
    $results += $result;
}

$errorCount = 0;
$successCount = 0;

for ($resultsIndex = 0; $resultsIndex -lt $results.Count; $resultsIndex++) {
    $code = ($results[$resultsIndex] | Select-Object -ExpandProperty "Code");
    if ($code -eq 200) {
        $successCount++;
    } else {
        $errorCount++;
    }
}

if ($successCount -ne 0) {
    Write-Output "Successfully enabled $successCount flows...";
}

if ($errorCount -ne 0) {
    Write-Output "Failed to enable $errorCount flows...";
}
```

There are a couple of issues with this approach, this module requires a `PSCredential` to be passed to it. If you have MFA turned on, this won't work and you'll get an error similar to the below;

![MFA Error](/images/power-automate-enable-flows-post-deployment/mfa-error.png)

I've spent a very small amount of time trying to get over this, but have left it for now.

I hope this has been informative.

Cheers,
Alex