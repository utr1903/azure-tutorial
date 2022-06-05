package com.iot.tutorial.deviceservice.commons.exceptions;

public class DeviceWithNameAlreadyExistsException extends BaseException {

    public DeviceWithNameAlreadyExistsException(
        String message
    )
    {
        super(message);
    }
}
