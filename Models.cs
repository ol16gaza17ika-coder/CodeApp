using SQLite;
namespace CodeTamagotchi;
// Таблица пользователей: ФИО, почта, хэш пароля
public class User { [PrimaryKey, AutoIncrement] public int Id { get; set; } public string FullName { get; set; } = ""; [Unique] public string Email { get; set; } = ""; public string Hash { get; set; } = ""; public string Salt { get; set; } = ""; }
// Питомец и общий прогресс
public class Pet { [PrimaryKey] public int UserId { get; set; } public string Name { get; set; } = ""; public int Xp { get; set; } public int Streak { get; set; } public DateTime LastFed { get; set; } public DateTime LastDay { get; set; } }
// Прогресс по уровням
public class LevelProgress { [PrimaryKey, AutoIncrement] public int Id { get; set; } public int UserId { get; set; } public int Level { get; set; } public int Correct { get; set; } public int Total { get; set; } public DateTime CompletedAt { get; set; } }
