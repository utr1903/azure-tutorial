package com.iot.tutorial.deviceservice.controllers;

import com.iot.tutorial.deviceservice.commons.dtos.BaseResponseDto;
import com.iot.tutorial.deviceservice.services.device.create.CreateDeviceService;
import com.iot.tutorial.deviceservice.services.device.create.dtos.CreateDeviceRequestDto;
import com.iot.tutorial.deviceservice.services.device.create.dtos.CreateDeviceResponseDto;
import com.iot.tutorial.deviceservice.services.device.list.ListDevicesService;
import com.iot.tutorial.deviceservice.services.device.list.dto.ListDevicesRequestDto;
import com.iot.tutorial.deviceservice.services.device.list.dto.ListDevicesResponseDto;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("deviceservice/api/v1/devices")
public class DeviceController {

    @Autowired
    private CreateDeviceService createDeviceService;

    @Autowired
    private ListDevicesService listDevicesService;

    @PostMapping("create")
    public ResponseEntity<BaseResponseDto<CreateDeviceResponseDto>> createDevice(
        @RequestBody CreateDeviceRequestDto requestDto
    ) {
        return createDeviceService.run(requestDto);
    }

    @GetMapping("list")
    public ResponseEntity<BaseResponseDto<ListDevicesResponseDto>> createDevice(
        ListDevicesRequestDto requestDto
    ) {
        return listDevicesService.run(requestDto);
    }
}
