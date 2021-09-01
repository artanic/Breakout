using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Discode.Breakout.Players;
using Discode.Breakout.Networking;
using Discode.Breakout.Levels;
using Discode.Breakout.Balls;
using Discode.Breakout.Paddles;
using Discode.Breakout.Bricks;
using Discode.Breakout.Client;

namespace Discode.Breakout.Server
{
	/// <summary>
	/// Handles server only logic. Only one per game.
	/// </summary>
	[DisallowMultipleComponent]
	public class ServerGameBehaviour : NetworkBehaviour
	{
		/// <summary>
		/// How far along the Z-Axis the level is positioned.
		/// </summary>
		[SerializeField, Tooltip("How far along the Z-Axis the level is positioned")]
		private float levelDepth = 10f;

		/// <summary>
		/// Reference to a LevelBuilder component is needed.
		/// </summary>
		[SerializeField, Required("Reference to a LevelBuilder component is needed")]
		private LevelBuilder levelBuilder = null;

		/// <summary>
		/// How far along the Z-Axis the level is positioned.
		/// </summary>
		[SerializeField, Tooltip("How far along the Z-Axis the level is positioned")]
		private float firstPlayerPaddlePosition = 0.2f;

		/// <summary>
		/// How far along the Z-Axis the level is positioned.
		/// </summary>
		[SerializeField, Tooltip("How far along the Z-Axis the level is positioned")]
		private float secondPlayerPaddlePosition = 0.4f;

		/// <summary>
		/// A list of balls active on the field, able to support more than one at a time
		/// </summary>
		private List<Ball> activeBalls = new List<Ball>();

		private GameController gameController = null;

		/// <summary>
		/// Paddle occupying first position.
		/// </summary>
		private Paddle firstPaddle = null;

		/// <summary>
		/// Paddle occupying second position.
		/// </summary>
		private Paddle secondPaddle = null;

		/// <summary>
		/// How far along the Z-Axis the level is positioned
		/// </summary>
		public float LevelDepth => levelDepth;

		private void Awake()
		{
			GameNetworkManager.OnAddPlayer += OnPlayerAdded;
			GameNetworkManager.OnServerDisconnection += OnServerDisconnection;

			ClientGameBehaviour.OnResetGameRequest += OnResetGameRequest;

			PaddleManager.OnRemoved += OnPaddleRemoved;

			BrickManager.OnBrickAdded += OnBrickAdded;
			BrickManager.OnBrickRemoved += OnBrickRemoved;
			BallManager.OnBallAdded += OnBallAdded;
			BallManager.OnBallRemoved += OnBallRemoved;
		}

		private void OnDestroy()
		{
			GameNetworkManager.OnAddPlayer -= OnPlayerAdded;
			GameNetworkManager.OnServerDisconnection -= OnServerDisconnection;

			ClientGameBehaviour.OnResetGameRequest -= OnResetGameRequest;

			PaddleManager.OnRemoved -= OnPaddleRemoved;

			BrickManager.OnBrickAdded -= OnBrickAdded;
			BrickManager.OnBrickRemoved -= OnBrickRemoved;
			BallManager.OnBallAdded -= OnBallAdded;
			BallManager.OnBallRemoved -= OnBallRemoved;
		}

		/// <summary>Like Start(), but only called on server and host.</summary>
		public override void OnStartServer()
		{
			GameObject controller = GameObject.FindGameObjectWithTag(UnityConstants.Tags.GameController);
			gameController = controller.GetComponent<GameController>();

			// Server constructs the level structure when starting
			levelBuilder.CreateBorderAndDropzone();
		}

		/// <summary>Stop event, only called on server and host.</summary>
		public override void OnStopServer()
		{
			// Server clears the level structure when stopping
			levelBuilder.Clear();
		}

		/// <summary>
		/// Create and assign paddle when a player has been added to PlayerManager.
		/// Starts the game if it has already.
		/// </summary>
		/// <param name="player"></param>
		private void OnPlayerAdded(NetworkConnection connection)
		{
			GameObject newPlayer = Instantiate(Mirror.NetworkManager.singleton.playerPrefab);
#if UNITY_EDITOR
			newPlayer.name = $"{Mirror.NetworkManager.singleton.playerPrefab.name} [connId={connection.connectionId}]";
#endif
			NetworkServer.AddPlayerForConnection(connection, newPlayer);
			Player player = newPlayer.GetComponent<Player>();
			PlayerManager.Instance.RegisterPlayer(connection, player);

			bool paddlleAtFirstPosition = false;

			float screenPosition = secondPlayerPaddlePosition;

			if (firstPaddle == null)
			{
				screenPosition = firstPlayerPaddlePosition;
				paddlleAtFirstPosition = true;
			}

			var screenBottomCenter = new Vector3(Screen.width / 2, Screen.height * screenPosition, 0);
			var inWorldPosition = Camera.main.ScreenToWorldPoint(screenBottomCenter);

			Paddle paddle = levelBuilder.CreateNewPaddle(new Vector3(inWorldPosition.x, inWorldPosition.y, LevelDepth));
			player.CurrentPaddle = paddle;

			if (paddlleAtFirstPosition)
			{
				firstPaddle = player.CurrentPaddle;
			}
			else
			{
				secondPaddle = player.CurrentPaddle;
			}

			// If player is now added to play.
			if (PlayerManager.Instance.PlayerCount == 1)
			{
				// Generate leve and start a new round.
				levelBuilder.Build();
				StartNewRound();
			}
		}

