// ROTARY ENCODER ORIGINAL (ASCII VERSION)

const int pinA = 2;
const int pinB = 3;

volatile long position = 0;

unsigned long lastTime = 0;

void setup() {
  Serial.begin(115200);

  pinMode(pinA, INPUT_PULLUP);
  pinMode(pinB, INPUT_PULLUP);

  attachInterrupt(digitalPinToInterrupt(pinA), readA, CHANGE);
  attachInterrupt(digitalPinToInterrupt(pinB), readB, CHANGE);

  lastTime = micros();
}

void loop() {
  // timestamp en secondes
  float t = micros() / 1e6;

  // distance (ex : chaque tick = 0.01)
  float distance = position * 0.01;

  // vitesse (simple dérivée)
  static float lastDistance = 0;
  float speed = (distance - lastDistance) / 0.005; // approx 200 Hz
  lastDistance = distance;

  // ENVOI ASCII EXACTEMENT COMME UNITY L’ATTENDAIT
  Serial.print(t, 6);
  Serial.print(",");
  Serial.print(distance, 4);
  Serial.print(",");
  Serial.println(speed, 4);

  delay(5); // ~200 Hz
}

void readA() {
  if (digitalRead(pinA) == digitalRead(pinB)) position++;
  else position--;
}

void readB() {
  if (digitalRead(pinA) != digitalRead(pinB)) position++;
  else position--;
}
