namespace CodeTamagotchi;
public partial class RegisterPage : ContentPage
{
    public RegisterPage() => InitializeComponent();
    async void OnRegister(object? s, EventArgs e)
    {
        try
        {
            var err = await Db.Register(Name.Text, Email.Text, Pass.Text, PetName.Text);
            if (err != null) { Err.Text = err; Err.IsVisible = true; return; }
            Application.Current!.MainPage = new NavigationPage(new HomePage()) { BarBackgroundColor = Color.FromArgb("#F6F4EF") };
        }
        catch (Exception ex)
        {
            Err.Text = $"Ошибка регистрации: {ex.Message}";
            Err.IsVisible = true;
        }
    }
}
