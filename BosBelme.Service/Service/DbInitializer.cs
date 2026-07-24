namespace BosBelme.Service.Service;

// Класс иницилизации данных в БД
public static class DbInitializer
{
    // Создает игры автоматически
    public static void Seed(AppDbContext context)
    {
        var gamesSet = context.Set<Game>();
        var playersCountSet = context.Set<PlayersCount>();

        if (!gamesSet.Any(g => g.NameGame == "Tank-A-Catch"))
        {
            Console.WriteLine("[DB_SEED] Игра 'Tank-A-Catch' отсутствует. Начинаем добавление...");

            var bounceGame = new Game
            {
                NameGame = "Tank-A-Catch",
                Discription = "Игра, где твое собственное оружие гарантированно станет причиной твоей гибели." +
                "\r\nПравила просты: у каждого по одной пуле. Ты стреляешь, промахиваешься, пуля начинает рикошетить с дикой скоростью, " +
                "а твой «друг» ловит ее корпусом, смотрит тебе в глаза и медленно прямой наводкой делает ВЫСТРЕЛ!!!",
                IsStrictRange = false
            };

            gamesSet.Add(bounceGame);

            var playersCountConfig = new PlayersCount
            {
                Game = bounceGame,
                Count = 2
            };

            playersCountSet.Add(playersCountConfig);

            context.SaveChanges();
            Console.WriteLine("[DB_SEED] Игра 'Tank-A-Catch' и конфигурация игроков (2 чел.) успешно добавлены в PostgreSQL!");
        }
    }
}
