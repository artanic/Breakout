using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Discode.Breakout.Effects
{
    public class SpecialEffect : MonoBehaviour
    {
        public delegate void SpecialEffectEventHanlder(SpecialEffect specialEffect, string key);

        public event SpecialEffectEventHanlder OnDone = null;

        /// <summary>
        /// Particle system driving special effect.
        /// </summary>
        [SerializeField, Required]
        private ParticleSystem system = null;

        /// <summary>
        /// Delay completing the effect effect after it's completed playing.
        /// Stops effect being prematurely cut off.
        /// </summary>
        [SerializeField]
        private float delay = 0.1f;

        /// <summary>
        /// Process run while effect is playing. To cleanup after it's finished.
        /// </summary>
        private Coroutine playRoutine = null;

        /// <summary>
        /// Returns if the object is still alive.
        /// </summary>
        public bool IsActive => playRoutine != null;

        /// <summary>
        /// Returns if the effect is currently playing
        /// </summary>
        public bool IsPlaying => system.isPlaying;

        public void Play(string key, Vector3 position)
		{
            transform.position = position;
            system.Play(true);
            playRoutine = StartCoroutine(WaitAndInvoke(key));
        }

        public void Stop()
		{
            if (system.isPlaying)
            {
                system.Stop(true);
            }
            if (playRoutine != null)
            {
                StopCoroutine(playRoutine);
                playRoutine = null;
            }
        }

        /// <summary>
        /// Process while playing effect. 
        /// </summary>
        /// <param name="key">Key for object pool</param>
        /// <returns></returns>
        private IEnumerator WaitAndInvoke(string key)
        {
            while (system.isPlaying)
			{
                yield return null;
			}

            yield return new WaitForSeconds(delay);

            OnDone?.Invoke(this, key);
            playRoutine = null;
        }
    }
}
