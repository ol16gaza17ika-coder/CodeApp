#if ANDROID || IOS
using Plugin.LocalNotification;
#endif
namespace CodeTamagotchi;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var b = MauiApp.CreateBuilder().UseMauiApp<App>();
#if ANDROID || IOS
        b.UseLocalNotification();
#endif
        return b.Build();
    }
}
