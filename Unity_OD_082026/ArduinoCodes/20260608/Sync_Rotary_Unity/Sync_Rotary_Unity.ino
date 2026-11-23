// Arduino UNO
// Rotary encoder: D2 / D3
// Poisson TTL sync input: D4 digital input
// Lick input: A0 digital input
//
// Serial output to Unity:
//
// POS,arduinoTime,distance,speed,lastSyncID,syncState,lickState
// SYNC,syncID,arduinoSyncTime

const int pinA = 2;
const int pinB = 3;
const int syncPin = 4;
const int lickPin = A0;

// Encoder position
volatile long encoderPosition = 0;

// Sync state
volatile bool lastSyncState = false;
volatile unsigned int syncID = 0;

// Small buffer for UNO RAM
const byte SYNC_BUF_SIZE = 32;
volatile unsigned long syncTimes[SYNC_BUF_SIZE];
volatile unsigned int syncIDs[SYNC_BUF_SIZE];
volatile byte syncWriteIdx = 0;
volatile byte syncReadIdx = 0;

// Position / speed
unsigned long lastPosSendMicros = 0;
unsigned long lastSpeedMicros = 0;
float lastDistance = 0.0;

// CHANGE THIS TO YOUR REAL CALIBRATION
const float tickToDistance = 0.01;

// 60 Hz serial position output
const unsigned long posSendInterval = 16667;

void setup() {
  Serial.begin(115200);

  pinMode(pinA, INPUT_PULLUP);
  pinMode(pinB, INPUT_PULLUP);

  // External TTL sync input on D4
  pinMode(syncPin, INPUT);

  // Lick input on A0, used as analog
  pinMode(lickPin, INPUT);

  // Encoder interrupts
  attachInterrupt(digitalPinToInterrupt(pinA), readA, CHANGE);
  attachInterrupt(digitalPinToInterrupt(pinB), readB, CHANGE);

  // Pin-change interrupt for D4
  // D4 = PD4 = PCINT20 = PCINT2 group
  PCICR  |= (1 << PCIE2);
  PCMSK2 |= (1 << PCINT20);

  lastPosSendMicros = micros();
  lastSpeedMicros = micros();
}

void loop() {
  // Send sync events first
  sendSyncEvents();

  unsigned long now = micros();

  if (now - lastPosSendMicros >= posSendInterval) {
    lastPosSendMicros = now;

    long posCopy;
    unsigned int syncIDCopy;
    bool syncStateCopy;

    noInterrupts();
    posCopy = encoderPosition;
    syncIDCopy = syncID;
    syncStateCopy = lastSyncState;
    interrupts();

    int lickState = analogRead(lickPin);

    float arduinoTime = now / 1000000.0;
    float distance = posCopy * tickToDistance;

    float dt = (now - lastSpeedMicros) / 1000000.0;
    float speed = 0.0;

    if (dt > 0.0) {
      speed = (distance - lastDistance) / dt;
    }

    lastDistance = distance;
    lastSpeedMicros = now;

    Serial.print("POS,");
    Serial.print(arduinoTime, 6);
    Serial.print(",");
    Serial.print(distance, 4);
    Serial.print(",");
    Serial.print(speed, 4);
    Serial.print(",");
    Serial.print(syncIDCopy);
    Serial.print(",");
    Serial.print(syncStateCopy ? 1 : 0);
    Serial.print(",");
    Serial.println(lickState);
  }
}

void sendSyncEvents() {
  while (true) {
    unsigned int idCopy;
    unsigned long timeCopy;

    noInterrupts();

    if (syncReadIdx == syncWriteIdx) {
      interrupts();
      break;
    }

    idCopy = syncIDs[syncReadIdx];
    timeCopy = syncTimes[syncReadIdx];

    syncReadIdx = (syncReadIdx + 1) % SYNC_BUF_SIZE;

    interrupts();

    float arduinoSyncTime = timeCopy / 1000000.0;

    Serial.print("SYNC,");
    Serial.print(idCopy);
    Serial.print(",");
    Serial.println(arduinoSyncTime, 6);
  }
}

void readA() {
  if (digitalRead(pinA) == digitalRead(pinB)) {
    encoderPosition++;
  } else {
    encoderPosition--;
  }
}

void readB() {
  if (digitalRead(pinA) != digitalRead(pinB)) {
    encoderPosition++;
  } else {
    encoderPosition--;
  }
}

// Pin-change interrupt for D4 sync
ISR(PCINT2_vect) {
  bool currentState = digitalRead(syncPin);

  // Rising edge only
  if (currentState && !lastSyncState) {
    syncID++;

    byte nextIdx = (syncWriteIdx + 1) % SYNC_BUF_SIZE;

    if (nextIdx != syncReadIdx) {
      syncIDs[syncWriteIdx] = syncID;
      syncTimes[syncWriteIdx] = micros();
      syncWriteIdx = nextIdx;
    }
  }

  lastSyncState = currentState;
}