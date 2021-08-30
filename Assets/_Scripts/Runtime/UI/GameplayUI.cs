using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Discode.Breakout.Gameplay;

namespace Discode.Breakout.UI
{
    public class GameplayUI : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text scoreDisplay = null;

		private void OnEnable()
		{
			GameObject controller = GameObject.FindGameObjectWithTag("GameController");
			GameController gameController = controller.GetComponent<GameController>();
			Scorer.OnScoreChanged += OnScoreChanged;
			OnScoreChanged(gameController.GetCurrentScore());
		}

		private void OnDisable()
		{
			Scorer.OnScoreChanged -= OnScoreChanged;
		}

		private void OnScoreChanged(int score)
		{
			scoreDisplay.text = score.ToString();
		}
	}
}
