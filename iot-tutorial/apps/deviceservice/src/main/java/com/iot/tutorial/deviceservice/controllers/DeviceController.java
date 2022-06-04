package com.iot.tutorial.deviceservice.controllers;

import com.iot.tutorial.deviceservice.commons.BaseResponseDto;
import com.iot.tutorial.deviceservice.services.device.create.CreateDeviceService;
import com.iot.tutorial.deviceservice.services.device.create.dtos.CreateDeviceRequestDto;
import com.iot.tutorial.deviceservice.services.device.create.dtos.CreateDeviceResponseDto;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("/api/v1/users")
public class DeviceController {

    @Autowired
    private CreateDeviceService createDeviceService;

    @GetMapping
    public String test() {
        return "OK";
    }

    @PostMapping
    public ResponseEntity<BaseResponseDto<CreateDeviceResponseDto>> createDevice(
            @RequestBody CreateDeviceRequestDto requestDto
    ) {
        return createDeviceService.run(requestDto);
    }
}
