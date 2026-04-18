using System;
using System.Collections.Generic;
using UnityEngine;

namespace Samples.MarioGame
{
    public enum TileType { Empty, Ground, Brick, QuestionBlock, CoinBlock, Pipe, Flagpole, FlagpoleTop, Castle }
    public enum EnemyType { Goomba }
    public enum ItemType { Mushroom }
    public enum GameRoute { Menu, Game }

    public struct LevelTile
    {
        public int Col;
        public int Row;
        public TileType Type;
        public bool Hit;
        public int HitCount;
        public float BumpY;
    }

    public struct EnemyState
    {
        public int Id;
        public float X;
        public float Y;
        public float Vx;
        public bool Alive;
        public EnemyType Type;
    }

    public struct ItemState
    {
        public int Id;
        public float X;
        public float Y;
        public float Vx;
        public float Vy;
        public bool Collected;
        public bool Spawned;
        public ItemType Type;
    }

    public struct PlayerState
    {
        public float X;
        public float Y;
        public float Vx;
        public float Vy;
        public bool FacingRight;
        public bool Grounded;
        public bool Ducking;
        public bool Alive;
        public bool Big;
        public float BigTimer;
        public float InvincibleTimer;
        public int AnimFrame;
        public float AnimTimer;
    }

    public struct GameState
    {
        public PlayerState Player;
        public List<EnemyState> Enemies;
        public List<ItemState> Items;
        public List<LevelTile> Tiles;
        public int Score;
        public int Lives;
        public float CameraX;
        public bool GameOver;
        public bool Won;
    }

    public static class MarioConstants
    {
        public const int TILE = 32;
        public const int VIEWPORT_W = 800;
        public const int VIEWPORT_H = 576;
        public const int SCREEN_COLS = 25;
        public const int NUM_SCREENS = 35;
        public const int LEVEL_COLS = SCREEN_COLS * NUM_SCREENS; // 875
        public const int GROUND_ROW = 16;
        public const float GRAVITY = 1400f;
        public const float JUMP_VEL = -620f;
        public const float WALK_SPEED = 200f;
        public const float ENEMY_SPEED = 60f;
        public const float ANIM_FRAME_TIME = 0.12f;
        public const int MARIO_W = 32;
        public const int MARIO_H = 64;
        public const int MARIO_DUCK_H = 44;
        public const int COIN_SCORE = 50;
        public const int COIN_HITS_MAX = 5;
        public const float BUMP_OFFSET = -6f;
        public const float BUMP_SPEED = 60f;
        public const int BIG_MARIO_H = 96;
        public const float BIG_DURATION = 10f;
        public const float INVINCIBLE_DURATION = 3f;
    }

    public static class LevelGenerator
    {
        // Screen template types
        enum ScreenType { Flat, Pit, Pipes, Staircase, Floating, Final }

        public static GameState GenerateLevel(int seed = 0)
        {
            var rng = seed != 0 ? new System.Random(seed) : new System.Random();
            var tiles = new List<LevelTile>(4000);
            var enemies = new List<EnemyState>();
            var items = new List<ItemState>();
            int enemyId = 1;
            int itemId = 1;

            for (int screen = 0; screen < MarioConstants.NUM_SCREENS; screen++)
            {
                int baseCol = screen * MarioConstants.SCREEN_COLS;
                float difficulty = (float)screen / (MarioConstants.NUM_SCREENS - 1); // 0..1

                if (screen == MarioConstants.NUM_SCREENS - 1)
                {
                    GenerateFinalScreen(tiles, baseCol);
                    continue;
                }

                // First screen is always flat/easy
                ScreenType type;
                if (screen == 0)
                {
                    type = ScreenType.Flat;
                }
                else
                {
                    type = PickScreenType(rng, difficulty);
                }

                GenerateScreen(rng, tiles, enemies, items, ref enemyId, ref itemId, baseCol, type, difficulty);
            }

            var player = new PlayerState
            {
                X = 2 * MarioConstants.TILE,
                Y = 14 * MarioConstants.TILE + 1,
                FacingRight = true,
                Grounded = true,
                Alive = true,
            };

            return new GameState
            {
                Player = player,
                Enemies = enemies,
                Items = items,
                Tiles = tiles,
                Score = 0,
                Lives = 3,
                CameraX = 0,
                GameOver = false,
                Won = false,
            };
        }

