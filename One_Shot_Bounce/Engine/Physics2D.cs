using System.Numerics;
using System.Runtime.CompilerServices;

namespace One_Shot_Bounce.Engine
{
    // Класс для расчетов физики движения
    public class Physisc2D
    {
        public static Vector2 Reflect(Vector2 direction, Vector2 normal)
        {
            return direction - 2 * Vector2.Dot(direction, normal) * normal;
        }
    
        public static bool HandleBoundaryCollision(ref Vector2 position, ref Vector2 direction, float mapWidth, float mapHeight, float radius)
        {
            bool bounced = false;

            if (position.X - radius < 0)
            {
                position.X = radius;
                direction.X = -direction.X;
                bounced = true;
            }
            else if (position.X + radius > mapWidth)
            {
                position.X = mapWidth - radius;
                direction.X = -direction.X;
                bounced = true;
            }

            if (position.Y - radius < 0)
            {
                position.Y = radius;
                direction.Y = -direction.Y;
                bounced = true;
            }
            else if (position.Y + radius > mapHeight)
            {
                position.Y = mapHeight - radius;
                direction.Y = -direction.Y;
                bounced = true;
            }

            return bounced;
        }

        public static Vector2 GetBlockNormal(Vector2 bulletPos, Vector2 prevBulletPos, int col, int row, float blockSize)
        {
            float blockLeft = col * blockSize;
            float blockTop = row * blockSize;

            return prevBulletPos switch
            {
                _ when prevBulletPos.X <= blockLeft => new Vector2(-1, 0),
                _ when prevBulletPos.X >= blockLeft + blockSize => new Vector2(1, 0),
                _ when prevBulletPos.Y <= blockTop => new Vector2(0, -1),
                _ when prevBulletPos.Y >= blockTop + blockSize => new Vector2(0, 1),
                _ => Vector2.Zero
            };
        }
    }
}
