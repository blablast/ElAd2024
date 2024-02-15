void setup() {
  // Start the serial communication with the PC
  Serial.begin(115200);

  // Start serial communication with the device on Serial2
  Serial1.begin(9600);
}

void loop() {
  // Read from PC and send to device
  if (Serial.available()) { // Check if data is available to read from the PC
    char dataFromPC = Serial.read(); // Read the incoming byte
    Serial.write("Sending to Serial2: ");
    Serial.write(dataFromPC);
    Serial1.write(dataFromPC); // Send that byte to the device
  }

  // Read from device and send to PC
  if (Serial1.available()) { // Check if data is available to read from the device
    char dataFromDevice = Serial1.read(); // Read the incoming byte
    byte asciiValue = dataFromDevice; // Cast the char to int to get the ASCII value

    Serial.print("Received character: ");
    Serial.print(dataFromDevice);
    Serial.print(" with ASCII value: ");
    Serial.println(asciiValue); // Print the ASCII value of the received character
  }
}