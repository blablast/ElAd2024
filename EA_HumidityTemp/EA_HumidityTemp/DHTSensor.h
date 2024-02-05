#ifndef DHTSensor_h
#define DHTSensor_h

#include "Grove_Temperature_And_Humidity_Sensor.h"

#define ERRORVALUE -1500  // Define a constant for error value

// Class representing a single DHT sensor
class DHTSensor {
private:
    DHT _sensor;                  // DHT object for interfacing with the sensor
    float _lastTemperature;       // Variable to store the last read temperature
    float _lastHumidity;          // Variable to store the last read humidity

public:
    // Constructor: initializes the sensor on a specific pin and type
    DHTSensor(int pin, int type) : _sensor(pin, type), _lastTemperature(ERRORVALUE), _lastHumidity(ERRORVALUE) {}

    // Initialize the sensor
    void begin() {
        _sensor.begin();
    }

    // Read data from the sensor
    void read() {
        float temp_vals[2] = {0};  // Array to store temperature and humidity values

        // Attempt to read temperature and humidity from the sensor
        if (!_sensor.readTempAndHumidity(temp_vals)) {
            _lastHumidity = temp_vals[0];   // Assign humidity value
            _lastTemperature = temp_vals[1]; // Assign temperature value
        } else {
            // If reading fails, set both values to ERRORVALUE
            _lastHumidity = ERRORVALUE;
            _lastTemperature = ERRORVALUE;
        }
    }

    // Get the last read temperature
    float getTemperature() const {
        return _lastTemperature;
    }

    // Get the last read humidity
    float getHumidity() const {
        return _lastHumidity;
    }
};

#endif // DHTSensor_h
