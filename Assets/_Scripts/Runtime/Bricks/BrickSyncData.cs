using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Discode.Breakout.Bricks
{
	public readonly struct BrickSyncData
	{
		/// <summary>
		/// Net Id of the Brick
		/// </summary>
		public readonly uint netId;
		/// <summary>
		/// Brick color ID.
		/// </summary>
		public readonly int ColorId;

		public BrickSyncData(uint netId, int colorId)
		{
			this.netId = netId;
			this.ColorId = colorId;
		}
	}
}
