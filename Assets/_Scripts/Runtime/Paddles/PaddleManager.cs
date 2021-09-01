using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Discode.Breakout.Paddles
{
	/// <summary>
	/// Manager for handling paddles.
	/// </summary>
    public class PaddleManager : MonoBehaviour
    {
		public delegate void PaddleEventhanlder(Paddle paddle);

		public static event PaddleEventhanlder OnAdded = null;
		public static event PaddleEventhanlder OnRemoved = null;

		/// <summary>
		/// Pseudo singleton reference for active instance of the manager.
		/// </summary>
		public static PaddleManager Instance { get; private set; }

		/// <summary>
		/// List of created paddles.
		/// </summary>
		private List<Paddle> paddles = new List<Paddle>();

#if UNITY_EDITOR
		/// <summary>
		/// [Editor Only] Returns the number of paddles registered.
		/// </summary>
		public int EditorPaddleCount => paddles.Count;
#endif

		/// <summary>
		/// Pseudo singleton logic to ensure only one is allowed.
		/// </summary>
		private void Awake()
		{
			if (Instance != null && Instance != this)
			{
				NetworkServer.Destroy(gameObject);
				return;
			}

			Instance = this;
		}

		/// <summary>
		/// Registers the paddle with the manager.
		/// </summary>
		/// <param name="paddle">Paddle to be registered</param>
		public void RegisterPaddle(Paddle paddle)
		{
			if (paddles.Contains(paddle))
			{
				return;
			}

			paddles.Add(paddle);
			OnAdded?.Invoke(paddle);
		}

		/// <summary>
		/// Unregisters the paddle with the manager.
		/// </summary>
		/// <param name="paddle">Paddle to be unregistered</param>
		public void UnregisterPaddle(Paddle paddle)
		{
			paddles.Remove(paddle);
			OnRemoved?.Invoke(paddle);
		}

		/// <summary>
		/// Find registered paddle using netId
		/// </summary>
		/// <param name="netId">netId of desired paddle</param>
		/// <returns>Matching Paddle</returns>
		public Paddle FindPaddle(uint netId)
		{
			foreach(Paddle paddle in paddles)
			{
				if (paddle.netId == netId)
				{
					return paddle;
				}
			}

			return null;
		}
    }
}
