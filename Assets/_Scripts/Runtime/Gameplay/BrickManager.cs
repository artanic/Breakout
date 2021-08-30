using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Discode.Breakout.Gameplay
{
    public class BrickManager : NetworkBehaviour
    {
		public static event Brick.BrickEventHandler OnBrickAdded = null;
		public static event Brick.BrickEventHandler OnBrickRemoved = null;

        public static BrickManager Instance { get; private set; }

		[SerializeField]
		private Vector2 blockCenter;

		[SerializeField]
		private int numberOfBricksPerRow = 10;

		[SerializeField]
		private int numberOfBrickLayers = 5;

		[SerializeField]
		private int levelDepth = 10;

		[SerializeField]
		private float rowHeight = 1;

		[SerializeField]
		private float rowHorizontalSpacing = 0;

		[SerializeField]
		private float rowVerticalSpacing = 0;

		[SerializeField]
		private Brick brickTemplate = null;

		private List<Brick> bricks = new List<Brick>();

		private void Awake()
		{
			if (Instance != null && Instance != this)
			{
				Destroy(gameObject);
				return;
			}

			Instance = this;
		}



		public void RegisterBrick(Brick brick)
		{
			if (bricks.Contains(brick))
			{
				return;
			}
			bricks.Add(brick);
			OnBrickAdded?.Invoke(brick);
		}

		public void UnregisterBrick(Brick brick)
		{
			bricks.Remove(brick);
			OnBrickRemoved?.Invoke(brick);
		}

		public Brick[] Getbricks() => bricks.ToArray();

		[Server]
		public void RemoveBrick(Brick brick)
		{
			NetworkServer.Destroy(brick.gameObject);
		}

		[Server]
		public void ClearBricks()
		{
			for (int i = bricks.Count - 1; i >= 0; i--)
			{
				RemoveBrick(bricks[i]);
			}
			bricks.Clear();
		}

		public void LayoutBricks()
		{
#if UNITY_EDITOR
			GameObject brickContainer = new GameObject("Bricks");
#endif
			float currentHeight = blockCenter.y - (((numberOfBrickLayers - 1) * (rowHeight + rowVerticalSpacing)) * 0.5f);
			for (int i = 0; i < numberOfBrickLayers; i++)
			{
				float currentWidth = blockCenter.x - (((numberOfBricksPerRow - 1) * (brickTemplate.Width + rowHorizontalSpacing)) * 0.5f);
				for (int j = 0; j < numberOfBricksPerRow; j++)
				{
					Brick newBlock = Instantiate(brickTemplate);
					newBlock.Layer = i;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
					newBlock.name = $"Y{i} X{j} Brick";
#endif
					newBlock.transform.position = new Vector3(
						currentWidth,
						currentHeight,
						levelDepth
					);
					NetworkServer.Spawn(newBlock.gameObject);
#if UNITY_EDITOR
					newBlock.transform.SetParent(brickContainer.transform);
#endif
					currentWidth += newBlock.Width + (j < numberOfBricksPerRow - 1 ? rowHorizontalSpacing : 0);
				}

				currentHeight += rowHeight + (i < numberOfBrickLayers - 1 ? rowVerticalSpacing : 0);
			}
		}
	}
}
