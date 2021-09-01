using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Discode.Breakout.Balls
{
	/// <summary>
	/// Manager for handling balls.
	/// </summary>
	public class BallManager : MonoBehaviour
    {
		public static event Ball.BallEventHanlder OnBallAdded = null;
		public static event Ball.BallEventHanlder OnBallRemoved = null;

		/// <summary>
		/// Pseudo singleton reference for active instance of the manager.
		/// </summary>
		public static BallManager Instance { get; private set; }

		/// <summary>
		/// List of created balls.
		/// </summary>
        private List<Ball> balls = new List<Ball>();

		public int BallCount => balls.Count;

		/// <summary>
		/// Pseudo singleton logic to ensure only one is allowed.
		/// </summary>
		private void Awake()
		{
			if (Instance != null && Instance != this)
			{
				Destroy(gameObject);
				return;
			}
			Instance = this;
		}

		/// <summary>
		/// Registers ball with the manager.
		/// </summary>
		/// <param name="ball">Ball to be registered</param>
		public void RegisterBall(Ball ball)
		{
			if (balls.Contains(ball))
			{
				return;
			}
			balls.Add(ball);
			OnBallAdded?.Invoke(ball);
		}

		/// <summary>
		/// Unregisters ball with the manager.
		/// </summary>
		/// <param name="ball">Ball to be unregistered</param>
		public void UnregisterBall(Ball ball)
		{
			balls.Remove(ball);
			OnBallRemoved?.Invoke(ball);
		}

		/// <summary>
		/// Get all balls.
		/// </summary>
		/// <returns>Array of all balls.</returns>
		public Ball[] GetAllBalls() => balls.ToArray();

		public Ball FindBall(uint netId)
		{
			foreach(Ball ball in balls)
			{
				if (ball.netId == netId)
				{
					return ball;
				}
			}

			return null;
		}
	}
}
