package com.iot.tutorial.deviceservice.controllers;

import com.iot.tutorial.deviceservice.commons.BaseResponseDto;
import com.iot.tutorial.deviceservice.services.userservice.create.CreateUserService;
import com.iot.tutorial.deviceservice.services.userservice.create.dtos.CreateUserRequestDto;
import com.iot.tutorial.deviceservice.services.userservice.create.dtos.CreateUserResponseDto;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("/api/v1/users")
public class UserController {

    @Autowired
    private CreateUserService createUserService;

    @GetMapping
    public String test() {
        return "OK";
    }

    @PostMapping
    public ResponseEntity<BaseResponseDto<CreateUserResponseDto>> createUser(
            @RequestBody CreateUserRequestDto requestDto
    ) {
        return createUserService.run(requestDto);
    }
}
