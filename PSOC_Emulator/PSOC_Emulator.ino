#define ARRAY_SIZE 50

// #define LEDR 13
// #define LEDG 13
// #define LEDB 13


struct ValueRange {
  int value;
  int minRange;
  int maxRange;
};

bool idle = true;
unsigned long idleTimer = 0;
unsigned long idleStep = 500;


bool currentPolarityIsPlus = false;
int currentHV = 0;
int updateStep = 100;
int phase4HV;
unsigned long cycleTimer;
int currentPhase = 0;
unsigned long lastPhaseTimer;

ValueRange settings[ARRAY_SIZE];  // Array to store the integer values

void setup() {
  Serial.begin(921600);   // Initialize serial communication
  pinMode(LEDR, OUTPUT);  // Set red LED as output
  pinMode(LEDG, OUTPUT);  // Set green LED as output
  pinMode(LEDB, OUTPUT);  // Set blue LED as output
  InitializeValues();
  currentPolarityIsPlus = settings[28].value == 1;
  idleTimer = millis();
}

void loop() {
  processSerialCommands();
  if (currentPhase == 4) {
    managePhaseFour();
  }
  if (idle && millis() - idleTimer >= idleStep)
  {
      digitalWrite(LEDG, !digitalRead(LEDG));
      idleTimer = millis();
  }
}

void processSerialCommands() {
  if (Serial.available() > 0) {
    String command = Serial.readStringUntil('\n');
    digitalWrite(LEDB, LOW); // Turn green LED on
    delay(500);
    digitalWrite(LEDB, HIGH);
    digitalWrite(LEDG, HIGH);
    command.trim();
    command.toUpperCase();
    if (command.startsWith("SET")) handleSetCommand(command);
    else if (command.startsWith("GET")) handleGetCommand(command);
    else if (command.startsWith("PUL")) handlePulseCommand(command);
    else if (command == "PUS DRP") resetPulseDrop();
    else 
    {
      Serial.println("Error: Invalid command: ");
    }
  }
}

void resetPulseDrop() {
  currentHV = 0;
  currentPhase = 0;
  digitalWrite(LEDR, HIGH);
  digitalWrite(LEDG, LOW);
  idle = true;
  idleTimer = millis();
  Serial.println("OK PUS DRP");
}

void handleSetCommand(String& command) {
  // Splitting the command based on spaces
  int firstSpaceIndex = command.indexOf(' ');
  if (firstSpaceIndex == -1) {
    Serial.println("Error: Invalid SET command format");
    return;
  }
  int secondSpaceIndex = command.indexOf(' ', firstSpaceIndex + 1);
  if (secondSpaceIndex == -1) {
    Serial.println("Error: Invalid SET command format");
    return;
  }

  // Parsing parameter index and value
  int paramIndex = command.substring(firstSpaceIndex + 1, secondSpaceIndex).toInt();
  int paramValue = command.substring(secondSpaceIndex + 1).toInt();

  // Validating parameter index
  if (paramIndex < 0 || paramIndex >= ARRAY_SIZE) {
    Serial.println("Error: Index out of bounds");
    return;
  }

  // Checking if the value is within the allowed range
  if (paramValue < settings[paramIndex].minRange || paramValue > settings[paramIndex].maxRange) {
    Serial.println("Error: Value out of range");
    return;
  }

  // Setting the parameter value
  settings[paramIndex].value = paramValue;
  replyGetSet(paramIndex, false);
}

void handleGetCommand(String& command) {
  // Finding the space index to separate the command from its parameter
  int spaceIndex = command.indexOf(' ');
  if (spaceIndex == -1) {
    Serial.println("Error: Invalid GET command format");
    return;
  }

  // Extracting the parameter index from the command
  int paramIndex = command.substring(spaceIndex + 1).toInt();

  // Validating the parameter index
  if (paramIndex < 0 || paramIndex >= ARRAY_SIZE) {
    Serial.println("Error: Index out of bounds");
    return;
  }

  // Responding with the requested parameter value
  replyGetSet(paramIndex, true);
}


void managePhaseFour() {
  if (millis() - lastPhaseTimer >= updateStep) {
    currentHV = getFloatingHV(phase4HV, 0.03);  // Adjust to bring the offset into the correct range
    replyPul();
    lastPhaseTimer = millis();
    digitalWrite(LEDR, !digitalRead(LEDR));
  }
}

int getFloatingHV(int value, float offset) {
  return (1 + offset) * value - (int)random(0, (2 * offset * abs(value)) + 1);
}


void handlePulseCommand(String data) {
  int spaceIndex = data.indexOf(' ');
  if (data == "PUL ST+") {
    currentPolarityIsPlus = true;
  } else if (data == "PUL ST-") {
    currentPolarityIsPlus = false;
  } else if (data == "PUL ST*") {
    currentPolarityIsPlus = !currentPolarityIsPlus;
  } else {
    Serial.println("Error: Invalid GET command format");
    return;
  }

  int phase1TargetValueMultiplier = currentPolarityIsPlus ? 1 : -1;
  int phase3TargetValueMultiplier = currentPolarityIsPlus ? -1 : 1;
  idle = false;
  cycleTimer = millis();
  digitalWrite(LEDR, LOW);
  digitalWrite(LEDG, HIGH);
  lastPhaseTimer = millis();
  lastPhaseTimer = raiseExp(1, 4, 0, phase1TargetValueMultiplier * settings[1].value, false);
  lastPhaseTimer = raiseExp(2, 5, phase1TargetValueMultiplier * settings[1].value, 0, true);
  lastPhaseTimer = raiseExp(3, 6, 0, phase3TargetValueMultiplier * settings[2].value, false);

  preparePhaseFour();
}