        static ScreenType PickScreenType(System.Random rng, float difficulty)
        {
            // Weighted random based on difficulty
            int r = rng.Next(100);
            if (difficulty < 0.2f)
            {
                // Easy: mostly flat
                if (r < 50) return ScreenType.Flat;
                if (r < 75) return ScreenType.Pit;
                return ScreenType.Pipes;
            }
            else if (difficulty < 0.5f)
            {
                // Medium
                if (r < 25) return ScreenType.Flat;
                if (r < 50) return ScreenType.Pit;
                if (r < 70) return ScreenType.Pipes;
                if (r < 90) return ScreenType.Staircase;
                return ScreenType.Floating;
            }
            else
            {
                // Hard
                if (r < 15) return ScreenType.Flat;
                if (r < 35) return ScreenType.Pit;
                if (r < 55) return ScreenType.Pipes;
                if (r < 75) return ScreenType.Staircase;
                return ScreenType.Floating;
            }
        }

        static void GenerateScreen(System.Random rng, List<LevelTile> tiles,
            List<EnemyState> enemies, List<ItemState> items,
            ref int enemyId, ref int itemId, int baseCol,
            ScreenType type, float difficulty)
        {
            switch (type)
            {
                case ScreenType.Flat:
                    GenerateFlatScreen(rng, tiles, enemies, items, ref enemyId, ref itemId, baseCol, difficulty);
                    break;
                case ScreenType.Pit:
                    GeneratePitScreen(rng, tiles, enemies, items, ref enemyId, ref itemId, baseCol, difficulty);
                    break;
                case ScreenType.Pipes:
                    GeneratePipeScreen(rng, tiles, enemies, items, ref enemyId, ref itemId, baseCol, difficulty);
                    break;
                case ScreenType.Staircase:
                    GenerateStaircaseScreen(rng, tiles, enemies, items, ref enemyId, ref itemId, baseCol, difficulty);
                    break;
                case ScreenType.Floating:
                    GenerateFloatingScreen(rng, tiles, enemies, items, ref enemyId, ref itemId, baseCol, difficulty);
                    break;
            }
        }

        // ----- Ground helpers -----

        static void AddGround(List<LevelTile> tiles, int col, int groundRow = 16)
        {
            tiles.Add(new LevelTile { Col = col, Row = groundRow, Type = TileType.Ground });
            tiles.Add(new LevelTile { Col = col, Row = groundRow + 1, Type = TileType.Ground });
        }

        static void AddFullGround(List<LevelTile> tiles, int baseCol, int count = 25, int groundRow = 16)
        {
            for (int c = 0; c < count; c++) AddGround(tiles, baseCol + c, groundRow);
        }

        // ----- FLAT -----

        static void GenerateFlatScreen(System.Random rng, List<LevelTile> tiles,
            List<EnemyState> enemies, List<ItemState> items,
            ref int enemyId, ref int itemId, int baseCol, float difficulty)
        {
            // Ground with occasional small gap
            for (int c = 0; c < 25; c++)
            {
                // ~10% chance of a 2-tile gap in later screens
                if (difficulty > 0.2f && c > 4 && c < 21 && rng.Next(100) < 8)
                {
                    c++; // skip 2 cols
                    continue;
                }
                AddGround(tiles, baseCol + c);
            }

            // Place 2-3 block clusters
            int clusters = 2 + rng.Next(2);
            int lastClusterEnd = 0;
            for (int i = 0; i < clusters; i++)
            {
                int startCol = baseCol + Math.Max(lastClusterEnd + 2, 3 + i * 7 + rng.Next(3));
                if (startCol > baseCol + 20) break;
                int row = 11 + rng.Next(3); // rows 11-13
                int width = 3 + rng.Next(3); // 3-5 blocks
                AddBlockCluster(rng, tiles, items, ref itemId, startCol, row, width, difficulty);
                lastClusterEnd = (startCol - baseCol) + width;
            }

            // Enemies: 1-3
            int numEnemies = 1 + rng.Next(2 + (difficulty > 0.3f ? 1 : 0));
            for (int i = 0; i < numEnemies; i++)
            {
                int col = baseCol + 4 + rng.Next(18);
                enemies.Add(new EnemyState
                {
                    Id = enemyId++, X = col * MarioConstants.TILE,
                    Y = 14 * MarioConstants.TILE,
                    Vx = rng.Next(2) == 0 ? -MarioConstants.ENEMY_SPEED : MarioConstants.ENEMY_SPEED,
                    Alive = true, Type = EnemyType.Goomba,
                });
            }
        }

        // ----- PIT -----

