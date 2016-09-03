using System;
using Newtonsoft.Json.Linq;

namespace DurianCode.iOS.Places.GooglePlace
{
	public class Place
	{
		public readonly string name;
		public readonly double latitude;
		public readonly double longitude;
		public readonly JObject raw;

		public Place(JObject json)
		{
			name = (string)json["result"]["name"];
			latitude = (double)json["result"]["geometry"]["location"]["lat"];
			longitude = (double)json["result"]["geometry"]["location"]["lng"];
			raw = json;
		}
	}
}

