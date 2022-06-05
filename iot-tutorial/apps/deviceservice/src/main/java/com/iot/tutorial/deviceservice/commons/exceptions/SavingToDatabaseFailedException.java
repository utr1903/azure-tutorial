package com.iot.tutorial.deviceservice.commons.exceptions;

public class SavingToDatabaseFailedException extends BaseException {

    public SavingToDatabaseFailedException(
            String message
    )
    {
        super(message);
    }
}
