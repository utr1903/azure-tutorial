package com.iot.tutorial.deviceservice.commons.exceptions;

public class BaseException extends  Exception {

    protected String message;

    public BaseException(
        String message
    )
    {
        this.message = message;
    }

    public String getMessage()
    {
        return message;
    }
}
