package com.iot.tutorial.deviceservice.services.userservice.create.dtos;

import lombok.AllArgsConstructor;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

@Getter
@Setter
@NoArgsConstructor
@AllArgsConstructor
public class CreateUserRequestDto {

    private String email;
    private String firstName;
    private String lastName;
}
