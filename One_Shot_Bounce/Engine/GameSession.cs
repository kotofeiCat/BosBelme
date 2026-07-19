using One_Shot_Bounce.Models;
using System.Numerics;
using System.Runtime.InteropServices;

namespace One_Shot_Bounce.Engine
{
    // Класс механики самой игры
    public class GameSession
    {
        public GameState State { get; }

        private readonly Lock _sessionLock = new();

        private readonly float _playerSpeed = 250f;
        private readonly float _playerRadius = 20f;
        private readonly float _bulletRadius = 8f;
        private readonly float _initialBulletSpeed = 450f;
        private readonly float _speedMultiplierPerBounce = 1.05f;
        private readonly float _maxShieldDuration = 0.2f;

        private readonly List<string> _bulletsToRemove = new(4);

        public GameSession(string roomId)
        {
            State = new GameState { RoomId = roomId };
        }

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

                    player.Position = new Vector2(
                        Math.Clamp(newPosition.X, _playerRadius, mapWidth - _playerRadius),
                        Math.Clamp(newPosition.Y, _playerRadius, mapHeight - _playerRadius)
                    );
                }
            }
        }

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
                Vector2 Glen = player.Position + direction * (_playerRadius + _bulletRadius + 2f);

                State.ActiveBullets.Add(new Bullet
                {
                    OwnerId = playerId,
                    Position = Glen,
                    Direction = direction,
                    Speed = _initialBulletSpeed,
                    BounceCount = 0
                });
            }
        }

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
                if (Physics2D.HandleBoundaryCollision(ref currentPos, ref currentDir, mapWidth, mapHeight, _bulletRadius))
                {
                    bullet.Position = currentPos;
                    bullet.Direction = currentDir;
                    bullet.BounceCount++;
                    bullet.Speed *= _speedMultiplierPerBounce;
                }

                int col = (int)(bullet.Position.X / blockSize);
                int row = (int)(bullet.Position.Y / blockSize);

                if (col >= 0 && col < State.CurrentMap.Columns && row >= 0 && row < State.CurrentMap.Rows)
                {
                    var blockType = State.CurrentMap.Grid[col, row];
                    if (blockType != BlockType.Empty)
                    {
                        Vector2 normal = Physics2D.GetBlockNormal(bullet.Position, previousPosition, col, row, blockSize);
                        if (normal != Vector2.Zero)
                        {
                            bullet.Direction = Physics2D.Reflect(bullet.Direction, normal);
                            bullet.BounceCount++;
                            bullet.Speed *= _speedMultiplierPerBounce;

                            if (blockType == BlockType.Destructible)
                            {
                                State.CurrentMap.Grid[col, row] = BlockType.Empty;
                            }
                        }
                    }
                }

                if (CheckBulletPlayerCollision(ref bullet, State.Player1) || CheckBulletPlayerCollision(ref bullet, State.Player2))
                {
                    _bulletsToRemove.Add(bullet.Id);
                    break; 
                }
            }

            if (_bulletsToRemove.Count > 0)
            {
                State.ActiveBullets.RemoveAll(b => _bulletsToRemove.Contains(b.Id));
            }
        }

        private bool CheckBulletPlayerCollision(ref Bullet bullet, Player? player)
        {
            if (player is not { IsAlive: true }) return false;

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

        public void StartNewRound()
        {
            State.Status = MatchStatus.Warmup;
            State.StatusTimer = 3.0f;

            if (State.Player1 != null) ResetPlayer(State.Player1, State.CurrentMap.Player1SpawnPoint);
            if (State.Player2 != null) ResetPlayer(State.Player2, State.CurrentMap.Player2SpawnPoint);

            State.ActiveBullets.Clear();
        }

        private static void ResetPlayer(Player player, Vector2 spawnPoint)
        {
            player.Position = spawnPoint;
            player.IsAlive = true;
            player.HasBullet = true;
            player.IsShieldActive = false;
        }

        private Player? GetPlayerById(string id)
        {
            if (State.Player1?.Id == id) return State.Player1;
            return State.Player2?.Id == id ? State.Player2 : null;
        }
    }
}
