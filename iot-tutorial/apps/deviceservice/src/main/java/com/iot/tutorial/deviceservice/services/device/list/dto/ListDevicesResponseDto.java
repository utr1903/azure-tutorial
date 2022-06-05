package com.iot.tutorial.deviceservice.services.device.list.dto;

import com.iot.tutorial.deviceservice.entities.Device;
import lombok.AllArgsConstructor;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

import java.util.List;

@Getter
@Setter
@NoArgsConstructor
@AllArgsConstructor
public class ListDevicesResponseDto {

    private List<Device> devices;
}
