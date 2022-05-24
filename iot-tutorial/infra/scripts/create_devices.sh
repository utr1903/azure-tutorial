#!/bin/bash

####################
### Device Setup ###
####################

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
iotHubName="iot${project}${locationShort}${platform}${stageShort}${instance}"

# Device01
declare -A device01
device01["id"]="device01"

# Device01
echo "Checking device [${device01[id]}]..."

device01=$(az iot hub device-identity show \
  --resource-group $resourceGroupName \
  --hub-name $iotHubName \
  --device-id ${device01[id]} \
  2> /dev/null)

if [[ $device01 == "" ]]; then
  echo " -> Device does not exist. Creating..."

  device01=$(az iot hub device-identity create \
    --resource-group $resourceGroupName \
    --hub-name $iotHubName \
    --device-id ${device01[id]})

  echo -e " -> Device is created successfully.\n"
else
  echo -e " -> Device already exists.\n"
fi
