using System.Numerics;

namespace One_Shot_Bounce.Models
{
    // Модель пули для игры
    public class Bullet
    {
        public string Id { get; init; } = Guid.NewGuid().ToString();

        public string OwnerId { get; init; } = string.Empty;

        public Vector2 Position { get; set; }

        public Vector2 Direction { get; set; }

        public float Speed { get; set; }

        public int BounceCount { get; set; }
    }
}
