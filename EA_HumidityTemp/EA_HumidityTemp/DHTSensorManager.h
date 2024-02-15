#ifndef DHTSensorManager_h
#define DHTSensorManager_h

#include "DHTSensor.h"

// Class to manage multiple DHT sensors
class DHTSensorManager {
private:
    DHTSensor* _sensors; // Pointer to an array of DHTSensor objects
    int _numSensors;     // Number of sensors being managed
    int _validReads;

    // Private method to calculate average temperature or humidity
    // Takes a boolean to determine whether to calculate for temperature or humidity
    float calculateAverage(bool isTemperature) {
        float sum = 0;
        int validValuesCount = 0;

        // Iterate through each sensor
        for (int i = 0; i < _numSensors; ++i) {
            // Get the value based on the isTemperature flag
            float value = isTemperature ? _sensors[i].getTemperature() : _sensors[i].getHumidity();

            // Only include valid readings in the average
            if (value != ERRORVALUE) {
                sum += value;
                validValuesCount++;
            }
        }
        _validReads = validValuesCount;
        // Calculate the average; return ERRORVALUE if no valid readings were found
        return validValuesCount > 0 ? sum / validValuesCount : ERRORVALUE;
    }

public:
    // Constructor
    // Takes a pointer to an array of DHTSensor objects and the number of sensors
    DHTSensorManager(DHTSensor* sensors, int numSensors) : _sensors(sensors), _numSensors(numSensors) {}

    // Method to read values from all sensors
    void readAllSensors() {
        for (int i = 0; i < _numSensors; i++) {
            _sensors[i].read();
        }
    }

    // Method to read a single sensor
    // Takes an index and returns true if the read was successful
    bool readSensor(byte i) {
        if (i >= 0 && i < _numSensors) {
            _sensors[i].read();
            return true;
        }
        return false;
    }

    // Method to get the average temperature from all sensors
    float getAverageTemperature() {
        return calculateAverage(true);
    }

    // Method to get the average humidity from all sensors
    float getAverageHumidity() {
        return calculateAverage(false);
    }

  // Prints all sensor humidity values and the average humidity
    void printHumidityReadings() {
        Serial.print("Humidity: ");
        for (int i = 0; i < _numSensors; i++) {
            _sensors[i].getHumidity() == ERRORVALUE ? Serial.print("--.--") : Serial.print(_sensors[i].getHumidity());
            Serial.print("%, ");
        }
        float avgHumidity = getAverageHumidity();
        Serial.print("AVG: ");
        avgHumidity != ERRORVALUE ? Serial.print(avgHumidity) : Serial.print("Error in reading");
        Serial.print("%, ");
    }

    // Prints all sensor temperature values and the average temperature
    void printTemperatureReadings() {
        Serial.print("\tTemperature: ");
        for (int i = 0; i < _numSensors; i++) {
            _sensors[i].getTemperature() == ERRORVALUE ? Serial.print("--.--") : Serial.print(_sensors[i].getTemperature());
            Serial.print(" *C, ");
        }
        float avgTemperature = getAverageTemperature();
        Serial.print("AVG: ");
        avgTemperature != ERRORVALUE ? Serial.print(avgTemperature) : Serial.print("Error in reading");
        Serial.println(" *C");
    }

    // Prints 2 floats in auto mode: temperature,humidity or Err
    void SendAuto() {
        Serial.print("A,");
        float avgTemperature = getAverageTemperature();
        avgTemperature != ERRORVALUE ? Serial.print(avgTemperature) : Serial.print("Err");
        Serial.print(',');
        float avgHumidity = getAverageHumidity();
        avgHumidity != ERRORVALUE ? Serial.print(avgHumidity) : Serial.print("Err");
        Serial.print(',');
        Serial.print(_validReads);
        Serial.println();
    }

    // Main debug function to print all sensor data
    void Debug() {
        printHumidityReadings();
        printTemperatureReadings();
    }

};


#endif // DHTSensorManager_h
