using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Discode.Breakout.Gameplay
{
    public class Scorer : NetworkBehaviour
    {
		public delegate void ScoreEventHandler(int score);

		public static event ScoreEventHandler OnScoreChanged = null;


		[SyncVar(hook = nameof(OnScoreValueChanged))]
		[SerializeField]
		private int currentScore;

		public int CurrentScore => currentScore;

		private void Start()
		{
			if (isClient)
			{
				OnScoreChanged?.Invoke(CurrentScore);
			}
		}

		[Server]
		public void AddScore(int amount)
		{
			if (amount == 0)
			{
				return;
			}
			currentScore += amount;
			OnScoreChanged.Invoke(currentScore);
		}

		[Server]
		public void SetScore(int amount)
		{
			if (currentScore != amount)
			{
				currentScore = amount;
				OnScoreChanged.Invoke(currentScore);
			}
		}

		private void OnScoreValueChanged(int oldValue, int newValue)
		{
			OnScoreChanged?.Invoke(newValue);
		}
	}
}
