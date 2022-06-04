package com.iot.tutorial.deviceservice.services.device.create.dtos;

import lombok.AllArgsConstructor;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

@Getter
@Setter
@NoArgsConstructor
@AllArgsConstructor
public class CreateDeviceRequestDto {

    private String name;
    private String description;
}
