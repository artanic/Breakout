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
		private BoxCollider boxCollider = null;

		[SerializeField]
		private RowColorStore rowColorStore = null;

		[SyncVar(hook = nameof(OnLayerChanged))]
		private int layer = -1;

		private Material instanceMaterial;

		public float Width => boxCollider.size.x;

		public int Layer
		{
			get { return layer; }
			set
			{
				if (layer != value)
				{
					if (isServer && isClient)
					{
						OnLayerChanged(layer, value);
					}
					layer = value;
				}
			}
		}

		private void OnEnable()
		{
			BrickManager.Instance.RegisterBrick(this);
		}

		private void OnDisable()
		{
			BrickManager.Instance.UnregisterBrick(this);
		}

		private void OnDestroy()
		{
			if (instanceMaterial != null)
			{
				Destroy(instanceMaterial);
			}
		}

		private void OnCollisionEnter(Collision collision)
		{
			OnDestryoed?.Invoke(this);
		}

		private void OnLayerChanged(int oldValue, int newValue)
		{
			MeshRenderer renderer = GetComponent<MeshRenderer>();
			renderer.material.SetColor("_BaseColor", rowColorStore.GetColor(newValue));
			instanceMaterial = renderer.material;
		}
	}
}
