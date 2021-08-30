using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Discode.Breakout.Players;
using Discode.Breakout.Networking;

namespace Discode.Breakout.Gameplay
{
    public class GameController : MonoBehaviour
    {
        [SerializeField]
        private GameBehaviour gameBehaviourTemplate = null;

		[SerializeField]
		private BrickManager brickManagerTemplate = null;

		[SerializeField]
		private Scorer scorerTemplate = null;

		private GameBehaviour currentGameBehaviour = null;
		
		public Scorer CurrentScorer { get; private set; }

		private void Awake()
		{
			MyNetworkManager.OnServerStart += OnServerStart;
		}

		private void OnDestroy()
		{
			MyNetworkManager.OnServerStart -= OnServerStart;
		}

		private void OnServerStart()
		{
			BrickManager newBrickManager = Instantiate(brickManagerTemplate);
			NetworkServer.Spawn(newBrickManager.gameObject);

			GameBehaviour newBehaviour = Instantiate(gameBehaviourTemplate);
			currentGameBehaviour = newBehaviour;
			NetworkServer.Spawn(newBehaviour.gameObject);

			Scorer newScorer = Instantiate(scorerTemplate);
			CurrentScorer = newScorer;
			NetworkServer.Spawn(CurrentScorer.gameObject);
		}

		public int GetCurrentScore() => CurrentScorer != null ? CurrentScorer.CurrentScore : 0;
	}
}
