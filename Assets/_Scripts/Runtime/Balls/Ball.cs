using Discode.Breakout.Audio;
using Discode.Breakout.Paddles;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Discode.Breakout.Balls
{
	/// <summary>
	/// Controls ball behaviour.
	/// </summary>
	public class Ball : NetworkBehaviour
	{
		public delegate void BallEventHanlder(Ball ball);

		/// <summary>
		/// Invoked when the ball is requesting to be destroyed by the server.
		/// </summary>
		public event BallEventHanlder OnBallDestroy = null;

		/// <summary>
		/// Invoked when the ball moves out of bounds.
		/// </summary>
		public event BallEventHanlder OnOutOfBounds = null;

		[SerializeField, Required]
		private SphereCollider sphereCollider = null;

		/// <summary>
		/// Access to the ball rigid body instead of .rigidbody.
		/// </summary>
		[SerializeField, Required]
		private Rigidbody ballRigidbody = null;

		/// <summary>
		/// Prefab of bounce sound.
		/// </summary>
		[SerializeField, Required, Tooltip("Prefab of bounce sound.")]
		private AudioOrigin bounceSoundTemplate = null;

		/// <summary>
		/// Prefab of launch ball sound.
		/// </summary>
		[SerializeField, Required, Tooltip("Prefab of launch ball sound.")]
		private AudioOrigin launchBallSoundTemplate = null;

		/// <summary>
		/// Minimum speed that the ball can move.
		/// </summary>
		[SerializeField, Tooltip("Minimum speed that the ball can move.")]
		private float minVelocity = 10f;

		/// <summary>
		/// The velocity of the ball last frame.
		/// </summary>
		private Vector3 lastFrameVelocity;

#if UNITY_EDITOR
		/// <summary>
		/// [Editor Only] Points of collisions.
		/// </summary>
		private List<Vector3> points = new List<Vector3>();

		/// <summary>
		/// [Editor Only] Normal of collisions.
		/// </summary>
		[SerializeField]
		private List<Vector3> normals = new List<Vector3>();
#endif

		/// <summary>
		/// The current mutiplier applied to the offset to prevent 'tunnelling'
		/// </summary>
		private int velocityOffsetMultiplier = 1;

		/// <summary>
		/// The second to last object the ball collided with.
		/// </summary>
		private GameObject secondToLastCollisionObject = null;

		/// <summary>
		/// The last object the ball collided with.
		/// </summary>
		private GameObject lastCollectionObject = null;

		/// <summary>
		/// Radius of the ball in world units.
		/// </summary>
		public float Size => sphereCollider.radius;

		public bool Moving { get; private set; }

		/// <summary>
		/// The last object the ball collided with.
		/// </summary>
		public GameObject LastCollectionObject
		{
			get { return lastCollectionObject; }
			set
			{
				secondToLastCollisionObject = lastCollectionObject;
				lastCollectionObject = value;
			}
		}

#if UNITY_EDITOR
		/// <summary>
		/// [Editor Only] Gets the second last collision object.
		/// </summary>
		public GameObject EditorSecondLastCollisionObject => secondToLastCollisionObject;

		/// <summary>
		/// [Editor Ony] Gets the velocity offset multiplier.
		/// </summary>
		public int EditorVelocityOffsetMultiplier => velocityOffsetMultiplier;

		/// <summary>
		/// [Editor Only] Gest the debug state.
		/// </summary>
		public bool EditorDebug { get; set; } = false;
#endif
		private void OnEnable()
		{
			BallManager.Instance.RegisterBall(this);
		}

		private void OnDisable()
		{
			BallManager.Instance.UnregisterBall(this);
		}


		private void Update()
		{
			if (Moving)
			{
				// Store last frame velocity value.
				lastFrameVelocity = ballRigidbody.velocity;
			}
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (Moving == false)
			{
				return;
			}

			//Store game object hit
			GameObject objectHit = collision.gameObject;

			// Trigger bounce sound on impact.
			if (isClient && (objectHit.CompareTag(UnityConstants.Tags.Obstacle) || objectHit.CompareTag(UnityConstants.Tags.Paddle)))
			{
				AudioManager.Instance.PlayAudioOrigin(GameConstants.AUDIO_KEY_BALL_BOUNCE, bounceSoundTemplate);
			}

#if UNITY_EDITOR
			// Tracks the position and normal direction for debug purposes.
			if (EditorDebug)
			{
				if (points.Count >= 10)
				{
					points.RemoveAt(0);
				}
				points.Add(collision.contacts[0].point);

				if (normals.Count >= 10)
				{
					normals.RemoveAt(0);
				}
				normals.Add(collision.contacts[0].normal);
			}
#endif

			// Ensure the ball maintians a minimum speed.
			float speed = Mathf.Max(lastFrameVelocity.magnitude, minVelocity);
			if (objectHit.CompareTag(UnityConstants.Tags.Brick))
			{
				// Reflect the velocity based of collision normal. Uses last frame velosity as collision already changed rigidbody velocity.
				ballRigidbody.velocity = Vector3.Reflect(lastFrameVelocity, collision.contacts[0].normal).normalized * speed;
			}
			else if (objectHit.CompareTag(UnityConstants.Tags.Obstacle))
			{
				ballRigidbody.velocity = Vector3.Reflect(lastFrameVelocity, collision.contacts[0].normal).normalized * speed;
			}
			else if (objectHit.CompareTag(UnityConstants.Tags.Paddle))
			{
				Paddle paddle = objectHit.GetComponent<Paddle>();

				//Check if the ball is lower than the paddle. If it is, just tread it as a bounce obstacle.
				if (transform.position.y < objectHit.transform.position.y + (paddle.Height * 0.5f))
				{
#if UNITY_EDITOR
					if (EditorDebug)
					{
						Debug.Log("Ball Under Paddle");
					}
#endif
					ballRigidbody.velocity = Vector3.Reflect(lastFrameVelocity, collision.contacts[0].normal).normalized * speed;
				}
				else
				{
					// Redirects the ball direction based on where it bounces against the paddle.
					//Calculate the new value of x for the ball to be directed.
					float paddleLength = paddle.RightEnd - paddle.LeftEnd;
					float ballLocation = transform.position.x - paddle.LeftEnd;
					// Determine the new direction increasing the angle the further out the collision.
					float xDirection = ((ballLocation / paddleLength) - 0.5f) * 5;
					//Set new ball direction.
					ballRigidbody.velocity = new Vector3(xDirection, 1f, 0f).normalized * speed;
				}
			}

			//Check if the ball is moving mostly horizontally. If the ball is 
			//too horizontal, increase or decrease the Y direction accordingly to prevent 'tunneling'
			if (objectHit == secondToLastCollisionObject)
			{
				if (ballRigidbody.velocity.y < (minVelocity * 0.5f) && ballRigidbody.velocity.y > 0f)
				{
					ballRigidbody.AddForce(new Vector2(0, 0.3f * velocityOffsetMultiplier));
				}
				if (ballRigidbody.velocity.y > -(minVelocity * 0.5f) && ballRigidbody.velocity.y <= 0f)
				{
					ballRigidbody.AddForce(new Vector2(0, -0.3f * velocityOffsetMultiplier));
				}

				velocityOffsetMultiplier++;
			}
			else
			{	
				// Reset the offset mulitplier.
				velocityOffsetMultiplier = 1;
			}

			// Set the last collided object.
			LastCollectionObject = objectHit;
			lastFrameVelocity = ballRigidbody.velocity;
		}

		private void OnTriggerEnter(Collider other)
		{
			if (other.CompareTag(UnityConstants.Tags.Dropzone))
			{
				OnOutOfBounds?.Invoke(this);
			}
		}

		/// <summary>
		/// Launch the ball at a random direction upwards. Limited to 90 degress.
		/// </summary>
		[Server]
		public Vector3 Launch()
		{
			if (Moving)
			{
				return ballRigidbody.velocity;
			}

			Moving = true;

			// Launch the ball at a random direction upwards. Limited to 90 degrees.
			ballRigidbody.velocity = Quaternion.AngleAxis(Random.Range(-45, 45), Vector3.forward) * Vector3.up * minVelocity;
			return lastFrameVelocity = ballRigidbody.velocity;
		}

		/// <summary>
		/// Launches the ball at a set velocity.
		/// </summary>
		/// <param name="velocity">Velocity of the ball on the server.</param>
		[Client]
		public void Launch(Vector3 velocity)
		{
			Moving = true;

			AudioManager.Instance.PlayAudioOrigin(GameConstants.AUDIO_KEY_BALL_LAUNCHED, launchBallSoundTemplate);

			ballRigidbody.velocity = velocity;
			lastFrameVelocity = ballRigidbody.velocity;
		}

		/// <summary>
		/// Destroy the ball after a delayed time.
		/// </summary>
		/// <param name="ball"></param>
		/// <param name="seconds"></param>
		[Server]
		public void DestroyAfterTime(Ball ball, float seconds = 1)
		{
			StartCoroutine(DestroyBallAfterTime(ball, seconds));
		}

		/// <summary>
		/// Wait process to then trigger request to be destroyed.
		/// </summary>
		/// <param name="ball"></param>
		/// <param name="seconds"></param>
		/// <returns></returns>
		[Server]
		private IEnumerator DestroyBallAfterTime(Ball ball, float seconds)
		{
			yield return new WaitForSeconds(seconds);

			OnBallDestroy?.Invoke(this);
		}

#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			if (EditorDebug)
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
						Gizmos.DrawLine(i == points.Count - 1 ? transform.position : points[i + 1], points[i]);
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
		}
#endif
	}
}
