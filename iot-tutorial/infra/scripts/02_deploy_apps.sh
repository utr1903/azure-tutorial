#!/bin/bash

##################
### Apps Setup ###
##################

### Set parameters
project="iot"
locationLong="westeurope"
locationShort="euw"
stageLong="dev"
stageShort="d"
instance="001"

platform="platform"
stats="stats"

### Set variables

# Azure
resourceGroupName="rg${project}${locationShort}${platform}${stageShort}${instance}"

serviceBusNamespaceName="sb${project}${locationShort}${platform}${stageShort}${instance}"
serviceBusQueueName="input"

eventHubNamespaceName="ehn${project}${locationShort}${platform}${stageShort}${instance}"
eventHubName="eh${project}${locationShort}${platform}${stageShort}${instance}"

aksName="aks${project}${locationShort}${platform}${stageShort}${instance}"

statsFunctionAppName="func${project}${locationShort}${stats}${stageShort}${instance}"

# Influx DB
declare -A influxdb
influxdb["name"]="influxdb"
influxdb["namespace"]="influxdb"
influxdb["port"]="8086"
influxdb["nodePoolName"]="timeseries"
influxdb["org"]="myorg"
influxdb["bucket"]="mybucket"

# Grafana
declare -A grafana
grafana["name"]="grafana"
grafana["namespace"]="grafana"
grafana["port"]="3000"
grafana["nodePoolName"]="timeseries"

# Input Processor
declare -A iproc
iproc["name"]="iproc"
iproc["namespace"]="iproc"
iproc["port"]="80"
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

# Newrelic
echo "Deploying Newrelic ..."

kubectl apply -f https://download.newrelic.com/install/kubernetes/pixie/latest/px.dev_viziers.yaml && \
kubectl apply -f https://download.newrelic.com/install/kubernetes/pixie/latest/olm_crd.yaml && \
helm repo add newrelic https://helm-charts.newrelic.com && helm repo update && \
kubectl create namespace newrelic ; helm upgrade --install newrelic-bundle newrelic/nri-bundle \
    --wait \
    --debug \
    --set global.licenseKey=$NEWRELIC_LICENSE_KEY \
    --set global.cluster=$aksName \
    --namespace=newrelic \
    --set newrelic-infrastructure.privileged=true \
    --set global.lowDataMode=true \
    --set ksm.enabled=true \
    --set kubeEvents.enabled=true \
    --set prometheus.enabled=true \
    --set logging.enabled=true \
    --set newrelic-pixie.enabled=true \
    --set newrelic-pixie.apiKey=$PIXIE_API_KEY \
    --set pixie-chart.enabled=true \
    --set pixie-chart.deployKey=$PIXIE_DEPLOY_KEY \
    --set pixie-chart.clusterName=$aksName

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
  --set name=${influxdb[name]} \
  --set nodePoolName=${influxdb[nodePoolName]} \
  --set dockerhubName=$DOCKERHUB_NAME \
  --set ports.http=${influxdb[port]} \
  ../charts/influxdb

echo -e " -> Influx DB is successfully deployed.\n"

echo -e "Creating Influx DB admin..."

kubectl exec "${influxdb[name]}-0" \
  --namespace ${influxdb[namespace]} \
  -- influx setup \
  --username "admin" \
  --password "admin123" \
  --org ${influxdb[org]} \
  --bucket ${influxdb[bucket]} \
  --retention "1h" \
  --force

echo -e " -> Admin is successfully created.\n"

# Grafana
echo "Deploying Grafana ..."

# influxDbTokenForGrafana=$(kubectl exec "${influxdb[name]}-0" \
#   --namespace ${influxdb[namespace]} \
#   -- influx auth create \
#   --all-access --json \
#   | jq -r .token)

helm upgrade ${grafana[name]} \
  --install \
  --wait \
  --debug \
  --create-namespace \
  --namespace ${grafana[namespace]} \
  --set name=${grafana[name]} \
  --set nodePoolName=${grafana[nodePoolName]} \
  --set dockerhubName=$DOCKERHUB_NAME \
  --set port=${grafana[port]} \
  ../charts/grafana

echo -e " -> Grafana is successfully deployed.\n"

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
  --set name=${iproc[name]} \
  --set nodePoolName=${iproc[nodePoolName]} \
  --set dockerhubName=$DOCKERHUB_NAME \
  --set port=${iproc[port]} \
  --set serviceBusConnectionString=$serviceBusConnectionString \
  --set serviceBusQueueName=$serviceBusQueueName \
  --set eventHubConnectionString=$eventHubConnectionString \
  --set eventHubName=$eventHubName \
  --set influxdbServiceName=${influxdb[name]} \
  --set influxdbNamespace=${influxdb[namespace]} \
  --set influxdbPort=${influxdb[port]} \
  --set influxdbOrganization=${influxdb[org]} \
  --set influxdbBucket=${influxdb[bucket]} \
  ../charts/InputProcessor

echo -e " -> Input Processor is successfully deployed.\n"

# # Stats Processor
# echo "Deploying Stats Processor ..."

# # dotnet build \
# #   ../../apps/StatsProcessor/StatsProcessor/ \
# #   -o StatsProcessor.dll

# # zip -r StatsProcessor.zip \
# #   ../../apps/StatsProcessor/StatsProcessor/ \
# #   -x \
# #   "../../apps/StatsProcessor/StatsProcessor/bin/*" \
# #   "../../apps/StatsProcessor/StatsProcessor/obj/*" \
# #   "../../apps/StatsProcessor/StatsProcessor/.gitignore" \
# #   "../../apps/StatsProcessor/StatsProcessor/local.settings.json"

# az functionapp deployment source config-zip \
#   --resource-group $resourceGroupName \
#   --name $statsFunctionAppName \
#   --src "StatsProcessor.zip"

# echo -e " -> Stats Processor is successfully deployed.\n"
