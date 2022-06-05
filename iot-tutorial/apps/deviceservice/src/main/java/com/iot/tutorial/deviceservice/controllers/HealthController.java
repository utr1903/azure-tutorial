package com.iot.tutorial.deviceservice.controllers;

import org.slf4j.Logger;
import org.springframework.web.bind.annotation.GetMapping;
import org.slf4j.LoggerFactory;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

@RestController
@RequestMapping("deviceservice/api/v1/health")
public class HealthController {

    private final Logger logger = LoggerFactory.getLogger(HealthController.class);

    @GetMapping
    public String checkHealth() {
        logger.info("OK!");
        return "OK!";
    }
}
