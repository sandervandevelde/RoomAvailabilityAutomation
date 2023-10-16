using RoomAvailabilityAutomationFunctionApp;

namespace RoomAvailabilityAutomationTestProject
{
	[TestClass]
	public class WebsiteCallsUnitTest
	{
		[TestMethod]
		public void TestMethodMessageTrueTrue()
		{
			var message = File.ReadAllText("m-true-true-2023-10-15T17.10.00Z.json");

			var response = OutlookMeetingProvider.GetMeetingTimeSuggestionsResult(message, "2023-10-15T17:10:00Z");

			Assert.IsNotNull(response);
			Assert.IsTrue(response.Current);
			Assert.IsTrue(response.Next);
		}

		[TestMethod]
		public void TestMethodMessageTrueFalse()
		{
			var message = File.ReadAllText("m-true-false-2023-10-15T17.10.00Z.json");

			var response = OutlookMeetingProvider.GetMeetingTimeSuggestionsResult(message, "2023-10-15T17:10:00Z");

			Assert.IsNotNull(response);
			Assert.IsTrue(response.Current);
			Assert.IsFalse(response.Next);
			Assert.AreEqual(50, response.NumberOfMinutesTillFullHour);
		}


		[TestMethod]
		public void TestMethodMessageFalseFalse()
		{
			var message = File.ReadAllText("m-false-false-2023-10-15T17.10.00Z.json");

			var response = OutlookMeetingProvider.GetMeetingTimeSuggestionsResult(message, "2023-10-15T17:10:00Z");

			Assert.IsNotNull(response);
			Assert.IsFalse(response.Current);
			Assert.IsFalse(response.Next);
			Assert.AreEqual(50, response.NumberOfMinutesTillFullHour);
		}

		[TestMethod]
		public void TestMethodMessageFalseTrue()
		{
			var message = File.ReadAllText("m-false-true-2023-10-15T17.10.00Z.json");

			var response = OutlookMeetingProvider.GetMeetingTimeSuggestionsResult(message, "2023-10-15T17:10:00Z");

			Assert.IsNotNull(response);
			Assert.IsFalse(response.Current);
			Assert.IsTrue(response.Next);
			Assert.AreEqual(50, response.NumberOfMinutesTillFullHour);
		}

		[TestMethod]
		public void TestMethodMessageEmpty()
		{
			var message = File.ReadAllText("m-empty.json");

			var response = OutlookMeetingProvider.GetMeetingTimeSuggestionsResult(message, "2023-10-15T17:10:00Z");

			Assert.IsNotNull(response);
			Assert.IsFalse(response.Current);
			Assert.IsFalse(response.Next);
			Assert.AreEqual(50, response.NumberOfMinutesTillFullHour);
		}

		[TestMethod]
		public void TestMethodMessageTrueFalse_2102()
		{
			var message = File.ReadAllText("m-true-true-2023-10-15T21.02.00Z.json");

			var response = OutlookMeetingProvider.GetMeetingTimeSuggestionsResult(message, "2023-10-15T21:02:12.5589924Z");

			Assert.IsNotNull(response);
			Assert.IsTrue(response.Current);
			Assert.IsFalse(response.Next);
			Assert.AreEqual(58, response.NumberOfMinutesTillFullHour);
		}

		[TestMethod]
		public void TestMethodMessageTrueTrue_2137()
		{
			var message = File.ReadAllText("m-true-true-2023-10-15T21.37.00Z.json");

			var response = OutlookMeetingProvider.GetMeetingTimeSuggestionsResult(message, "2023-10-15T21:37:42.1807859Z");

			Assert.IsNotNull(response);
			Assert.IsTrue(response.Current);
			Assert.IsTrue(response.Next);
			Assert.AreEqual(23, response.NumberOfMinutesTillFullHour);
		}
	}
}