using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using Meadow.Hardware;
using Radios.RF24;

namespace nRF24DeviceClient
{
   public class MeadowApp : App<F7Micro, MeadowApp>
   {
		private const string BaseStationAddress = "Node2";
		private const string DeviceAddress = "Dev01";
		private const byte nRF24Channel = 20;
		private RF24 Radio = new RF24();

		public MeadowApp()
      {
			try
			{
								var config = new Meadow.Hardware.SpiClockConfiguration(
												2000,
												Meadow.Hardware.SpiClockConfiguration.Mode.Mode0);

								ISpiBus spiBus = Device.CreateSpiBus(Device.Pins.SCK,
									 Device.Pins.MOSI,
									 Device.Pins.MISO,
									 config);

				//ISpiBus spiBus = Device.CreateSpiBus();

				Radio.OnDataReceived += Radio_OnDataReceived;
				Radio.OnTransmitFailed += Radio_OnTransmitFailed;
				Radio.OnTransmitSuccess += Radio_OnTransmitSuccess;

				Radio.Initialize(Device, spiBus, Device.Pins.D09, Device.Pins.D10, Device.Pins.D11);
				Console.WriteLine("---Address write: " + BaseStationAddress);
				Radio.Address = Encoding.UTF8.GetBytes(BaseStationAddress);
				Console.WriteLine("---Address read: " + Encoding.UTF8.GetString(Radio.Address));

				Radio.Channel = nRF24Channel;
				Radio.PowerLevel = PowerLevel.High;
				Radio.DataRate = DataRate.DR250Kbps;
				Radio.IsEnabled = true;

				Radio.IsAutoAcknowledge = true;
				Radio.IsDyanmicAcknowledge = false;
				Radio.IsDynamicPayload = true;

				Console.WriteLine("Address: " + Encoding.UTF8.GetString(Radio.Address));
				Console.WriteLine("PA: " + Radio.PowerLevel);
				Console.WriteLine("IsAutoAcknowledge: " + Radio.IsAutoAcknowledge);
				Console.WriteLine("Channel: " + Radio.Channel);
				Console.WriteLine("DataRate: " + Radio.DataRate);
				Console.WriteLine("IsDynamicAcknowledge: " + Radio.IsDyanmicAcknowledge);
				Console.WriteLine("IsDynamicPayload: " + Radio.IsDynamicPayload);
				Console.WriteLine("IsEnabled: " + Radio.IsEnabled);
				Console.WriteLine("Frequency: " + Radio.Frequency);
				Console.WriteLine("IsInitialized: " + Radio.IsInitialized);
				Console.WriteLine("IsPowered: " + Radio.IsPowered);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}

			while (true)
			{
				string payload = "hello " + DateTime.Now.Second;
				Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss}-TX {payload.Length} byte message {payload}");
				Radio.SendTo(Encoding.UTF8.GetBytes(DeviceAddress), Encoding.UTF8.GetBytes(payload));

				Task.Delay(30000).Wait();
			}
		}

		private void Radio_OnDataReceived(byte[] data)
		{
			// Display as Unicode
			string unicodeText = Encoding.UTF8.GetString(data);
			Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss}-RX Unicode Length {0} Unicode Length {1} Unicode text {2}", data.Length, unicodeText.Length, unicodeText);

			// display as hex
			Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss}-RX Hex Length {0} Payload {1}", data.Length, BitConverter.ToString(data));
		}

		private void Radio_OnTransmitSuccess()
		{
			Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss}-TX Succeeded!");
		}

		private void Radio_OnTransmitFailed()
		{
			Console.WriteLine("Transmit Failed!");
		}
	}
}
