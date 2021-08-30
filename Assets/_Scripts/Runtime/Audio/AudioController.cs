using Discode.Breakout.Networking;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Discode.Breakout.Audio
{
    public class AudioController : MonoBehaviour
    {
        [SerializeField]
        private AudioOrigin launchSoundTemplate = null;

        [SerializeField]
        private AudioOrigin bounceSoundTemplate = null;

        [SerializeField]
        private AudioOrigin breakBrickSoundTemplate = null;

        [SerializeField]
        private AudioOrigin dropzoneSoundTemplate = null;

        [SerializeField]
        private AudioSource musicAudioSource = null;

		private void OnEnable()
		{
            MyNetworkManager.OnClientStart += OnClientStart;
            MyNetworkManager.OnClientStop += OnClientStop;
        }

		private void OnDisable()
		{
            MyNetworkManager.OnClientStart -= OnClientStart;
            MyNetworkManager.OnClientStop -= OnClientStop;
        }

        private void OnClientStart()
		{
            musicAudioSource.Play();
            NetworkClient.RegisterHandler<AudioEventMessage>(OnAudioEvent, false);
        }

        private void OnClientStop()
		{
            musicAudioSource.Stop();
            NetworkClient.UnregisterHandler<AudioEventMessage>();
        }

        private void OnAudioEvent(AudioEventMessage message)
		{
            AudioOrigin audioOriginToPlay;

            switch (message.AudioEvent)
			{
                case AudioEventType.Launch:
                    audioOriginToPlay = launchSoundTemplate;
                    break;

                default:
                case AudioEventType.Bounce:
                    audioOriginToPlay = bounceSoundTemplate;
                    break;

                case AudioEventType.Break:
                    audioOriginToPlay = breakBrickSoundTemplate;
                    break;

                case AudioEventType.Dropout:
                    audioOriginToPlay = dropzoneSoundTemplate;
                    break;
			}

            AudioOrigin audioOrigin = Instantiate(audioOriginToPlay);
            audioOrigin.Play();
        }
    }
}
