﻿using Android.App;
using Android.Content;
using Avalonia.Android;

namespace GesturesDemo.Android;

[Activity(Theme = "@style/MyTheme.Splash", MainLauncher = true, NoHistory = true)]
public class SplashActivity : AvaloniaSplashActivity<App>
{
    protected override void OnResume()
    {
        base.OnResume();

        StartActivity(new Intent(Application.Context, typeof(MainActivity)));
    }
}
