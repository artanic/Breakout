using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Discode.Breakout.Gameplay
{
	public class LevelBuilder : MonoBehaviour
	{
		[SerializeField]
		private int levelDepth = 10;

		[SerializeField]
		private BoxCollider borderTemplate = null;

		[SerializeField]
		private BoxCollider dropZoneTemplate = null;

		private void BuildBorder()
		{
#if UNITY_EDITOR
			GameObject borderContainer = new GameObject("Border");
#endif
			// Create the left border
			Vector3 screenSideCenter = new Vector3(0, Screen.height * 0.5f, 0);
			Vector3 inWorldPosition = Camera.main.ScreenToWorldPoint(screenSideCenter);
			inWorldPosition.x -= borderTemplate.size.x * 0.5f;
			inWorldPosition.z = levelDepth;
			BoxCollider newBorder = Instantiate(borderTemplate, inWorldPosition, Quaternion.identity);
#if UNITY_EDITOR
			newBorder.gameObject.name = "Left Border";
			newBorder.transform.SetParent(borderContainer.transform);
#endif
			newBorder.size = new Vector3(newBorder.size.x, 100, newBorder.size.z);

			// Create the right border
			screenSideCenter.x = Screen.width;
			inWorldPosition = Camera.main.ScreenToWorldPoint(screenSideCenter);
			inWorldPosition.x += borderTemplate.size.x * 0.5f;
			inWorldPosition.z = levelDepth;

			newBorder = Instantiate(borderTemplate, inWorldPosition, Quaternion.identity);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			newBorder.gameObject.name = "Right Border";
			newBorder.transform.SetParent(borderContainer.transform);
#endif
			newBorder.size = new Vector3(newBorder.size.x, 100, newBorder.size.z);

			// Create the top border
			screenSideCenter = new Vector3(Screen.width * 0.5f, Screen.height, 0);
			inWorldPosition = Camera.main.ScreenToWorldPoint(screenSideCenter);
			inWorldPosition.y += borderTemplate.size.y * 0.5f;
			inWorldPosition.z = levelDepth;

			newBorder = Instantiate(borderTemplate, inWorldPosition, Quaternion.identity);

			newBorder.size = new Vector3(100, newBorder.size.y, newBorder.size.z);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			newBorder.gameObject.name = "Top Border";
			newBorder.transform.SetParent(borderContainer.transform);
#endif
		}

		private void BuildDropZone()
		{
			Vector3 screenSideCenter = new Vector3(Screen.width * 0.5f, 0, 0);
			Vector3 inWorldPosition = Camera.main.ScreenToWorldPoint(screenSideCenter);
			inWorldPosition.y -= borderTemplate.size.y * 0.5f;
			inWorldPosition.z = levelDepth;

			BoxCollider newDropZone = Instantiate(dropZoneTemplate, inWorldPosition, Quaternion.identity);

			newDropZone.size = new Vector3(100, newDropZone.size.y, newDropZone.size.z);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			newDropZone.gameObject.name = "Drop Zone";
#endif
		}

		[Server]
		public void Build()
		{
			BuildBorder();
			BuildDropZone();
			BrickManager.Instance.LayoutBricks();
		}
	}
}
