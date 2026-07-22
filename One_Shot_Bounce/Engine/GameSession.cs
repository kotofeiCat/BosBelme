using One_Shot_Bounce.Models;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;

namespace One_Shot_Bounce.Engine;

// Класс основной логики игры
public class GameSession
{
    public GameState State { get; }

    private readonly Lock _sessionLock = new();

    private readonly float _playerSpeed = 250f;
    private readonly float _playerRadius = 20f;
    private readonly float _bulletRadius = 8f;
    private readonly float _initialBulletSpeed = 450f;
    private readonly float _speedMultiplierPerBounce = 1.05f;
    private readonly float _maxShieldDuration = 0.4f;

    private readonly List<string> _bulletsToRemove = new(4);

    public GameSession(string roomId)
    {
        State = new GameState
        {
            RoomId = roomId,
            CurrentMap = new GameMap()
        };
    }

    // Метод обновленние кадров
    public void Update(float deltaTime)
    {
        lock (_sessionLock)
        {
            switch (State.Status)
            {
                case MatchStatus.Warmup:
                    UpdateWarmup(deltaTime);
                    break;
                case MatchStatus.RoundInProgress:
                    UpdatePhysics(deltaTime);
                    break;
                case MatchStatus.RoundEnded:
                    UpdateRoundEnded(deltaTime);
                    break;
            }
        }
    }

    // Метод обработки передвижения игрока
    public void MovePlayer(string playerId, Vector2 moveDirection, float deltaTime)
    {
        lock (_sessionLock)
        {
            if (State.Status != MatchStatus.RoundInProgress) return;

            var player = GetPlayerById(playerId);
            if (player is not { IsAlive: true }) return;

            if (moveDirection != Vector2.Zero)
            {
                Vector2 newPosition = player.Position + Vector2.Normalize(moveDirection) * _playerSpeed * deltaTime;

                float mapWidth = State.CurrentMap.Columns * State.CurrentMap.BlockSize;
                float mapHeight = State.CurrentMap.Rows * State.CurrentMap.BlockSize;
                float blockSize = State.CurrentMap.BlockSize;

                newPosition.X = Math.Clamp(newPosition.X, _playerRadius, mapWidth - _playerRadius);
                newPosition.Y = Math.Clamp(newPosition.Y, _playerRadius, mapHeight - _playerRadius);

                int minCol = Math.Max(0, (int)((newPosition.X - _playerRadius) / blockSize));
                int maxCol = Math.Min(State.CurrentMap.Columns - 1, (int)((newPosition.X + _playerRadius) / blockSize));
                int minRow = Math.Max(0, (int)((newPosition.Y - _playerRadius) / blockSize));
                int maxRow = Math.Min(State.CurrentMap.Rows - 1, (int)((newPosition.Y + _playerRadius) / blockSize));

                for (int c = minCol; c <= maxCol; c++)
                {
                    for (int r = minRow; r <= maxRow; r++)
                    {
                        if (State.CurrentMap.Grid[c][r] != BlockType.Empty)
                        {
                            float blockLeft = c * blockSize;
                            float blockRight = blockLeft + blockSize;
                            float blockTop = r * blockSize;
                            float blockBottom = blockTop + blockSize;

                            float closestX = Math.Clamp(newPosition.X, blockLeft, blockRight);
                            float closestY = Math.Clamp(newPosition.Y, blockTop, blockBottom);

                            float distanceX = newPosition.X - closestX;
                            float distanceY = newPosition.Y - closestY;
                            float distanceSquared = (distanceX * distanceX) + (distanceY * distanceY);

                            if (distanceSquared < _playerRadius * _playerRadius)
                            {
                                float distance = MathF.Sqrt(distanceSquared);
                                if (distance > 0f)
                                {
                                    float overlap = _playerRadius - distance;
                                    newPosition.X += (distanceX / distance) * overlap;
                                    newPosition.Y += (distanceY / distance) * overlap;
                                }
                            }
                        }
                    }
                }

                player.Position = newPosition;
            }

            if (State.Player1 != null && State.Player2 != null && State.Player1.IsAlive && State.Player2.IsAlive)
            {
                float minDist = _playerRadius * 2f;
                float currentDist = Vector2.Distance(State.Player1.Position, State.Player2.Position);

                if (currentDist < minDist)
                {
                    float overlap = minDist - currentDist;
                    Vector2 pushDir = currentDist > 0f ? Vector2.Normalize(State.Player2.Position - State.Player1.Position) : new Vector2(1, 0);

                    if (playerId == State.Player1.Id)
                    {
                        State.Player1.Position -= pushDir * (overlap * 0.5f);
                        State.Player2.Position += pushDir * (overlap * 0.5f);
                    }
                    else
                    {
                        State.Player2.Position += pushDir * (overlap * 0.5f);
                        State.Player1.Position -= pushDir * (overlap * 0.5f);
                    }
                }
            }
        }
    }

