﻿using System;
using System.Collections.Generic;
using System.Drawing;
using Aiv.Fast2D;
using Aiv.Vorbis;
using OpenTK;
using OpenTK.Input;

namespace Bomberman
{
	class Game
	{
		public static Window window;
		public static Map map;
		public static Texture BombTexture;
		public static Texture WallTexture;
		public static Texture DestrWallTexture;
		public static Texture EnemyTexture;
		public static Texture PowerUpTexture;
		public static Player player;
		public static Player player2;
		public static Texture PlayerTexture;

		public static Texture BackgroundTexture;
		public static Sprite BackgroundSprite;

		public static Enemy[] EnemiesList;

		public static int stateJohn = 0;
		private static Bomb bombCena;

		static Game()
		{
			init();
		}

		private static void init()
		{
			////init window
			window = new Window(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height, "Bomberman", true);

			PlayerTexture = new Texture("../../assets/player.png");
			BombTexture = new Texture("../../assets/bomb.png");
			WallTexture = new Texture("../../assets/wall.png");
			DestrWallTexture = new Texture("../../assets/destrWall.png");
			EnemyTexture = new Texture("../../assets/enemy.png");
			PowerUpTexture = new Texture("../../assets/defaultPowerUp.png");

			BackgroundTexture = new Texture(1, 1);
			BackgroundTexture.Bitmap[0] = 0;
			BackgroundTexture.Bitmap[1] = 115;
			BackgroundTexture.Bitmap[2] = 0;
			BackgroundTexture.Bitmap[3] = 255;
			BackgroundTexture.Update();
			BackgroundSprite = new Sprite(window.Width, window.Height);

			//init map
			map = new Map(32);

			//init player's key map and player
			KeyMap player1KeyMap = new KeyMap(KeyCode.Left, KeyCode.Right, KeyCode.Up, KeyCode.Down, KeyCode.Space);
			player = new Player(1, 1, player1KeyMap);

			KeyMap player2KeyMap = new KeyMap(KeyCode.A, KeyCode.D, KeyCode.W, KeyCode.S, KeyCode.F);
			player2 = new Player(1, 1, player2KeyMap);

			//init enemies
			EnemiesList = new Enemy[5];
			for (int i = 0; i < EnemiesList.Length; i++)
			{
				int rX, rY;
				do
				{
					rX = Utils.Randomize(7, map.Width);
					rY = Utils.Randomize(7, map.Height);
				} while (map.Tiles[Utils.GetPos(rX, rY, map.Width)] != Map.TileType.None);
				EnemiesList[i] = new Enemy(1, Utils.Randomize(70, 81), rX, rY);
			}

		}

		public static void Update()
		{
			Console.SetCursorPosition(0, 0);
			Console.WriteLine("FPS: {0}            ", 1/Game.window.deltaTime);

			BackgroundSprite.DrawTexture(BackgroundTexture, 0, 0);

			player.CheckMovement();

			EnemiesAI();

			map.Draw();
			player.Draw();
			DrawEnemies();

			player.PrintInfo();

			Text.ChangeColor(Color.White);
			Text.PrintText("Fuse time " + player.BombFuseTime + " seconds", 0, 580, 12, 20);
			Text.PrintText("Bombs " + (player.BombsAvailable + player.BombsPlaced), 0, 580 + 20, 12, 20);
			Text.PrintText("Movement Speed " + player.MovSpeed + " pps", 0, 580 + 40, 12, 20);
			Text.PrintText("Radius " + player.BombRadius, 0, 580 + 60, 12, 20);
			Text.PrintText("Spiny Bombs " + player.SpinyBombs, 0, 580 + 80, 12, 20);
			//Text.PrintText(window.mouseX + " " + window.mouseY, 0, 580 + 100, 12, 20);

			if (stateJohn == 21)
			{
				bombCena.johnSprite.DrawTexture(bombCena.johnTexture, ((int)(bombCena.johnIndex % 5) * bombCena.johnSprite.Width / 5) + map.Scroll, ((int)(bombCena.johnIndex / 5) * bombCena.johnSprite.Height / 5), bombCena.johnSprite.Width / 5, bombCena.johnSprite.Height / 5);
				bombCena.johnIndex += 20 * window.deltaTime;
				if (bombCena.johnIndex >= 16)
					stateJohn = 22;
			}

			//Fuse bombs
			List<Bomb> toRemove = new List<Bomb>();
			foreach (Bomb bomb in player.Bombs)
			{
				if (bomb.Fuse(player))
				{
					if (stateJohn == 21)
					{
						bombCena = bomb;
					}
					toRemove.Add(bomb);
				}

			}
			foreach (var bombToRemove in toRemove)
			{
				player.Bombs.Remove(bombToRemove);
			}
			toRemove.Clear();
			//Draw
			Game.window.Update();

			//Esc + Return (Enter) closes the game
			if (Game.window.GetKey(KeyCode.Esc) && Game.window.GetKey(KeyCode.Return))
				Game.window.opened = false;

			//ee
			if (window.mouseLeft)
			{
				if (stateJohn == 0 && window.mouseX == 0 && window.mouseY == window.Height - 1)
				{
					stateJohn = 1;
				}
				else if (stateJohn >= 1 && stateJohn < 20 && window.mouseX >= player.X - 16 && window.mouseX <= player.X + 16 && window.mouseY >= player.Y - 16 &&
				         window.mouseY <= player.Y + 16)
				{
					stateJohn++;
				}
			}
		}

