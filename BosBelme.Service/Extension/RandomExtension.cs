using System.Security.Cryptography;
using System.Text;

namespace BosBelme.Service.Extension;

// Класс расширения для случайных генерайий.
public static class RandomExtension
{
    private static readonly char[] chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();

    private static readonly List<string> names = new List<string>
    {
        "CyberBosBelme 2077", "Босбелме и точка", "Осторожно, Босбелме закипает", "Босбелмешная №1",
        "Босбелме головного мозга", "Следствие ведут Босбелме", "Босбелме комнатного масштаба", "Ты не поверишь, это Босбелме",
        "Босбелме против Лямбда-выражений", "BosBelme 95 OSR2", "Клуб анонимных Босбелмеров", "No SQL, Only BosBelme", 
        "BosBelme microservices network", "Семь раз отмерь, один раз Босбелме", "Босбелме шлёп"
    };


    extension(String str)
    {
        // Метод для генерации случайной строки заданной длины. Возвращает строку, состоящую из случайных символов из массива chars.
        public static string GetRandomString(int length = 10)
        {
            var result = new StringBuilder(length);

            byte[] randBytes = RandomNumberGenerator.GetBytes(length);

            foreach (byte b in randBytes)
            {
                int index = b % chars.Length;
                result.Append(chars[index]);
            }

            return result.ToString();
        }

        // Метод для получения случайного имени из списка names. Возвращает случайное имя из списка.
        public static string GetRandomName()
        {
            return names[Random.Shared.Next(names.Count)];
        }
    }
}