		/// <summary>
		/// Remove paddle from player and restarts the game state if needed.
		/// </summary>
		/// <param name="player"></param>
		private void OnServerDisconnection(NetworkConnection connection)
		{
			if (PlayerManager.Instance.TryGetPlayer(connection, out Player player))
			{
				if (player.CurrentPaddle != null)
				{
					if (player.CurrentPaddle.HasBall)
					{
						Ball ball = player.CurrentPaddle.ReturnBall();
						OnBallDestroyed(ball);
					}

					levelBuilder.RemovePaddle(player.CurrentPaddle);
					player.CurrentPaddle = null;
				}

				PlayerManager.Instance.UnregisterPlayer(connection, player);
			}

			if (PlayerManager.Instance.PlayerCount > 0 && activeBalls.Count == 0)
			{
				// If the ball was being held by the player being removed. Start a new round.
				StartNewRound();
			}
			else if (PlayerManager.Instance.PlayerCount == 0)
			{
				// If all players have left the game. Reset and clear the field.
				gameController.Scorer.SetScore(0);
				ClearAllBricks();
				RemoveAllBalls();
			}
		}

		/// <summary>
		/// Trigger a reset and starts a new game.
		/// </summary>
		private void OnResetGameRequest()
		{
			StartNewGame();
		}

		/// <summary>
		/// Adds game logic on brick events.
		/// </summary>
		/// <param name="brick"></param>
		private void OnBrickAdded(Brick brick)
		{
			brick.OnDestryoed += OnBrickDestroyed;
		}

		/// <summary>
		/// Removes game logic on brick events.
		/// </summary>
		/// <param name="brick"></param>
		private void OnBrickRemoved(Brick brick)
		{
			brick.OnDestryoed -= OnBrickDestroyed;
		}

		/// <summary>
		/// Handles the game logic when bricks are destroyed.
		/// </summary>
		/// <param name="brick"></param>
		private void OnBrickDestroyed(Brick brick)
		{
			gameController.Scorer.AddScore(100);
			levelBuilder.RemoveBrick(brick);
		}

		/// <summary>
		/// Adds game logic on ball events.
		/// </summary>
		/// <param name="ball"></param>
		private void OnBallAdded(Ball ball)
		{
			ball.OnBallDestroy += OnBallDestroyed;
			ball.OnOutOfBounds += OnBallOutOfBounds;
		}

		/// <summary>
		/// Removes game logic on ball evnets.
		/// </summary>
		/// <param name="ball"></param>
		private void OnBallRemoved(Ball ball)
		{
			activeBalls.Remove(ball);
			ball.OnBallDestroy -= OnBallDestroyed;
			ball.OnOutOfBounds -= OnBallOutOfBounds;
		}

		/// <summary>
		/// Logic the occurs when the ball hits dropzone.
		/// </summary>
		/// <param name="ball"></param>
		private void OnBallOutOfBounds(Ball ball)
		{
			// Remove from active list.
			activeBalls.Remove(ball);
			// Trigger destructions after delay to make sure the ball has left the visible area.
			ball.DestroyAfterTime(ball, 5);

			NetworkServer.SendToAll(new GameEventMessage(GameEventType.Dropout));

			// If active balls are zero. Stat new round.
			if (activeBalls.Count == 0)
			{
				StartNewRound();
			}
		}

		/// <summary>
		/// Removes ball on request from the game.
		/// </summary>
		/// <param name="ball">Ball to remove</param>
		private void OnBallDestroyed(Ball ball)
		{
			activeBalls.Remove(ball);
			levelBuilder.RemoveBall(ball);
		}

		/// <summary>
		/// Removes paddle from position.
		/// </summary>
		/// <param name="paddle"></param>
		private void OnPaddleRemoved(Paddle paddle)
		{
			// Remove paddle from either first or second position.
			if (firstPaddle == paddle)
			{
				firstPaddle = null;
			}
			else if (secondPaddle == paddle)
			{
				secondPaddle = null;
			}
		}

		/// <summary>
		/// Clear all active balls that exist.
		/// </summary>
		private void RemoveAllBalls()
		{
			if (BallManager.Instance.BallCount == 0)
			{
				return;
			}

			for (int i = activeBalls.Count - 1; i >= 0; i--)
			{
				levelBuilder.RemoveBall(activeBalls[i]);
			}
			activeBalls.Clear();
		}

		/// <summary>
		/// Clear all the bricks that exist
		/// </summary>
		private void ClearAllBricks()
		{
			var bricks = BrickManager.Instance.Getbricks();
			foreach(Brick brick in  bricks)
			{
				if(brick == null)
				{
					return;
				}
				levelBuilder.RemoveBrick(brick);
			}
		}

		/// <summary>
		/// Resets current game and starts a new one.
		/// </summary>
		[Server]
		public void StartNewGame()
		{
			// Clear the currently generated bricks.
			ClearAllBricks();
			// Build the brick layout.
			levelBuilder.Build();
			RemoveAllBalls();
			// Start a new round.
			StartNewRound();
			// Inform clients a new game has started so that update their states.
			NetworkServer.SendToAll(new GameEventMessage(GameEventType.NewGame));
		}

		/// <summary>
		/// Clears the current state and resets the ball to start position.
		/// </summary>
		[Server]
		public void StartNewRound()
		{
			// Create new ball.
			Ball newBall = levelBuilder.GenerateNewBall();
			activeBalls.Add(newBall);
			// Assign it to first player.
			if (firstPaddle != null)
			{
				firstPaddle.AssignBall(newBall);
				return;
			}

			secondPaddle?.AssignBall(newBall);
		}
	}
}