    // Метод обработки выстрела
    public void HandleShoot(string playerId, float targetAngle)
    {
        lock (_sessionLock)
        {
            if (State.Status != MatchStatus.RoundInProgress) return;

            var player = GetPlayerById(playerId);
            if (player is not { IsAlive: true, HasBullet: true }) return;

            player.HasBullet = false;
            player.RotationAngle = targetAngle;

            Vector2 direction = new Vector2(MathF.Cos(targetAngle), MathF.Sin(targetAngle));
            Vector2 spawnPos = player.Position + direction * (_playerRadius + _bulletRadius + 2f);

            State.ActiveBullets.Add(new Bullet
            {
                Id = Guid.NewGuid().ToString(),
                OwnerId = playerId,
                Position = spawnPos,
                Direction = direction,
                Speed = _initialBulletSpeed,
                BounceCount = 0
            });
        }
    }

    // Метод активации щита
    public void ActivateShield(string playerId)
    {
        lock (_sessionLock)
        {
            if (State.Status != MatchStatus.RoundInProgress) return;

            var player = GetPlayerById(playerId);
            if (player is not { IsAlive: true, IsShieldActive: false }) return;

            player.IsShieldActive = true;
            player.ShieldDurationLeft = _maxShieldDuration;
        }
    }

    // Метод обновления физики игры
    private void UpdatePhysics(float deltaTime)
    {
        float mapWidth = State.CurrentMap.Columns * State.CurrentMap.BlockSize;
        float mapHeight = State.CurrentMap.Rows * State.CurrentMap.BlockSize;
        float blockSize = State.CurrentMap.BlockSize;

        UpdatePlayerShields(deltaTime);

        Span<Bullet> bulletsSpan = CollectionsMarshal.AsSpan(State.ActiveBullets);
        _bulletsToRemove.Clear();

        for (int i = 0; i < bulletsSpan.Length; i++)
        {
            ref Bullet bullet = ref bulletsSpan[i];
            Vector2 previousPosition = bullet.Position;

            bullet.Position += bullet.Direction * bullet.Speed * deltaTime;

            Vector2 currentPos = bullet.Position;
            Vector2 currentDir = bullet.Direction;

            bool bounced = Physics2D.HandleBoundaryCollision(ref currentPos, ref currentDir, mapWidth, mapHeight, _bulletRadius);
            if (bounced)
            {
                bullet.Position = currentPos;
                bullet.Direction = currentDir;
                bullet.BounceCount++;
                bullet.Speed = MathF.Min(bullet.Speed * _speedMultiplierPerBounce, 1000f);
            }
            else
            {
                int centerCol = (int)(bullet.Position.X / blockSize);
                int centerRow = (int)(bullet.Position.Y / blockSize);

                for (int dc = -1; dc <= 1; dc++)
                {
                    for (int dr = -1; dr <= 1; dr++)
                    {
                        int col = centerCol + dc;
                        int row = centerRow + dr;

                        if (col >= 0 && col < State.CurrentMap.Columns && row >= 0 && row < State.CurrentMap.Rows)
                        {
                            var blockType = State.CurrentMap.Grid[col][row];
                            if (blockType != BlockType.Empty)
                            {
                                currentPos = bullet.Position;
                                currentDir = bullet.Direction;

                                if (Physics2D.HandleBlockCollision(ref currentPos, ref currentDir, previousPosition, col, row, blockSize, _bulletRadius))
                                {
                                    bullet.Position = currentPos;
                                    bullet.Direction = currentDir;
                                    bullet.BounceCount++;
                                    bullet.Speed = MathF.Min(bullet.Speed * _speedMultiplierPerBounce, 1000f);

                                    if (blockType == BlockType.Destructible)
                                    {
                                        State.CurrentMap.Grid[col][row] = BlockType.Empty;
                                    }

                                    bounced = true;
                                    break; 
                                }
                            }
                        }
                    }
                    if (bounced) break; 
                }
            }

            string currentBulletId = bullet.Id;

            if (CheckBulletPlayerCollision(ref bullet, State.Player1) || CheckBulletPlayerCollision(ref bullet, State.Player2))
            {
                _bulletsToRemove.Add(currentBulletId);
                if (State.Status == MatchStatus.RoundEnded || State.Status == MatchStatus.MatchOver) return;
            }
        }

        if (_bulletsToRemove.Count > 0)
        {
            State.ActiveBullets.RemoveAll(b => _bulletsToRemove.Contains(b.Id));
        }
    }

