#!/bin/bash

##################
### Apps Setup ###
##################

### Set parameters
project="iot"
locationLong="westeurope"
locationShort="euw"
platform="platform"
stageLong="dev"
stageShort="d"
instance="001"

### Set variables

# Azure
resourceGroupName="rg${project}${locationShort}${platform}${stageShort}${instance}"

serviceBusNamespaceName="sb${project}${locationShort}${platform}${stageShort}${instance}"
serviceBusQueueName="input"

eventHubNamespaceName="ehn${project}${locationShort}${platform}${stageShort}${instance}"
eventHubName="eh${project}${locationShort}${platform}${stageShort}${instance}"

aksName="aks${project}${locationShort}${platform}${stageShort}${instance}"

# InputProcessor
declare -A iproc
iproc["name"]="iproc"
iproc["namespace"]="iproc"

### Build & Push

# InputProcessor
echo -e "\n--- InputProcessor ---\n"
docker build \
    --tag "${DOCKERHUB_NAME}/${iproc[name]}" \
    ../../apps/InputProcessor/InputProcessor/.
docker push "${DOCKERHUB_NAME}/${iproc[name]}"
echo -e "\n------\n"

### Helm

serviceBusConnectionString=$(az servicebus namespace authorization-rule keys list \
  --resource-group $resourceGroupName \
  --namespace-name $serviceBusNamespaceName \
  --name "RootManageSharedAccessKey" \
  | jq .primaryConnectionString)

eventHubConnectionString=$(az eventhubs namespace authorization-rule keys list \
  --resource-group $resourceGroupName \
  --namespace-name $eventHubNamespaceName \
  --name "RootManageSharedAccessKey" \
  | jq .primaryConnectionString)

# InputProcessor
echo "Deploying InputProcessor ..."

helm upgrade ${iproc[name]} \
  --install \
  --wait \
  --debug \
  --create-namespace \
  --namespace ${iproc[namespace]} \
  --set dockerhubName=$DOCKERHUB_NAME \
  --set serviceBusConnectionString=$serviceBusConnectionString \
  --set serviceBusQueueName=$serviceBusQueueName \
  --set eventHubConnectionString=$eventHubConnectionString \
  --set eventHubName=$eventHubName \
  ../charts/InputProcessor
