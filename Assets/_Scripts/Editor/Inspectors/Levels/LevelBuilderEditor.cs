using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Discode.Breakout.Levels;

namespace Discode.Breakout.Editor
{
    [CustomEditor(typeof(LevelBuilder))]
    public class LevelBuilderEditor : InspectorEditorBase
    {
		protected bool showlevelDefinitions = false;

		private SerializedProperty borderTemplate = null;
        private SerializedProperty dropZoneTemplate = null;
		private SerializedProperty paddleTemplate = null;
		private SerializedProperty ballTemplate = null;
		private SerializedProperty brickTemplate = null;
		private SerializedProperty levelDefinitions = null;
        private SerializedProperty defaultLevelDefinition = null;

		protected override void OnStart()
		{
			showPropertiesFoldout = false;

			base.OnStart();
		}

		protected override void FindProperties()
		{
            borderTemplate = GetProperty(nameof(borderTemplate));
            dropZoneTemplate = GetProperty(nameof(dropZoneTemplate));
			paddleTemplate = GetProperty(nameof(paddleTemplate));
			ballTemplate = GetProperty(nameof(ballTemplate));
			brickTemplate = GetProperty(nameof(brickTemplate));
			levelDefinitions = GetProperty(nameof(levelDefinitions));
            defaultLevelDefinition = GetProperty(nameof(defaultLevelDefinition));
        }

		protected override void DrawContents()
		{
			showlevelDefinitions = Scribe.EditorGUILayoutFoldout("Level Definitions", "Level Definitions", showlevelDefinitions);

			if (showlevelDefinitions)
			{
				Scribe.EditorGUILayoutBegingGroup();
				Scribe.EditorGUILayoutBeginIndent();

				EditorGUILayout.PropertyField(levelDefinitions);
				EditorGUILayout.PropertyField(defaultLevelDefinition);

				Scribe.EditorGUILayoutEndIndent();
				Scribe.EditorGUILayoutEndGroup();
			}

			base.DrawContents();
		}

		protected override void DrawReferences()
		{
            EditorGUILayout.PropertyField(borderTemplate);
            EditorGUILayout.PropertyField(dropZoneTemplate);
			EditorGUILayout.PropertyField(paddleTemplate);
			EditorGUILayout.PropertyField(ballTemplate);
			EditorGUILayout.PropertyField(brickTemplate);
		}
	}
}
