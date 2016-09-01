using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GPA
{
	public class LocationPredictions
	{
		public LocationPredictions() { }

		[JsonProperty("status")]
		public string Status { get; set; }

		[JsonProperty("predictions")]
		public List<Prediction> Predictions { get; set; }

	}
}

