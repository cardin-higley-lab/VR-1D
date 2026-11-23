/*
To test whether Arduino is connected to computer
- need to open Tools > Serial Montior to check the operation

void setup() {
  Serial.begin(9600);
}

void loop() {
  Serial.println("Hello from Arduino!");
  delay(1000);
}
*/

/*
To test whether Arduino is connected to computer and the Arduino device itself (by auto-connected LED13)
- need to open Tools > Serial Montior to check the operation
*/

void setup() {
  Serial.begin(9600);
  pinMode(13, OUTPUT);
}

void loop() {
  digitalWrite(13, HIGH);
  Serial.println("LED ON");
  delay(500);
  
  digitalWrite(13, LOW);
  Serial.println("LED OFF");
  delay(500);
}
