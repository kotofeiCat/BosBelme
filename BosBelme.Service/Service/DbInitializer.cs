namespace BosBelme.Service.Service;

// Класс иницилизации данных в БД
public static class DbInitializer
{
    // Создает игры автоматически
    public static void Seed(AppDbContext context)
    {
        var gamesSet = context.Set<Game>();
        var playersCountSet = context.Set<PlayersCount>();

        if (!gamesSet.Any(g => g.NameGame == "One-Shot Bounce"))
        {
            Console.WriteLine("[DB_SEED] Игра 'One-Shot Bounce' отсутствует. Начинаем добавление...");

            var bounceGame = new Game
            {
                NameGame = "One-Shot Bounce",
                Discription = "Динамичная неоновая 2D-дуэль на выживание. Один выстрел, бесконечные рикошеты, щиты-ловушки и разрушаемые блоки!"
            };

            gamesSet.Add(bounceGame);

            var playersCountConfig = new PlayersCount
            {
                Game = bounceGame,
                Count = 2
            };

            playersCountSet.Add(playersCountConfig);

            context.SaveChanges();
            Console.WriteLine("[DB_SEED] Игра 'One-Shot Bounce' и конфигурация игроков (2 чел.) успешно добавлены в PostgreSQL!");
        }
    }
}
