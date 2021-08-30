using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Discode.Breakout.Gameplay
{
    public class Paddle : NetworkBehaviour
    {

        [SerializeField]
        private BoxCollider boxCollider = null;

        [SerializeField]
        private float speed = 1;

        private Ball ball = null;
        private Coroutine holdBallRoutine = null;

        public bool HasBall => ball != null;

        public Vector3 LeftEnd => new Vector3(transform.position.x - (boxCollider.size.x * 0.5f), transform.position.y, transform.position.z);

        public Vector3 RightEnd => new Vector3(transform.position.x + (boxCollider.size.x * 0.5f), transform.position.y, transform.position.z);

        public float Height => boxCollider.size.y;

        [Server]
        public void AssignBall(Ball ball)
		{
            this.ball = ball;
            holdBallRoutine = StartCoroutine(HoldBall());
        }

        [Server]
        public void ReleaseBall()
		{
            if (holdBallRoutine != null)
			{
                StopCoroutine(holdBallRoutine);
                holdBallRoutine = null;
			}
            ball.Launch();
		}

        [Server]
        public void MoveLeft()
		{
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(LeftEnd);

            if (screenPosition.x < 1)
			{
                return;
			}

            transform.position = Vector3.Lerp(transform.position, transform.position + Vector3.left, speed * Time.deltaTime);
        }

        [Server]
        public void MoveRight()
        {
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(RightEnd);

            if (screenPosition.x > Screen.width - 1)
            {
                return;
            }

            transform.position = Vector3.Lerp(transform.position, transform.position + Vector3.right, speed * Time.deltaTime);
        }

        private IEnumerator HoldBall()
		{
            while(HasBall)
			{
                ball.transform.position = new Vector3(transform.position.x, transform.position.y + ((boxCollider.size.y + ball.Size) * 0.5f), transform.position.z);
                yield return null;
            }

            holdBallRoutine = null;
        }
    }
}
