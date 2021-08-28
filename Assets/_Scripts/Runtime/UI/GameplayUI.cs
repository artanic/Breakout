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

		private Scorer scorer = null;

		private void OnEnable()
		{
			GameObject controller = GameObject.FindGameObjectWithTag("GameController");
			scorer = controller.GetComponent<Scorer>();
			scorer.OnScoreChanged += OnScoreChanged;
			OnScoreChanged(scorer.CurrentScore);
		}

		private void OnDisable()
		{
			if (scorer != null)
			{
				scorer.OnScoreChanged -= OnScoreChanged;
			}
		}

		private void OnScoreChanged(int score)
		{
			scoreDisplay.text = score.ToString();
		}
	}
}
