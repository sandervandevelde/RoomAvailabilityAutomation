using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomAvailabilityAutomationFunctionApp
{
	public class OutlookMeetingProvider
	{
		public static FreeSlots GetMeetingTimeSuggestionsResult(string meetingTimeSuggestionsResult, string currentDate)
		{
			var meetingTimeSuggestionsResultObject = Newtonsoft.Json.JsonConvert.DeserializeObject<MeetingTimeSuggestionsResult>(meetingTimeSuggestionsResult);

			var meetingSlots = new List<MeetingSlot>();

			foreach (var meetingTimeSuggestion in meetingTimeSuggestionsResultObject.meetingTimeSuggestions)
			{
				var meetingTimeSlot = meetingTimeSuggestion.meetingTimeSlot;

				var start = meetingTimeSlot.start;
				var end = meetingTimeSlot.end;

				var startDateTime = DateTime.Parse(start.dateTime);
				var endDateTime = DateTime.Parse(end.dateTime);

				meetingSlots.Add(new MeetingSlot { Start = startDateTime, End = endDateTime });
			}

			bool currentSlotIsFree, nextSlotIsFree;

			var currentDateCurrentSlot = DateTime.Parse(currentDate).ToUniversalTime();

			CreatePatternOne(currentDateCurrentSlot, meetingSlots, out currentSlotIsFree, out nextSlotIsFree);

			var numberOfMinutesTillFullHour = 60 - currentDateCurrentSlot.Minute;

			return new FreeSlots(currentSlotIsFree, nextSlotIsFree, numberOfMinutesTillFullHour);
		}

		private static void CreatePatternOne(DateTime currentDateCurrentSlot, List<MeetingSlot> meetingSlots, out bool currentSlotIsFree, out bool nextSlotIsFree)
		{
			currentSlotIsFree = meetingSlots.Any(x => x.Start <= currentDateCurrentSlot && x.End > currentDateCurrentSlot);
			
			var startDateNextSlot = currentDateCurrentSlot + TimeSpan.FromMinutes(30);

			nextSlotIsFree = meetingSlots.Any(x => x.Start <= startDateNextSlot && x.End > startDateNextSlot);
		}
	}

	public class FreeSlots
	{
        public FreeSlots(bool current, bool next, int numberOfMinutesTillFullHour)
        {
            Current = current;
			Next = next;
			NumberOfMinutesTillFullHour = numberOfMinutesTillFullHour;
        }
        public bool Current { get; private set; }

		public bool Next { get; private set; }

		public int NumberOfMinutesTillFullHour { get; private set; }
	}

	public class MeetingSlot
	{ 
		public DateTime Start { get; set; }
		public DateTime End { get; set; }
	}


	//{
	//	"@odata.context": "https://graph.microsoft.com/v1.0/$metadata#microsoft.graph.meetingTimeSuggestionsResult",
	//  "emptySuggestionsReason": "",
	//  "meetingTimeSuggestions": [

	//	{
	//		"confidence": 100.0,
	//      "organizerAvailability": "free",
	//      "attendeeAvailability": [

	//		{
	//			"availability": "free",
	//          "attendee": {
	//				"emailAddress": {
	//					"address": "rsc-room-ehv-fourth-duitsland@alten.nl"

	//			}
	//			}
	//		}
	//      ],
	//      "locations": [],
	//      "meetingTimeSlot": {
	//			"start": {
	//				"dateTime": "2023-10-15T15:30:00.0000000",
	//          "timeZone": "UTC"

	//		},
	//        "end": {
	//				"dateTime": "2023-10-15T16:00:00.0000000",
	//          "timeZone": "UTC"

	//		}
	//		}
	//	},
	//    {
	//		"confidence": 100,
	//      "organizerAvailability": "free",
	//      "attendeeAvailability": [

	//		{
	//			"availability": "free",
	//          "attendee": {
	//				"emailAddress": {
	//					"address": "rsc-room-ehv-fourth-duitsland@alten.nl"

	//			}
	//			}
	//		}
	//      ],
	//      "locations": [],
	//      "meetingTimeSlot": {
	//			"start": {
	//				"dateTime": "2023-10-15T16:00:00.0000000",
	//          "timeZone": "UTC"

	//		},
	//        "end": {
	//				"dateTime": "2023-10-15T16:30:00.0000000",
	//          "timeZone": "UTC"

	//		}
	//		}
	//	}
	//  ]
	//}

	public class MeetingTimeSuggestionsResult
	{
		public string emptySuggestionsReason { get; set; }
		public MeetingTimeSuggestion[] meetingTimeSuggestions { get; set; }
	}

	public class MeetingTimeSuggestion
	{
		public float confidence { get; set; }
		public string organizerAvailability { get; set; }
		public AttendeeAvailability[] attendeeAvailability { get; set; }
		public object[] locations { get; set; }
		public MeetingTimeSlot meetingTimeSlot { get; set; }
	}

	public class AttendeeAvailability
	{
		public string availability { get; set; }
		public Attendee attendee { get; set; }
	}

	public class Attendee
	{
		public EmailAddress emailAddress { get; set; }
	}

	public class EmailAddress
	{
		public string address { get; set; }
	}

	public class MeetingTimeSlot
	{
		public MeetingDataTime start { get; set; }
		public MeetingDataTime end { get; set; }
	}

	public class MeetingDataTime
	{
		public string dateTime { get; set; }
		public string timeZone { get; set; }
	}
}
