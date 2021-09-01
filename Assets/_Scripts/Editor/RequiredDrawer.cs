using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Discode.Breakout.Editor
{
	[CustomPropertyDrawer(typeof(RequiredAttribute))]
	public class RequiredDrawer : PropertyDrawer
	{
		public static readonly Color ErrorColor = Color.red;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return base.GetPropertyHeight(property, label);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			var isRequiredAndEmpty = (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue == null) ||
									 (property.propertyType == SerializedPropertyType.String && string.IsNullOrEmpty(property.stringValue));

			using (new DrawerColorBlock(ErrorColor, isRequiredAndEmpty))
			{
				if (isRequiredAndEmpty && string.IsNullOrEmpty(label.tooltip))
				{
					RequiredAttribute required = attribute as RequiredAttribute;

					label.tooltip = string.IsNullOrEmpty(required.Message) ? "This field is required" : required.Message;
				}

				EditorGUI.PropertyField(position, property, label);
			}

			EditorGUI.EndProperty();
		}
	}

	public class DrawerColorBlock : IDisposable
	{
		private readonly Color _before;
		private readonly bool _active;
		public DrawerColorBlock(Color color)
			: this(color, true)
		{

		}

		public DrawerColorBlock(Color color, bool active)
		{
			_before = GUI.color;
			_active = active;

			if (_active)
			{
				GUI.color = color;
			}
		}


		public void Dispose()
		{
			if (_active)
			{
				GUI.color = _before;
			}
		}
	}
}
