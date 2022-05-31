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

### Terraform destroy

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
  -var diagnostics_event_hub_name=$projectEventHubName \
  -var diagnostics_event_hub_consumer_group_name=$diagnosticsEventHubConsumerGroupName \
  -out "./tfplan"

terraform -chdir=../terraform/01_platform apply --destroy tfplan
