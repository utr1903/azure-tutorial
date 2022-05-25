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

# Influx DB
declare -A influxdb
influxdb["name"]="influxdb"
influxdb["namespace"]="influxdb"
influxdb["nodePoolName"]="timeseries"


# InputProcessor
declare -A iproc
iproc["name"]="iproc"
iproc["namespace"]="iproc"
iproc["nodePoolName"]="input"

### Build & Push

# InputProcessor
echo -e "\n--- Input Processor ---\n"
docker build \
    --tag "${DOCKERHUB_NAME}/${iproc[name]}" \
    ../../apps/InputProcessor/InputProcessor/.
docker push "${DOCKERHUB_NAME}/${iproc[name]}"
echo -e "\n------\n"

### Helm

# Ingress Controller
echo "Deploying Ingress Controller ..."

helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx && \
helm repo update; \
helm upgrade --install ingress-nginx ingress-nginx/ingress-nginx \
    --namespace nginx --create-namespace \
    --wait \
    --debug \
    --set controller.replicaCount=1 \
    --set controller.nodeSelector."kubernetes\.io/os"="linux" \
    --set controller.image.image="ingress-nginx/controller" \
    --set controller.image.tag="v1.1.1" \
    --set controller.image.digest="" \
    --set controller.service.externalTrafficPolicy=Local \
    --set controller.admissionWebhooks.patch.nodeSelector."kubernetes\.io/os"="linux" \
    --set controller.admissionWebhooks.patch.image.image="ingress-nginx/kube-webhook-certgen" \
    --set controller.admissionWebhooks.patch.image.tag="v1.1.1" \
    --set controller.admissionWebhooks.patch.image.digest="" \
    --set defaultBackend.nodeSelector."kubernetes\.io/os"="linux" \
    --set defaultBackend.image.image="defaultbackend-amd64" \
    --set defaultBackend.image.tag="1.5" \
    --set defaultBackend.image.digest=""

echo -e " -> Ingress Controller is successfully deployed.\n"

# Influx DB
echo "Deploying Influx DB ..."

helm upgrade ${influxdb[name]} \
  --install \
  --wait \
  --debug \
  --create-namespace \
  --namespace ${influxdb[namespace]} \
  --set dockerhubName=$DOCKERHUB_NAME \
  --set serviceBusConnectionString=$serviceBusConnectionString \
  --set serviceBusQueueName=$serviceBusQueueName \
  --set eventHubConnectionString=$eventHubConnectionString \
  --set eventHubName=$eventHubName \
  --set nodePoolName=${influxdb[nodePoolName]} \
  ../charts/influxdb

echo -e " -> InfluxDB is successfully deployed.\n"

# Input Processor
echo "Deploying Input Processor ..."

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
  --set nodePoolName=${iproc[nodePoolName]} \
  ../charts/InputProcessor

echo -e " -> Input Processor is successfully deployed.\n"