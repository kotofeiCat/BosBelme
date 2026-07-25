using System;
using System.Numerics;

namespace One_Shot_Bounce.Models;

// Модель игрока для игры
public class Player
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public Vector2 Position { get; set; }

    public float RotationAngle { get; set; }

    public int BulletCount { get; set; } = 1;

    public bool HasBullet { get; set; } = true;

    public bool IsAlive { get; set; } = true;

    public bool IsShieldActive { get; set; }

    public float ShieldDurationLeft { get; set; }

    public float ShieldCooldownLeft { get; set; }

    public int Score { get; set; } = 0;
}