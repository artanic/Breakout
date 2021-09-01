using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Discode.Breakout
{
	public enum GameEventType : byte
	{
		Undefined,
		Dropout,
		NewGame,
		NewRound,
		Complete
	}
}
