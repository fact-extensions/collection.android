// Akavache Fact.Extensions support DLL not working at this time
//#define ENABLE_AKAVACHE

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
using Akavache;

using System.Reactive.Linq;

using System.Threading.Tasks;

// Had to fiddle with deploy to get debugging to work.  Viva la FIDDLY XAMARIN:
// http://stackoverflow.com/questions/32589438/xamarin-android-visual-studio-2015-could-not-connect-to-the-debugger
namespace Fact.Extensions.Collection.TestApp
{
    [Activity(Label = "Fact.Extensions.Collection.TestApp", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        int count = 1;
        const string TAG = nameof(MainActivity);

        static MainActivity()
        {
            BlobCache.ApplicationName = "Fact.Extensions.Collection.Android.TestApp";
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var sharedPreferences = Android.Preferences.PreferenceManager.GetDefaultSharedPreferences(this);
            var bag = new PreferenceBag(sharedPreferences);

            bag.PropertyChanged += Bag_PropertyChanged;

            // NOTE: seems to be some kind of memory leak/aggressive freeing situation here
            //var prefs = sharedPreferences.ToInterface<IAppPreferences>();
            var prefs = bag.ToInterface<IAppPreferences>();
            var testPref1 = prefs.TestPref1;
            prefs.TestPref1 = "testing pref: " + DateTime.Now;
            var testOne = Resources.GetString(Resource.String.testOne);
            bag.Set("test.pref", "testing pref!");
            var testPref = bag.Get<string>("test.pref");
            testPref1 = bag.Get<string>("TestPref1");
            try
            {
                // FIX: Generates null exception when it should be a DEBUG message.  Gotta set a policy
                // somewhere I think...
                bag.Set("crasher", new { anonymous = true });
            }
            catch(Exception e)
            {

            }

            bag.PropertyChanged -= Bag_PropertyChanged;
            
            //bag.Set(null, typeof(int));
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            //var akavacheBag = BlobCache.UserAccount.ToBag();
#if ENABLE_AKAVACHE
            var akavacheBag = BlobCache.InMemory.ToBag();
            var key = "test.cache";

            Log.Info(TAG, "Phase 1");

            var expiry = new SlidingTimeExpiration(TimeSpan.FromSeconds(30));
            akavacheBag.Set(key, "testing pref again!", typeof(string), expiry);

            Log.Info(TAG, "Phase 2");
            //akavacheBag.Set(key, "testing pref again!");
            var testCache = akavacheBag.Get<string>(key);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.MyButton);

            Task.Run(async delegate
            {
                var key2 = key + "2";
                var key3 = key + "2";
                byte[] byteArray = new byte[] { 1, 2, 3, 4, 5 };
                var _expiry = TimeSpan.FromSeconds(30);
                await BlobCache.InMemory.Insert(key2, byteArray, _expiry);
                var byteArray2 = await BlobCache.InMemory.Get(key2);
                await akavacheBag.SetAsync(key2, "test pref yet again!", typeof(string), expiry);
                testCache = await akavacheBag.GetAsync<string>(key3.Substring(0));
            }).ContinueWith(t =>
            {
                var e = t.Exception;
            }, TaskContinuationOptions.OnlyOnFaulted);

            button.Click += delegate { button.Text = string.Format("{0} clicks!", count++); };

            var btnAkavache = FindViewById<Button>(Resource.Id.btnAkavacheTest1);

            btnAkavache.Click += delegate 
            {
                var containsKey = akavacheBag.ContainsKey(key);
                Log.Info(TAG, "Test");
            };
#endif
        }

        private void Bag_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Log.Info(TAG, "Prop changed: " + e.PropertyName);
        }
    }
}

