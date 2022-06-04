package com.iot.tutorial.deviceservice.services.userservice.create;

import com.iot.tutorial.deviceservice.commons.BaseResponseDto;
import com.iot.tutorial.deviceservice.repositories.UserRepository;
import com.iot.tutorial.deviceservice.services.userservice.create.dtos.CreateUserRequestDto;
import com.iot.tutorial.deviceservice.services.userservice.create.dtos.CreateUserResponseDto;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.stereotype.Service;

import java.util.UUID;

@Service
public class CreateUserService {

    private final Logger logger = LoggerFactory.getLogger(CreateUserService.class);

    @Autowired
    private UserRepository userRepository;

    public ResponseEntity<BaseResponseDto<CreateUserResponseDto>> run(
        CreateUserRequestDto requestDto
    ) {
        var userData = new CreateUserResponseDto(
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
