using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using Fact.Extensions.Collection;
using Fact.Extensions.Caching;
using Fact.Extensions.Collection.Interceptor;
using Android.Util;

// Had to fiddle with deploy to get debugging to work.  Viva la FIDDLY XAMARIN:
// http://stackoverflow.com/questions/32589438/xamarin-android-visual-studio-2015-could-not-connect-to-the-debugger
namespace Fact.Extensions.Collection.TestApp
{
    [Activity(Label = "Fact.Extensions.Collection.TestApp", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        int count = 1;
        const string TAG = nameof(MainActivity);

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var sharedPreferences = Android.Preferences.PreferenceManager.GetDefaultSharedPreferences(this);
            var bag = new PreferenceBag(sharedPreferences);

            bag.PropertyChanged += Bag_PropertyChanged;

            // NOTE: seems to be some kind of memory leak/aggressive freeing situation here
            var prefs = sharedPreferences.ToInterface<IAppPreferences>();
            //var prefs = bag.ToInterface<IAppPreferences>();
            var testPref1 = prefs.TestPref1;
            prefs.TestPref1 = "testing pref: " + DateTime.Now;
            var testOne = Resources.GetString(Resource.String.testOne);
            bag.Set("test.pref", "testing pref!");
            var testPref = bag.Get<string>("test.pref");
            testPref1 = bag.Get<string>("TestPref1");

            bag.PropertyChanged -= Bag_PropertyChanged;
            
            //bag.Set(null, typeof(int));
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.MyButton);

            button.Click += delegate { button.Text = string.Format("{0} clicks!", count++); };
        }

        private void Bag_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Log.Info(TAG, "Prop changed: " + e.PropertyName);
        }
    }
}

