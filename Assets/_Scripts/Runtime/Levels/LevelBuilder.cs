using Discode.Breakout.Balls;
using Discode.Breakout.Bricks;
using Discode.Breakout.Paddles;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Discode.Breakout.Levels
{
	/// <summary>
	/// Handles the generation of level contents.
	/// </summary>
	public class LevelBuilder : NetworkBehaviour
	{
		/// <summary>
		/// Border Prefab for cloning.
		/// </summary>
		[SerializeField, Required, Tooltip("Border Prefab for cloning.")]
		private BoxCollider borderTemplate = null;

		/// <summary>
		/// Dropzone Prefab for cloning.
		/// </summary>
		[SerializeField, Required, Tooltip("Dropzone Prefab for cloning.")]
		private BoxCollider dropZoneTemplate = null;

		/// <summary>
		/// Paddle Prefab for cloning.
		/// </summary>
		[SerializeField, Required, Tooltip("Paddle Prefab for cloning.")]
		private Paddle paddleTemplate = null;

		/// <summary>
		/// Ball Prefab for cloning.
		/// </summary>
		[SerializeField, Required, Tooltip("Ball Prefab for cloning.")]
		private Ball ballTemplate = null;

		/// <summary>
		/// Brick template for cloning.
		/// </summary>
		[SerializeField, Required, Tooltip("Brick Prefab for cloning.")]
		private Brick brickTemplate = null;

		/// <summary>
		/// List of possible level definitions.
		/// </summary>
		[SerializeField, Tooltip("List of possible level definitions.")]
		private LevelDefinition[] levelDefinitions = new LevelDefinition[0];

		/// <summary>
		/// Default level definition.
		/// </summary>
		[SerializeField, Required, Tooltip("Default level definition.")]
		private LevelDefinition defaultLevelDefinition = null;

#if UNITY_EDITOR
		/// <summary>
		/// [Editor Only] Level element container.
		/// </summary>
		private GameObject container = null;

		/// <summary>
		/// [Editor Only] Birck Container.
		/// </summary>
		private GameObject bricksContainer = null;
#endif

		/// <summary>
		/// All elements that have been cloned.
		/// </summary>
		private List<GameObject> elements = new List<GameObject>();

		/// <summary>
		/// Current active level definition.
		/// </summary>
		private LevelDefinition activeDefinition = null;

		/// <summary>
		/// List of possible level definitions.
		/// </summary>
		public LevelDefinition[] LevelDefinitions => levelDefinitions;

		/// <summary>
		/// Current active level definition.
		/// </summary>
		public LevelDefinition ActiveDefinition
		{
			get
			{
				// Set the active definition if null.
				if (activeDefinition == null)
				{
					activeDefinition = defaultLevelDefinition;
				}

				return activeDefinition;
			}
		}

		/// <summary>
		/// Create the collision objects for border and dropzone.
		/// </summary>
		public void CreateBorderAndDropzone()
		{
#if UNITY_EDITOR
			GameObject borderContainer = new GameObject("Border");
			container = borderContainer;
#endif
			// Get the top left of the screen in world units.
			Vector3 screenTopLeft = new Vector3(0, Screen.height, 0);
			Vector3 topLeftPosition = Camera.main.ScreenToWorldPoint(screenTopLeft);

			// Get the bottom right of the sreen in in world units.
			Vector3 screenBottomRight = new Vector3(Screen.width, 0, 0);
			Vector3 bottomRightPosition = Camera.main.ScreenToWorldPoint(screenBottomRight);

			// Wdith and height of the screen in world unitys.
			float screenWidth = bottomRightPosition.x - topLeftPosition.x;
			float screenHeight = topLeftPosition.y - bottomRightPosition.y;

			// Create the left border
			BoxCollider newBorder = Instantiate(borderTemplate);
			newBorder.transform.position = new Vector3(topLeftPosition.x - (borderTemplate.size.x * 0.5f), bottomRightPosition.y + (screenHeight * 0.5f), ActiveDefinition.LevelDepth);
#if UNITY_EDITOR
			newBorder.gameObject.name = "Left Border";
			newBorder.transform.SetParent(borderContainer.transform);
#endif
			newBorder.size = new Vector3(newBorder.size.x, screenHeight, newBorder.size.z);
			elements.Add(newBorder.gameObject);

			// Create the right border
			newBorder = Instantiate(
				borderTemplate,
				new Vector3(bottomRightPosition.x + (borderTemplate.size.x * 0.5f), bottomRightPosition.y + (screenHeight * 0.5f), ActiveDefinition.LevelDepth),
				Quaternion.identity
			);
#if UNITY_EDITOR
			newBorder.gameObject.name = "Right Border";
			newBorder.transform.SetParent(borderContainer.transform);
#endif
			newBorder.size = new Vector3(newBorder.size.x, screenHeight, newBorder.size.z);
			elements.Add(newBorder.gameObject);

			// Create the top border
			newBorder = Instantiate(
				borderTemplate,
				new Vector3(topLeftPosition.x + (screenWidth * 0.5f), topLeftPosition.y + (borderTemplate.size.y * 0.5f), ActiveDefinition.LevelDepth),
				Quaternion.identity
			);

			newBorder.size = new Vector3(screenWidth, newBorder.size.y, newBorder.size.z);
			elements.Add(newBorder.gameObject);

#if UNITY_EDITOR
			newBorder.gameObject.name = "Top Border";
			newBorder.transform.SetParent(borderContainer.transform);
#endif

			// Create Dropzone
			BoxCollider newDropZone = Instantiate(
				dropZoneTemplate,
				new Vector3(topLeftPosition.x + (screenWidth * 0.5f), bottomRightPosition.y - (borderTemplate.size.y * 0.5f), ActiveDefinition.LevelDepth),
				Quaternion.identity
			);

			newDropZone.size = new Vector3(screenWidth, newDropZone.size.y, newDropZone.size.z);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			newDropZone.gameObject.name = "Drop Zone";
#endif
			elements.Add(newDropZone.gameObject);
		}

		/// <summary>
		/// Clear all levels elements except for bricks.
		/// </summary>
		public void Clear()
		{
			foreach(GameObject element in elements)
			{
				Destroy(element);
			}
			elements.Clear();
#if UNITY_EDITOR
			Destroy(container);
			Destroy(bricksContainer);
#endif
		}

		/// <summary>
		///  Build the brick layout.
		/// </summary>
		public void Build()
		{
			ActiveDefinition.LayoutBricks(CreateBrick);
		}

		/// <summary>
		/// Create a new paddle.
		/// </summary>
		/// <param name="position">Position the paddle is initially spawned.</param>
		/// <returns>New Paddle Instance.</returns>
		[Server]
		public Paddle CreateNewPaddle(Vector3 position)
		{
			Paddle newPaddle = Instantiate(paddleTemplate, position, Quaternion.identity);
			NetworkServer.Spawn(newPaddle.gameObject);
			return newPaddle;
		}

		[Server]
		/// <summary>
		/// Remove a specific paddle from the game.
		/// </summary>
		/// <param name="paddle">Paddle to be removed</param>
		public void RemovePaddle(Paddle paddle)
		{
			NetworkServer.Destroy(paddle.gameObject);
		}

		[Server]
		/// <summary>
		/// Create a new ball.
		/// </summary>
		/// <returns>Ball instance</returns>
		public Ball GenerateNewBall()
		{
			Ball newBall = Instantiate(ballTemplate);
#if UNITY_EDITOR
			newBall.gameObject.name = "Ball";
#endif
			NetworkServer.Spawn(newBall.gameObject);
			return newBall;
		}

		[Server]
		/// <summary>
		/// Remove a specific ball from the game.
		/// </summary>
		/// <param name="ball">Ball to be removed</param>
		public void RemoveBall(Ball ball)
		{
			NetworkServer.Destroy(ball.gameObject);
		}

		/// <summary>
		/// Create a new brick.
		/// </summary>
		/// <param name="colorID">layer the brick is on</param>
		/// <param name="position">position to spawn</param>
		/// <returns>Brick instance</returns>
		[Server]
		public Brick CreateBrick(Vector3 position)
		{
			Brick newBlock = Instantiate(brickTemplate);
			newBlock.transform.position = position;
			NetworkServer.Spawn(newBlock.gameObject);
			return newBlock;
		}

		/// <summary>
		/// Remove a specific brick from the game.
		/// </summary>
		/// <param name="brick">Brick to be removed</param>
		[Server]
		public void RemoveBrick(Brick brick)
		{
			NetworkServer.Destroy(brick.gameObject);
		}
	}
}
