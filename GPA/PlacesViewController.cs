using Foundation;
using System;
using System.Diagnostics;
using UIKit;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace DurianCode.iOS.Places
{
	public delegate void PlaceSelected(object sender, JObject locationData);

	public enum PlaceType
	{
		All, Geocode, Address, Establishment, Regions, Cities
	}

	public class LocationBias
	{
		public readonly double latitude;
		public readonly double longitude;
		public readonly int radius;

		public LocationBias(double latitude, double longitude, int radius) 
		{
			this.latitude = latitude;
			this.longitude = longitude;
			this.radius = radius;
		}

		public override string ToString()
		{
			return $"&location={latitude},{longitude}&radius={radius}";
		}
	}

	public class LocationObject
	{
		public double lat { get; set; }
		public double lon { get; set; }
		public string placeName { get; set; }
		public string placeID { get; set; }
	}

	public partial class PlacesViewController : UIViewController
	{
		public PlacesViewController(IntPtr handle) : base(handle) { }

		public PlacesViewController() { }


		public UIView backgroundView;
		UISearchBar searchBar;
		UIImageView googleAttribution;
		UITableView resultsTable;
		UITableViewSource tableSource;
		public string apiKey { get; set; }
		LocationBias locationBias;
		string CustomPlaceType;


		public event PlaceSelected PlaceSelected;


		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			EdgesForExtendedLayout = UIRectEdge.None;

			backgroundView = new UIView(View.Frame);
			backgroundView.BackgroundColor = UIColor.White;
			View.AddSubview(backgroundView);

			searchBar = new UISearchBar();
			searchBar.TranslatesAutoresizingMaskIntoConstraints = false;
			searchBar.ReturnKeyType = UIReturnKeyType.Done;
			View.AddSubview(searchBar);
			AddSearchBarConstraints();
			searchBar.BecomeFirstResponder();

			// TODO - add 'powered by google' attribution image before resultsTable
			googleAttribution = new UIImageView();
			googleAttribution.Image = UIImage.FromBundle("powered_by_google_on_white");
			googleAttribution.TranslatesAutoresizingMaskIntoConstraints = false;
			googleAttribution.ContentMode = UIViewContentMode.ScaleAspectFit;
			View.AddSubview(googleAttribution);
			AddAttributionConstraints();

			resultsTable = new UITableView();
			tableSource = new ResultsTableSource();
			resultsTable.TranslatesAutoresizingMaskIntoConstraints = false;
			resultsTable.Source = tableSource;
			((ResultsTableSource)resultsTable.Source).apiKey = apiKey;
			((ResultsTableSource)resultsTable.Source).RowItemSelected += OnPlaceSelection;
			View.AddSubview(resultsTable);
			AddResultsTableConstraints();

			searchBar.TextChanged += SearchInputChanged;
			resultsTable.Hidden = true;

			NavigationItem.SetLeftBarButtonItem(
				new UIBarButtonItem(UIBarButtonSystemItem.Stop, (sender, args) =>
			{
				DismissViewController(true, null);
			}), true);

		}

		void AddSearchBarConstraints()
		{
			if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
			{
				var sbLeft = searchBar.LeftAnchor.ConstraintEqualTo(View.LeftAnchor);
				var sbRight = searchBar.RightAnchor.ConstraintEqualTo(View.RightAnchor);
				var sbTop = searchBar.TopAnchor.ConstraintEqualTo(View.TopAnchor);
				var sbHeight = searchBar.HeightAnchor.ConstraintEqualTo(45.0f);
				NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[]
				{
				sbLeft, sbRight, sbTop, sbHeight
				});
				UpdateViewConstraints();
			}
			else
			{
				searchBar.Frame = new CoreGraphics.CGRect(0, 0, View.Frame.Width, 45.0f);
			}
		}

		void AddAttributionConstraints()
		{
			if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
			{
				var gaTop = NSLayoutConstraint.Create(googleAttribution,
													  NSLayoutAttribute.Top,
													  NSLayoutRelation.Equal,
													  searchBar,
													  NSLayoutAttribute.Bottom,
													  1, 30.0f);
				var gaCenterX = googleAttribution.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor);
				var gaWidth = googleAttribution.WidthAnchor.ConstraintEqualTo(100.0f);
				var gaHeight = googleAttribution.HeightAnchor.ConstraintEqualTo(20.0f);
				NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[]
				{
					gaTop, gaCenterX, gaWidth, gaHeight
				});
				UpdateViewConstraints();
			}
			else
			{
				googleAttribution.Frame = new CoreGraphics.CGRect(
					(View.Frame.Width / 2) - (100 / 2),
					searchBar.Frame.Bottom + 30,
					100,
					20);
			}
		}

		void AddResultsTableConstraints()
		{
			if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
			{
				var rtLeft = resultsTable.LeftAnchor.ConstraintEqualTo(View.LeftAnchor);
				var rtRight = resultsTable.RightAnchor.ConstraintEqualTo(View.RightAnchor);
				var rtTop = resultsTable.TopAnchor.ConstraintEqualTo(searchBar.BottomAnchor);
				var rtBottom = resultsTable.BottomAnchor.ConstraintEqualTo(View.BottomAnchor);
				NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[]
				{
				rtLeft, rtRight, rtTop, rtBottom
				});
				UpdateViewConstraints();
			}
			else
			{
				resultsTable.Frame = new CoreGraphics.CGRect(0, 45.0f, View.Frame.Width, View.Frame.Height - 45.0f);
			}
		}

		public void SetLocationBias(LocationBias locationBias)
		{
			this.locationBias = locationBias;
		}

		public void SetPlaceType(PlaceType placeType)
		{
			switch (placeType)
			{
				case PlaceType.All:
					CustomPlaceType = "";
					break;
				case PlaceType.Geocode:
					CustomPlaceType = "geocode";
					break;
				case PlaceType.Address:
					CustomPlaceType = "address";
					break;
				case PlaceType.Establishment:
					CustomPlaceType = "establishment";
					break;
				case PlaceType.Regions:
					CustomPlaceType = "(regions)";
					break;
				case PlaceType.Cities:
					CustomPlaceType = "(cities)";
					break;
			}
		}

		async void SearchInputChanged(object sender, UISearchBarTextChangedEventArgs e)
		{
			if (e.SearchText == "")
			{
				resultsTable.Hidden = true;
			}
			else
			{
				resultsTable.Hidden = false;
				var predictions = await GetPlaces(e.SearchText);
				UpdateTableWithPredictions(predictions);
			}
		}

		async Task<string> GetPlaces(string searchText)
		{
			if (searchText == "")
				return "";

			var requestURI = CreatePredictionsUri(searchText);

			try
			{
				WebRequest request = WebRequest.Create(requestURI);
				request.Method = "GET";
				request.ContentType = "application/json";
				WebResponse response = await request.GetResponseAsync();
				string responseStream = string.Empty;
				using (StreamReader sr = new StreamReader(response.GetResponseStream()))
				{
					responseStream = sr.ReadToEnd();
				}
				response.Close();
				return responseStream;
			}
			catch
			{
				Debug.WriteLine("Something's going wrong with my HTTP request");
				return "ERROR";
			}
		}

		void UpdateTableWithPredictions(string predictions)
		{
			if (predictions == "")
				return;

			if (predictions == "ERROR")
				return; // TODO - handle this better

			var deserializedPredictions = JsonConvert.DeserializeObject<LocationPredictions>(predictions);

			((ResultsTableSource)resultsTable.Source).predictions = deserializedPredictions;
			resultsTable.ReloadData();
		}

		protected virtual void OnPlaceSelection(object sender, JObject location)
		{
			if (PlaceSelected != null)
				PlaceSelected(this, location);

			DismissViewController(true, null);
		}


		string CreatePredictionsUri(string searchText)
		{
			var url = "https://maps.googleapis.com/maps/api/place/autocomplete/json";
			var input = Uri.EscapeUriString(searchText);

			var pType = "";
			if (CustomPlaceType != null)
				pType = CustomPlaceType;

			var constructedUrl = $"{url}?input={input}&types={pType}&key={apiKey}";

			if (this.locationBias != null)
				constructedUrl = constructedUrl + locationBias;

			Console.WriteLine(constructedUrl);
			return constructedUrl;
		}

	}


	public class ResultsTableSource : UITableViewSource
	{
		public LocationPredictions predictions { get; set; }
		const string cellIdentifier = "TableCell";
		public event PlaceSelected RowItemSelected;
		public string apiKey { get; set; }

		public ResultsTableSource()
		{
			predictions = new LocationPredictions();
		}

		public ResultsTableSource(LocationPredictions predictions)
		{
			this.predictions = predictions;
		}

		public override nint RowsInSection(UITableView tableview, nint section)
		{
			if (predictions.Predictions != null)
				return predictions.Predictions.Count;

			return 0;
		}

		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{
			UITableViewCell cell = tableView.DequeueReusableCell(cellIdentifier);

			if (cell == null)
				cell = new UITableViewCell(UITableViewCellStyle.Default, cellIdentifier);

			cell.TextLabel.Text = predictions.Predictions[indexPath.Row].Description;

			return cell;
		}

		public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
		{
			var selectedPrediction = predictions.Predictions[indexPath.Row].Place_ID;
			ReturnPlaceDetails(selectedPrediction);
		}

		async void ReturnPlaceDetails(string selectionID)
		{
			try
			{
				WebRequest request = WebRequest.Create(CreateDetailsRequestUri(selectionID));
				request.Method = "GET";
				request.ContentType = "application/json";
				WebResponse response = await request.GetResponseAsync();
				string responseStream = string.Empty;
				using (StreamReader sr = new StreamReader(response.GetResponseStream()))
				{
					responseStream = sr.ReadToEnd();
				}
				response.Close();

				JObject jObject = JObject.Parse(responseStream);
				if (jObject != null && RowItemSelected != null)
					RowItemSelected(this, jObject);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		string CreateDetailsRequestUri(string place_id)
		{
			var url = "https://maps.googleapis.com/maps/api/place/details/json";
			return $"{url}?placeid={Uri.EscapeUriString(place_id)}&key={apiKey}";
		}
	}






}
