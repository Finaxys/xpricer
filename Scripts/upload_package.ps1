#.\upload_package.ps1 -AzureRmResourceGroup xpricerresourcegroup -AzureRmBatchAccount xpricerbatchaccount -ApplicationName xpricer -ApplicationVersion 1

param
    (
          [Parameter(Mandatory=$false)]  [String]$AzureRmResourceGroup = "xpricerresourcegroup",
          [Parameter(Mandatory=$false)]  [String]$AzureRmBatchAccount = "xpricerbatchaccount",
          [Parameter(Mandatory=$false)]  [String]$ApplicationName = "xpricer",		 
          [Parameter(Mandatory=$false)]  [String]$ApplicationVersion = "1"		  
    )

# Get-Module PowerShellGet -list | Select-Object Name,Version,Path
# Install-Module AzureRM -Force

Login-AzureRmAccount

$source = "..\XPricer.TaskRunner\bin\Debug"
$destination = "..\XPricer.TaskRunner\bin\xpricer.zip"

If(Test-path $destination) {Remove-item $destination}

Add-Type -assembly "system.io.compression.filesystem"
[io.compression.zipfile]::CreateFromDirectory($Source, $destination) 

New-AzureRmBatchApplicationPackage -AccountName "$AzureRmBatchAccount" -ResourceGroupName "$AzureRmResourceGroup" -ApplicationId "$ApplicationName" -ApplicationVersion "$ApplicationVersion" -Format zip -FilePath $destination