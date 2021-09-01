using Discode.Breakout.Paddles;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Discode.Breakout.Editor
{
    [CustomEditor(typeof(PaddleManager))]
    public class PaddleManagerEditor : InspectorEditorBase
    {
        private PaddleManager paddleManager = null;

        protected override void OnStart()
        {
            showPropertiesFoldout = false;
            showReferencesFoldout = false;

            paddleManager = target as PaddleManager;
        }

        protected override void DrawWhilePlaying()
        {
            Scribe.DrawTitle("Diagnostics");

            Scribe.EditorGUILayoutBegingGroup();
            Scribe.EditorGUILayoutBeginIndent();

            EditorGUILayout.LabelField("Number of paddles", paddleManager.EditorPaddleCount.ToString());

            Scribe.EditorGUILayoutEndIndent();
            Scribe.EditorGUILayoutEndGroup();
        }
    }
}
