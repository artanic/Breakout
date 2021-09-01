using Discode.Breakout.Bricks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Discode.Breakout.Levels
{
	/// <summary>
	/// Level definition for generating a retangular block shape of bricks.
	/// </summary>
	[CreateAssetMenu(fileName = "Block Level Definition", menuName = "Breakout/Levels/Block Level Definition")]
    public class BlockLevelDefinition : LevelDefinition
    {
		/// <summary>
		/// The center of the area the brick will be generated.
		/// </summary>
		[SerializeField, Tooltip("The center of the area the brick will be generated.")]
		private Vector2 blockCenter = new Vector2(0.5f, 0.5f);

		/// <summary>
		/// Number of brick generated per row.
		/// </summary>
		[SerializeField, Min(1), Tooltip("Number of bricks generated per row.")]
		private int numberOfBricksPerRow = 10;

		/// <summary>
		/// Number of rows of bricks.
		/// </summary>
		[SerializeField, Min(1), Tooltip("Number of rows of bricks.")]
		private int numberOfBrickLayers = 5;

		/// <summary>
		/// The height of a brick.
		/// </summary>
		[SerializeField, Min(0.1f), Tooltip("The height of a brick")]
		private float rowHeight = 1;

		/// <summary>
		/// The width of a brick.
		/// </summary>
		[SerializeField, Min(0.1f), Tooltip("The width of a brick")]
		private float brickWidth = 1;

		/// <summary>
		/// Spacing between each brick horiontally.
		/// </summary>
		[SerializeField, Min(0), Tooltip("Spacing between each brick horizontally.")]
		private float rowHorizontalSpacing = 0;

		/// <summary>
		/// Spacing between each brick vertically.
		/// </summary>
		[SerializeField, Min(0), Tooltip("Spacing between each brick vertically.")]
		private float rowVerticalSpacing = 0;

#if UNITY_EDITOR
		/// <summary>
		/// [Editor Only] Gets Center position of the brick layout.
		/// </summary>
		public Vector2 EditorBlockCenter
		{
			get { return blockCenter; }
			set { blockCenter = value; }
		}
#endif

		/// <summary>
		/// Generate the layout of bricks in a retangular bock shape.
		/// </summary>
		/// <param name="brickCreator">Function that spawn a brick instance.</param>
		public override void LayoutBricks(Func<Vector3, Brick> brickCreator)
		{
			Vector3 centerWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(blockCenter.x * Screen.width, blockCenter.y * Screen.height, 0));

#if UNITY_EDITOR
			GameObject bricksContainer = GameObject.Find("Bricks") ?? new GameObject("Bricks");
#endif
			float currentHeight = centerWorldPosition.y - (((numberOfBrickLayers - 1) * (rowHeight + rowVerticalSpacing)) * 0.5f);
			for (int i = 0; i < numberOfBrickLayers; i++)
			{
				float currentWidth = centerWorldPosition.x - (((numberOfBricksPerRow - 1) * (brickWidth + rowHorizontalSpacing)) * 0.5f);
				for (int j = 0; j < numberOfBricksPerRow; j++)
				{
					Brick newBlock = brickCreator(new Vector3(currentWidth, currentHeight, LevelDepth));
					newBlock.ColorId = i;			
#if UNITY_EDITOR
					newBlock.name = $"Brick {newBlock.netId}";
					newBlock.transform.SetParent(bricksContainer.transform);
#endif
					currentWidth += brickWidth + (j < numberOfBricksPerRow - 1 ? rowHorizontalSpacing : 0);
				}

				currentHeight += rowHeight + (i < numberOfBrickLayers - 1 ? rowVerticalSpacing : 0);
			}
		}
	}
}
