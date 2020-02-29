//---------------------------------------------------------------------------------
// Copyright (c) Feb 2020, devMobile Software
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
//---------------------------------------------------------------------------------
namespace devMobile.IoT.nRf24L01
{
   using System;
   using System.Text;
   using System.Threading;
   using Meadow;
   using Meadow.Devices;
   using Meadow.Hardware;

   public class MeadowApp : App<F7Micro, MeadowApp>
   {
      const byte SETUP_AW = 0x03;
      const byte RX_ADDR_P0 = 0x0A;
      const byte R_REGISTER = 0b00000000;
      const byte W_REGISTER = 0b00100000;
      ISpiBus spiBus;
      SpiPeripheral nrf24L01Device;
      IDigitalOutputPort spiPeriphChipSelect;
      IDigitalOutputPort ChipEnable;


      public MeadowApp()
      {
         ConfigureSpiPort();
         SetPipe0RxAddress("ZYXWV");
      }

      public void ConfigureSpiPort()
      {
         try
         {
            ChipEnable = Device.CreateDigitalOutputPort(Device.Pins.D09, initialState: false);
            if (ChipEnable == null)
            {
               Console.WriteLine("chipEnable == null");
            }

            var spiClockConfiguration = new SpiClockConfiguration(2000, SpiClockConfiguration.Mode.Mode0);
            spiBus = Device.CreateSpiBus(Device.Pins.SCK,
                                         Device.Pins.MOSI,
                                         Device.Pins.MISO,
                                         spiClockConfiguration);
            if (spiBus == null)
            {
               Console.WriteLine("spiBus == null");
            }

            Console.WriteLine("Creating SPI NSS Port...");
            spiPeriphChipSelect = Device.CreateDigitalOutputPort(Device.Pins.D10, initialState: true);
            if (spiPeriphChipSelect == null)
            {
               Console.WriteLine("spiPeriphChipSelect == null");
            }

            Console.WriteLine("nrf24L01Device Device...");
            nrf24L01Device = new SpiPeripheral(spiBus, spiPeriphChipSelect);
            if (nrf24L01Device == null)
            {
               Console.WriteLine("nrf24L01Device == null");
            }

            Thread.Sleep(100);

            Console.WriteLine("ConfigureSpiPort Done...");
         }
         catch (Exception ex)
         {
            Console.WriteLine("ConfigureSpiPort " + ex.Message);
         }
      }

      public void SetPipe0RxAddress(string address)
      {
         try
         {
            // Read the Address width
            byte[] txBuffer1 = new byte[] { SETUP_AW | R_REGISTER, 0x0 };
            Console.WriteLine(" txBuffer:" + BitConverter.ToString(txBuffer1));

            /*
            // Appears to work but not certain it actually does
            Console.WriteLine(" nrf24L01Device.WriteRead...SETUP_AW");
            byte[] rxBuffer1 = nrf24L01Device.WriteRead(txBuffer1, (ushort)txBuffer1.Length);
            */

            byte[] rxBuffer1 = new byte[txBuffer1.Length];
            Console.WriteLine(" spiBus.ExchangeData...RX_ADDR_P0");
            spiBus.ExchangeData(spiPeriphChipSelect, ChipSelectMode.ActiveLow, txBuffer1, rxBuffer1);

            Console.WriteLine(" rxBuffer:" + BitConverter.ToString(rxBuffer1));

            // Extract then adjust the address width
            byte addressWidthValue = rxBuffer1[1];
            addressWidthValue &= 0b00000011;
            addressWidthValue += 2;
            Console.WriteLine("Address width 0x{0:x2} - Value 0X{1:x2} - Bits {2} Value adjusted {3}", SETUP_AW, rxBuffer1[1], Convert.ToString(rxBuffer1[1], 2).PadLeft(8, '0'), addressWidthValue);
            Console.WriteLine();


            // Write Pipe0 Receive address
            Console.WriteLine("Address write 1");
            byte[] txBuffer2 = new byte[addressWidthValue + 1];
            txBuffer2[0] = RX_ADDR_P0 | W_REGISTER;
            Array.Copy(Encoding.UTF8.GetBytes(address), 0, txBuffer2, 1, addressWidthValue);
            Console.WriteLine(" txBuffer:" + BitConverter.ToString(txBuffer2));

            /*
            // Appears to work but not certain it does
            Console.WriteLine(" nrf24L01Device.Write...RX_ADDR_P0");
            nrf24L01Device.WriteBytes(txBuffer2);
            */

            Console.WriteLine(" spiBus.SendData...RX_ADDR_P0");
            spiBus.SendData(spiPeriphChipSelect, ChipSelectMode.ActiveLow, txBuffer2);

            Console.WriteLine();

            // Read Pipe0 Receive address
            Console.WriteLine("Address read 1");
            byte[] txBuffer3 = new byte[addressWidthValue + 1];
            txBuffer3[0] = RX_ADDR_P0 | R_REGISTER;
            Console.WriteLine(" txBuffer:" + BitConverter.ToString(txBuffer3));

            /*
            // Broken returns  Address 0x0a - RX Buffer 5A-5A-5A-5A-59-58 RX Address 5A-5A-5A-59-58 Address ZZZYX
            Console.WriteLine(" nrf24L01Device.WriteRead...RX_ADDR_P0");
            byte[] rxBuffer3 = nrf24L01Device.WriteRead(txBuffer3, (ushort)txBuffer3.Length);
            */

            byte[] rxBuffer3 = new byte[addressWidthValue + 1];
            Console.WriteLine(" spiBus.ExchangeData...RX_ADDR_P0");
            spiBus.ExchangeData(spiPeriphChipSelect, ChipSelectMode.ActiveLow, txBuffer3, rxBuffer3);

            Console.WriteLine();

            Console.WriteLine("Address 0x{0:x2} - RX Buffer {1} RX Address {2} Address {3}", RX_ADDR_P0, BitConverter.ToString(rxBuffer3, 0), BitConverter.ToString(rxBuffer3, 1), UTF8Encoding.UTF8.GetString(rxBuffer3, 1, addressWidthValue));
         }
         catch (Exception ex)
         {
            Console.WriteLine("ReadDeviceIDDiy " + ex.Message);
         }
      }