void preparePhaseFour() {
  currentPhase = 4;
  phase4HV = (currentPolarityIsPlus ? -1 : 1) * settings[2].value;
}

unsigned long raiseExp(int newPhase, unsigned long timeRegister, int baseValue, int targetValue, bool invertExp) {
  currentPhase = newPhase;
  unsigned long startTime = millis();
  unsigned long raisingTime = calculateRandomRaisingTime(settings[timeRegister].value * 100);  // Adjust if needed
  unsigned long endTime = startTime + settings[timeRegister].value * 100;
  unsigned long stepTime = lastPhaseTimer;

  double k = log(abs(baseValue + targetValue)) / raisingTime;
  int multiplier = (invertExp ? baseValue : targetValue) < 0 ? -1 : 1;

  while (millis() <= endTime) {
    unsigned long currentTime = millis();
    if (currentTime - stepTime >= updateStep) {
      unsigned long elapsedTime = currentTime - startTime;
      if (currentTime <= startTime + raisingTime) {
        currentHV = invertExp ? multiplier * exp(k * (raisingTime - elapsedTime)) : baseValue + multiplier * exp(k * elapsedTime);
      } else {
        currentHV = getFloatingHV(targetValue, 0.03);
      }
      digitalWrite(LEDR, !digitalRead(LEDR));
      replyPul();
      stepTime = currentTime;
    }
  }
  return stepTime;
}

void replyPul() {
  Serial.print("A:");
  Serial.print(currentPhase);
  Serial.print(",");
  Serial.print((millis() - cycleTimer) / 100);
  Serial.print(",");
  Serial.print(currentHV);
  Serial.println();
}


unsigned long calculateRandomRaisingTime(int baseTimeInMilliseconds) {
  unsigned long minTime = baseTimeInMilliseconds * 0.5;
  unsigned long maxTime = baseTimeInMilliseconds * 1;
  return random(minTime, maxTime + 1);
}

void replyGetSet(int index, bool isGet) {
  if (index >= 0 && index < ARRAY_SIZE) {
    Serial.print("OK ");
    Serial.print(isGet ? "GET " : "SET ");
    Serial.print(index);
    Serial.print(" ");
    Serial.print(settings[index].value);
    if (isGet) {
      Serial.print(" <");
      Serial.print(settings[index].minRange);
      Serial.print(", ");
      Serial.print(settings[index].maxRange);
      Serial.print(">");
    }
    Serial.println();
  } else {
    Serial.println("Error: Index out of bounds");
  }
}

void InitializeValues() {
  // 0. Low Level Control Settings
  settings[0] = { 0, 0, 31 };

  // 1. Saturation Phase Voltage
  settings[1] = { 5000, 0, 15000 };

  // 2. Maintenance Phase Voltage
  settings[2] = { 5000, 0, 15000 };

  // 3. PWM Generator Frequency
  settings[3] = { 33005, 10000, 48000 };

  // 4. Saturation Phase Duration
  settings[4] = { 20, 0, 99 };

  // 5. Intermediate Phase Duration
  settings[5] = { 5, 0, 99 };

  // 6. Maintenance Phase Duration
  settings[6] = { 20, 0, 99 };

  // 7. Dead Time Transistor Control
  settings[7] = { 100, 20, 255 };

  // 8. HV Voltage Auto Regulation
  settings[8] = { 0, 0, 1 };

  // 9. Set Command Write Protect
  settings[9] = { 0, 0, 1 };

  // 10.

  // 11.

  // 12. Green Debug Messages Switch
  settings[12] = { 0, 0, 1 };

  // 13. PWM Signal Block"
  settings[13] = { 0, 0, 1 };

  // 14. HV Voltage Cutoff Overvoltage
  settings[8] = { 1, 0, 1 };

  // 15. Max HV Voltage Correction Upward
  settings[17] = { 2000, 0, 2000 };

  // 16. Max HV Voltage Correction Downward
  settings[16] = { 0, 0, 2000 };

  // 17. Single Regulation Step Size"
  settings[17] = { 250, 0, 2000 };

  // 18. Delay Before Phase I Regulation Activation"
  settings[18] = { 3, 0, 99 };

  // 19. Delay Before Phase III Regulation Activation"
  settings[19] = { 10, 0, 99 };

  // 20. Manual HV Output Control"
  settings[20] = { 0, 0, 3 };

  // 21. HV+ Transformer Ratio Correction"
  settings[21] = { 402, 0, 20000 };

  // 22. HV- Transformer Ratio Correction"
  settings[22] = { 603, 0, 20000 };

  // 23. HV+ Current Limit"
  settings[23] = { 1100, 0, 50000 };

  // 24. HV- Current Limit"
  settings[24] = { 1100, 0, 50000 };

  // 25. Low Voltage Power Supply Current Limit"
  settings[25] = { 2500, 0, 10000 };

  // 26. HV Current Limit Toggle"
  settings[26] = { 0, 0, 1 };

  // 27. LV Power Supply Current Limit Toggle"
  settings[27] = { 0, 0, 1 };

  // 28. Initial Polarity (Plus == true)"
  settings[28] = { 0, 0, 1 };
}