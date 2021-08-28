using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Discode.Breakout.Gameplay
{
	public class LevelBuilder : NetworkBehaviour
	{
		[SerializeField]
		private Vector3 center = Vector3.zero;

		[SerializeField]
		private int numberOfBricksPerRow = 10;

		[SerializeField]
		private int numberOfBrickLayers = 5;

		[SerializeField]
		private float layerHeight = 1;

		[SerializeField]
		private float layerHorizontalSpacing = 0;

		[SerializeField]
		private float layerVerticalSpacing = 0;

		[SerializeField]
		private Brick brickTemplate = null;

		[SerializeField]
		private BoxCollider borderTemplate = null;

		[SerializeField]
		private BoxCollider dropZoneTemplate = null;

		[SerializeField]
		private LevelController levelController = null;

		[ServerCallback]
		public override void OnStartServer()
		{
			Rebuild();
		}

		[Server]
		private void BuildBorder()
		{
#if UNITY_EDITOR
			GameObject borderContainer = new GameObject("Border");
#endif
			// Create the left border
			Vector3 screenSideCenter = new Vector3(0, Screen.height * 0.5f, 0);
			Vector3 inWorldPosition = Camera.main.ScreenToWorldPoint(screenSideCenter);
			inWorldPosition.x -= borderTemplate.size.x * 0.5f;
			inWorldPosition.z = center.z;
			BoxCollider newBorder = Instantiate(borderTemplate, inWorldPosition, Quaternion.identity);
			NetworkServer.Spawn(newBorder.gameObject);
#if UNITY_EDITOR
			newBorder.gameObject.name = "Left Border";
			newBorder.transform.SetParent(borderContainer.transform);
#endif
			newBorder.size = new Vector3(newBorder.size.x, 100, newBorder.size.z);

			// Create the right border
			screenSideCenter.x = Screen.width;
			inWorldPosition = Camera.main.ScreenToWorldPoint(screenSideCenter);
			inWorldPosition.x += borderTemplate.size.x * 0.5f;
			inWorldPosition.z = center.z;
			newBorder = Instantiate(borderTemplate, inWorldPosition, Quaternion.identity);
			NetworkServer.Spawn(newBorder.gameObject);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			newBorder.gameObject.name = "Right Border";
			newBorder.transform.SetParent(borderContainer.transform);
#endif
			newBorder.size = new Vector3(newBorder.size.x, 100, newBorder.size.z);

			// Create the top border
			screenSideCenter = new Vector3(Screen.width * 0.5f, Screen.height, 0);
			inWorldPosition = Camera.main.ScreenToWorldPoint(screenSideCenter);
			inWorldPosition.y += borderTemplate.size.y * 0.5f;
			inWorldPosition.z = center.z;
			newBorder = Instantiate(borderTemplate, inWorldPosition, Quaternion.identity);
			NetworkServer.Spawn(newBorder.gameObject);
			newBorder.size = new Vector3(100, newBorder.size.y, newBorder.size.z);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			newBorder.gameObject.name = "Top Border";
			newBorder.transform.SetParent(borderContainer.transform);
#endif
		}

		[Server]
		private void BuildDropZone()
		{
			Vector3 screenSideCenter = new Vector3(Screen.width * 0.5f, 0, 0);
			Vector3 inWorldPosition = Camera.main.ScreenToWorldPoint(screenSideCenter);
			inWorldPosition.y -= borderTemplate.size.y * 0.5f;
			inWorldPosition.z = center.z;
			BoxCollider newDropZone = Instantiate(dropZoneTemplate, inWorldPosition, Quaternion.identity);
			NetworkServer.Spawn(newDropZone.gameObject);
			newDropZone.size = new Vector3(100, newDropZone.size.y, newDropZone.size.z);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			newDropZone.gameObject.name = "Drop Zone";
#endif
		}

		[Server]
		private void LayoutBricks(int bricksPerRow, int numberOfLayers)
		{
#if UNITY_EDITOR
			GameObject brickContainer = new GameObject("Bricks");
#endif
			float currentHeight = center.y - (((numberOfLayers - 1) * (layerHeight + layerVerticalSpacing)) * 0.5f);
			for (int i = 0; i < numberOfLayers; i++)
			{
				float currentWidth = center.x - (((bricksPerRow - 1) * (brickTemplate.Width + layerHorizontalSpacing)) * 0.5f);
				for (int j = 0; j < bricksPerRow; j++)
				{
					var newBlock = Instantiate(brickTemplate);
					NetworkServer.Spawn(newBlock.gameObject);
					levelController.RegisterBrick(newBlock);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
					newBlock.name = $"Y{i} X{j} Brick";
#endif
					newBlock.transform.position = new Vector3(
						currentWidth,
						currentHeight,
						center.z
					);
#if UNITY_EDITOR
					newBlock.transform.SetParent(brickContainer.transform);
#endif
					currentWidth += newBlock.Width + (j < bricksPerRow - 1 ? layerHorizontalSpacing : 0);
				}

				currentHeight += layerHeight + (i < numberOfLayers - 1 ? layerVerticalSpacing : 0);
			}
		}

		[Server]
		public void Rebuild()
		{
			var currentBricks = levelController.GetBricks();

			if (currentBricks.Length > 0)
			{
				foreach (var brick in currentBricks)
				{
					levelController.UnregisterBrick(brick);
					NetworkServer.Destroy(brick.gameObject);
				}
			}

			BuildBorder();
			BuildDropZone();
			LayoutBricks(numberOfBricksPerRow, numberOfBrickLayers);
		}
	}
}
