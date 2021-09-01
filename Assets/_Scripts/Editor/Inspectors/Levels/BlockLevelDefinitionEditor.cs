using Discode.Breakout.Bricks;
using Discode.Breakout.Levels;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Discode.Breakout.Editor
{
    [CustomEditor(typeof(BlockLevelDefinition))]
    public class BlockLevelDefinitionEditor : InspectorEditorBase
    {
        private SerializedProperty blockCenter = null;
        private SerializedProperty numberOfBricksPerRow = null;
        private SerializedProperty numberOfBrickLayers = null;
        private SerializedProperty rowHeight = null;
        private SerializedProperty brickWidth = null;
        private SerializedProperty rowHorizontalSpacing = null;
        private SerializedProperty rowVerticalSpacing = null;

        private BlockLevelDefinition blockLevelDefinition = null;

        bool valueChanged = false;
        float centerX = 0;
        float centerY = 0;

        protected override void OnStart()
        {
            showReferencesFoldout = false;
            showProperties = true;

            blockLevelDefinition = target as BlockLevelDefinition;
        }

        protected override void FindProperties()
        {
            blockCenter = GetProperty(nameof(blockCenter));
            numberOfBricksPerRow = GetProperty(nameof(numberOfBricksPerRow));
            numberOfBrickLayers = GetProperty(nameof(numberOfBrickLayers));
            rowHeight = GetProperty(nameof(rowHeight));
            brickWidth = GetProperty(nameof(brickWidth));
            rowHorizontalSpacing = GetProperty(nameof(rowHorizontalSpacing));
            rowVerticalSpacing = GetProperty(nameof(rowVerticalSpacing));
        }

		protected override void DrawProperties()
		{
            EditorGUILayout.PropertyField(blockCenter);
            EditorGUILayout.PropertyField(numberOfBricksPerRow);
            EditorGUILayout.PropertyField(numberOfBrickLayers);
            EditorGUILayout.PropertyField(rowHeight);
            EditorGUILayout.PropertyField(brickWidth);
            EditorGUILayout.PropertyField(rowHorizontalSpacing);
            EditorGUILayout.PropertyField(rowVerticalSpacing);

            valueChanged = false;
            centerX = blockLevelDefinition.EditorBlockCenter.x;
            centerY = blockLevelDefinition.EditorBlockCenter.y;
            if (centerX > 1 || centerX < 1)
			{
                centerX = Mathf.Clamp(centerX, 0, 1);
                valueChanged = true;

            }

            if (centerY > 1 || centerY < 1)
			{
                centerY = Mathf.Clamp(centerY, 0, 1);
                valueChanged = true;
            }

            if (valueChanged)
			{
                blockLevelDefinition.EditorBlockCenter = new Vector2(centerX, centerY);
            }
        }
	}
}
