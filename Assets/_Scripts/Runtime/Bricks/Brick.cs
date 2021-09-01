using Discode.Breakout.Audio;
using Discode.Breakout.Effects;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Discode.Breakout.Bricks
{
	/// <summary>
	/// Controls brick behaviour
	/// </summary>
    public class Brick : NetworkBehaviour
    {
		public delegate void BrickEventHandler(Brick brick);

		/// <summary>
		/// Invoked when the brick is considered broken.
		/// </summary>
		public event BrickEventHandler OnDestryoed = null;

		[SerializeField, Required]
		private BoxCollider boxCollider = null;

		/// <summary>
		/// Store for coloring the brick.
		/// </summary>
		[SerializeField, Required]
		private BrickColorDefinition brickColorDefinition = null;
		
		/// <summary>
		/// Break sound Prefab.
		/// </summary>
		[SerializeField, Required]
        private AudioOrigin breakSoundTemplate = null;

		/// <summary>
		/// Effects to be used when brick is broken.
		/// </summary>
		[SerializeField]
		private SpecialEffect[] breakEffects = new SpecialEffect[0];

		private int colorId = -1;

		private Material instanceMaterial;

		/// <summary>
		/// Width of the Brick.
		/// </summary>
		public float Width => boxCollider.size.x;

		/// <summary>
		/// Layer that bricks exists on.
		/// </summary>
		public int ColorId
		{
			get { return colorId; }
			set
			{
				colorId = value;
				if (isServer == false || NetworkClient.isHostClient)
				{
					OnColorIDChanged(colorId);
				}
			}
		}

		private void Start()
		{
			BrickManager.Instance.RegisterBrick(this);
		}

		private void OnDestroy()
		{
			BrickManager.Instance.UnregisterBrick(this);

			// Clean up instance asset to prevent leak.
			if (instanceMaterial != null)
			{
				Destroy(instanceMaterial);
			}
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (collision.gameObject.CompareTag(UnityConstants.Tags.Ball))
			{
				if (isServer)
				{
					DestroyWithEffects();
				}

				if (NetworkClient.isHostClient)
				{
					DestroyWithEffectsForHost();
				}

				// Only one hit kill a brick.
				OnDestryoed?.Invoke(this);
			}
		}

		/// <summary>
		/// When Color ID changes. Update color on material (Warning: Creates instance). 
		/// </summary>
		/// <param name="oldValue">Old layer</param>
		/// <param name="newValue">New layer</param>
		private void OnColorIDChanged(int newValue)
		{
			MeshRenderer renderer = GetComponent<MeshRenderer>();
			renderer.material.SetColor("_BaseColor", brickColorDefinition.GetColor(colorId));
			instanceMaterial = renderer.material;
		}

		/// <summary>
		/// Destroys brick (after possible animation/effect is executed)
		/// </summary>
		[ClientRpc]
		private void DestroyWithEffects()
		{
			if (breakEffects.Length > 0)
			{
				SpecialEffect effect = breakEffects[Random.Range(0, breakEffects.Length)];
				SpecialEffectManager.Instance.PlayEffect(effect.name, effect, transform.position);
			}
			AudioManager.Instance.PlayAudioOrigin(GameConstants.AUDIO_KEY_BRICK_BREAK, breakSoundTemplate);
		}

		/// <summary>
		/// Host alternative since the client RPC doesn't triggering when client is also host.
		/// </summary>
		private void DestroyWithEffectsForHost()
		{
			if (breakEffects.Length > 0)
			{
				SpecialEffect effect = breakEffects[Random.Range(0, breakEffects.Length)];
				SpecialEffectManager.Instance.PlayEffect(effect.name, effect, transform.position);
			}
			AudioManager.Instance.PlayAudioOrigin(GameConstants.AUDIO_KEY_BRICK_BREAK, breakSoundTemplate);
		}
	}
}