		public static void DrawEnemies()
		{
			foreach (var enemy in EnemiesList)
			{
				enemy.Draw();
			}
		}

		public static void EnemiesAI()
		{
			foreach (var enemy in EnemiesList)
			{
				if (Utils.Randomize(0, 100) == 0 && enemy.X % 32 >= 14 && enemy.X % 32 <= 18 && enemy.Y % 32 >= 14 && enemy.Y % 32 <= 18)
				{
					enemy.DirectionMoving = (Enemy.Direction)Utils.Randomize(0, 4);
					enemy.spriteState = Utils.Randomize(0, 4);

				}
				switch (enemy.DirectionMoving)
				{
					case Enemy.Direction.UP:
						if (map.Tiles[Utils.GetPos(enemy.X / map.TileSize, (enemy.Y - (int)(enemy.MovSpeed * window.deltaTime) - 16) / map.TileSize, map.Width)] != Map.TileType.None)
						{
							if (map.Tiles[Utils.GetPos(enemy.X / map.TileSize, (enemy.Y + (int)(enemy.MovSpeed * window.deltaTime) + 16) / map.TileSize, map.Width)] != Map.TileType.None)
								enemy.RandomDirection();
							else
							{
								if (Utils.Randomize(0, 5) == 0)
									enemy.RandomDirection();
								else
									enemy.DirectionMoving = Enemy.Direction.DOWN;
								enemy.spriteState = Utils.Randomize(0, 4);
							}
						}
						break;
					case Enemy.Direction.RIGHT:
						if (map.Tiles[Utils.GetPos((enemy.X + (int)(enemy.MovSpeed * window.deltaTime) + 16) / map.TileSize, enemy.Y / map.TileSize, map.Width)] != Map.TileType.None)
							if (map.Tiles[Utils.GetPos((enemy.X - (int)(enemy.MovSpeed * window.deltaTime) - 16) / map.TileSize, enemy.Y / map.TileSize, map.Width)] != Map.TileType.None)
								enemy.RandomDirection();
							else
							{
								if (Utils.Randomize(0, 5) == 0)
									enemy.RandomDirection();
								else
									enemy.DirectionMoving = Enemy.Direction.LEFT;
								enemy.spriteState = Utils.Randomize(0, 4);
							}
						break;
					case Enemy.Direction.DOWN:
						if (map.Tiles[Utils.GetPos(enemy.X / map.TileSize, (enemy.Y + (int)(enemy.MovSpeed * window.deltaTime) + 16) / map.TileSize, map.Width)] != Map.TileType.None)
							if (map.Tiles[Utils.GetPos(enemy.X / map.TileSize, (enemy.Y - (int)(enemy.MovSpeed * window.deltaTime) - 16) / map.TileSize, map.Width)] != Map.TileType.None)
								enemy.RandomDirection();
							else
							{
								if (Utils.Randomize(0, 5) == 0)
									enemy.RandomDirection();
								else
									enemy.DirectionMoving = Enemy.Direction.UP;
								enemy.spriteState = Utils.Randomize(0, 4);
							}
						break;
					case Enemy.Direction.LEFT:
						if (map.Tiles[Utils.GetPos((enemy.X - (int)(enemy.MovSpeed * window.deltaTime) - 16) / map.TileSize, enemy.Y / map.TileSize, map.Width)] != Map.TileType.None)
							if (map.Tiles[Utils.GetPos((enemy.X + (int)(enemy.MovSpeed * window.deltaTime) + 16) / map.TileSize, enemy.Y / map.TileSize, map.Width)] != Map.TileType.None)
								enemy.RandomDirection();
							else
							{
								if (Utils.Randomize(0, 5) == 0)
									enemy.RandomDirection();
								else
									enemy.DirectionMoving = Enemy.Direction.RIGHT;
								enemy.spriteState = Utils.Randomize(0, 4);
							}
						break;
				}
				enemy.Move();
			}
		}

		public static void StartMusic()
		{

		}
	}
}
