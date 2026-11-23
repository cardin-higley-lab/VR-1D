const int lickPin = A0;

void setup() {
  Serial.begin(115200);
}

void loop() {
  int val = analogRead(lickPin);  // 0–1023
  Serial.println(val);
  delay(5);                       // 200 Hz → largement suffisant
}
