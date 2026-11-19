namespace CodeTamagotchi;
public partial class LoginPage : ContentPage
{
    bool _anim;

    public LoginPage() => InitializeComponent();

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_anim) return;
        _anim = true;
        await Egg.RotateToAsync(8, 600);
        Wiggle();
    }

    async void Wiggle()
    {
        while (_anim)
        {
            await Egg.RotateToAsync(-10, 500);
            await Egg.RotateToAsync(10, 500);
        }
    }

    protected override void OnDisappearing()
    {
        _anim = false;
        base.OnDisappearing();
    }

    async void OnLogin(object? s, EventArgs e)
    {
        try
        {
            if (await Db.Login(Email.Text, Pass.Text)) Application.Current!.MainPage = new NavigationPage(new HomePage()) { BarBackgroundColor = Color.FromArgb("#F6F4EF") };
            else { Err.Text = "Неверный email или пароль"; Err.IsVisible = true; }
        }
        catch (Exception ex)
        {
            Err.Text = $"Ошибка входа: {ex.Message}";
            Err.IsVisible = true;
        }
    }
    async void OnReg(object? s, EventArgs e) => await Navigation.PushAsync(new RegisterPage());
}
