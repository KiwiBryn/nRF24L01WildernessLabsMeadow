//---------------------------------------------------------------------------------
// Copyright ® Jan 2018, devMobile Software
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// Thanks to the creators and maintainers of the libraries used by this project
//   https://github.com/maniacbug/RF24
//
#include <RF24.h>

// nRF24L01 ISM wireless module setup for Arduino Uno R3/Seeeduino V4.2 using http://embeddedcoolness.com/shop/rfx-shield/
// RF24 radio( ChipeEnable , ChipSelect )
RF24 radio(3,7);
const byte FieldGatewayChannel = 20 ;
const byte FieldGatewayAddress[] = "Node1";
const byte DeviceAddress[] = "Dev01";

// Payload configuration
const int PayloadSizeMaximum = 32 ;
char payload[PayloadSizeMaximum] = "";

const int LoopSleepDelaySeconds = 10 ;

void setup() 
{
  Serial.begin(9600);
  Serial.println("Setup called");

  // Configure the nRF24 module
  Serial.println("nRF24 setup");
  radio.begin();
  radio.setChannel(FieldGatewayChannel);
  radio.openWritingPipe(FieldGatewayAddress);
  radio.openReadingPipe(1,DeviceAddress);
  radio.setDataRate(RF24_250KBPS) ;
  radio.setPALevel(RF24_PA_MAX);
  radio.enableDynamicPayloads();

  radio.startListening();
  
  Serial.println("Setup done");
}


void loop() 
{
  int payloadLength = 0 ;  
  Serial.println("Loop called");
  memset(payload, 0, sizeof(payload));

  // See of there is an inbound message availabl, then display it
  if ( radio.available())
  {
    memset(payload, 0, sizeof(payload));
    radio.read( payload, radio.getDynamicPayloadSize());
    Serial.println(payload);
  }
  radio.stopListening();     
  
  memcpy( &payload[payloadLength], "Hello ",  strlen("hello ")) ;
  payloadLength += strlen("hello");
  payloadLength += strlen( itoa( millis(),(char *)&payload[payloadLength],10));

  // Send the payload to base station
  Serial.print( "nRF24 Payload length:");
  Serial.println( payloadLength );

  boolean result = radio.write(payload, payloadLength);
  if (result)
    Serial.println("Write Ok...");
  else
    Serial.println("Write failed.");

  radio.startListening();

  Serial.println("Loop done");

  delay(LoopSleepDelaySeconds * 1000l);
}
