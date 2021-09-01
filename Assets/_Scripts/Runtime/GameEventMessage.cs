using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Discode.Breakout
{
	public readonly struct GameEventMessage : NetworkMessage
	{
		public readonly GameEventType GameEvent;

		public GameEventMessage(GameEventType gameEvent)
		{
			GameEvent = gameEvent;
		}
	}
}
