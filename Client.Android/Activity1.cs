using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using RunGun.Client;

namespace AndroidClient
{
	[Activity(Label = "Client.Android"
		, MainLauncher = true
		, Icon = "@drawable/icon"
		, Theme = "@style/Theme.Splash"
		, AlwaysRetainTaskState = true
		, LaunchMode = Android.Content.PM.LaunchMode.SingleInstance
		, ScreenOrientation = ScreenOrientation.FullUser
		, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.ScreenLayout)]
	public class Activity1 : Microsoft.Xna.Framework.AndroidGameActivity
	{
		protected override void OnCreate(Bundle bundle) {
			base.OnCreate(bundle);
			var g = new ClientMain("nick", "127.0.0.1", "22222");
			SetContentView((View)g.Services.GetService(typeof(View)));
			g.Run();
		}
	}
}

