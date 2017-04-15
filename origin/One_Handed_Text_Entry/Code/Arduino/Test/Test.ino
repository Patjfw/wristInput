int LDR_Pin0 = A0; //analog pin 0
int LDR_Pin1 = A1; //analog pin 1
int LDR_Pin2 = A2; //analog pin 2
int LDR_Pin3 = A3; //analog pin 3
int LDR_Pin4 = A4; //analog pin 4
int LDR_Pin5 = A5; //analog pin 5
int LDR_Pin6 = A6; //analog pin 6
int LDR_Pin7 = A7; //analog pin 7
int LDR_Pin8 = A8; //analog pin 8
int LDR_Pin9 = A9; //analog pin 9
int LDR_Pin10 = A10; //analog pin 10
int LDR_Pin11 = A11; //analog pin 11
//int LDR_Pin12 = A12; //analog pin 12
//int LDR_Pin13 = A13; //analog pin 13
//int LDR_Pin14 = A14; //analog pin 14
//int LDR_Pin15 = A15; //analog pin 15
//int count = 0;

void setup() {
  Serial.begin(9600);
}

void loop() {

  //int endtime = 0;
  //unsigned long begintime = micros();
  int LDRReading0 = analogRead(LDR_Pin0);
  int LDRReading1 = analogRead(LDR_Pin1);
  int LDRReading2 = analogRead(LDR_Pin2);
  int LDRReading3 = analogRead(LDR_Pin3);
  int LDRReading4 = analogRead(LDR_Pin4);
  int LDRReading5 = analogRead(LDR_Pin5);
  int LDRReading6 = analogRead(LDR_Pin6);
  int LDRReading7 = analogRead(LDR_Pin7);
  int LDRReading8 = analogRead(LDR_Pin8);
  int LDRReading9 = analogRead(LDR_Pin9);
  int LDRReading10 = analogRead(LDR_Pin10);
  int LDRReading11 = analogRead(LDR_Pin11);

//  String str = LDRReading6 + " ";
//  str += LDRReading7 + " ";
//  str += LDRReading8 + " ";
//  str += LDRReading9 + " ";
//  str += LDRReading10 + " ";
//  str += LDRReading11 + " ";
//  str += LDRReading0 + " ";
//  str += LDRReading1 + " ";
//  str += LDRReading2 + " ";
//  str += LDRReading3 + " ";
//  str += LDRReading4 + " ";
//  str += LDRReading5 + " ";
//  Serial.println(str);

//int LDRReading12 = analogRead(LDR_Pin12);
 //int LDRReading13 = analogRead(LDR_Pin13);
 // int LDRReading14 = analogRead(LDR_Pin14);
  //int LDRReading15 = analogRead(LDR_Pin15);

 // Serial.print("S0:");
  Serial.print(LDRReading6);

 // Serial.print(" S1:");
  Serial.print(" ");
  Serial.print(LDRReading5);

 // Serial.print(" S2:");
  Serial.print(" ");
  Serial.print(LDRReading4);

 // Serial.print(" S3:");
  Serial.print(" ");
  Serial.print(LDRReading3);

 // Serial.print(" S4:");
  Serial.print(" ");
  Serial.print(LDRReading2);

 // Serial.print(" S5:");
  Serial.print(" ");
  Serial.print(LDRReading1);

  //Serial.print(" S6:");
  Serial.print(" ");
  Serial.print(LDRReading0);

  //Serial.print(" S7:");
  Serial.print(" ");
  Serial.print(LDRReading11);

  //Serial.print(" S8:");
  Serial.print(" ");
  Serial.print(LDRReading10);
  
  //Serial.print(" S9:");
  Serial.print(" ");
  Serial.print(LDRReading9);
  
  //Serial.print(" S10:");
  Serial.print(" ");
  Serial.print(LDRReading8);
  
  //Serial.print(" S11:");
  Serial.print(" ");
  Serial.println(LDRReading7);

  //Serial.print(" ");
  //Serial.println(LDRReading15);

  //Serial.print(" ");
  //Serial.print(LDRReading13);

  //Serial.print(" ");
  //Serial.println(LDRReading15);
  //Serial.println();

  //endtime = micros();
  //Serial.print("Time for print out 12 sensor data: ");
  //count = count + 1;
  //Serial.print(count);
  
  //Serial.print("    ");
  //Serial.println(micros());
  
  //Serial.println(""); 
//  Serial.print(" S12:");
//  Serial.print(LDRReading3);
//  
//  Serial.print(" S13:");
//  Serial.print(LDRReading4);

//  
//  Serial.print(" S14:");
//  Serial.print(LDRReading5);
//  
//  Serial.print(" S15:");
//  Serial.print(LDRReading6);
  
//delay(200); //just here to slow down the output for easier reading
}
