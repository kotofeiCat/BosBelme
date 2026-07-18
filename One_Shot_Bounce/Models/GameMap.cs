using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace One_Shot_Bounce.Models
{
    public enum BlockType
    {
        Empty,
        Wall,
        Destructible
    }

    public class GameMap
    {
        public BlockType[,] Grid { get; set; } = new BlockType[0, 0];

        public float BlockSize { get; init; } = 40f;

        public Vector2 Player1SpawnPoint { get; set; }
        public Vector2 Player2SpawnPoint { get; set; }

        public int Columns => Grid.GetLength(0);
        public int Rows => Grid.GetLength(1);
    }
}
