using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Discode.Breakout.Gameplay
{
    public class Paddle : NetworkBehaviour
    {
        [SerializeField]
        private MeshFilter meshFilter = null;

        [SerializeField]
        private float speed = 1;

        private Ball ball = null;
        private Coroutine holdBallRoutine = null;

        public bool HasBall => ball != null;

        public void AssignBall(Ball ball)
		{
            this.ball = ball;
            holdBallRoutine = StartCoroutine(HoldBall());
        }

        public void ReleaseBall()
		{
            if (holdBallRoutine != null)
			{
                StopCoroutine(holdBallRoutine);
                holdBallRoutine = null;
			}
            ball.Launch(new Vector3(0, 0, Mathf.Cos(Mathf.Deg2Rad * 90)));
		}

        public void MoveLeft()
		{
            Vector3 farLeftSide = new Vector3(transform.position.x - (meshFilter.sharedMesh.bounds.extents.x * transform.localScale.x), transform.position.y, transform.position.z);

            Vector3 screenPosition = Camera.main.WorldToScreenPoint(farLeftSide);

            if (screenPosition.x < 1)
			{
                return;
			}

            transform.position = Vector3.Lerp(transform.position, transform.position + Vector3.left, speed * Time.deltaTime);
        }

        public void MoveRight()
        {
            Vector3 farRightSide = new Vector3(transform.position.x + (meshFilter.sharedMesh.bounds.extents.x * transform.localScale.x), transform.position.z);

            Vector3 screenPosition = Camera.main.WorldToScreenPoint(farRightSide);

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
                ball.transform.position = new Vector3(transform.position.x, transform.position.y + (meshFilter.sharedMesh.bounds.extents.y * transform.localScale.y) + (ball.Size * 0.5f), transform.position.z);
                yield return null;
            }

            holdBallRoutine = null;
        }
    }
}
