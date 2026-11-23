const int ttlPin = 12;
const float lambda = 5.0;  // taux moyen en Hz

unsigned long nextPulseTime = 0;

void scheduleNextPulse() {
  float U = random(1, 10000) / 10000.0;
  float interval_s = -log(U) / lambda;
  nextPulseTime = millis() + (unsigned long)(interval_s * 1000.0);
}

void setup() {
  pinMode(ttlPin, OUTPUT);

  randomSeed(analogRead(A0));
  scheduleNextPulse();
}

void loop() {
  if (millis() >= nextPulseTime) {
    digitalWrite(ttlPin, HIGH);
    delay(5);                // largeur d'impulsion TTL
    digitalWrite(ttlPin, LOW);

    scheduleNextPulse();
  }
}