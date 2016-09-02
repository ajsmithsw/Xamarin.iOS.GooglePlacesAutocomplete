# Xamarin.iOS.GooglePlacesAutocomplete

A simple Google Places API autocompleting search view controller for native Xamarin iOS apps. Inspired by the [brilliant Swift example](https://github.com/watsonbox/ios_google_places_autocomplete) created by [Howard Wilson](http://watsonbox.github.com/).

 <center><img src="https://1.bp.blogspot.com/-boNRr4Kj-Jw/V8luLjb11-I/AAAAAAAAEvI/Bc9xI4JUkl0FzJMciMLPOKQGKVfhUAS4wCLcB/s320/GPA_example.png"/></center>


### Contributing
Solution is currently incomplete. If you wish to contribute, you're very welcome to submit pull requests or get in touch.


### Obtaining API key
Use the [Google Developers Console](https://console.developers.google.com/) to enable the 'Google Places API Web Service' and create a 'Server' API key credential. In both cases do not use the iOS options.



## How to implement PlacesViewController


> Ensure that you install **Newtonsoft.Json** in your iOS project. 


### Using the PlacesViewController programmatically

If you are creating your iOS app without using storyboards, or wish to launch the view programmatically:

```csharp
// 1. Instantiate the PlacesViewController
placesViewController = new PlacesViewController();
placesViewController.apiKey = "<Your API key here>";
// TODO - set PlaceType

// 2. Subscribe to PlaceSelected delegate to get place details
placesViewController.PlaceSelected += HandlePlaceSelection;

// 3. Instantiate the UINavigationController to contain the PlacesViewController
placesViewContainer = new UINavigationController(placesViewController);

// 4. Present the view
PresentViewController(placesViewContainer, true, null);
```
Your HandlePlaceSelection delegate method:

```csharp
void HandlePlaceSelection(object sender, JObject placeData)
{ 
    // 5. Handle the place details however you wish
    Console.WriteLine($"{placeData}");
}
```



### Using the PlacesViewController with Storyboards (iOS Designer)

To implement using the iOS designer for Xamarin or XCode Storyboard Editor:

1. Drag a new UINavigationController into the storyboard (ensuring it also places a child view controller)

2. Select the new child view controller and in Identity>Class dropdown, select PlacesViewController:
![classes dropdown](HowTo/STORYBOARD_class_identity.png)

3. Add a new 'Push Modal' segue from the view controller you wish to launch the Places view controller from, to the new Navigation controller.

4. In the properties window, give the segue a custom Name

5. back in your view controller, add this action to a control such as a UIButton:
```csharp
MyButton.TouchUpInside += (sender, e) => // For example
{ 
    PerformSegue("MyCustomSegue", this);
};
```
6. Override the `PrepareForSegue` method as follows:
```csharp
public override void PrepareForSegue(UIStoryboardSegue segue, Foundation.NSObject sender)
    {
	    base.PrepareForSegue(segue, sender);

		if (segue.Identifier.Equals("MyCustomSegue"))
		{ 
			var vc = (PlacesViewController)segue.DestinationViewController.ChildViewControllers[0];
			vc.apiKey = "<Your API key here>";
			// TODO - Set Placetype and other parameters
			vc.PlaceSelected += HandlePlaceSelection;
		}
	}
```

7. Finally, create the `HandlePlaceSelection()`, which will be called whenever a place is selected:
```csharp
void HandlePlaceSelection(object sender, JObject placeDetails)
{
	// Handle as you wish
	Console.WriteLine($"{placeDetails}");
}
```


### Styling

You can style the Places view controller to match your app's UI:
```csharp
// If using programmatically, style the view after placing in container UINavigationController:

placesViewContainer = new UINavigationController(placesViewController);

placesViewController.NavigationController.NavigationBar.BarStyle = UIBarStyle.Default;
placesViewController.NavigationController.NavigationBar.Translucent = false;
placesViewController.NavigationController.NavigationBar.TintColor = UIColor.Magenta;
placesViewController.NavigationController.NavigationBar.BarTintColor = UIColor.Yellow;
placesViewController.Title = "Type Address";
```
```csharp
// If using with storyboards, style the view in the PrepareForSegue override:

public override void PrepareForSegue(UIStoryboardSegue segue, Foundation.NSObject sender)
{
    base.PrepareForSegue(segue, sender);

    if (segue.Identifier.Equals("MyCustomSegue"))
    {
        var vc = (PlacesViewController)segue.DestinationViewController.ChildViewControllers[0];
        vc.apiKey = "<Your API key here>";

        vc.NavigationController.NavigationBar.BarStyle = UIBarStyle.Default;
        vc.NavigationController.NavigationBar.Translucent = false;
        vc.NavigationController.NavigationBar.TintColor = UIColor.Magenta;
        vc.NavigationController.NavigationBar.BarTintColor = UIColor.Yellow;
        vc.Title = "Type Address";

        vc.PlaceSelected += HandlePlaceSelection;
    }
}
```