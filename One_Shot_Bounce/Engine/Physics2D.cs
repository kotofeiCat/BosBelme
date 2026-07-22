using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace One_Shot_Bounce.Engine;

// Класс для расчета физики
public static class Physics2D
{
    // Обработчик колизиций
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HandleBoundaryCollision(ref Vector2 position, ref Vector2 direction, float mapWidth, float mapHeight, float radius)
    {
        bool bounced = false;
        float maxEx = mapWidth - radius;
        float maxEy = mapHeight - radius;

        if (position.X < radius)
        {
            position.X = radius;
            direction.X = -direction.X;
            bounced = true;
        }
        else if (position.X > maxEx)
        {
            position.X = maxEx;
            direction.X = -direction.X;
            bounced = true;
        }

        if (position.Y < radius)
        {
            position.Y = radius;
            direction.Y = -direction.Y;
            bounced = true;
        }
        else if (position.Y > maxEy)
        {
            position.Y = maxEy;
            direction.Y = -direction.Y;
            bounced = true;
        }

        return bounced;
    }

    public static bool HandleBlockCollision(ref Vector2 position, ref Vector2 direction, Vector2 prevPosition, int col, int row, float blockSize, float radius)
    {
        float blockLeft = col * blockSize - radius;
        float blockRight = blockLeft + blockSize + (radius * 2f);
        float blockTop = row * blockSize - radius;
        float blockBottom = blockTop + blockSize + (radius * 2f);

        if (prevPosition.X > blockLeft && prevPosition.X < blockRight &&
            prevPosition.Y > blockTop && prevPosition.Y < blockBottom)
        {
            float distToLeft = prevPosition.X - blockLeft;
            float distToRight = blockRight - prevPosition.X;
            float distToTop = prevPosition.Y - blockTop;
            float distToBottom = blockBottom - prevPosition.Y;
            float minDistance = MathF.Min(MathF.Min(distToLeft, distToRight), MathF.Min(distToTop, distToBottom));

            Vector2 escapeNormal = Vector2.Zero;
            if (minDistance == distToLeft) escapeNormal = new Vector2(-1, 0);
            else if (minDistance == distToRight) escapeNormal = new Vector2(1, 0);
            else if (minDistance == distToTop) escapeNormal = new Vector2(0, -1);
            else if (minDistance == distToBottom) escapeNormal = new Vector2(0, 1);

            direction = direction - 2 * Vector2.Dot(direction, escapeNormal) * escapeNormal;
            position = prevPosition + (escapeNormal * minDistance) + (escapeNormal * 0.5f);
            return true;
        }

        Vector2 delta = position - prevPosition;
        if (delta.X == 0 && delta.Y == 0) return false;

        float tXMin = float.MinValue, tXMax = float.MaxValue;
        if (delta.X != 0)
        {
            float t1 = (blockLeft - prevPosition.X) / delta.X;
            float t2 = (blockRight - prevPosition.X) / delta.X;
            tXMin = MathF.Min(t1, t2);
            tXMax = MathF.Max(t1, t2);
        }
        else if (prevPosition.X < blockLeft || prevPosition.X > blockRight)
        {
            return false;
        }

        float tYMin = float.MinValue, tYMax = float.MaxValue;
        if (delta.Y != 0)
        {
            float t1 = (blockTop - prevPosition.Y) / delta.Y;
            float t2 = (blockBottom - prevPosition.Y) / delta.Y;
            tYMin = MathF.Min(t1, t2);
            tYMax = MathF.Max(t1, t2);
        }
        else if (prevPosition.Y < blockTop || prevPosition.Y > blockBottom)
        {
            return false;
        }

        float tNear = MathF.Max(tXMin, tYMin);
        float tFar = MathF.Min(tXMax, tYMax);

        if (tNear > tFar || tFar < 0f || tNear > 1f)
        {
            return false;
        }

        Vector2 finalNormal;
        if (tXMin > tYMin)
        {
            finalNormal = (delta.X > 0) ? new Vector2(-1, 0) : new Vector2(1, 0);
        }
        else
        {
            finalNormal = (delta.Y > 0) ? new Vector2(0, -1) : new Vector2(0, 1);
        }

        position = prevPosition + delta * tNear + finalNormal * 0.5f;
        direction = direction - 2 * Vector2.Dot(direction, finalNormal) * finalNormal;

        return true;
    }
}