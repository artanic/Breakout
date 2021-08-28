using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Discode.Breakout.Gameplay;

namespace Discode.Breakout.Players
{
	public class Player : NetworkBehaviour
	{
		[SerializeField]
		private Paddle paddle;

		public Paddle CurrentPaddle => paddle;

		private void Update()
		{
			if (hasAuthority == false)
			{
				return;
			}

			if (Input.GetKeyDown(KeyCode.Space))
			{
				if (paddle.HasBall)
				{
					paddle.ReleaseBall();
				}
			}

			if (Input.GetKey(KeyCode.RightArrow))
			{
				paddle.MoveRight();
			}
			else if (Input.GetKey(KeyCode.LeftArrow))
			{
				paddle.MoveLeft();
			}
		}
	}
}
