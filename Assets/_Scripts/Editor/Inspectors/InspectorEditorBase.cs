using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Discode.Breakout.Editor
{
	public abstract class InspectorEditorBase : UnityEditor.Editor
	{
		protected bool showProperties = false;
		protected bool showReferences = false;

		protected bool showPropertiesFoldout = true;
		protected bool showReferencesFoldout = true;

		private string propertyNotFoundMessageFormat;
		private List<string> errors = new List<string>();

		protected virtual bool EditorHasErrors { get; set; } = false;

		protected void OnEnable()
		{
			propertyNotFoundMessageFormat = "Cannot find property '{0}'";
			EditorHasErrors = false;
			errors.Clear();
			FindProperties();
			OnStart();
		}

		protected virtual void OnStart() { }

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			if (EditorHasErrors)
			{
				Scribe.DrawTitle("Errors");

				Scribe.EditorGUILayoutBegingGroup();
				Scribe.EditorGUILayoutBeginIndent();

				DrawErrors();

				Scribe.EditorGUILayoutEndIndent();
				Scribe.EditorGUILayoutEndGroup();
			}
			else
			{
				DrawSubHeader();

				if (showPropertiesFoldout)
				{
					showProperties = Scribe.EditorGUILayoutFoldout("General Properties", "General Properties", showProperties);

					if (showProperties)
					{
						Scribe.EditorGUILayoutBegingGroup();
						Scribe.EditorGUILayoutBeginIndent();

						DrawProperties();

						Scribe.EditorGUILayoutEndIndent();
						Scribe.EditorGUILayoutEndGroup();
					}
				}

				DrawContents();

				if (showReferencesFoldout)
				{
					showReferences = Scribe.EditorGUILayoutFoldout("References", "References", showReferences);

					if (showReferences)
					{
						Scribe.EditorGUILayoutBegingGroup();
						Scribe.EditorGUILayoutBeginIndent();

						DrawReferences();

						Scribe.EditorGUILayoutEndIndent();
						Scribe.EditorGUILayoutEndGroup();
					}
				}

				if (EditorApplication.isPlaying)
				{
					DrawWhilePlaying();
				}

				DrawaFooter();
			}

			serializedObject.ApplyModifiedProperties();
		}

		protected virtual void FindProperties() { }

		protected virtual SerializedProperty GetProperty(string name)
		{
			SerializedProperty serializedProperty = serializedObject.FindProperty(name);
			if (serializedProperty == null)
			{
				AddError(string.Format(propertyNotFoundMessageFormat, name));
				Debug.LogError(string.Format($"{GetType().Name}: " + propertyNotFoundMessageFormat, name));
			}
			return serializedProperty;
		}

		protected virtual void AddError(string errorMessage)
		{
			errors.Add(errorMessage);
			EditorHasErrors = true;
		}

		protected virtual void DrawSubHeader() { }

		protected virtual void DrawContents() { }

		protected virtual void DrawProperties() { }

		protected virtual void DrawReferences() { }

		protected virtual void DrawWhilePlaying() { }

		protected virtual void DrawaFooter() { }

		protected virtual void DrawErrors()
		{
			foreach (string message in errors)
			{
				EditorGUILayout.LabelField(message);
			}
		}
	}
}
