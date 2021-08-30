using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Discode.Breakout
{
    public class AudioOrigin : MonoBehaviour
    {
        [SerializeField]
        private AudioSource audioSource = null;

        [SerializeField]
        private float delay = 0.1f;

        [ContextMenu("Play")]
        public void Play()
		{
            audioSource.Play();
            StartCoroutine(WaitAndDestroy(audioSource.clip.length));
		}

        private IEnumerator WaitAndDestroy(float seconds)
		{
            yield return new WaitForSeconds(seconds + delay);

            Destroy(gameObject);
		}
    }
}
