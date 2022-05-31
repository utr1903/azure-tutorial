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
instance="003"

### Set variables

# Azure
resourceGroupName="rg${project}${locationShort}${platform}${stageShort}${instance}"
iotHubName="iot${project}${locationShort}${platform}${stageShort}${instance}"

# Device01
declare -A device01
device01["id"]="device01"

# Device02
declare -A device02
device02["id"]="device02"

# Device03
declare -A device03
device03["id"]="device03"

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

# Device02
echo "Checking device [${device02[id]}]..."

device02=$(az iot hub device-identity show \
  --resource-group $resourceGroupName \
  --hub-name $iotHubName \
  --device-id ${device02[id]} \
  2> /dev/null)

if [[ $device02 == "" ]]; then
  echo " -> Device does not exist. Creating..."

  device02=$(az iot hub device-identity create \
    --resource-group $resourceGroupName \
    --hub-name $iotHubName \
    --device-id ${device02[id]})

  echo -e " -> Device is created successfully.\n"
else
  echo -e " -> Device already exists.\n"
fi

# Device03
echo "Checking device [${device03[id]}]..."

device03=$(az iot hub device-identity show \
  --resource-group $resourceGroupName \
  --hub-name $iotHubName \
  --device-id ${device03[id]} \
  2> /dev/null)

if [[ $device03 == "" ]]; then
  echo " -> Device does not exist. Creating..."

  device03=$(az iot hub device-identity create \
    --resource-group $resourceGroupName \
    --hub-name $iotHubName \
    --device-id ${device03[id]})

  echo -e " -> Device is created successfully.\n"
else
  echo -e " -> Device already exists.\n"
fi