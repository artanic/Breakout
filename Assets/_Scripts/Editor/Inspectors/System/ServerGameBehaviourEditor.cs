using Discode.Breakout.Server;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Discode.Breakout.Editor
{
    [CustomEditor(typeof(ServerGameBehaviour))]
    public sealed class ServerGameBehaviourEditor : InspectorEditorBase
    {
        private SerializedProperty levelDepth = null;

        private SerializedProperty levelBuilder = null;

        private SerializedProperty firstPlayerPaddlePosition = null;

        private SerializedProperty secondPlayerPaddlePosition = null;

        protected override void FindProperties()
        {
            levelDepth = GetProperty(nameof(levelDepth));
            levelBuilder = GetProperty(nameof(levelBuilder));
            firstPlayerPaddlePosition = GetProperty(nameof(firstPlayerPaddlePosition));
            secondPlayerPaddlePosition = GetProperty(nameof(secondPlayerPaddlePosition));
        }

		protected override void DrawProperties()
		{
            EditorGUILayout.PropertyField(firstPlayerPaddlePosition);
            EditorGUILayout.PropertyField(secondPlayerPaddlePosition);
        }

		protected override void DrawReferences()
		{
            EditorGUILayout.PropertyField(levelBuilder);
        }
	}
}
