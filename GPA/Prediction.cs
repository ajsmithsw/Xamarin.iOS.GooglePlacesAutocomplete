using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DurianCode.iOS.Places
{

	public class Prediction
	{
		public Prediction() { }

		[JsonProperty("description")]
		public string Description { get; set; }

		[JsonProperty("id")]
		public string ID { get; set; }

		[JsonProperty("place_id")]
		public string Place_ID { get; set; }

		[JsonProperty("reference")]
		public string Reference { get; set; }
	}

}
