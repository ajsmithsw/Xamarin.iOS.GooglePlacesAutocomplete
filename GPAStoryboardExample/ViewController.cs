using System;
using GPA;
using UIKit;
using Newtonsoft.Json.Linq;

namespace GPAStoryboardExample
{
	public partial class ViewController : UIViewController
	{
		protected ViewController(IntPtr handle) : base(handle) { }

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			MyButton.TouchUpInside += (sender, e) => { 
				PerformSegue("MyCustomSegue", this);
			};
		}

		void HandlePlaceSelection(object sender, JObject placeDetails)
		{
			// Handle as you wish
			Console.WriteLine($"{placeDetails}");
		}

		public override void PrepareForSegue(UIStoryboardSegue segue, Foundation.NSObject sender)
		{
			base.PrepareForSegue(segue, sender);

			if (segue.Identifier.Equals("MyCustomSegue"))
			{ 
				var vc = (PlacesViewController)segue.DestinationViewController.ChildViewControllers[0];
				vc.apiKey = "<Your API Key Here>"; // Constants.apiKey;
				// TODO - Set Placetype and other parameters
				vc.PlaceSelected += HandlePlaceSelection;
			}
		}

	}
}

