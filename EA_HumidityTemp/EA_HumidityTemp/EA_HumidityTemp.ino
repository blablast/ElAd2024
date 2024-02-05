#include "DHTSensorManager.h"
#include "DHTSensor.h"

// Define the DHT type and pins
#define DHTTYPE DHT22   // DHT 22 (AM2302)
#define DHTPIN1 2
#define DHTPIN2 4
#define DHTPIN3 6
#define DHTPIN4 8

// Define the number of sensors
const int numSensors = 4;

bool sendAuto = false;
bool debug = false;

// Initialize DHT sensors
DHTSensor sensors[] = {
    DHTSensor(DHTPIN1, DHTTYPE),
    DHTSensor(DHTPIN2, DHTTYPE),
    DHTSensor(DHTPIN3, DHTTYPE),
    DHTSensor(DHTPIN4, DHTTYPE)
};

DHTSensorManager sensorManager(sensors, 4);

void setup() {
    Serial.begin(115200);
    // Initialize each sensor
    for (DHTSensor& sensor : sensors) {
        sensor.begin();
    }
}

void loop() {
    // Read data from each sensor
    for (int i = 0; i < numSensors; i++) {
        sensorManager.readSensor(i);
        checkAndRespondToSerial();
    }

    if (sendAuto) sensorManager.SendAuto();
    if (debug) sensorManager.Debug();
}

void checkAndRespondToSerial() {
    if (Serial.available() > 0) {
        char valueStr[20]; // Buffer for the converted float value
        String command = Serial.readStringUntil('\n');
        command.trim(); // Removes any whitespace or '\r' characters at the end
        bool success = false;

        if (command == "GET T") {
            success = formatFloatValue(sensorManager.getAverageTemperature(), valueStr);
        } else if (command == "GET H") {
            success = formatFloatValue(sensorManager.getAverageHumidity(), valueStr);
        } else if (command == "GET T,H") {
            success = formatCombinedValues(sensorManager.getAverageTemperature(), sensorManager.getAverageHumidity(), valueStr);
        } else if (command == "SEND AUTO ON") {
            debug = false;
            sendAuto = true;
            strcpy(valueStr, "EXECUTED");
            success = true;
        } else if (command == "SEND AUTO OFF") {
            sendAuto = false;
            strcpy(valueStr, "EXECUTED");
            success = true;
        } else if (command == "DEBUG ON") {
            sendAuto = false;
            debug = true;
            strcpy(valueStr, "EXECUTED");
            success = true;
        } else if (command == "DEBUG OFF") {
            debug = false;
            strcpy(valueStr, "EXECUTED");
            success = true;
        } else if (command == "IDENTITY") {
            strcpy(valueStr, "TH Sensors");
            success = true;
        } else {
            Serial.print("ERR 01 Unknown Command: ");
            Serial.println(command);
            return;
        }

        if (success) {
            Serial.print("OK ");
            Serial.print(command);
            Serial.print(" ");
            Serial.println(valueStr);
        } else {
            Serial.println("ERR 00 in reading temperature");
        }
    }
}

bool formatFloatValue(float value, char* buffer) {
    // Format a float value to a string with 2 decimal places
    if (value != ERRORVALUE) {
        dtostrf(value, 4, 2, buffer);
        return true;
    }
    return false;
}

bool formatCombinedValues(float temp, float humidity, char* buffer) {
    // Format combined temperature and humidity values
    if (temp != ERRORVALUE && humidity != ERRORVALUE) {
        char humidityStr[10];
        dtostrf(temp, 4, 2, buffer);
        dtostrf(humidity, 4, 2, humidityStr);
        strcat(buffer, ",");
        strcat(buffer, humidityStr);
        return true;
    }
    return false;
}