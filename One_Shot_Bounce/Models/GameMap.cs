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
        public BlockType[][] Grid { get; set; } = Array.Empty<BlockType[]>();

        public float BlockSize { get; init; } = 40f;

        public Vector2 Player1SpawnPoint { get; set; }
        public Vector2 Player2SpawnPoint { get; set; }

        public int Columns => Grid.Length;
        public int Rows => Grid.Length > 0 ? Grid[0].Length : 0;
    }
}
