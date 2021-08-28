using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Discode.Breakout.Players;
using Discode.Breakout.Networking;

namespace Discode.Breakout.Gameplay
{
    public class LevelController : NetworkBehaviour
    {
		[SerializeField]
		private Scorer scorer = null;

		[SerializeField]
		private Ball ballTemplate = null;

		[SerializeField]
		private float firstPlayerPaddlePosition = 0.2f;

		[SerializeField]
		private float secondPlayerPaddlePosition = 0.4f;

		private List<Player> players = new List<Player>();
		private List<Brick> bricks = new List<Brick>();
		

		public Ball CurrentBall { get; private set; }


		private void OnEnable()
		{
			MyNetworkManager.OnAddPlayer += OnAddPlayer;
		}

		private void OnDisable()
		{
			MyNetworkManager.OnAddPlayer -= OnAddPlayer;
		}

		[Server]
		private void OnAddPlayer(NetworkConnection connection)
		{
			float screenPosition = NetworkManager.singleton.numPlayers == 0 ? firstPlayerPaddlePosition : secondPlayerPaddlePosition;

			var screenBottomCenter = new Vector3(Screen.width / 2, Screen.height * screenPosition, 0);
			var inWorldPosition = Camera.main.ScreenToWorldPoint(screenBottomCenter);

			GameObject newPlayer = Instantiate(NetworkManager.singleton.playerPrefab, new Vector3(inWorldPosition.x, inWorldPosition.y, 10), Quaternion.identity);
			newPlayer.name = $"{NetworkManager.singleton.playerPrefab.name} [connId={connection.connectionId}]";
			NetworkServer.AddPlayerForConnection(connection, newPlayer);
			Player player = newPlayer.GetComponent<Player>();

			GameObject controller = GameObject.FindGameObjectWithTag("GameController");
			LevelController levelController = controller.GetComponent<LevelController>();
			levelController.RegisterPlayer(player);

			if (NetworkManager.singleton.numPlayers > 0)
			{
				levelController.StartNewRound();
			}
		}

		[Server]
		private Ball UnregisterCurrentBall()
		{
			CurrentBall.OnOutOfBounds -= OnBallOutOfBounds;
			CurrentBall = null;
			return CurrentBall;
		}

		[Server]
		private void OnBallOutOfBounds(Ball ball)
		{
			UnregisterCurrentBall();
			StartCoroutine(DestroyBallAfterTime(ball, 3));
			StartNewRound();
		}

		[Server]
		private void OnBrickDestroyed(Brick brick)
		{
			Debug.Log("OnBrickDestroyed");
			scorer.AddScore(100);
			UnregisterBrick(brick);
		}

		[Server]
		public void RegisterBrick(Brick brick)
		{
			if (bricks.Contains(brick))
			{
				return;
			}
			brick.OnDestryoed += OnBrickDestroyed;
			bricks.Add(brick);
		}

		[Server]
		public void UnregisterBrick(Brick brick)
		{
			brick.OnDestryoed -= OnBrickDestroyed;
			bricks.Remove(brick);
		}

		[Server]
		public Brick[] GetBricks()
		{
			return bricks.ToArray();
		}

		[Server]
		public void RegisterPlayer(Player player)
		{
			if (players.Contains(player))
			{
				return;
			}
			players.Add(player);
		}

		[Server]
		public void UnregisterPlayer(Player player)
		{
			players.Remove(player);
		}

		[Command(requiresAuthority = false)]
		public void StartNewRound()
		{
			if (CurrentBall != null)
			{
				NetworkServer.Destroy(UnregisterCurrentBall().gameObject);
			}

			if (players.Count == 0)
			{
				return;
			}

			CurrentBall = Instantiate(ballTemplate);
			NetworkServer.Spawn(CurrentBall.gameObject);
			players[0].CurrentPaddle.AssignBall(CurrentBall);
			CurrentBall.OnOutOfBounds += OnBallOutOfBounds;
		}

		[Server]
		private IEnumerator DestroyBallAfterTime(Ball ball, float seconds)
		{
			yield return new WaitForSeconds(seconds);

			NetworkServer.Destroy(ball.gameObject);
		}
	}
}
