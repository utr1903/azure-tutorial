package com.iot.tutorial.deviceservice.services.device.create;

import com.iot.tutorial.deviceservice.commons.BaseResponseDto;
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
    private DeviceRepository userRepository;

    public ResponseEntity<BaseResponseDto<CreateDeviceResponseDto>> run(
        CreateDeviceRequestDto requestDto
    ) {
        var userData = new CreateDeviceResponseDto(
            UUID.randomUUID(),
            requestDto.getEmail(),
            requestDto.getFirstName(),
            requestDto.getLastName()
        );

        var responseDto = new BaseResponseDto<>(
            "User is created successfully.",
            userData
        );

        var responseEntity = new ResponseEntity<>(
            responseDto, HttpStatus.CREATED
        );

        return responseEntity;
    }
}
