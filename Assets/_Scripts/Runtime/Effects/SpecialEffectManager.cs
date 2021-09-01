using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Discode.Breakout.Effects
{
	/// <summary>
	/// Manager for handling special effect.
	/// </summary>
	public class SpecialEffectManager : MonoBehaviour
    {
		/// <summary>
		/// Pseudo singleton reference for active instance of the manager.
		/// </summary>
		public static SpecialEffectManager Instance { get; private set; }

		/// <summary>
		/// Dictionary of object pools.
		/// </summary>
		private Dictionary<string, ObjectPool<SpecialEffect>> effectPools = new Dictionary<string, ObjectPool<SpecialEffect>>();

		/// <summary>
		/// List of all specialEffectInstance.
		/// </summary>
		private List<SpecialEffect> specialEffects = new List<SpecialEffect>();

		/// <summary>
		/// List of all active special effects.
		/// </summary>
		private List<SpecialEffect> activeEffects = new List<SpecialEffect>();

		private void Awake()
		{
			if (Instance != null && Instance != this)
			{
				Destroy(gameObject);
				return;
			}

			Instance = this;
		}

		/// <summary>
		/// Create a special effect instance from template.
		/// </summary>
		/// <param name="template">Prefab for cloning</param>
		/// <returns></returns>
		private SpecialEffect CreateEffect(SpecialEffect template)
		{
			SpecialEffect specialEffect = Instantiate(template);
			specialEffects.Add(specialEffect);
			return specialEffect;
		}

		/// <summary>
		/// Invoked when effect is done.
		/// </summary>
		/// <param name="specialEffect"></param>
		/// <param name="key"></param>
		private void OnEffectDone(SpecialEffect specialEffect, string key)
		{
			activeEffects.Remove(specialEffect);
			specialEffect.OnDone -= OnEffectDone;
			if (effectPools.ContainsKey(key))
			{
				effectPools[key].Return(specialEffect);
				specialEffect.gameObject.SetActive(false);
				return;
			}

			Destroy(specialEffect.gameObject);
		}

		/// <summary>
		/// Play effect. Fetch from object pool if possible.
		/// </summary>
		/// <param name="key">Object pool key</param>
		/// <param name="template">Preab of desired effect to play</param>
		/// <param name="position">Position in world space to playat</param>
		public void PlayEffect(string key, SpecialEffect template, Vector3 position)
		{
			if (effectPools.ContainsKey(key) == false)
			{
				ObjectPool<SpecialEffect> newPool = new ObjectPool<SpecialEffect>(() => CreateEffect(template));
				effectPools.Add(key, newPool);
			}

			SpecialEffect specialEffect = effectPools[key].Get();
			specialEffect.gameObject.SetActive(true);
			specialEffect.OnDone += OnEffectDone;
			activeEffects.Add(specialEffect);
			specialEffect.Play(key, position);
		}

		/// <summary>
		/// Clear and delete all currently existing objects tracked by the manager.
		/// </summary>
		public void Cleanup()
		{
			foreach(SpecialEffect specialEffect in specialEffects)
			{
				specialEffect.OnDone -= OnEffectDone;
				specialEffect.Stop();
				Destroy(specialEffect.gameObject);
			}
			specialEffects.Clear();
			activeEffects.Clear();
			effectPools.Clear();
		}
	}
}