package com.iot.tutorial.deviceservice.services.device.create.dtos;

import lombok.AllArgsConstructor;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

import java.util.UUID;

@Getter
@Setter
@NoArgsConstructor
@AllArgsConstructor
public class CreateDeviceResponseDto {

    private UUID id;
    private String email;
    private String firstName;
    private String lastName;
}
