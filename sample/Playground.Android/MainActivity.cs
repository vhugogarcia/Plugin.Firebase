﻿using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Firebase;
using Playground.Common.Services.Composition;
using Playground.Common.Services.Logging;
using Playground.Common.Services.Scheduler;
using Playground.Droid.Services.Composition;
using Plugin.Firebase.Auth;
using Plugin.Firebase.CloudMessaging;
using Plugin.Firebase.DynamicLinks;
using Xamarin.Forms;

namespace Playground.Droid
{
    // Activity attribute is not needed since MainActivity gets registered
    // in AndroidManifest.xml because of the Firebase Dynamic Link feature
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Forms.SetFlags("Markup_Experimental");
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            Forms.Init(this, savedInstanceState);
            LogOutputService.Initialize();
            FirebaseApp.InitializeApp(this);
            // googleRequestIdToken from Web Client instead of Android (see https://console.developers.google.com/apis/credentials)
            FirebaseAuthImplementation.Initialize(this, savedInstanceState, "537235599720-723cgj10dtm47b4ilvuodtp206g0q0fg.apps.googleusercontent.com");

            var compositionRoot = new CompositionRoot();
            ViewModelResolver.Initialize(compositionRoot);
            Schedulers.Initialize(compositionRoot.ResolveSchedulerService());
            HandleIntent(Intent);
            CreateNotificationChannelIfNeeded();
            LoadApplication(new App());
        }
        
        private static void HandleIntent(Intent intent)
        {
            FirebaseCloudMessagingImplementation.OnNewIntent(intent);
            FirebaseDynamicLinksImplementation.HandleDynamicLinkAsync(intent).Ignore();
        }
        
        private void CreateNotificationChannelIfNeeded()
        {
            if(Build.VERSION.SdkInt >= BuildVersionCodes.O) {
                CreateNotificationChannel();
            }
        }

        private void CreateNotificationChannel()
        {
            var channelId = $"{PackageName}.general";
            var notificationManager = (NotificationManager) GetSystemService(NotificationService);
            var channel = new NotificationChannel(channelId, "General", NotificationImportance.Default);
            notificationManager.CreateNotificationChannel(channel);
            FirebaseCloudMessagingImplementation.ChannelId = channelId;
        }
        
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            FirebaseAuthImplementation.HandleActivityResultAsync(requestCode, resultCode, data);
        }
        
        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            HandleIntent(intent);
        }
    }
}