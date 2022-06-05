package com.iot.tutorial.deviceservice.services.iot;

import com.iot.tutorial.deviceservice.commons.exceptions.DeviceAlreadyExistsInAzureException;
import com.iot.tutorial.deviceservice.commons.exceptions.DeviceCouldNotBeCreatedInAzureException;
import com.iot.tutorial.deviceservice.commons.exceptions.DeviceDoesNotExistInAzureException;
import com.microsoft.azure.sdk.iot.service.registry.Device;
import com.microsoft.azure.sdk.iot.service.registry.RegistryClient;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Service;

@Service
public class AzureIotHubHandler {

    private final Logger logger = LoggerFactory.getLogger(AzureIotHubHandler.class);

    private RegistryClient registryClient;

    public AzureIotHubHandler() {
        initializeRegistryClient();
    }

    private void initializeRegistryClient() {
        logger.info("Initializing Azure IoT Hub registry handler...");

        var iotHubConnectionString = System.getenv("IOT_HUB_CONNECTION_STRING");
        registryClient = new RegistryClient(iotHubConnectionString);

        logger.info("Azure IoT Hub registry handler is successfully initialized.");
    }

    public Device getDevice(
        String deviceId
    ) throws DeviceDoesNotExistInAzureException {
        try {
            logger.info("Retrieving device [" + deviceId + "]...");

            var device = registryClient.getDevice(deviceId);

            logger.info("Device [" + deviceId + "] is successfully retrieved.");

            return device;
        }
        catch (Exception e) {
            var message = "Device [" + deviceId + "] does " +
                "not exist in Azure.";

            logger.error(message);

            throw new DeviceDoesNotExistInAzureException(
                message
            );
        }
    }

    public Device createDevice(
        String deviceId
    ) throws DeviceCouldNotBeCreatedInAzureException,
            DeviceAlreadyExistsInAzureException {

        var deviceAlreadyExists = false;

        try {
            logger.info("Checking if device [" + deviceId + "] already" +
                " exists in Azure...");

            registryClient.getDevice(deviceId);

            // Mark "device exists" flag.
            deviceAlreadyExists = true;
        }
        catch (Exception e) {
            logger.info("Device [" + deviceId + "] does not" +
                " exist in Azure.");
        }

        // If device already exists, throw an exception.
        if (deviceAlreadyExists) {
            var message = "Device [" + deviceId + "] already" +
                " exists in Azure.";

            logger.error(message);
            throw new DeviceAlreadyExistsInAzureException(
                    message
            );
        }

        try {

            logger.info("Creating new device [" + deviceId + "] in Azure...");

            var device = registryClient.addDevice(new Device(deviceId));

            logger.info("New device [" + deviceId + "] is successfully created.");

            return device;
        }
        catch (Exception e)
        {
            throw new DeviceCouldNotBeCreatedInAzureException(
                "New device [" + deviceId + "] could not be created."
            );
        }
    }
}
