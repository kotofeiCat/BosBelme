using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace One_Shot_Bounce.Models;

// Модель игрока для игры
public class Player
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public Vector2 Position { get; set; }

    public float RotationAngle { get; set; }

    public bool HasBullet { get; set; } = true;

    public bool IsAlive { get; set; } = true;

    public bool IsShieldActive { get; set; }

    public float ShieldDurationLeft { get; set; }
}
