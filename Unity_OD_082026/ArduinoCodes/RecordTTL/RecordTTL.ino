const int ttlPin = 2;

volatile unsigned long count = 0;
volatile bool updated = false;

void onRise() {
  count++;          // +1 à chaque front montant
  updated = true;   // dire à loop() qu’il y a du nouveau
}

void setup() {
  Serial.begin(115200);
  pinMode(ttlPin, INPUT);
  attachInterrupt(digitalPinToInterrupt(ttlPin), onRise, RISING);
}

void loop() {
  if (updated) {
    noInterrupts();
    unsigned long c = count;
    updated = false;
    interrupts();

    Serial.println(c);   // imprime le nombre de TTL détectés
  }
}
