using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Discode.Breakout.Players;
using Discode.Breakout.Networking;

namespace Discode.Breakout.Gameplay
{
    public class ServerGameBehaviour : NetworkBehaviour
	{
		[SerializeField]
		private float levelDepth = 10f;

		[SerializeField]
		private Ball ballTemplate = null;

		[SerializeField]
		private Paddle paddleTemplate = null;

		[SerializeField]
		private LevelBuilder levelBuilder = null;

		[SerializeField]
		private float firstPlayerPaddlePosition = 0.2f;

		[SerializeField]
		private float secondPlayerPaddlePosition = 0.4f;

		private List<Player> players = new List<Player>();
		public float LevelDepth => levelDepth;

		public Ball CurrentBall { get; private set; }

		public GameController GameController { get; private set; }

		private void Awake()
		{
			MyNetworkManager.OnAddPlayer += OnAddPlayer;
			BrickManager.OnBrickAdded += OnBrickAdded;
			BrickManager.OnBrickRemoved += OnBrickRemoved;
		}

		private void OnDestroy()
		{
			MyNetworkManager.OnAddPlayer -= OnAddPlayer;
			BrickManager.OnBrickAdded -= OnBrickAdded;
			BrickManager.OnBrickRemoved -= OnBrickRemoved;
		}

		private void Start()
		{
			GameObject controller = GameObject.FindGameObjectWithTag("GameController");
			GameController = controller.GetComponent<GameController>();
		}

		[Server]
		private void OnAddPlayer(NetworkConnection connection)
		{
			GameObject newPlayer = Instantiate(NetworkManager.singleton.playerPrefab);
			newPlayer.name = $"{NetworkManager.singleton.playerPrefab.name} [connId={connection.connectionId}]";
			NetworkServer.AddPlayerForConnection(connection, newPlayer);
			Player player = newPlayer.GetComponent<Player>();

			RegisterPlayer(player);

			if (players.Count == 1)
			{
				GenerateLevel();
				SetupNewRound();
			}
		}

		private void OnBrickAdded(Brick brick)
		{
			brick.OnDestryoed += OnBrickDestroyed;
		}


		private void OnBrickRemoved(Brick brick)
		{
			brick.OnDestryoed -= OnBrickDestroyed;
		}

		[Server]
		private void OnBrickDestroyed(Brick brick)
		{
			GameController.CurrentScorer.AddScore(100);
			BrickManager.Instance.RemoveBrick(brick);
		}

		[Server]
		private Ball UnregisterCurrentBall()
		{
			Ball ball = CurrentBall;
			ball.OnOutOfBounds -= OnBallOutOfBounds;
			CurrentBall = null;
			return ball;
		}

		[Server]
		private void OnBallOutOfBounds(Ball ball)
		{
			UnregisterCurrentBall();
			StartCoroutine(DestroyBallAfterTime(ball, 3));
			SetupNewRound();
		}

		[Server]
		private void RemoveCurrentBall()
		{
			if (CurrentBall != null)
			{
				NetworkServer.Destroy(UnregisterCurrentBall().gameObject);
			}
		}

		[Server]
		private void GenerateNewBall()
		{
			CurrentBall = Instantiate(ballTemplate);
			NetworkServer.Spawn(CurrentBall.gameObject);
			players[0].CurrentPaddle.AssignBall(CurrentBall);
			CurrentBall.OnOutOfBounds += OnBallOutOfBounds;
		}

		[Server]
		private void SetupNewRound()
		{
			RemoveCurrentBall();

			if (players.Count == 0)
			{
				return;
			}

			GenerateNewBall();
		}

		[Server]
		public void RegisterPlayer(Player player)
		{
			if (players.Contains(player))
			{
				return;
			}

			float screenPosition = players.Count == 0 ? firstPlayerPaddlePosition : secondPlayerPaddlePosition;

			var screenBottomCenter = new Vector3(Screen.width / 2, Screen.height * screenPosition, 0);
			var inWorldPosition = Camera.main.ScreenToWorldPoint(screenBottomCenter);
			Paddle newPaddle = Instantiate(paddleTemplate, new Vector3(inWorldPosition.x, inWorldPosition.y, LevelDepth), Quaternion.identity);
			NetworkServer.Spawn(newPaddle.gameObject);
			player.OnLeavingGame += UnregisterPlayer;
			player.CurrentPaddle = newPaddle;

			players.Add(player);
		}

		[Server]
		public void UnregisterPlayer(Player player)
		{
			bool removedPlayerHasBall = false;
			player.OnLeavingGame -= UnregisterPlayer;
			if (player.CurrentPaddle != null)
			{
				if (player.CurrentPaddle.HasBall)
				{
					removedPlayerHasBall = true;
					RemoveCurrentBall();
				}
				NetworkServer.Destroy(player.CurrentPaddle.gameObject);
			}
			players.Remove(player);
			if (players.Count > 0 && removedPlayerHasBall)
			{
				SetupNewRound();
			}
			else if (players.Count == 0)
			{
				ClearLevel();
			}
		}

		[Server]
		private void GenerateLevel()
		{
			levelBuilder.Build();
		}

		[Server]
		public void ClearLevel()
		{
			BrickManager.Instance.ClearBricks();
		}

		[Command(requiresAuthority = false)]
		public void BuildLevel()
		{
			GenerateLevel();
		}

		[Command(requiresAuthority = false)]
		public void StartNewRound()
		{
			SetupNewRound();
		}

		[Server]
		private IEnumerator DestroyBallAfterTime(Ball ball, float seconds)
		{
			yield return new WaitForSeconds(seconds);

			NetworkServer.Destroy(ball.gameObject);
		}
	}
}
