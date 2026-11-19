namespace CodeTamagotchi;
public partial class App : Application
{
    public App() { InitializeComponent(); MainPage = new NavigationPage(new LoginPage()) { BarBackgroundColor = Color.FromArgb("#F6F4EF") }; }
}