      public void ReadPipe0RxAddressV0()
      {
         try
         {
            byte[] txBuffer;
            byte[] rxBuffer;
            byte[] replyBuffer;

            // Read the Address width
            txBuffer = new byte[] { SETUP_AW | R_REGISTER, 0x0 };

            //Console.WriteLine(" spiBus.WriteRead...SETUP_AW");
            //Console.WriteLine(" txBuffer:" + BitConverter.ToString(txBuffer));
            //rxBuffer = nrf24L01Device.WriteRead(txBuffer, (ushort)txBuffer.Length);
            //Console.WriteLine(" rxBuffer:" + BitConverter.ToString(rxBuffer));

            Console.WriteLine(" spiBus.ExchangeData...SETUP_AW");
            rxBuffer = new byte[txBuffer.Length];
            spiBus.ExchangeData(spiPeriphChipSelect, ChipSelectMode.ActiveLow, txBuffer, rxBuffer);
            Console.WriteLine(" rxBuffer:" + BitConverter.ToString(rxBuffer));

            // Extract then adjust the address width
            byte addressWidthValue = rxBuffer[1];
            addressWidthValue &= 0b00000011;
            addressWidthValue += 2;
            Console.WriteLine("Address width 0x{0:x2} - Value 0X{1:x2} - Bits {2} Value adjusted {3}", SETUP_AW, rxBuffer[1], Convert.ToString(rxBuffer[1], 2).PadLeft(8, '0'), addressWidthValue);

            // Write Pipe0 Receive address
            Console.WriteLine("Address write 1");
            txBuffer = new byte[addressWidthValue + 1];
            txBuffer[0] = RX_ADDR_P0 | W_REGISTER;
            rxBuffer = new byte[addressWidthValue + 1];
            Array.Copy(Encoding.UTF8.GetBytes("ZYXWV"), 0, txBuffer, 1, addressWidthValue);
            Console.WriteLine(" spiBus.Write...RX_ADDR_P0");
            Console.WriteLine(" txBuffer:" + BitConverter.ToString(txBuffer));
            nrf24L01Device.WriteBytes(txBuffer);
            //spiBus.ExchangeData(spiPeriphChipSelect, ChipSelectMode.ActiveLow, txBuffer, rxBuffer);
            Console.WriteLine(" spiBus.Write...RX_ADDR_P0");

            // Read Pipe0 Receive address
            Console.WriteLine("Address read 1");

            txBuffer = new byte[addressWidthValue + 1];
            txBuffer[0] = RX_ADDR_P0 | R_REGISTER;
            Console.WriteLine(" spiBus.WriteRead...RX_ADDR_P0");
            Console.WriteLine(" txBuffer:" + BitConverter.ToString(txBuffer));
            //rxBuffer = nrf24L01Device.WriteRead(txBuffer, (ushort)txBuffer.Length);
            rxBuffer = new byte[addressWidthValue + 1];
            spiBus.ExchangeData(spiPeriphChipSelect, ChipSelectMode.ActiveLow, txBuffer, rxBuffer);
            Console.WriteLine(" spiBus.WriteRead...RX_ADDR_P0");
            Console.WriteLine("Address 0x{0:x2} - Value {1} Value {2}", RX_ADDR_P0, BitConverter.ToString(rxBuffer, 1), UTF8Encoding.UTF8.GetString(rxBuffer, 1, addressWidthValue));

            //Console.WriteLine(" rxBuffer:" + BitConverter.ToString(rxBuffer));


            // Extract the address from the rxBuffer
            //replyBuffer = new byte[addressWidthValue];
            //Array.Copy(rxBuffer, 1, replyBuffer, 0, addressWidthValue);
            //Console.WriteLine("Address 0x{0:x2} - Value {1} Value {2}", RX_ADDR_P0, BitConverter.ToString(replyBuffer), UTF8Encoding.UTF8.GetString(replyBuffer));

            /*        
            // Read the RF Channel
            Console.WriteLine("Channel read 1");
            txBuffer = new byte[] { RF_CH | R_REGISTER, 0x0 };
            Console.WriteLine(" spiBus.WriteRead...RF_CH");
            Console.WriteLine(" txBuffer:" + BitConverter.ToString(txBuffer));
            //rxBuffer = nrf24L01Device.WriteRead(txBuffer, (ushort)txBuffer.Length);
            rxBuffer = new byte[4];
            spiBus.ExchangeData(spiPeriphChipSelect, ChipSelectMode.ActiveLow, txBuffer, rxBuffer);
            Console.WriteLine(" spiBus.WriteRead...RF_CH");
            Console.WriteLine(" rxBuffer:" + BitConverter.ToString(rxBuffer));

            ushort rfChannel1 = rxBuffer[1];
            rfChannel1 += 2400;
            Console.WriteLine("RF Channel 1 0x{0:x2} - Value 0X{1:x2} - Bits {2} Value adjusted {3}", RF_CH, rxBuffer[1], Convert.ToString(rxBuffer[1], 2).PadLeft(8, '0'), rfChannel1);

            // Write the RF Channel
            Console.WriteLine("Channel write");
            txBuffer = new byte[] { RF_CH | W_REGISTER, 0x12, 0x13, 0x14, 0x15 };
            Console.WriteLine(" spiBus.WriteRead...RF_CH");
            Console.WriteLine(" txBuffer:" + BitConverter.ToString(txBuffer));
            ChipEnable.State = false;
            Thread.Sleep(1000);
            nrf24L01Device.WriteRead(txBuffer, (ushort)txBuffer.Length);
            Thread.Sleep(1000);
            ChipEnable.State = true;
            Console.WriteLine(" spiBus.WriteRead...RF_CH");

            // Read the RF Channel
            Console.WriteLine("Channel read 2");
            txBuffer = new byte[] { RF_CH | R_REGISTER, 0x0 };
            Console.WriteLine(" spiBus.WriteRead...RF_CH");
            Console.WriteLine(" txBuffer:" + BitConverter.ToString(txBuffer));
            rxBuffer = nrf24L01Device.WriteRead(txBuffer, (ushort)txBuffer.Length);
            Console.WriteLine(" spiBus.WriteRead...RF_CH");
            Console.WriteLine(" rxBuffer:" + BitConverter.ToString(rxBuffer));

            ushort rfChannel2 = rxBuffer[1];
            rfChannel2 += 2400;
            Console.WriteLine("RF Channel 2 0x{0:x2} - Value 0X{1:x2} - Bits {2} Value adjusted {3}", RF_CH, rxBuffer[1], Convert.ToString(rxBuffer[1], 2).PadLeft(8, '0'), rfChannel2);
            */
            Console.WriteLine("------");
         }
         catch (Exception ex)
         {
            Console.WriteLine("ReadDeviceIDDiy " + ex.Message);
         }
      }
   }
}