    // Метод попадания пули в игрока
    private bool CheckBulletPlayerCollision(ref Bullet bullet, Player? player)
    {
        if (player is not { IsAlive: true }) return false;

        if (bullet.OwnerId == player.Id && bullet.BounceCount == 0) return false;

        if (Vector2.DistanceSquared(bullet.Position, player.Position) <= (_playerRadius + _bulletRadius) * (_playerRadius + _bulletRadius))
        {
            if (player.IsShieldActive)
            {
                player.HasBullet = true;
                return true;
            }

            player.IsAlive = false;
            EndRound(player.Id == State.Player1?.Id ? State.Player2?.Id : State.Player1?.Id);
            return true;
        }

        return false;
    }

    // Метод обновления щита игрка
    private void UpdatePlayerShields(float deltaTime)
    {
        if (State.Player1 is { IsShieldActive: true })
        {
            State.Player1.ShieldDurationLeft -= deltaTime;
            if (State.Player1.ShieldDurationLeft <= 0) State.Player1.IsShieldActive = false;
        }
        if (State.Player2 is { IsShieldActive: true })
        {
            State.Player2.ShieldDurationLeft -= deltaTime;
            if (State.Player2.ShieldDurationLeft <= 0) State.Player2.IsShieldActive = false;
        }
    }

    // Методы обработки режима игры
    private void UpdateWarmup(float deltaTime)
    {
        State.StatusTimer -= deltaTime;
        if (State.StatusTimer <= 0) State.Status = MatchStatus.RoundInProgress;
    }

    private void UpdateRoundEnded(float deltaTime)
    {
        State.StatusTimer -= deltaTime;
        if (State.StatusTimer <= 0) StartNewRound();
    }

    private void EndRound(string? winnerId)
    {
        State.Status = MatchStatus.RoundEnded;
        State.StatusTimer = 3.0f;
        State.ActiveBullets.Clear();

        if (!string.IsNullOrEmpty(winnerId))
        {
            State.Scores[winnerId] = State.Scores.GetValueOrDefault(winnerId, 0) + 1;
            if (State.Scores[winnerId] >= 5) State.Status = MatchStatus.MatchOver;
        }
    }

    // Метод начала нового раунда
    public void StartNewRound()
    {
        State.Status = MatchStatus.Warmup;
        State.StatusTimer = 3.0f;

        if (State.Player1 != null) ResetPlayer(State.Player1, State.CurrentMap.Player1SpawnPoint);
        if (State.Player2 != null) ResetPlayer(State.Player2, State.CurrentMap.Player2SpawnPoint);

        State.ActiveBullets.Clear();
    }

    // Метод сброса состояния игрока
    private static void ResetPlayer(Player player, Vector2 spawnPoint)
    {
        player.Position = spawnPoint;
        player.IsAlive = true;
        player.HasBullet = true;
        player.IsShieldActive = false;
    }

    // Вспомогательный метод поиска ID игрка
    private Player? GetPlayerById(string id)
    {
        if (State.Player1?.Id == id) return State.Player1;
        return State.Player2?.Id == id ? State.Player2 : null;
    }
}