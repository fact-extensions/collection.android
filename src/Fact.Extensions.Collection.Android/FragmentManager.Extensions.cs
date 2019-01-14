using Android.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Collection
{
    public static class FragmentManager_Extensions
    {
        /// <summary>
        /// Calls BeginTransaction, then calls tx.Replace then tx.Commit
        /// </summary>
        /// <param name="fragmentManager"></param>
        public static void Show(this FragmentManager fragmentManager, int containerViewId, Fragment fragment)
        {
            FragmentTransaction tx = fragmentManager.BeginTransaction();
            tx.Replace(containerViewId, fragment);
            tx.Commit();
        }


        /// <summary>
        /// Calls BeginTransaction, then calls tx.Replace then tx.Commit
        /// </summary>
        /// <param name="fragmentManager"></param>
        public static T Show<T>(this FragmentManager fragmentManager, int containerViewId)
            where T : Fragment, new()
        {
            var fragment = new T();
            Show(fragmentManager, containerViewId, fragment);
            return fragment;
        }

        /// <summary>
        /// Calls BeginTransaction, then calls tx.Replace then tx.Commit
        /// </summary>
        /// <param name="fragmentManager"></param>
        public static void Show(this Android.Support.V4.App.FragmentManager fragmentManager, 
            int containerViewId,
            Android.Support.V4.App.Fragment fragment)
        {
            Android.Support.V4.App.FragmentTransaction tx = fragmentManager.BeginTransaction();
            tx.Replace(containerViewId, fragment);
            tx.Commit();
        }


        /// <summary>
        /// Calls BeginTransaction, then calls tx.Replace then tx.Commit
        /// </summary>
        /// <param name="fragmentManager"></param>
        public static T Show<T>(this Android.Support.V4.App.FragmentManager fragmentManager, int containerViewId)
            where T : Android.Support.V4.App.Fragment, new()
        {
            var fragment = new T();
            Show(fragmentManager, containerViewId, fragment);
            return fragment;
        }
    }
}
