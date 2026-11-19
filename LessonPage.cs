using System.Text.RegularExpressions;
using Microsoft.Maui.Controls.Shapes;
namespace CodeTamagotchi;
// Один урок: теория -> вопросы (с повторением) -> задание на код
public class LessonPage : ContentPage
{
    readonly int _i; readonly Level _l; readonly VerticalStackLayout _s = new() { Padding = 24, Spacing = 12 };
    List<(Q q, bool rev)> _qs = new(); int _n, _ok;
    static readonly Color Ink = Color.FromArgb("#26332B"), Acc = Color.FromArgb("#3E9E6B");
    public LessonPage(int i)
    {
        _i = i; _l = Lessons.All[i]; Title = $"Уровень {i + 1}";
        Background = (Brush)Application.Current.Resources["Bg"]; Content = new ScrollView { Content = _s }; Theory();
    }
    static Label T(string s, double size = 16, bool bold = false, Color c = null) =>
        new Label { Text = s, FontSize = size, FontAttributes = bold ? FontAttributes.Bold : FontAttributes.None, TextColor = c ?? Ink };
    static Border Card(View v) => new Border { Style = (Style)Application.Current.Resources["CardBox"], Content = v };
    static Border Code(string s) => new Border { BackgroundColor = Color.FromArgb("#22262B"), Padding = 12, StrokeShape = new RoundRectangle { CornerRadius = 14 }, Content = new Label { Text = s, TextColor = Color.FromArgb("#B9E3C0"), FontFamily = "Consolas" } };
    static Button Btn(string t, Action a, string st = "Neon") { var b = new Button { Text = t, Style = (Style)Application.Current.Resources[st] }; b.Clicked += (_, _) => a(); return b; }
    void Add(params View[] v) { foreach (var x in v) _s.Children.Add(x); }

    void Theory()
    {
        _s.Children.Clear();
        Add(T($"УРОВЕНЬ {_i + 1}", 12, true, Acc), T(_l.Title, 24, true),
            Card(new VerticalStackLayout { Spacing = 10, Children = { T(_l.Theory), Code(_l.Example) } }));
        if (_i > 0) Add(T("В вопросах будет и повторение прошлых тем", 13, false, Color.FromArgb("#6F7A72")));
        Add(Btn("К вопросам", StartQuiz));
    }
    void StartQuiz()
    {
        int cnt = _i >= 3 ? 2 : _i >= 1 ? 1 : 0;
        var rev = Lessons.All.Take(_i).SelectMany(x => x.Qs).OrderBy(_ => Random.Shared.Next()).Take(cnt).Select(q => (q, true));
        _qs = _l.Qs.Select(q => (q, false)).Concat(rev).ToList(); _n = 0; _ok = 0; ShowQ();
    }
    void ShowQ()
    {
        if (_n >= _qs.Count) { CodeStep(); return; }
        _s.Children.Clear(); var (q, rev) = _qs[_n];
        Add(T($"Вопрос {_n + 1} из {_qs.Count}", 13, true, Acc));
        if (rev) Add(T("ПОВТОРЕНИЕ", 12, true, Acc));
        Add(Card(T(q.Text, 18, true)));
        var btns = new List<Button>(); var info = new VerticalStackLayout { Spacing = 10 };
        for (int k = 0; k < q.Options.Length; k++)
        {
            int idx = k;
            var b = new Button { Text = q.Options[k], BackgroundColor = Color.FromArgb("#EEEBE3"), TextColor = Ink, CornerRadius = 10, HeightRequest = 50 };
            b.Clicked += async (_, _) =>
            {
                foreach (var x in btns) x.IsEnabled = false;
                bool ok = idx == q.Answer; btns[q.Answer].BackgroundColor = Color.FromArgb("#3E9E6B"); btns[q.Answer].TextColor = Colors.White;
                if (!ok) { b.BackgroundColor = Color.FromArgb("#C8503F"); b.TextColor = Colors.White; }
                if (ok) { _ok++; await Db.AddXp(10); }
                info.Children.Add(Card(T(ok ? "Верно! +10 очков" : "Объяснение: " + q.Explain, 15, true)));
                info.Children.Add(Btn("Дальше", () => { _n++; ShowQ(); }));
            };
            btns.Add(b); Add(b);
        }
        Add(info);
    }
    void CodeStep()
    {
        _s.Children.Clear();
        var ed = new Editor { HeightRequest = 190, BackgroundColor = Color.FromArgb("#22262B"), TextColor = Color.FromArgb("#E6EDE8"), FontFamily = "Consolas", Placeholder = "// напиши код здесь" };
        var fb = new Label { TextColor = Ink, FontAttributes = FontAttributes.Bold };
        Add(T("ПРАКТИКА", 12, true, Acc), Card(T(_l.Task, 18, true)), ed, fb);
        Add(Btn("Проверить", async () =>
        {
            var r = _l.Checks.Select(c => (ok: Regex.IsMatch(ed.Text ?? "", c.Pattern), c.Msg)).ToList();
            if (r.All(x => x.ok))
            {
                await Db.CompleteLevel(_i, _ok, _qs.Count);
                _s.Children.Clear();
                Add(new Image { Source = "pet_3_0.png", HeightRequest = 150 }, T("Уровень пройден!", 24, true), T($"Верных ответов: {_ok} из {_qs.Count}. +30 очков за код"),
                    Btn("К урокам", async () => await Navigation.PopToRootAsync()));
            }
            else fb.Text = "Пока не хватает:\n" + string.Join("\n", r.Select(x => (x.ok ? "✓ " : "✗ ") + x.Msg));
        }));
        Add(Btn("Показать подсказку", () => fb.Text = _l.Solution, "Ghost"));
    }
}
