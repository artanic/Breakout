using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Discode.Breakout
{
	public enum AudioEventType : byte
	{
		None,
		Bounce,
		Break,
		Dropout,
		Launch
	}
}
