using System;

using UIKit;
using GPA;
using Newtonsoft.Json;
using System.Runtime.Remoting.Channels;
using Newtonsoft.Json.Linq;

namespace GPACodeExample
{
	public class ViewController : UIViewController
	{
		public UIButton getLocationButton;
		public UINavigationController placesViewContainer;
		public PlacesViewController placesViewController;

		public ViewController()
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			View.BackgroundColor = UIColor.White;

			getLocationButton = new UIButton();
			getLocationButton.TranslatesAutoresizingMaskIntoConstraints = false;
			getLocationButton.SetTitle("Get Location", UIControlState.Normal);
			getLocationButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);

			getLocationButton.TouchUpInside += (sender, e) => 
			{
				// 1. Instantiate the PlacesViewController
				placesViewController = new PlacesViewController();
				placesViewController.apiKey = Constants.apiKey; // "<Your API key here>";
				// TODO - set PlaceType

				// 2. Subscribe to PlaceSelected delegate to get place details
				placesViewController.PlaceSelected += HandlePlaceSelection;

				// 3. Instantiate the UINavigationController to contain the PlacesViewController
				placesViewContainer = new UINavigationController(placesViewController);

				// 4. Present the view
				PresentViewController(placesViewContainer, true, null);
			};

			View.AddSubview(getLocationButton);
			SetConstraintsForButton();
		}

		void SetConstraintsForButton()
		{
			var left = getLocationButton.LeftAnchor.ConstraintEqualTo(View.LeftAnchor);
			var right = getLocationButton.RightAnchor.ConstraintEqualTo(View.RightAnchor);
			var top = getLocationButton.TopAnchor.ConstraintEqualTo(View.TopAnchor);
			var height = getLocationButton.HeightAnchor.ConstraintEqualTo(View.Frame.Height);

			NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[]
			{
				left, right, top, height
			});

			UpdateViewConstraints();
		}

		void HandlePlaceSelection(object sender, JObject placeData)
		{ 
			// 5. Handle the place details however you wish
			Console.WriteLine($"{placeData}");

			// TODO - Create helper class 'Location'
		}

	}
}

