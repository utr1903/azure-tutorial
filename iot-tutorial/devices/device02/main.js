// Main script

// Import libraries.
const Protocol = require('azure-iot-device-mqtt').Mqtt;
const { Client, Message } = require('azure-iot-device');

// Set variables.
const deviceId = `device02`;

// IoT Hub
let client;
let sendInterval;

function disconnectHandler () {
    clearInterval(sendInterval);

    sendInterval = null;

    client.open().catch((err) => {
        console.error(err.message);
    });
}

function messageHandler (msg) {
    console.log('Id: ' + msg.messageId + ' Body: ' + msg.data);
    client.complete(msg, printResultFor('completed'));
}

function generateMessage () {

    const temperature = 20 + (Math.random() * 10); // range: [20, 30]

    const data = JSON.stringify({
        deviceId: deviceId,
        deviceName: deviceId,
        deviceValue: temperature,
        tags: {
            sensorType: "temperature",
            sensorUnit: "Celcius",
        }
    });

    const message = new Message(data);

    return message;
}

function errorHandler (err) {
    console.error(err.message);
}

function connectHandler () {
    console.log('Client connected');

    // Create a message and send it to the IoT Hub every two seconds
    if (!sendInterval) {
        sendInterval = setInterval(() => {
            const message = generateMessage();
            console.log('Sending message: ' + message.getData());
            client.sendEvent(message, printResultFor('send'));
        }, 1000);
    }
}

function printResultFor(op) {
    return function printResult(err, res) {
        if (err) console.log(op + ' error: ' + err.toString());
        if (res) console.log(op + ' status: ' + res.constructor.name);
    };
}

async function main() {

    // Get Service Bus credentials.
    const deviceConnectionString = "";

    client = Client.fromConnectionString(deviceConnectionString, Protocol);
    client.on('connect', connectHandler);
    client.on('error', errorHandler);
    client.on('disconnect', disconnectHandler);
    client.on('message', messageHandler);

    client.open()
        .catch(err => {
        console.error('Could not connect: ' + err.message);
    });
};

main();