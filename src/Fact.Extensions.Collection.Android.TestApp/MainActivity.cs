using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using Fact.Extensions.Collection;

// Had to fiddle with deploy to get debugging to work.  Viva la FIDDLY XAMARIN:
// http://stackoverflow.com/questions/32589438/xamarin-android-visual-studio-2015-could-not-connect-to-the-debugger
namespace Fact.Extensions.Collection.Android.TestApp
{
    [Activity(Label = "Fact.Extensions.Collection.Android.TestApp", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        int count = 1;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var bag = new PreferencesBag(global::Android.Preferences.PreferenceManager.GetDefaultSharedPreferences(this));

            bag.Set("test.pref", "testing pref!");
            var testPref = bag.Get<string>("test.pref");
            //bag.Set(null, typeof(int));
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.MyButton);

            button.Click += delegate { button.Text = string.Format("{0} clicks!", count++); };
        }
    }
}

