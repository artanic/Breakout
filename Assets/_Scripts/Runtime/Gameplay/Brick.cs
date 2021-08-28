using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Discode.Breakout.Gameplay
{
    public class Brick : NetworkBehaviour
    {
		public delegate void BrickEventHandler(Brick brick);

		public event BrickEventHandler OnDestryoed = null;

		[SerializeField]
		private MeshFilter meshFilter = null;

		public float Width => meshFilter.sharedMesh.bounds.size.x * transform.localScale.x;

		[ServerCallback]
		private void OnCollisionEnter(Collision collision)
		{
			OnDestryoed?.Invoke(this);
			NetworkServer.Destroy(gameObject);
		}
	}
}
