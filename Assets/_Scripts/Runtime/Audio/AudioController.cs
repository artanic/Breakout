using Discode.Breakout.Effects;
using Discode.Breakout.Networking;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Discode.Breakout.Audio
{
    /// <summary>
    /// Controls local sound behaviour.
    /// </summary>
    public class AudioController : MonoBehaviour
    {
        /// <summary>
        /// Template for cloning
        /// </summary>
        [SerializeField, Required]
        private AudioOrigin dropoutSoundTemplate = null;

        /// <summary>
        /// Audio source for background muusic.
        /// </summary>
        [SerializeField, Required]
        private AudioSource musicAudioSource = null;
        
        /// <summary>
        /// Play the background music when the client join a game.
        /// </summary>
        [SerializeField]
        private bool playMusicOnJoin = false;

        public bool IsMusicPlaying => musicAudioSource.isPlaying;

		private void OnEnable()
		{
            GameNetworkManager.OnClientStart += OnClientStart;
            GameNetworkManager.OnClientStop += OnClientStop;
        }

		private void OnDisable()
		{
            GameNetworkManager.OnClientStart -= OnClientStart;
            GameNetworkManager.OnClientStop -= OnClientStop;
        }

        private void OnClientStart()
		{
            if (playMusicOnJoin)
            {
                musicAudioSource.Play();
            }
        }

        private void OnClientStop()
		{
            musicAudioSource.Stop();
            AudioManager.Instance.Cleanup();
            SpecialEffectManager.Instance.Cleanup();
        }

        /// <summary>
        /// Play the dropout sound effect.
        /// </summary>
        public void PlayDropoutSound()
		{
            AudioManager.Instance.PlayAudioOrigin(GameConstants.AUDIO_KEY_OUT_OF_BOUNDS, dropoutSoundTemplate);
		}

        /// <summary>
        /// Toggle the mute state for music.
        /// </summary>
        public void ToggleMusic()
		{
            if (musicAudioSource.isPlaying)
			{
                musicAudioSource.Stop();
                return;
			}

            musicAudioSource.Play();
		}
	}
}
