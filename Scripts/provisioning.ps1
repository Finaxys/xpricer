#.\xPricer.ps1 -Purge no -AzureRmResourceGroup xpricerresourcegroup -AzureRmStorageAccount xpricerstorageaccount -AzureRmBatchAccount xpricerbatchaccount -AzureStorageContainer xpricerstoragecontainer -NbrVM 4 -PoolName xpricerpool

param
    (
          [Parameter(Mandatory=$false)]  [String]$Purge ="no", 
          [Parameter(Mandatory=$false)]  [String]$AzureRmResourceGroup = "xpricerresourcegroup",
          [Parameter(Mandatory=$false)]  [String]$AzureRmStorageAccount = "xpricerstorageaccount",
          [Parameter(Mandatory=$false)]  [String]$AzureRmBatchAccount = "xpricerbatchaccount",
          [Parameter(mandatory=$false)]  [String]$AzureStorageContainer = "xpricerstoragecontainer",
          [Parameter(mandatory=$false)]  [int]$NbrVM ="4",
          [Parameter(mandatory=$false)]  [String]$PoolName="xpricerpool",
          [Parameter(mandatory=$false)]  [String]$template_json_file="c:\template.json",
          [Parameter(mandatory=$false)]  [String]$FakeMarketData="C:\Azure\FakeMarketData",
          [Parameter(mandatory=$false)]  [String]$settings_file="c:\Settings.settings"

      )

# Get-Module PowerShellGet -list | Select-Object Name,Version,Path
# Install-Module AzureRM -Force

Login-AzureRmAccount
    
$global:ReturnsxPricerKeys = [System.Collections.ArrayList]@("")
       
if (($Purge -eq "yes") -or ($Purge -eq "YES") -or ($Purge -eq "Yes")) {    
    # Warning !!! All ressources will be delete
    #Get-AzureRmresourceGroup | Select ResourceGroupName | Remove-AzureRmResourceGroup -Force
    Remove-AzureRmResourceGroup -Name "$AzureRmResourceGroup" -Force        
}     

# create a resource Group
New-AzureRmResourceGroup –Name "$AzureRmResourceGroup" –Location “West Europe” 

# create storage account
New-AzureRmStorageAccount –ResourceGroup "$AzureRmResourceGroup" –StorageAccountName "$AzureRmStorageAccount" –Location "West Europe" –Type "Standard_LRS" 

# create batch service
New-AzureRmBatchAccount –AccountName "$AzureRmBatchAccount" –Location "West Europe" –ResourceGroupName "$AzureRmResourceGroup"

# keys taken from the new batch account created
$Account = Get-AzureRmBatchAccountKeys –AccountName "$AzureRmBatchAccount"
$PrimaryAccountKey = $Account.PrimaryAccountKey
$SecondaryAccountKey = $Account.SecondaryAccountKey 
$global:ReturnsxPricerKeys.Add("$PrimaryAccountKey")
$global:ReturnsxPricerKeys.Add("$SecondaryAccountKey")

$Key = $ReturnsxPricerKeys[1]

# variable for the service batch address URL
$ServiceBatchURL = Get-AzureRmBatchAccount –AccountName "$AzureRmBatchAccount"
$BatchURL = $ServiceBatchURL.TaskTenantUrl

# create pool and retire his url
$context = Get-AzureRmBatchAccountKeys -AccountName "$AzureRmBatchAccount"
$configuration = New-Object -TypeName "Microsoft.Azure.Commands.Batch.Models.PSCloudServiceConfiguration" -ArgumentList @(4,"*")
New-AzureBatchPool -Id "$PoolName" -VirtualMachineSize "Small" -CloudServiceConfiguration $configuration -AutoScaleFormula '$TargetDedicated=4;' -BatchContext $context

#$AzureStorageContainerContext = New-AzureStorageContext -StorageAccountName "$AzureRmStorageAccount" -StorageAccountKey $ReturnsxPricerKeys[1] 

$AzureStorageContainerContext = New-AzureStorageContext -ConnectionString "DefaultEndpointsProtocol=https;AccountName=$AzureRmStorageAccount;AccountKey=$Key;"
$AzureStorageContainerContext

New-AzureStorageContainer -Name $AzureStorageContainer -Context $AzureStorageContainerContext
Write-Host "Pool $PoolName has been created ..."

# Upload FakeMarketData files 
ls $FakeMarketData | Set-AzureStorageBlobContent –Container $AzureStorageContainer –ConcurrentTaskCount 16

# write all these information to a json file
$json = @"
{
   
}
"@

$jobj = ConvertFrom-Json -InputObject $json    
    
$jobj | add-member "AzureRmStorageAccount" "$AzureRmStorageAccount" -MemberType NoteProperty
$jobj | add-member "PrivateKey" "$Key" -MemberType NoteProperty
$jobj | add-member "AzureRmBatchAccount" "$AzureRmBatchAccount" -MemberType NoteProperty
$jobj | add-member "ServiceBatchURL" "$BatchURL" -MemberType NoteProperty
$jobj | add-member "PoolName" "$PoolName" -MemberType NoteProperty
$jobj | add-member "AzureStorageContainer" "$AzureStorageContainer" -MemberType NoteProperty
   
ConvertTo-Json $jobj | Out-File $template_json_file
(Get-Content -path "$template_json_file" -Encoding Unicode) | Set-Content -Encoding "Default" -Path "$template_json_file"

# write all these information to a json file

$settings = @"
{
<?xml version='1.0' encoding='utf-8'?>
<SettingsFile xmlns="http://schemas.microsoft.com/VisualStudio/2004/01/settings" CurrentProfile="(Default)" GeneratedClassNamespace="XPricer.Scheduler" GeneratedClassName="Settings">
  <Profiles />
  <Settings>
    <Setting Name="$BatchURL" Type="System.String" Scope="User">
      <Value Profile="(Default)">https://(YourAccount).(region).batch.azure.com</Value>
    </Setting>
    <Setting Name="$AzureRmBatchAccount" Type="System.String" Scope="User">
      <Value Profile="(Default)" />
    </Setting>
    <Setting Name="$Key" Type="System.String" Scope="User">
      <Value Profile="(Default)" />
    </Setting>
    <Setting Name="StorageServiceUrl" Type="System.String" Scope="User">
      <Value Profile="(Default)">core.windows.net</Value>
    </Setting>
    <Setting Name="$AzureRmStorageAccount" Type="System.String" Scope="User">
      <Value Profile="(Default)" />
    </Setting>
    <Setting Name="StorageAccountKey" Type="System.String" Scope="User">
      <Value Profile="(Default)" />
    </Setting>
    <Setting Name="$AzureStorageContainer" Type="System.String" Scope="User">
      <Value Profile="(Default)" />
    </Setting>
  </Settings>
</SettingsFile>
"@

$settings | Out-File $settings_file
(Get-Content -path "$settings_file" -Encoding Unicode) | Set-Content -Encoding "Default" -Path "$settings_file" 
