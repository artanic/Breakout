using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Discode.Breakout.Gameplay;
using Discode.Breakout.Networking;

namespace Discode.Breakout.UI
{
    public class GameplayUI : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text scoreDisplay = null;

		private void OnEnable()
		{
			MyNetworkManager.OnClientStart += OnClientStart;
			Scorer.OnScoreChanged += OnScoreChanged;
			OnScoreChanged(0);
		}

		private void OnDisable()
		{
			MyNetworkManager.OnClientStart -= OnClientStart;
			Scorer.OnScoreChanged -= OnScoreChanged;
		}

		private void OnClientStart()
		{
			GameObject controller = GameObject.FindGameObjectWithTag("GameController");
			GameController gameController = controller.GetComponent<GameController>();
			OnScoreChanged(gameController.GetCurrentScore());
		}

		private void OnScoreChanged(int score)
		{
			scoreDisplay.text = score.ToString();
		}
	}
}
