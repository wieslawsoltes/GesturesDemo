﻿using Android.App;
using Android.Content.PM;
using Avalonia.Android;

namespace GesturesDemo.Android;

[Activity(Label = "GesturesDemo", Theme = "@style/MyTheme.NoActionBar", Icon = "@drawable/icon", LaunchMode = LaunchMode.SingleInstance, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
public class MainActivity : AvaloniaMainActivity
{
}
