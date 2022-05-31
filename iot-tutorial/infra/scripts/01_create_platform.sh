#!/bin/bash

###################
### Infra Setup ###
###################

### Set parameters
project="iot"
locationLong="westeurope"
locationShort="euw"
stageLong="dev"
stageShort="d"
instance="003"

shared="shared"
platform="platform"
diagnostics="diagnostics"

### Set variables

# Shared
sharedResourceGroupName="rg${project}${locationShort}${shared}x000"
sharedStorageAccountName="st${project}${locationShort}${shared}x000"

# Platform
projectResourceGroupName="rg${project}${locationShort}${platform}${stageShort}${instance}"

projectIotHubName="iot${project}${locationShort}${platform}${stageShort}${instance}"

projectServiceBusNamespaceName="sb${project}${locationShort}${platform}${stageShort}${instance}"
projectServiceBusQueueNameInput="${projectIotHubName}input"

projectEventHubNamespaceName="ehn${project}${locationShort}${platform}${stageShort}${instance}"
projectEventHubName="eh${project}${locationShort}${platform}${stageShort}${instance}"
projectEventHubConsumerGroupNameTsi="tsi"
projectEventHubConsumerGroupNameStats="statsprocessor"

diagnosticsEventHubName="eh${project}${locationShort}${diagnostics}${stageShort}${instance}"
diagnosticsEventHubConsumerGroupName="diagnostics"

projectStorageAccountName="st${project}${locationShort}${platform}${stageShort}${instance}"
projectBlobContainerNameStats="statsprocessor"
projectBlobContainerNameDiags="diagsprocessor"

projectTimeseriesInsightsName="tsi${project}${locationShort}${platform}${stageShort}${instance}"

projectAksName="aks${project}${locationShort}${platform}${stageShort}${instance}"
projectAksNodepoolName="rgaks${project}${locationShort}${platform}${stageShort}${instance}"

### Shared Terraform storage account

# Resource group
echo "Checking shared resource group [${sharedResourceGroupName}]..."
sharedResourceGroup=$(az group show \
  --name $sharedResourceGroupName \
  2> /dev/null)

if [[ $sharedResourceGroup == "" ]]; then
  echo " -> Shared resource group does not exist. Creating..."

  sharedResourceGroup=$(az group create \
    --name $sharedResourceGroupName \
    --location $locationLong)

  echo -e " -> Shared resource group is created successfully.\n"
else
  echo -e " -> Shared resource group already exists.\n"
fi

# Storage account
echo "Checking shared storage account [${sharedStorageAccountName}]..."
sharedStorageAccount=$(az storage account show \
    --resource-group $sharedResourceGroupName \
    --name $sharedStorageAccountName \
  2> /dev/null)

if [[ $sharedStorageAccount == "" ]]; then
  echo " -> Shared storage account does not exist. Creating..."

  sharedStorageAccount=$(az storage account create \
    --resource-group $sharedResourceGroupName \
    --name $sharedStorageAccountName \
    --sku "Standard_LRS" \
    --encryption-services "blob")

  echo -e " -> Shared storage account is created successfully.\n"
else
  echo -e " -> Shared storage account already exists.\n"
fi

# Terraform blob container
echo "Checking Terraform blob container [${project}]..."
terraformBlobContainer=$(az storage container show \
  --account-name $sharedStorageAccountName \
  --name $project \
  2> /dev/null)

if [[ $terraformBlobContainer == "" ]]; then
  echo " -> Terraform blob container does not exist. Creating..."

  terraformBlobContainer=$(az storage container create \
    --account-name $sharedStorageAccountName \
    --name $project \
    2> /dev/null)

  echo -e " -> Terraform blob container is created successfully.\n"
else
  echo -e " -> Terraform blob container already exists.\n"
fi

### Project Terraform deployment
azureAccount=$(az account show)
tenantId=$(echo $azureAccount | jq .tenantId)
subscriptionId=$(echo $azureAccount | jq .id)

echo -e 'tenant_id='"${tenantId}"'
subscription_id='"${subscriptionId}"'
resource_group_name=''"'${sharedResourceGroupName}'"''
storage_account_name=''"'${sharedStorageAccountName}'"''
container_name=''"'${project}'"''
key=''"'${stageShort}${instance}.tfstate'"''' \
> ../terraform/01_platform/backend.config

terraform -chdir=../terraform/01_platform init --backend-config="./backend.config"

terraform -chdir=../terraform/01_platform plan \
  -var project=$project \
  -var location_long=$locationLong \
  -var location_short=$locationShort \
  -var stage_short=$stageShort \
  -var stage_long=$stageLong \
  -var instance=$instance \
  -var project_resource_group_name=$projectResourceGroupName \
  -var project_service_bus_namespace_name=$projectServiceBusNamespaceName \
  -var project_service_bus_queue_name_input=$projectServiceBusQueueNameInput \
  -var project_event_hub_namespace_name=$projectEventHubNamespaceName \
  -var project_event_hub_name=$projectEventHubName \
  -var project_event_hub_consumer_group_name_tsi=$projectEventHubConsumerGroupNameTsi \
  -var project_event_hub_consumer_group_name_stats=$projectEventHubConsumerGroupNameStats \
  -var project_iot_hub_name=$projectIotHubName \
  -var project_storage_account_name=$projectStorageAccountName \
  -var project_blob_container_name_stats=$projectBlobContainerNameStats \
  -var project_blob_container_name_diags=$projectBlobContainerNameDiags \
  -var project_timeseries_insight_name=$projectTimeseriesInsightsName \
  -var project_kubernetes_cluster_name=$projectAksName \
  -var project_kubernetes_cluster_nodepool_name=$projectAksNodepoolName \
  -var diagnostics_event_hub_name=$diagnosticsEventHubName \
  -var diagnostics_event_hub_consumer_group_name=$diagnosticsEventHubConsumerGroupName \
  -out "./tfplan"

terraform -chdir=../terraform/01_platform apply tfplan

# Get AKS credentials
az aks get-credentials \
    --resource-group $projectResourceGroupName \
    --name $projectAksName \
    --overwrite-existing
