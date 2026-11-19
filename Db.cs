using System.Security.Cryptography;
using SQLite;
using Plugin.LocalNotification.Core.Models;

#if ANDROID || IOS
using Plugin.LocalNotification;
#endif
namespace CodeTamagotchi;
public static class Db
{
    static SQLiteAsyncConnection? c;
    public static User? Current { get; set; }

    public static readonly int[] Next = { 30, 100, 200, 350 };
    public static readonly string[] StageName = { "Хамелеончик", "Юный хамелеон", "Умный хамелеон", "Мастер C#" };
    public static int Stage(int xp) => xp < 30 ? 0 : xp < 100 ? 1 : xp < 200 ? 2 : 3;

    static async Task Init()
    {
        if (c != null) return;
        c = new SQLiteAsyncConnection(Path.Combine(FileSystem.AppDataDirectory, "uchim_csharp.db3"));
        await c.CreateTablesAsync<User, Pet, LevelProgress>();
    }
    static string Hash(string p, string salt) =>
        Convert.ToBase64String(Rfc2898DeriveBytes.Pbkdf2(p, Convert.FromBase64String(salt), 100_000, HashAlgorithmName.SHA256, 32));

    public static async Task<string?> Register(string fullName, string email, string pass, string petName)
    {
        await Init();
        fullName = (fullName ?? "").Trim();
        email = (email ?? "").Trim().ToLowerInvariant();
        pass ??= "";
        petName = (petName ?? "").Trim();
        if (fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length < 2) return "Укажите ФИО полностью";
        if (!email.Contains('@') || petName.Length == 0) return "Проверьте email и имя питомца";
        if (pass.Length < 6) return "Пароль — минимум 6 символов";
        if (await c!.Table<User>().Where(u => u.Email == email).FirstOrDefaultAsync() != null) return "Эта почта уже занята";
        var salt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
        var u = new User { FullName = fullName, Email = email, Salt = salt, Hash = Hash(pass, salt) };
        await c!.InsertAsync(u);
        await c.InsertAsync(new Pet { UserId = u.Id, Name = petName, LastFed = DateTime.Now, LastDay = DateTime.MinValue });
        Current = u; return null;
    }
    public static async Task<bool> Login(string email, string pass)
    {
        await Init(); email = (email ?? "").Trim().ToLowerInvariant();
        var u = await c!.Table<User>().Where(x => x.Email == email).FirstOrDefaultAsync();
        if (u == null || Hash(pass ?? "", u.Salt) != u.Hash) return false;
        Current = u; return true;
    }
    public static async Task<Pet> GetPet()
    {
        await Init();
        var user = Current ?? throw new InvalidOperationException("Пользователь не авторизован");
        var pet = await c!.Table<Pet>().Where(x => x.UserId == user.Id).FirstOrDefaultAsync();
        if (pet != null) return pet;

        pet = new Pet { UserId = user.Id, Name = "Хамелеон", LastFed = DateTime.Now, LastDay = DateTime.MinValue };
        await c!.InsertAsync(pet);
        return pet;
    }
    public static async Task<int> DoneLevels()
    {
        await Init();
        var user = Current ?? throw new InvalidOperationException("Пользователь не авторизован");
        return await c!.Table<LevelProgress>().Where(x => x.UserId == user.Id).CountAsync();
    }

    public static async Task AddXp(int n)
    {
        var p = await GetPet(); var today = DateTime.Today;
        if (p.LastDay != today) p.Streak = p.LastDay == today.AddDays(-1) ? p.Streak + 1 : 1;
        p.LastDay = today; p.LastFed = DateTime.Now; p.Xp += n;
        await c!.UpdateAsync(p);
    }
    public static async Task CompleteLevel(int level, int correct, int total)
    {
        await Init();
        var user = Current ?? throw new InvalidOperationException("Пользователь не авторизован");
        var ex = await c!.Table<LevelProgress>().Where(x => x.UserId == user.Id && x.Level == level).FirstOrDefaultAsync();
        if (ex == null) await c.InsertAsync(new LevelProgress { UserId = user.Id, Level = level, Correct = correct, Total = total, CompletedAt = DateTime.Now });
        await AddXp(30);
    }

    // Ежедневное напоминание в 19:00 (работает на Android и iOS)
    public static async Task ScheduleReminder()
    {
#if ANDROID || IOS
        if (!await LocalNotificationCenter.Current.AreNotificationsEnabled()) await LocalNotificationCenter.Current.RequestNotificationPermission();
        LocalNotificationCenter.Current.Cancel(1);
        var t = DateTime.Today.AddHours(19); if (t < DateTime.Now) t = t.AddDays(1);
        await LocalNotificationCenter.Current.Show(new NotificationRequest
        {
            NotificationId = 1,
            Title = "Хамелеон скучает",
            Description = "Пройди урок и сохрани стрик!",
            Schedule = new NotificationRequestSchedule { NotifyTime = t, RepeatType = NotificationRepeat.Daily }
        });
#else
        await Task.CompletedTask;
#endif
    }
}