        static void GeneratePitScreen(System.Random rng, List<LevelTile> tiles,
            List<EnemyState> enemies, List<ItemState> items,
            ref int enemyId, ref int itemId, int baseCol, float difficulty)
        {
            // 1-3 pits
            int numPits = 1 + rng.Next(difficulty < 0.4f ? 2 : 3);
            var pits = new List<(int start, int width)>();
            int cursor = 3;

            for (int i = 0; i < numPits; i++)
            {
                int gap = 3 + rng.Next(3); // solid ground before pit
                int pitW = 2 + rng.Next(3); // 2-4 wide
                if (cursor + gap + pitW + 2 > 24) break;
                pits.Add((cursor + gap, pitW));
                cursor += gap + pitW;
            }

            for (int c = 0; c < 25; c++)
            {
                bool inPit = false;
                foreach (var (start, width) in pits)
                {
                    if (c >= start && c < start + width) { inPit = true; break; }
                }
                if (!inPit) AddGround(tiles, baseCol + c);
            }

            // Stepping blocks above/near pits
            foreach (var (start, width) in pits)
            {
                int row = 12 + rng.Next(2);
                // Bridge block in middle of wide pits
                if (width >= 3)
                {
                    tiles.Add(new LevelTile { Col = baseCol + start + width / 2, Row = row, Type = TileType.Brick });
                }
                // Blocks on edges of pits for easier jumping
                if (rng.Next(2) == 0)
                {
                    tiles.Add(new LevelTile { Col = baseCol + start - 1, Row = row, Type = TileType.QuestionBlock });
                }
            }

            // Block clusters on solid sections
            int clusterCol = baseCol + 1 + rng.Next(3);
            AddBlockCluster(rng, tiles, items, ref itemId, clusterCol, 12, 3 + rng.Next(2), difficulty);

            // Second cluster after last pit if room
            if (cursor + 3 < 23)
            {
                AddBlockCluster(rng, tiles, items, ref itemId, baseCol + cursor + 2, 11, 3, difficulty);
            }

            // 1-2 enemies
            int numEnemies = 1 + rng.Next(2);
            for (int i = 0; i < numEnemies; i++)
            {
                int eCol = baseCol + 2 + rng.Next(6) + i * 10;
                if (eCol > baseCol + 22) break;
                enemies.Add(new EnemyState
                {
                    Id = enemyId++, X = eCol * MarioConstants.TILE,
                    Y = 14 * MarioConstants.TILE,
                    Vx = -MarioConstants.ENEMY_SPEED, Alive = true, Type = EnemyType.Goomba,
                });
            }
        }

        // ----- PIPES -----

        static void GeneratePipeScreen(System.Random rng, List<LevelTile> tiles,
            List<EnemyState> enemies, List<ItemState> items,
            ref int enemyId, ref int itemId, int baseCol, float difficulty)
        {
            AddFullGround(tiles, baseCol);

            // 2-4 pipes
            int numPipes = 2 + rng.Next(3);
            int cursor = 2;
            for (int i = 0; i < numPipes && cursor < 21; i++)
            {
                int pipeH = 2 + rng.Next(3); // 2-4 tiles tall
                int pipeCol = cursor;
                for (int r = 0; r < pipeH; r++)
                {
                    int row = MarioConstants.GROUND_ROW - pipeH + r;
                    tiles.Add(new LevelTile { Col = baseCol + pipeCol, Row = row, Type = TileType.Pipe });
                    tiles.Add(new LevelTile { Col = baseCol + pipeCol + 1, Row = row, Type = TileType.Pipe });
                }
                cursor += 4 + rng.Next(3); // 4-6 gap
            }

            // Block cluster between pipes
            int bCol = baseCol + 6 + rng.Next(4);
            AddBlockCluster(rng, tiles, items, ref itemId, bCol, 11, 3 + rng.Next(2), difficulty);
            // Second cluster
            if (rng.Next(2) == 0)
            {
                int bCol2 = baseCol + 15 + rng.Next(4);
                AddBlockCluster(rng, tiles, items, ref itemId, bCol2, 12, 3, difficulty);
            }

            // 1-2 enemies between pipes
            int numEnemies = 1 + rng.Next(2);
            for (int i = 0; i < numEnemies; i++)
            {
                int eCol = baseCol + 4 + rng.Next(16);
                enemies.Add(new EnemyState
                {
                    Id = enemyId++, X = eCol * MarioConstants.TILE,
                    Y = 14 * MarioConstants.TILE,
                    Vx = rng.Next(2) == 0 ? -MarioConstants.ENEMY_SPEED : MarioConstants.ENEMY_SPEED,
                    Alive = true, Type = EnemyType.Goomba,
                });
            }
        }

