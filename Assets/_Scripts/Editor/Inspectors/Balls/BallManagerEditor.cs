using Discode.Breakout.Balls;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Discode.Breakout.Editor
{
    [CustomEditor(typeof(BallManager))]
    public class BallManagerEditor : InspectorEditorBase
    {
        private BallManager ballManager = null;

        protected override void OnStart()
        {
            showPropertiesFoldout = false;
            showReferencesFoldout = false;

            ballManager = target as BallManager;
        }

        protected override void DrawWhilePlaying()
        {
            Scribe.DrawTitle("Diagnostics");

            Scribe.EditorGUILayoutBegingGroup();
            Scribe.EditorGUILayoutBeginIndent();

            EditorGUILayout.LabelField("Ball Count", ballManager.BallCount.ToString());

            Scribe.EditorGUILayoutEndIndent();
            Scribe.EditorGUILayoutEndGroup();
        }
    }
}
