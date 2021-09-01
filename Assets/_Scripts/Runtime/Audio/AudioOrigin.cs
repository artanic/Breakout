using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Discode.Breakout
{
    /// <summary>
    /// Behaviour to handle the playing of audio clips.
    /// </summary>
    public class AudioOrigin : MonoBehaviour
    {
        public delegate void AudioOriginEventHandler(AudioOrigin audioOrigin, string key);

        /// <summary>
        /// Invoked when audio finished playing.
        /// </summary>
        public event AudioOriginEventHandler OnDone = null;

        [SerializeField, Required]
        private AudioSource audioSource = null;

        /// <summary>
        /// Delay completing the audio effect after it's completed playing.
        /// Stops audio being prematurely cut off.
        /// </summary>
        [SerializeField]
        private float delay = 0.1f;

        /// <summary>
        /// Process run while audio is playing. To cleanup after it's finished.
        /// </summary>
        private Coroutine playRoutine = null;

        /// <summary>
        /// Returns if the object is still alive.
        /// </summary>
        public bool IsActive => playRoutine != null;

        /// <summary>
        /// Returns if the audio source is currently playing
        /// </summary>
        public bool IsPlaying => audioSource.isPlaying;

        /// <summary>
        /// Play the audio source.
        /// </summary>
        /// <param name="key">Key for object pool</param>
        public void Play(string key)
		{
            audioSource.Play();
            playRoutine = StartCoroutine(WaitAndInvoke(audioSource.clip.length, key));
		}

        /// <summary>
        /// Stop the audio source from playing. (Does not execute complete logic)
        /// </summary>
        public void Stop()
		{
            if (audioSource.isPlaying)
			{
                audioSource.Stop();
			}
            if (playRoutine!= null)
			{
                StopCoroutine(playRoutine);
                playRoutine = null;
            }
		}

        /// <summary>
        /// Process while playing music. 
        /// </summary>
        /// <param name="seconds">Number of seconds</param>
        /// <param name="key">Key for object pool</param>
        /// <returns></returns>
        private IEnumerator WaitAndInvoke(float seconds, string key)
		{
            yield return new WaitForSeconds(seconds + delay);

            OnDone?.Invoke(this, key);
            playRoutine = null;
        }
    }
}
