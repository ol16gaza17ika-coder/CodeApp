namespace CodeTamagotchi;
public partial class HomePage : ContentPage
{
    bool _anim;
    public HomePage() => InitializeComponent();
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            var user = Db.Current;
            if (user == null)
            {
                Application.Current!.MainPage = new NavigationPage(new LoginPage());
                return;
            }

            var p = await Db.GetPet();
            var done = await Db.DoneLevels();
            var parts = user.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            Hello.Text = $"Привет, {(parts.Length > 1 ? parts[1] : parts[0])}!";
            Streak.Text = $"Стрик: {p.Streak}";
            int st = Db.Stage(p.Xp), prev = st == 0 ? 0 : Db.Next[st - 1], nx = Db.Next[st];
            PetName.Text = $"{p.Name} · {Db.StageName[st]}";
            Bar.Progress = st == 3 ? 1 : (double)(p.Xp - prev) / (nx - prev);
            XpText.Text = $"{p.Xp} очков знаний";
            Profile.Text = $"{user.FullName} · {user.Email}";
            var h = (DateTime.Now - p.LastFed).TotalHours;
            int mood = h < 20 ? 0 : h < 48 ? 1 : 2;
            Status.Text = new[] { $"{p.Name} доволен и готов к новым знаниям!", $"{p.Name} скучает. Пройдём урок вместе?", $"{p.Name} грустит… Он давно тебя ждёт!" }[mood];
            PetImg.Source = $"pet_{st}_{mood}.png";
            Levels.Children.Clear();
            for (int i = 0; i < Lessons.All.Length; i++)
            {
                int idx = i;
                var b = new Button { Text = $"{i + 1}. {Lessons.All[i].Title}" + (i < done ? "  — пройден" : i > done ? "  — закрыт" : ""), BackgroundColor = Colors.White, BorderColor = Color.FromArgb("#DDD8CC"), BorderWidth = 1, TextColor = (Color)Application.Current!.Resources["Ink"], CornerRadius = 10, HeightRequest = 52, Opacity = i > done ? .5 : 1 };
                b.Clicked += async (_, _) => { if (idx <= done) await Navigation.PushAsync(new LessonPage(idx)); };
                Levels.Children.Add(b);
            }

            if (!_anim) { _anim = true; Bounce(); }
            try { await Db.ScheduleReminder(); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Не удалось запланировать напоминание: {ex}"); }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка загрузки главной страницы: {ex}");
            await DisplayAlertAsync("Ошибка", "Не удалось загрузить данные профиля. Войдите снова.", "OK");
            Db.Current = null;
            Application.Current!.MainPage = new NavigationPage(new LoginPage());
        }
    }
    async void Bounce() { while (_anim) { await PetImg.TranslateToAsync(0, -8, 700, Easing.SinInOut); await PetImg.TranslateToAsync(0, 0, 700, Easing.SinInOut); } }
    protected override void OnDisappearing() { base.OnDisappearing(); _anim = false; }
    void OnLogout(object? s, EventArgs e) { Db.Current = null; Application.Current!.MainPage = new NavigationPage(new LoginPage()); }
}
