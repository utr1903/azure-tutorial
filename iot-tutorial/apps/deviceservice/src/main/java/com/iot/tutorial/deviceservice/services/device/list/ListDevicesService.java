package com.iot.tutorial.deviceservice.services.device.list;

import com.iot.tutorial.deviceservice.commons.dtos.BaseResponseDto;
import com.iot.tutorial.deviceservice.commons.exceptions.DevicesCouldNotBeRetrievedException;
import com.iot.tutorial.deviceservice.repositories.DeviceRepository;
import com.iot.tutorial.deviceservice.services.device.list.dto.ListDevicesRequestDto;
import com.iot.tutorial.deviceservice.services.device.list.dto.ListDevicesResponseDto;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.data.domain.PageRequest;
import org.springframework.data.domain.Pageable;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.stereotype.Service;

@Service
public class ListDevicesService {

    private final Logger logger = LoggerFactory.getLogger(ListDevicesService.class);

    @Autowired
    private DeviceRepository deviceRepository;

    public ResponseEntity<BaseResponseDto<ListDevicesResponseDto>> run(
        ListDevicesRequestDto requestDto
    ) {

        try {
            // Create pagination.
            var pagination = createPagination(requestDto);

            // Retrieve all devices.
            var devicesData = getAllDevices(pagination);

            return createSuccessfulResponse(devicesData);
        }
        catch (Exception e) {
            return createFailedResponse(e, HttpStatus.INTERNAL_SERVER_ERROR);
        }
    }

    private Pageable createPagination(
        ListDevicesRequestDto requestDto
    ) {
        return PageRequest.of(
            requestDto.getPage(),
            requestDto.getNumber()
        );
    }

    private ListDevicesResponseDto getAllDevices(
        Pageable pagination
    )
        throws DevicesCouldNotBeRetrievedException {

        try {
            logger.info("Retrieving all devices...");

            var allDevices = deviceRepository.findAll(pagination)
                .getContent();

            var devicesData = new ListDevicesResponseDto(
                allDevices
            );

            logger.info("All devices are successfully retrieved.");

            return devicesData;
        }
        catch (Exception e) {
            String message = "Devices could not be retrieved from the database.";
            logger.info(message);
            throw new DevicesCouldNotBeRetrievedException(message);
        }
    }

    private ResponseEntity<BaseResponseDto<ListDevicesResponseDto>>
        createSuccessfulResponse(
            ListDevicesResponseDto devicesData
    ) {

        var responseDto = new BaseResponseDto<>(
            "Devices are retrieved successfully.",
            devicesData
        );

        return new ResponseEntity<>(
            responseDto, HttpStatus.OK
        );
    }

    private ResponseEntity<BaseResponseDto<ListDevicesResponseDto>>
        createFailedResponse(
            Exception e,
            HttpStatus httpStatus
    ) {

        var responseDto = new BaseResponseDto<ListDevicesResponseDto>(
            e.getMessage(),
            null
        );

        return new ResponseEntity<>(
            responseDto, httpStatus
        );
    }
}
