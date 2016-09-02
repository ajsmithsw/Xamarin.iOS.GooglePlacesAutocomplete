using Foundation;
using System;
using System.Diagnostics;
using UIKit;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json.Linq;
using CoreGraphics;
using Newtonsoft.Json;

namespace GPA
{
	// Event handler for sending LocationObject to parent view controller
	public delegate void PlaceSelected(object sender, JObject locationData);

	public enum PlaceType
	{
		// TODO - Set this up like the swift wrapper... make this a static class instead?
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
		UISearchBar searchBar;
		// TODO - UIImageView googleAttribution;
		UITableView resultsTable;
		UITableViewSource tableSource;

		public string apiKey { get; set; }


		public event PlaceSelected PlaceSelected;

		public PlacesViewController(IntPtr handle) : base(handle) { }

		public PlacesViewController() { }


		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			EdgesForExtendedLayout = UIRectEdge.None;

			var backgroundView = new UIView(View.Frame);
			backgroundView.BackgroundColor = UIColor.Yellow;
			View.AddSubview(backgroundView);

			searchBar = new UISearchBar();
			searchBar.TranslatesAutoresizingMaskIntoConstraints = false;
			searchBar.ReturnKeyType = UIReturnKeyType.Done;
			View.AddSubview(searchBar);
			AddSearchBarConstraints();
			searchBar.BecomeFirstResponder();

			// TODO - add 'powered by google' attribution image before resultsTable

			resultsTable = new UITableView();
			tableSource = new ResultsTableSource();
			resultsTable.Source = tableSource;
			((ResultsTableSource)resultsTable.Source).apiKey = apiKey;
			((ResultsTableSource)resultsTable.Source).RowItemSelected += OnPlaceSelection;
			resultsTable.TranslatesAutoresizingMaskIntoConstraints = false;
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


		void AddResultsTableConstraints()
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
			var placeType = "(cities)";
			return $"{url}?input={input}&types={placeType}&key={apiKey}";
		}


	}


	public class ResultsTableSource : UITableViewSource
	{
		public LocationPredictions predictions { get; set; }
		string cellIdentifier = "TableCell";
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
			// request a recycled cell to save memory
			UITableViewCell cell = tableView.DequeueReusableCell(cellIdentifier);
			// if there are no cells to reuse, create a new one
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
				Debug.WriteLine($"uh oh... {e}");
			}
		}

		string CreateDetailsRequestUri(string place_id)
		{
			var url = "https://maps.googleapis.com/maps/api/place/details/json";
			return $"{url}?placeid={Uri.EscapeUriString(place_id)}&key={apiKey}";
		}
	}






}
