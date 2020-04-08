using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using RunGun.Client;
using System;
using System.Net;

namespace RunGun.AndroidClient
{
	[Activity(Label = "Client.Android"
		, MainLauncher = true
		, Icon = "@drawable/icon"
		, Theme = "@style/Theme.Splash"
		, AlwaysRetainTaskState = false
		, LaunchMode = Android.Content.PM.LaunchMode.SingleInstance
		, ScreenOrientation = ScreenOrientation.Landscape
		, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.ScreenLayout)]
	public class Activity1 : Microsoft.Xna.Framework.AndroidGameActivity
	{

		protected override void OnCreate(Bundle bundle) {
			//this.SetRequestedOrientation(ActivityInfo.SCREEN_ORIENTATION_PORTRAIT);
			AndroidEnvironment.UnhandledExceptionRaiser += (sender, args) => {
				var ex = args.Exception;

				Console.WriteLine("OH FUCK " + ex.Source + " " + ex.Message + " " + ex.StackTrace + " " + ex.TargetSite);
			};

			base.OnCreate(bundle);
			AndroidClient game = new AndroidClient();
			game.Nickname = "androidpl";
			SetContentView((View)game.Services.GetService(typeof(View)));

			game.ConnectToServer(new IPEndPoint(IPAddress.Parse("192.168.0.2"), 22222));

			game.Run();

		}
	}
}

