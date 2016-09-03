using System;
using GPA;
using UIKit;
using Newtonsoft.Json.Linq;

namespace GPACodeExample
{
	public class ViewController : UIViewController
	{
		public UIButton getLocationButton;
		public UINavigationController placesViewContainer;
		public PlacesViewController placesViewController;

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
				placesViewController.apiKey = Constants.apiKey; //"<Your API key here>";

				// 2. Set the search criteria to match your needs

				//  Note: If you do not supply the location and radius, 
				//	the API will attempt to detect the user's location 
				//	from their IP address, and will bias the results to 
				//	that location. If you would prefer to have no 
				//  location bias, set the location to '0,0' and radius 
				//  to '20000000' (20 thousand kilometers), to encompass 
				//  the entire world.
				
				//placesViewController.SetPlaceType(PlaceType.Cities);
				//placesViewController.SetLocationBias(new LocationBias(40.7058316, -74.2581935, 1000000));

				// 3. Subscribe to PlaceSelected delegate to get place details
				placesViewController.PlaceSelected += HandlePlaceSelection;

				// 4. Instantiate the UINavigationController to contain the PlacesViewController
				placesViewContainer = new UINavigationController(placesViewController);

				// Optional: Customize the view styling
				//placesViewController.NavigationController.NavigationBar.BarStyle = UIBarStyle.Default;
				//placesViewController.NavigationController.NavigationBar.Translucent = false;
				//placesViewController.NavigationController.NavigationBar.TintColor = UIColor.Magenta;
				//placesViewController.NavigationController.NavigationBar.BarTintColor = UIColor.Yellow;
				//placesViewController.Title = "Type Address";

				// 5. Present the view
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
			// 6. Handle the place details however you wish
			Console.WriteLine($"{placeData}");

			// TODO - Create helper class 'Place'
		}

	}
}

