using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Discode.Breakout.Audio
{
	/// <summary>
	/// Manager for handling audio.
	/// </summary>
	public class AudioManager : MonoBehaviour
    {
		/// <summary>
		/// Pseudo singleton reference for active instance of the manager.
		/// </summary>
		public static AudioManager Instance { get; private set; }

		/// <summary>
		/// Dictionary of object pools.
		/// </summary>
		private Dictionary<string, ObjectPool<AudioOrigin>> audioPools = new Dictionary<string, ObjectPool<AudioOrigin>>();

		/// <summary>
		/// List of all audio origin instances.
		/// </summary>
		private List<AudioOrigin> audioOrigins = new List<AudioOrigin>();

		/// <summary>
		/// List of all active playing audio instances.
		/// </summary>
		private List<AudioOrigin> activeAudio = new List<AudioOrigin>();

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
		/// Create an audio origin instance from template.
		/// </summary>
		/// <param name="template"></param>
		/// <returns></returns>
		private AudioOrigin CreateAudioOrigin(AudioOrigin template)
		{
			AudioOrigin newAudio = Instantiate(template);
			audioOrigins.Add(newAudio);
			return newAudio;
		}

		/// <summary>
		/// Invoked when audio origin done playing.
		/// </summary>
		/// <param name="audioOrigin"></param>
		/// <param name="key"></param>
		private void OnAudioDone(AudioOrigin audioOrigin, string key)
		{
			activeAudio.Remove(audioOrigin);
			audioOrigin.OnDone -= OnAudioDone;
			if (audioPools.ContainsKey(key))
			{
				audioPools[key].Return(audioOrigin);
				audioOrigin.gameObject.SetActive(false);
				return;
			}

			Destroy(audioOrigin.gameObject);
		}

		/// <summary>
		/// Play audio. Fetch from object pool if possible.
		/// </summary>
		/// <param name="key">Object pool key</param>
		/// <param name="template">Prefab of desired audio origin to play</param>
		public void PlayAudioOrigin(string key, AudioOrigin template)
		{
			if (audioPools.ContainsKey(key) == false)
			{
				ObjectPool<AudioOrigin> newPool = new ObjectPool<AudioOrigin>(() => CreateAudioOrigin(template));
				audioPools.Add(key, newPool);
			}

			AudioOrigin audioOrigin = audioPools[key].Get();
			audioOrigin.gameObject.SetActive(true);
			audioOrigin.OnDone += OnAudioDone;
			activeAudio.Add(audioOrigin);
			audioOrigin.Play(key);
		}

		/// <summary>
		/// Clear and delete all currently existing objects tracked by the audio manager.
		/// </summary>
		public void Cleanup()
		{
			foreach (AudioOrigin audioOrigin in audioOrigins)
			{
				audioOrigin.OnDone -= OnAudioDone;
				audioOrigin.Stop();
				Destroy(audioOrigin.gameObject);
			}
			audioOrigins.Clear();
			activeAudio.Clear();
			audioPools.Clear();
		}
	}
}
