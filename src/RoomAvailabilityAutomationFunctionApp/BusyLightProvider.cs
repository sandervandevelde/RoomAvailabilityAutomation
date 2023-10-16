using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace RoomAvailabilityAutomationFunctionApp
{
	internal class BusyLightLoraProvider
	{
		private static MqttClient _mqttClient;

		private string _lightStrategy;

		private string _network;

		private string _applicationname;

		private  string _deviceid;

		private string _apiKey;

        public BusyLightLoraProvider(string applicationname, string deviceid, string apiKey, string network, string lightStrategy)
        {
			_applicationname = applicationname;
			_deviceid = deviceid;
			_apiKey = apiKey;
			_network = network;
			_lightStrategy = lightStrategy;
        }

        public void ChangeLight(bool current, bool next, int numberOfMinutesTillFullHour, ILogger log)
		{
			_mqttClient = new MqttClient(_network);

			_mqttClient.Subscribe(
				new[] { $"v3/{_applicationname}/devices/{_deviceid}/up" },
				new[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });

			_mqttClient.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;

			var response = _mqttClient.Connect(Guid.NewGuid().ToString(), _applicationname, _apiKey);

			log.LogInformation($"TTN connect response: '{response}' (0=connected; 5=Connection Refused: Authorization error) "); // https://www.vtscada.com/help/Content/D_Tags/D_MQTT_ErrMsg.htm

			SendMessage(current, next, numberOfMinutesTillFullHour, log); 
		}

		private async void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
		{
			var deviceId = e.Topic.Split('/')[3];
			var jsonText = Encoding.UTF8.GetString(e.Message);
			Console.WriteLine($"{deviceId} - {jsonText}");
			await Task.Delay(1);
		}

		private void SendMessage(bool current, bool next, int numberOfMinutesTillFullHour, ILogger log)
		{
			// var rawbytes = new byte[2] { 4, 10 }; // set command and control bytes instead of changing lights

			byte[] rawbytes = null;

			switch (_lightStrategy)
			{
				case "1":
					rawbytes = GetPatternOne(current, next, numberOfMinutesTillFullHour);
					break;
				case "2":
					rawbytes = GetPatternTwo(current, next, numberOfMinutesTillFullHour);
					break;
				default:
					throw new Exception($"Invalid light strategy '{_lightStrategy}'");
			}

			var payloadData = Convert.ToBase64String(rawbytes);
			var payload = new Payload { downlinks = new List<Downlink> { new Downlink { frm_payload = payloadData } }.ToArray() };

			var jsonMessage = JsonConvert.SerializeObject(payload);
			var bytes = Encoding.UTF8.GetBytes(jsonMessage);
			var mqttResult = _mqttClient.Publish(
				 $"v3/{_applicationname}/devices/{_deviceid}/down/replace",  // "/push
				 bytes);

			log.LogInformation($"TTN downlink response: '{mqttResult}'");
		}

		private byte[] GetPatternOne(bool current, bool next, int numberOfMinutesTillFullHour)
		{
			// free free
			if (current && next)
			{
				// green

				return new byte[5] { 0, 0, 100, 255, 0 }; // set lights and time
			}

			// free false
			if (current && !next)
			{
				// orange

				return new byte[5] { 100, 0, 100, 255, 0 }; // set lights and time
			}

			// false free
			if (!current && next)
			{
				// orange + blink

				return new byte[5] { 100, 0, 100, 255, 50 }; // set lights and time
			}

			// false false
			if (!next && !current)
			{
				// red

				return new byte[5] { 100, 0, 0, 255, 0 }; // set lights and time
			}

			throw new Exception("Invalid pattern");
		}

		private byte[] GetPatternTwo(bool current, bool next, int numberOfMinutesTillFullHour)
		{
			// both free free sessions
			if (current && next)
			{
				// green

				return new byte[5] { 0, 0, 100, 255, 0 }; // set lights and time
			}

			// first is free but second not free
			if (current && !next)
			{
				// orange

				return new byte[5] { 100, 0, 100, 255, 0 }; // set lights and time
			}

			// first is not free but second is
			if (!current && next)
			{
				// Blue

				return new byte[5] { 0, 100, 0, 255, 0 }; // set lights and time
			}

			// both sessions are not free
			if (!next && !current)
			{
				// less than 10 minutes : orange blinking

				if (numberOfMinutesTillFullHour < 10)
				{
					return new byte[5] { 100, 0, 100, 255, 50 }; // set lights and time
				}

				// red

				return new byte[5] { 100, 0, 0, 255, 0 }; // set lights and time
			}

			throw new Exception("Invalid pattern");
		}
	}

	public class Payload
	{
		public Downlink[] downlinks { get; set; }
	}

	public class Downlink
	{
		public int f_port { get; set; } = 15; // see document
		public string frm_payload { get; set; }
		public string priority { get; set; } = "NORMAL";
	}

	public class KuandoDownlink
	{
		public byte red { get; set; } = 0;
		public byte blue { get; set; } = 0;
		public byte green { get; set; } = 0;
		public byte ontime { get; set; } = 0;
		public byte offtime { get; set; } = 0;
	}
}
