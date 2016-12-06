using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Fact.Extensions.Collection.TestApp
{
    public interface IAppPreferences
    {
        string TestPref1 { get; set; }
    }
}