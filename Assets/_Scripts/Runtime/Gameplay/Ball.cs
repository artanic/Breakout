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
		private SphereCollider sphereCollider = null;

		[SerializeField]
		private Rigidbody ballRigidbody = null;

		[SerializeField]
		private Vector3 initialVelocity = new Vector3(0, 10 , 0);

		[SerializeField]
		private float minVelocity = 10f;

		private Vector3 lastFrameVelocity;

		private List<Vector3> points = new List<Vector3>();

		[SerializeField]
		private List<Vector3> normals = new List<Vector3>();

		public float Size => meshFilter.sharedMesh.bounds.size.x * transform.localScale.x;

		public Vector3 Veloity = Vector3.zero;

		public Vector3 Velocity { get; set; }

		public bool Moving { get; private set; }

		private int velocityOffsetMultiplier = 0;

		private GameObject secondToLastCollisionObject = null;

		private GameObject lastCollectionObject = null;
		public GameObject LastCollectionObject
		{
			get { return lastCollectionObject; }
			set
			{
				secondToLastCollisionObject = lastCollectionObject;
				lastCollectionObject = value;
			}
		}

		private void Update()
		{
			if (Moving && isServer)
			{
				lastFrameVelocity = ballRigidbody.velocity;
			}
		}

		//void FixedUpdate()
		//{
		//	if (Moving && isServer)
		//	{
		//		ballRigidbody.velocity = ballRigidbody.velocity.normalized * Mathf.Max(ballRigidbody.velocity.magnitude, minVelocity);
		//	}
		//}

		private void OnCollisionEnter(Collision collision)
		{
			if (isServer == false || Moving is false)
			{
				return;
			}

#if UNITY_EDITOR
			if (points.Count >= 10)
			{
				points.RemoveAt(0);
			}
			points.Add(collision.contacts[0].point);

			Debug.Log("Collision Count = " + collision.contactCount);
#endif

			Vector3 normal = collision.contacts[0].normal;

#if UNITY_EDITOR
			if (points.Count >= 10)
			{
				normals.RemoveAt(0);
			}
			normals.Add(normal);
#endif

			//Store game object hit
			GameObject objectHit = collision.gameObject;

			float speed = Mathf.Max(lastFrameVelocity.magnitude, minVelocity);
			if (objectHit.CompareTag("Brick"))
			{
				NetworkServer.SendToAll(new AudioEventMessage { AudioEvent = AudioEventType.Break });

				ballRigidbody.velocity = Vector3.Reflect(lastFrameVelocity, normal).normalized * speed;
			}
			else if (objectHit.CompareTag("Obstacle"))
			{

				NetworkServer.SendToAll(new AudioEventMessage { AudioEvent = AudioEventType.Bounce });

				ballRigidbody.velocity = Vector3.Reflect(lastFrameVelocity, normal).normalized * speed;
			}
			else if (objectHit.CompareTag("Paddle"))
			{
				NetworkServer.SendToAll(new AudioEventMessage { AudioEvent = AudioEventType.Bounce });

				Paddle paddle = objectHit.GetComponent<Paddle>();

				//If ball is lower than the paddle, exit this method and
				//allow the ball to continue towards the screen bottom
				if (transform.position.y < objectHit.transform.position.y + (paddle.Height * 0.5f))
				{
#if UNITY_EDITOR
					Debug.Log("Ball Under Paddle");
#endif
					ballRigidbody.velocity = Vector3.Reflect(lastFrameVelocity, normal).normalized * speed;
				}
				else
				{
					//Get paddle end points
					float p1 = paddle.LeftEnd.x; // paddleLeftEnd.transform.position.x;
					float p2 = paddle.RightEnd.x; // paddleRightEnd.transform.position.x;
												  //Calculate the new value of x for the ball to be directed
					float PaddleLength = p2 - p1;
					float BallLocation = transform.position.x - p1;
					float xDirection = ((BallLocation / PaddleLength) - 0.5f) * 5;
					//Set new ball direction
					var direction = new Vector3(xDirection, 1f, 0f).normalized;
					ballRigidbody.velocity = direction * speed;
				}
			}

			//Check if the ball is moving mostly horizontally. If the ball is 
			//too horizontal, increase or decrease the Y direction accordingly
			if (objectHit == secondToLastCollisionObject)
			{
				velocityOffsetMultiplier++;

				if (ballRigidbody.velocity.y < (minVelocity * 0.5f) && ballRigidbody.velocity.y > 0f)
				{
					ballRigidbody.AddForce(new Vector2(0, 0.2f * velocityOffsetMultiplier));
				}
				if (ballRigidbody.velocity.y > -(minVelocity * 0.5f) && ballRigidbody.velocity.y <= 0f)
				{
					ballRigidbody.AddForce(new Vector2(0, -0.2f * velocityOffsetMultiplier));
				}
			}
			else
			{				
				velocityOffsetMultiplier = 0;
			}

			LastCollectionObject = objectHit;
			lastFrameVelocity = ballRigidbody.velocity;
		}

		private void OnTriggerEnter(Collider other)
		{
			if (other.CompareTag("Dropzone"))
			{
				if (isServer)
				{
					NetworkServer.SendToAll(new AudioEventMessage { AudioEvent = AudioEventType.Dropout });
				}

				OnOutOfBounds?.Invoke(this);
			}
		}

		[Server]
		public void Launch()
		{
			if (isServer == false || Moving)
			{
				return;
			}

			Moving = true;
			ballRigidbody.velocity = Quaternion.AngleAxis(Random.Range(-45, 45), Vector3.forward) * Vector3.up * minVelocity;
			lastFrameVelocity = ballRigidbody.velocity;
			NetworkServer.SendToAll(new AudioEventMessage { AudioEvent = AudioEventType.Launch, HorizontalPosition = transform.position.x, VerticalPosition = transform.position.y });
		}

#if UNITY_EDITOR
		void OnDrawGizmos()
		{
			if (points.Count > 1)
			{
				for (int i = points.Count - 1; i >= 0; i--)
				{
					if (i == 0)
					{
						continue;
					}
					Gizmos.color = Color.red;
					Gizmos.DrawLine(i == points.Count - 1 ? transform.position : points[ i + 1], points[i]);
				}
			}

			if (normals.Count > 0)
			{
				for (int i = normals.Count - 1; i >= 0; i--)
				{
					Gizmos.color = Color.green;
					Gizmos.DrawLine(points[i], points[i] + normals[i]);
				}
			}
		}
#endif
	}
}
