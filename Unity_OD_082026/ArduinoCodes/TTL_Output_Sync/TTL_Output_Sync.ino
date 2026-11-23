const int TTL_PIN = 12;

void setup() {
  pinMode(TTL_PIN, OUTPUT);
  digitalWrite(TTL_PIN, LOW);

  Serial.begin(115200);
}

void loop() {
  if (Serial.available() > 0) {
    byte x = Serial.read();

    
    digitalWrite(TTL_PIN, HIGH);
    delayMicroseconds(1000);
    digitalWrite(TTL_PIN, LOW);
  }
}
