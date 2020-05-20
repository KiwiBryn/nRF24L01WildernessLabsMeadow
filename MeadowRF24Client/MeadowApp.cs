//---------------------------------------------------------------------------------
// Copyright (c) March 2020, devMobile Software
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
namespace devMobile.IoT.FieldGateway.nRF24Client
{
   using System;
   using System.Text;
   using System.Threading.Tasks;

   using Meadow;
   using Meadow.Devices;
   using Meadow.Hardware;

   using Radios.RF24;

   public class MeadowApp : App<F7Micro, MeadowApp>
   {
      private const string BaseStationAddress = "Base1";
      private const string DeviceAddress = "Dev01";
      private const byte nRF24Channel = 15;
      private RF24 Radio = new RF24();

      public MeadowApp()
      {
         try
         {
            var config = new Meadow.Hardware.SpiClockConfiguration(2000, SpiClockConfiguration.Mode.Mode0);

            ISpiBus spiBus = Device.CreateSpiBus( Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO, config);

            Radio.OnDataReceived += Radio_OnDataReceived;
            Radio.OnTransmitFailed += Radio_OnTransmitFailed;
            Radio.OnTransmitSuccess += Radio_OnTransmitSuccess;

            Radio.Initialize(Device, spiBus, Device.Pins.D09, Device.Pins.D10, Device.Pins.D11);

            Radio.Address = Encoding.UTF8.GetBytes(DeviceAddress);
            Radio.Channel = nRF24Channel;
            Radio.PowerLevel = PowerLevel.High;
            Radio.DataRate = DataRate.DR250Kbps;
            Radio.IsEnabled = true;
            Radio.IsAutoAcknowledge = true;
            Radio.IsDyanmicAcknowledge = false;
            Radio.IsDynamicPayload = true;

            Console.WriteLine($"Address: {Encoding.UTF8.GetString(Radio.Address)}");
            Console.WriteLine($"Channel: {Radio.Channel}");
            Console.WriteLine($"Frequency: {Radio.Frequency}");
            Console.WriteLine($"PowerLevel: {Radio.PowerLevel}");
            Console.WriteLine($"DataRate: {Radio.DataRate}");
            Console.WriteLine($"IsEnabled: {Radio.IsEnabled}");
            Console.WriteLine($"IsAutoAcknowledge: {Radio.IsAutoAcknowledge}");
            Console.WriteLine($"IsDynamicAcknowledge: {Radio.IsDyanmicAcknowledge}");
            Console.WriteLine($"IsDynamicPayload: {Radio.IsDynamicPayload}");
            Console.WriteLine($"IsInitialized: {Radio.IsInitialized}");
            Console.WriteLine($"IsPowered: {Radio.IsPowered}");

            while (true)
            {
               string payload = "hello " + DateTime.Now.ToShortTimeString();
               Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss}-TX {payload.Length} byte message {payload}");

               Radio.SendTo(Encoding.UTF8.GetBytes(BaseStationAddress), Encoding.UTF8.GetBytes(payload));

               Task.Delay(10000).Wait();
            }
         }
         catch (Exception ex)
         {
            Console.WriteLine(ex.Message);

            return;
         }
      }

      private void Radio_OnTransmitSuccess()
      {
         Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss}-TX Succeeded!");
      }

      private void Radio_OnTransmitFailed()
      {
         Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss}-TX failed!");
      }

      private void Radio_OnDataReceived(byte[] data)
      {
         // Display as Unicode
         string unicodeText = Encoding.UTF8.GetString(data);
         Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss}-RX Data Length {data.Length} Unicode Length {unicodeText.Length} Unicode text {unicodeText}");

         // display as hex
         Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss}-RX Hex Length {data.Length} Payload {BitConverter.ToString(data)}");
      }
   }
}
