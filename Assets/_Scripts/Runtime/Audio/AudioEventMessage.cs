using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Discode.Breakout
{
	public struct AudioEventMessage : NetworkMessage
	{
		public AudioEventType AudioEvent;
		public float HorizontalPosition;
		public float VerticalPosition;
		public bool bugProof;
	}
}
