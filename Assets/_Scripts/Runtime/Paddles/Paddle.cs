using Discode.Breakout.Balls;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Discode.Breakout.Paddles
{
    /// <summary>
    /// Controls paddle behaviour.
    /// </summary>
    public class Paddle : NetworkBehaviour
    {
        /// <summary>
        /// Collider for the paddle.
        /// </summary>
        [SerializeField, Required, Tooltip("Collider for the paddle.")]
        private BoxCollider boxCollider = null;

        /// <summary>
        /// The speed at which the paddle moves.
        /// </summary>
        [SerializeField, Tooltip("The speed at which the paddle moves.")]
        private float speed = 1;

        /// <summary>
        /// References to the ball held by the paddle.
        /// </summary>
        private Ball assignedBall = null;

        /// <summary>
        /// Reference to routine for stopping.
        /// </summary>
        private Coroutine holdBallRoutine = null;

        /// <summary>
        /// Does the paddle have a ball under its control.
        /// </summary>
        public bool HasBall => assignedBall != null;

        public Ball AssignedBall => assignedBall;

        /// <summary>
        /// Position of the left end of the paddle.
        /// </summary>
        public float LeftEnd => transform.position.x - (boxCollider.size.x * 0.5f);

        /// <summary>
        /// Position of the right end of the paddle.
        /// </summary>
        public float RightEnd => transform.position.x + (boxCollider.size.x * 0.5f);

        /// <summary>
        /// Heights of the paddle.
        /// </summary>
        public float Height => boxCollider.size.y;

        /// <summary>
        /// The desired direction (Horizontally) the paddle should be moved.
        /// </summary>
        public float DesiredDirection { get; set; }

        /// <summary>
        /// Position in world space of the right side of the level bounds.
        /// </summary>
        private float rightScreenPosition = 999;

        /// <summary>
        /// Position in world space of the left side of the level bounds.
        /// </summary>
        float leftScreenPosition = -999;

        private void OnEnable()
		{
            // Determine screen limitations.
            rightScreenPosition = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x;
            leftScreenPosition = Camera.main.ScreenToWorldPoint(Vector3.zero).x;

            PaddleManager.Instance.RegisterPaddle(this);
		}

		private void OnDisable()
		{
            PaddleManager.Instance.UnregisterPaddle(this);
		}

		private void Update()
		{
            // Move the paddle based of the desired direction passed from the player.
			if (isServer && (DesiredDirection > 0.1f || DesiredDirection < -0.1f))
			{
                if ( DesiredDirection > 0 )
				{
                    MoveRight();
				}
                else
				{
                    MoveLeft();
				}
			}
		}

		/// <summary>
		/// Assigns a ball to be held to the center of the paddle.
		/// Submit null to release a ball without launching it.
		/// </summary>
		/// <param name="ball">Ball to be held</param>
		[Server]
        public void AssignBall(Ball ball)
		{
            this.assignedBall = ball;
            if (this.assignedBall == null)
			{
                if (holdBallRoutine != null)
                {
                    StopCoroutine(holdBallRoutine);
                    holdBallRoutine = null;
                }
                assignedBall = null;
                return;
			}
            holdBallRoutine = StartCoroutine(HoldBall());
        }

        /// <summary>
        /// Stops holding ball assigned to paddle.
        /// </summary>
        /// <returns>Assigned Ball</returns>
        [Server]
        public Ball ReturnBall()
		{
            Ball ball = assignedBall;
            AssignBall(null);
            return ball;
        }

        [Server]
        /// <summary>
        /// Release held ball and launch it.
        /// </summary>
        public Vector3 ReleaseBall()
		{
            if (holdBallRoutine != null)
			{
                StopCoroutine(holdBallRoutine);
                holdBallRoutine = null;
			}
            if (assignedBall == null)
			{
                return Vector3.zero;
			}
            Vector3 velocity = assignedBall.Launch();
            assignedBall = null;
            return velocity;
        }

        /// <summary>
        /// Move the paddle left.
        /// </summary>
        [Server]
        public void MoveLeft()
		{
			// Limit movement so the paddle cannot move off screen.
			if (leftScreenPosition + 0.05f > LeftEnd)
			{
				return;
			}

			transform.position = Vector3.Lerp(transform.position, transform.position + Vector3.left, speed * Time.deltaTime);
        }

        /// <summary>
        /// Move the paddle right.
        /// </summary>
        [Server]
        public void MoveRight()
        {
			
			// Limit movement so paddle cannot move off screen.
			if (rightScreenPosition - 0.05f < RightEnd)
			{
				return;
			}

			transform.position = Vector3.Lerp(transform.position, transform.position + Vector3.right, speed * Time.deltaTime);
        }

        /// <summary>
        /// Routine for holding the ball at the center of the paddle.
        /// </summary>
        /// <returns></returns>
        private IEnumerator HoldBall()
		{
            while(HasBall)
			{
                // Ensure the bal position is at the center 'top' of the paddle.
                assignedBall.transform.position = new Vector3(transform.position.x, transform.position.y + assignedBall.Size + (boxCollider.size.y * 0.5f), transform.position.z);
                yield return null;
            }

            holdBallRoutine = null;
        }
    }
}
