package com.iot.tutorial.deviceservice.services.device.create;

import com.iot.tutorial.deviceservice.commons.dtos.BaseResponseDto;
import com.iot.tutorial.deviceservice.commons.exceptions.BaseException;
import com.iot.tutorial.deviceservice.commons.exceptions.DeviceWithNameAlreadyExistsException;
import com.iot.tutorial.deviceservice.commons.exceptions.SavingToDatabaseFailedException;
import com.iot.tutorial.deviceservice.entities.Device;
import com.iot.tutorial.deviceservice.repositories.DeviceRepository;
import com.iot.tutorial.deviceservice.services.device.create.dtos.CreateDeviceRequestDto;
import com.iot.tutorial.deviceservice.services.device.create.dtos.CreateDeviceResponseDto;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.stereotype.Service;

import java.util.UUID;

@Service
public class CreateDeviceService {

    private final Logger logger = LoggerFactory.getLogger(CreateDeviceService.class);

    @Autowired
    private DeviceRepository deviceRepository;

    public ResponseEntity<BaseResponseDto<CreateDeviceResponseDto>> run(
        CreateDeviceRequestDto requestDto
    ) {

        try {
            // Check if device with given name exists.
            checkIfDeviceNameExists(requestDto.getName());

            // Create device data.
            var deviceData = createDeviceData(requestDto);

            // Save device into DB.
            saveDevice(deviceData);

            return createCreatedResponse(deviceData);
        }
        catch (DeviceWithNameAlreadyExistsException e) {
            return createFailedResponse(e, HttpStatus.BAD_REQUEST);
        }
        catch (SavingToDatabaseFailedException e) {
            return createFailedResponse(e, HttpStatus.INTERNAL_SERVER_ERROR);
        }
    }

    private void checkIfDeviceNameExists(
        String deviceName
    ) throws DeviceWithNameAlreadyExistsException {

        logger.info("Checking if a device with the name [" +
            deviceName + "] already exists...");

        var device = deviceRepository.findByName(deviceName);
        if (device != null) {

            logger.error("Device with name [" + deviceName +
                "] already exists.");

            throw new DeviceWithNameAlreadyExistsException(
                "Device with name [" + deviceName + "] already exists."
            );
        }

        logger.info("No device exists with the name [" +
                deviceName + "]...");
    }

    private CreateDeviceResponseDto createDeviceData(
            CreateDeviceRequestDto requestDto
    ) {
        return new CreateDeviceResponseDto(
            UUID.randomUUID(),
            requestDto.getName(),
            requestDto.getDescription()
        );
    }

    private void saveDevice(
        CreateDeviceResponseDto deviceData
    ) throws SavingToDatabaseFailedException {

        logger.info("Saving device " + deviceData.toString() + " into database...");

        try {
            var device = new Device(
                deviceData.getId(),
                deviceData.getName(),
                deviceData.getDescription()
            );

            deviceRepository.save(device);

            logger.info("Device " + device + "is successfully saved" +
                " into database.");
        }
        catch (Exception e)
        {
            String message = "Device " + deviceData +
                " could not be saved into the database!";

            logger.error(message);

            throw new SavingToDatabaseFailedException(
                message
            );
        }
    }

    private ResponseEntity<BaseResponseDto<CreateDeviceResponseDto>>
        createCreatedResponse(
            CreateDeviceResponseDto deviceData
    ) {

        var responseDto = new BaseResponseDto<>(
            "Device is created successfully.",
            deviceData
        );

        return new ResponseEntity<>(
            responseDto, HttpStatus.CREATED
        );
    }

    private ResponseEntity<BaseResponseDto<CreateDeviceResponseDto>>
        createFailedResponse(
            BaseException e,
            HttpStatus httpStatus
    ) {

        var responseDto = new BaseResponseDto<CreateDeviceResponseDto>(
            e.getMessage(),
            null
        );

        return new ResponseEntity<>(
            responseDto, httpStatus
        );
    }
}
