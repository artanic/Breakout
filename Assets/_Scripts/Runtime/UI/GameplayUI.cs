using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Discode.Breakout.Networking;

namespace Discode.Breakout.UI
{
    public class GameplayUI : MonoBehaviour
    {
        [SerializeField, Required]
        private TMP_Text scoreDisplay = null;

		private void OnEnable()
		{
			GameNetworkManager.OnClientStart += OnClientStart;
			GameNetworkManager.OnClientStop += OnClientStop;
			Scorer.OnScoreChanged += OnScoreChanged;
			GameController.OnScorerChanged += OnScorerChanged;
			OnScoreChanged(0);
		}

		private void OnDisable()
		{
			GameNetworkManager.OnClientStart -= OnClientStart;
			GameNetworkManager.OnClientStop -= OnClientStop;
			Scorer.OnScoreChanged -= OnScoreChanged;
			GameController.OnScorerChanged -= OnScorerChanged;
		}

		private void OnClientStart()
		{
			GameObject controller = GameObject.FindGameObjectWithTag(UnityConstants.Tags.GameController);
			GameController gameController = controller.GetComponent<GameController>();
			if (gameController.Scorer != null)
			{
				OnScoreChanged(gameController.Scorer.CurrentScore);
			}
		}

		private void OnClientStop()
		{
			OnScoreChanged(0);
		}

		private void OnScorerChanged(Scorer scorer)
		{
			OnScoreChanged(scorer.CurrentScore);
		}

		private void OnScoreChanged(int score)
		{
			scoreDisplay.text = score.ToString();
		}
	}
}