        // ----- STAIRCASE -----

        static void GenerateStaircaseScreen(System.Random rng, List<LevelTile> tiles,
            List<EnemyState> enemies, List<ItemState> items,
            ref int enemyId, ref int itemId, int baseCol, float difficulty)
        {
            bool ascending = rng.Next(2) == 0;
            int stairStart = 6 + rng.Next(4);
            int stairLen = 5 + rng.Next(3); // 5-7 steps

            for (int c = 0; c < 25; c++)
            {
                int relC = c - stairStart;
                if (relC >= 0 && relC < stairLen)
                {
                    int step = ascending ? relC : (stairLen - 1 - relC);
                    int topRow = MarioConstants.GROUND_ROW - step;
                    for (int r = topRow; r <= MarioConstants.GROUND_ROW + 1; r++)
                    {
                        tiles.Add(new LevelTile { Col = baseCol + c, Row = r, Type = TileType.Ground });
                    }
                }
                else
                {
                    AddGround(tiles, baseCol + c);
                }
            }

            // Blocks at the top and on flat sections
            int blockRow = MarioConstants.GROUND_ROW - stairLen - 1;
            if (blockRow >= 6)
            {
                AddBlockCluster(rng, tiles, items, ref itemId, baseCol + stairStart + stairLen / 2, blockRow, 3, difficulty);
            }
            // Extra blocks on flat section before staircase
            AddBlockCluster(rng, tiles, items, ref itemId, baseCol + 2, 12, 3 + rng.Next(2), difficulty);

            // 1-2 enemies
            enemies.Add(new EnemyState
            {
                Id = enemyId++, X = (baseCol + 3) * MarioConstants.TILE,
                Y = 14 * MarioConstants.TILE,
                Vx = -MarioConstants.ENEMY_SPEED, Alive = true, Type = EnemyType.Goomba,
            });
            if (rng.Next(2) == 0)
            {
                enemies.Add(new EnemyState
                {
                    Id = enemyId++, X = (baseCol + 20) * MarioConstants.TILE,
                    Y = 14 * MarioConstants.TILE,
                    Vx = MarioConstants.ENEMY_SPEED, Alive = true, Type = EnemyType.Goomba,
                });
            }
        }

        // ----- FLOATING -----

        static void GenerateFloatingScreen(System.Random rng, List<LevelTile> tiles,
            List<EnemyState> enemies, List<ItemState> items,
            ref int enemyId, ref int itemId, int baseCol, float difficulty)
        {
            // Ground on entry/exit sides
            for (int c = 0; c < 5; c++) AddGround(tiles, baseCol + c);
            for (int c = 20; c < 25; c++) AddGround(tiles, baseCol + c);

            // Floating platforms to cross the gap
            int numPlats = 4 + rng.Next(3); // 4-6 platforms
            int cursor = 6;
            for (int i = 0; i < numPlats && cursor < 19; i++)
            {
                int pw = 2 + rng.Next(2); // 2-3 wide
                int row = 12 + rng.Next(4); // rows 12-15
                for (int c = 0; c < pw; c++)
                {
                    tiles.Add(new LevelTile { Col = baseCol + cursor + c, Row = row, Type = TileType.Brick });
                }
                cursor += pw + 1 + rng.Next(2); // gap 1-2
            }

            // Question blocks scattered above platforms
            tiles.Add(new LevelTile { Col = baseCol + 8, Row = 9, Type = TileType.QuestionBlock });
            tiles.Add(new LevelTile { Col = baseCol + 15, Row = 10, Type = TileType.QuestionBlock });
            items.Add(new ItemState
            {
                Id = itemId++, X = (baseCol + 8) * MarioConstants.TILE,
                Y = 9 * MarioConstants.TILE,
                Collected = false, Spawned = false, Type = ItemType.Mushroom,
            });

            // Enemies on entry/exit ground and on platforms
            enemies.Add(new EnemyState
            {
                Id = enemyId++, X = (baseCol + 3) * MarioConstants.TILE,
                Y = 14 * MarioConstants.TILE,
                Vx = -MarioConstants.ENEMY_SPEED, Alive = true, Type = EnemyType.Goomba,
            });
            enemies.Add(new EnemyState
            {
                Id = enemyId++, X = (baseCol + 22) * MarioConstants.TILE,
                Y = 14 * MarioConstants.TILE,
                Vx = -MarioConstants.ENEMY_SPEED, Alive = true, Type = EnemyType.Goomba,
            });
        }

