using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Discode.Breakout.Gameplay;

namespace Discode.Breakout.Players
{
	public class Player : NetworkBehaviour
	{
		public delegate void PlayerEventHandler(Player player);

		public event PlayerEventHandler OnLeavingGame = null;

		public int PlayerID { get; set; }

		public Paddle CurrentPaddle { get; set; }

		private void OnDestroy()
		{
			OnLeavingGame?.Invoke(this);
		}

		private void Update()
		{
			if (hasAuthority == false)
			{
				return;
			}

			if (Input.GetKeyDown(KeyCode.Space))
			{
				ReleaseBall();
			}

			if (Input.GetKey(KeyCode.RightArrow))
			{
				MovePaddleRight();
			}
			else if (Input.GetKey(KeyCode.LeftArrow))
			{
				MovePaddleLeft();
			}
		}

		[Command]
		private void MovePaddleLeft()
		{
			CurrentPaddle?.MoveLeft();
		}

		[Command]
		private void MovePaddleRight()
		{
			CurrentPaddle?.MoveRight();
		}

		[Command]
		private void ReleaseBall()
		{
			if (CurrentPaddle.HasBall)
			{
				CurrentPaddle.ReleaseBall();
			}
		}
	}
}
