using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Discode.Breakout.Gameplay
{
	public class Ball : NetworkBehaviour
	{
		public delegate void BallEventHanlder(Ball ball);

		public event BallEventHanlder OnOutOfBounds = null;

		[SerializeField]
		private MeshFilter meshFilter = null;

		[SerializeField]
		private Rigidbody ballRigidbody = null;

		[SerializeField]
		private float contantSpeed = 5;

		[SerializeField]
		private Vector3 initialVelocity = new Vector3(0, 10 , 0);

		[SerializeField]
		private float minVelocity = 10f;

		private Vector3 lastFrameVelocity;

		public float Size => meshFilter.sharedMesh.bounds.size.x * transform.localScale.x;

		public Vector3 Veloity = Vector3.zero;

		public bool Moving { get; private set; }

		[Server]
		private void LateUpdate()
		{
			if (Moving)
			{
				ballRigidbody.velocity = contantSpeed * ballRigidbody.velocity.normalized;
				lastFrameVelocity = ballRigidbody.velocity;
			}
		}

		[ServerCallback]
		private void OnCollisionEnter(Collision collision)
		{
			if (Moving is false)
			{
				return;
			}

			if (collision.gameObject.CompareTag("Obstacle"))
			{
				var speed = lastFrameVelocity.magnitude;
				var direction = Vector3.Reflect(lastFrameVelocity.normalized, collision.contacts[0].normal);

				//Debug.Log("Out Direction: " + direction);
				ballRigidbody.velocity = direction * contantSpeed; //Mathf.Max(speed, minVelocity);
				ballRigidbody.AddForce(-collision.contacts[0].normal + new Vector3(Random.Range(-1, 1), Random.Range(-1, 1)));
			}
		}

		[ServerCallback]
		private void OnTriggerEnter(Collider other)
		{
			OnOutOfBounds?.Invoke(this);
		}

		[Command(requiresAuthority = false)]
		public void Launch(Vector3 direction)
		{
			Moving = true;
			ballRigidbody.velocity = Quaternion.AngleAxis(Random.Range(-45, 45), Vector3.forward) * (Vector3.up * contantSpeed);
			//StartCoroutine(MovingProcess());
		}

		//private IEnumerator MovingProcess()
		//{
		//	while (Moving)
		//	{
		//		ballRigidbody.velocity = contantSpeed * ballRigidbody.velocity.normalized;
		//		lastFrameVelocity = ballRigidbody.velocity;

		//		yield return null;
		//	}
		//}
	}
}
