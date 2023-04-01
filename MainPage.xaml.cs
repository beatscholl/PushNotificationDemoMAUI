using CommunityToolkit.Mvvm.Messaging;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;
using PushNotificationDemoMAUI.Models;
using System.Reflection;
using System.Text;
using Flurl.Http;
using System.Net;

namespace PushNotificationDemoMAUI;

public partial class MainPage : ContentPage
{    
    private string _deviceToken;
    private string? DeviceToken => _deviceToken ??= Preferences.ContainsKey("DeviceToken") 
        ? Preferences.Get("DeviceToken", "") 
        : null;

    public MainPage()
    {
        InitializeComponent();

        WeakReferenceMessenger.Default.Register<PushNotificationReceived>(this, (r, m) =>
        {
            string msg = m.Value;
            NavigateToPage();
        });        

        NavigateToPage();
    }

    private void NavigateToPage()
    {

        if (Preferences.ContainsKey("NavigationID"))
        {
            string id = Preferences.Get("NavigationID", "");
            if (id == "1")
            {
                AppShell.Current.GoToAsync(nameof(NewPage1));
            }
            if (id == "2")
            {
                AppShell.Current.GoToAsync(nameof(NewPage2));
            }
            Preferences.Remove("NavigationID");
        }
    }

    private async void OnCounterClicked(object sender, EventArgs e)
    {
        string projectId = "e-mergency-maui-prototype";

        Dictionary<string, string> androidNotificationObject = new()
        {
            { "NavigationID", "2" }
        };

        var pushNotificationRequest = new
        {
            message = new
            {
                token = DeviceToken,
                notification = new
                {
                    title = "Notification Title",
                    body = "Notification body",
                },
                data = androidNotificationObject,
            }
        };

        string url = $"https://fcm.googleapis.com/v1/projects/{projectId}/messages:send";

        try
        {
            var response = await url.WithOAuthBearerToken(await GetAccessTokenAsync())
                                    .PostJsonAsync(pushNotificationRequest);

            if (response.StatusCode == (int)HttpStatusCode.OK)
            {
                await App.Current.MainPage.DisplayAlert("Notification sent", "notification sent", "OK");
            }
        }
        catch (FlurlHttpException ex)
        {
            Console.WriteLine("an error occured! error: {0}", ex.Message);
            throw;
        }
    }

    private async Task<string> GetAccessTokenAsync()
    {
        // Replace with the name of your service account JSON file
        string serviceAccountJsonFileName = "service-account.json";
        string resourceName = $"{Assembly.GetExecutingAssembly().GetName().Name}.Resources.{serviceAccountJsonFileName}";

        GoogleCredential credential;
        using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
        {
            if (stream == null)
            {
                throw new FileNotFoundException($"Resource not found: {resourceName}");
            }

            credential = GoogleCredential.FromStream(stream).CreateScoped("https://www.googleapis.com/auth/firebase.messaging");
        }

        ITokenAccess tokenAccess = credential;
        var accessToken = await tokenAccess.GetAccessTokenForRequestAsync("https://www.googleapis.com/auth/firebase.messaging");
        return accessToken;
    }
}

