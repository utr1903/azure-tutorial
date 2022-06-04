package com.iot.tutorial.deviceservice.services.device.list.dto;

import lombok.*;

@Getter
@Setter
@NoArgsConstructor
@AllArgsConstructor
public class ListDevicesRequestDto {

    private int page = 0;
    private int number = 20;
}