        // ----- FINAL (Flagpole) -----

        static void GenerateFinalScreen(List<LevelTile> tiles, int baseCol)
        {
            AddFullGround(tiles, baseCol);

            // Staircase leading up (cols 8-14, ascending)
            for (int step = 0; step < 7; step++)
            {
                int col = baseCol + 8 + step;
                int topRow = MarioConstants.GROUND_ROW - 1 - step;
                for (int r = topRow; r < MarioConstants.GROUND_ROW; r++)
                {
                    tiles.Add(new LevelTile { Col = col, Row = r, Type = TileType.Ground });
                }
            }

            // Flagpole (col 16, rows 5-15)
            int flagCol = baseCol + 16;
            tiles.Add(new LevelTile { Col = flagCol, Row = 5, Type = TileType.FlagpoleTop });
            for (int r = 6; r < MarioConstants.GROUND_ROW; r++)
            {
                tiles.Add(new LevelTile { Col = flagCol, Row = r, Type = TileType.Flagpole });
            }

            // Castle (cols 19-23, rows 10-15)
            for (int c = 19; c <= 23; c++)
            {
                for (int r = 10; r < MarioConstants.GROUND_ROW; r++)
                {
                    tiles.Add(new LevelTile { Col = baseCol + c, Row = r, Type = TileType.Castle });
                }
            }
        }

        // ----- Block cluster helper -----

        static bool IsTileOccupied(List<LevelTile> tiles, int col, int row)
        {
            for (int i = tiles.Count - 1; i >= 0 && i >= tiles.Count - 200; i--)
            {
                var t = tiles[i];
                if (t.Col == col && t.Row == row) return true;
            }
            return false;
        }

        static void AddBlockCluster(System.Random rng, List<LevelTile> tiles,
            List<ItemState> items, ref int itemId,
            int startCol, int row, int width, float difficulty)
        {
            // Determine layout: single row, L-shape, or T-shape
            int layout = rng.Next(10);
            bool multiRow = layout < 3 && width >= 3; // 30% chance of 2nd row if wide enough

            // Build main row with smart type distribution
            int coinCount = 0;
            bool placedQuestion = false;
            for (int i = 0; i < width; i++)
            {
                int col = startCol + i;
                if (IsTileOccupied(tiles, col, row)) continue; // don't overlap pipes/ground

                TileType bt;
                if (i == width / 2 && !placedQuestion)
                {
                    // Center block is always a question block
                    bt = TileType.QuestionBlock;
                    placedQuestion = true;
                }
                else if (coinCount == 0 && rng.Next(100) < 12)
                {
                    bt = TileType.CoinBlock;
                    coinCount++;
                }
                else if (!placedQuestion && rng.Next(100) < 25)
                {
                    bt = TileType.QuestionBlock;
                    placedQuestion = true;
                }
                else
                {
                    bt = TileType.Brick;
                }

                tiles.Add(new LevelTile { Col = col, Row = row, Type = bt });

                // Every question block spawns a mushroom
                if (bt == TileType.QuestionBlock)
                {
                    items.Add(new ItemState
                    {
                        Id = itemId++, X = col * MarioConstants.TILE,
                        Y = row * MarioConstants.TILE,
                        Collected = false, Spawned = false, Type = ItemType.Mushroom,
                    });
                }
            }

            // Optional second row (above, offset by 1-2 cols)
            if (multiRow)
            {
                int offset = rng.Next(2); // start 0-1 cols in from main row
                int row2 = row - 3; // 3 rows above (jumpable gap)
                int w2 = Math.Max(2, width - 1 - rng.Next(2));
                for (int i = 0; i < w2; i++)
                {
                    int col = startCol + offset + i;
                    if (IsTileOccupied(tiles, col, row2)) continue;
                    if (row2 < 6) continue; // don't place too high

                    TileType bt2 = rng.Next(100) < 30 ? TileType.QuestionBlock : TileType.Brick;
                    tiles.Add(new LevelTile { Col = col, Row = row2, Type = bt2 });

                    if (bt2 == TileType.QuestionBlock)
                    {
                        items.Add(new ItemState
                        {
                            Id = itemId++, X = col * MarioConstants.TILE,
                            Y = row2 * MarioConstants.TILE,
                            Collected = false, Spawned = false, Type = ItemType.Mushroom,
                        });
                    }
                }
            }
        }
    }
}
