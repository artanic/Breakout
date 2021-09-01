using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Discode.Breakout
{
	/// <summary>
	/// Manager for handling the game score.
	/// </summary>
    public class Scorer : NetworkBehaviour
    {
		public delegate void ScoreEventHandler(int score);

		/// <summary>
		/// Invoked on score change.
		/// </summary>
		public static event ScoreEventHandler OnScoreChanged = null;

		/// <summary>
		/// Score value synced across clients.
		/// </summary>
		[SyncVar(hook = nameof(OnScoreValueChanged))]
		private int currentScore;

		public int CurrentScore => currentScore;

		private void Start()
		{
			// Unpdates score when host server and client are the same.
			if (isClient)
			{
				OnScoreChanged?.Invoke(CurrentScore);
			}
		}

		/// <summary>
		/// Add amount to the score.
		/// </summary>
		/// <param name="amount"></param>
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

		/// <summary>
		/// Sets the score to an amount.
		/// </summary>
		/// <param name="amount"></param>
		[Server]
		public void SetScore(int amount)
		{
			if (currentScore != amount)
			{
				currentScore = amount;
				OnScoreChanged.Invoke(currentScore);
			}
		}

		/// <summary>
		/// When syncvar changes. Invoke event.
		/// </summary>
		/// <param name="oldValue"></param>
		/// <param name="newValue"></param>
		private void OnScoreValueChanged(int oldValue, int newValue)
		{
			OnScoreChanged?.Invoke(newValue);
		}
	}
}
