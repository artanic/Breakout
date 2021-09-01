using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using System.Reflection;
using System;

namespace Discode.Breakout.Editor
{
	public static class Scribe
	{
		#region Constants
		// Constants for major settings
		public const int CONTROLS_DEFAULT_LABEL_WIDTH = 140;
		public const string FOLD_OUT_TOOL_TIP = "Click to Expand/Collapse";
		private static List<string> layers = new List<string>();
		#endregion

		#region Unknown Functions
		/// <summary>
		/// Gets the object the property represents.
		/// </summary>
		/// <param name="prop"></param>
		/// <returns></returns>
		public static object GetTargetObjectOfProperty(SerializedProperty prop)
		{
			var path = prop.propertyPath.Replace(".Array.data[", "[");
			object obj = prop.serializedObject.targetObject;
			var elements = path.Split('.');
			foreach (var element in elements)
			{
				if (element.Contains("["))
				{
					var elementName = element.Substring(0, element.IndexOf("["));
					var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
					obj = GetValue_Imp(obj, elementName, index);
				}
				else
				{
					obj = GetValue_Imp(obj, element);
				}
			}
			return obj;
		}

		private static object GetValue_Imp(object source, string name)
		{
			if (source == null)
				return null;
			var type = source.GetType();

			while (type != null)
			{
				var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
				if (f != null)
					return f.GetValue(source);

				var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
				if (p != null)
					return p.GetValue(source, null);

				type = type.BaseType;
			}
			return null;
		}


		private static object GetValue_Imp(object source, string name, int index)
		{
			var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
			if (enumerable == null) return null;
			var enm = enumerable.GetEnumerator();
			//while (index-- >= 0)
			//    enm.MoveNext();
			//return enm.Current;

			for (int i = 0; i <= index; i++)
			{
				if (!enm.MoveNext()) return null;
			}
			return enm.Current;
		}
		#endregion

		#region Value Functions
		/// <summary>
		/// An int field with a max field and boolean to clamp the value to match the max value.
		/// </summary>
		/// <param name="rect"></param>
		/// <param name="isLocked"></param>
		/// <param name="value"></param>
		/// <param name="max"></param>
		/// <param name="content"></param>
		/// <param name="style"></param>
		/// <returns></returns>
		public static int IntCappedField(ref bool isLocked, int value, ref int max, GUIContent content)
		{
			EditorGUI.BeginChangeCheck();
			float previousLabelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 40;
			EditorGUILayout.BeginVertical();
			EditorGUILayout.LabelField(content);
			EditorGUILayout.BeginHorizontal();
			isLocked = EditorGUILayout.Toggle("Lock:", isLocked);
			value = EditorGUILayout.IntField("Value:", value);
			max = EditorGUILayout.IntField("Max:", max);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
			EditorGUIUtility.labelWidth = previousLabelWidth;

			if (EditorGUI.EndChangeCheck())
			{
				if (value > max || isLocked)
				{
					value = max;
				}
			}

			return value;
		}

		public static int IntField(int value, int min, int max, GUIContent content = null)
		{
			EditorGUILayout.BeginHorizontal();
			if (content != null && string.IsNullOrEmpty(content.text) == false)
			{
				GUILayout.Label(content);
			}
			value = EditorGUILayout.IntField(value);
			if (GUILayout.Button("▴", EditorStyles.miniButton/*, GUILayout.MaxHeight(10)*/))
			{
				value++;
			}
			if (GUILayout.Button("▾", EditorStyles.miniButton))
			{
				value--;
			}
			EditorGUILayout.EndHorizontal();

			if (value < min)
			{
				value = min;
			}
			if (value > max)
			{
				value = max;
			}

			return value;
		}

		public static int IntField(Rect rect, int value, int min, int max, GUIContent content = null)
		{
			if (content != null && string.IsNullOrEmpty(content.text) == false)
			{
				EditorGUILayout.LabelField(content);
			}
			value = EditorGUI.IntField(new Rect(rect.x, rect.y, rect.width - (rect.height * 2), rect.height), value);
			if (GUI.Button(new Rect(rect.x + (rect.width - (rect.height * 2)), rect.y, rect.height, rect.height), "▴", EditorStyles.miniButton/*, GUILayout.MaxHeight(10)*/))
			{
				value++;
			}
			if (GUI.Button(new Rect(rect.x + (rect.width - (rect.height * 1)), rect.y, rect.height, rect.height), "▾", EditorStyles.miniButton))
			{
				value--;
			}

			if (value < min)
			{
				value = min;
			}
			if (value > max)
			{
				value = max;
			}

			return value;
		}

		public static float Slider(float value, float min, float max, GUIContent content)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(content);
			value = EditorGUILayout.Slider(value, min, max);
			float previousLabelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 0;
			value = Mathf.Clamp(EditorGUILayout.FloatField(value), min, max);
			if (GUILayout.Button(new GUIContent("Round", "Clamps value to nearest integer")))
			{
				value = Mathf.Round(value);
			}
			EditorGUIUtility.labelWidth = previousLabelWidth;
			EditorGUILayout.EndHorizontal();

			return value;
		}
		#endregion

		#region Mask Functions
		public static LayerMask LayerMaskField(GUIContent content, LayerMask selected)
		{
			if (layers == null)
			{
				layers = new List<string>();
			}
			else
			{
				layers.Clear();
			}

			int emptyLayers = 0;
			for (int i = 0; i < 32; i++)
			{
				string layerName = LayerMask.LayerToName(i);

				if (layerName != "")
				{
					for (;emptyLayers > 0; emptyLayers--)
					{
						layers.Add("Layer " + (i - emptyLayers));
					}

					layers.Add(layerName);
				}
				else
				{
					emptyLayers++;
				}
			}

			//for (int i = 0; i < layerNames.Length; i++)
			//{
			//	layerNames[i] = layers[i];
			//}

			selected.value = EditorGUILayout.MaskField(content, selected.value, layers.ToArray());

			return selected;
		}


		/// <summary>
		/// Draws a menu Control. Shows a list of items split into pages, with the ability to trigger adding an additional item.
		/// </summary>
		/// <param name="listSize"></param>
		/// <param name="scrollPosition"></param>
		/// <param name="itemsPerPage"></param>
		/// <param name="currentPage"></param>
		/// <param name="onAddItem"></param>
		/// <param name="onDrawItem"></param>
		/// <param name="searchString"></param>
		public static void ObjectList<T>(T[] objList, ref Vector2 scrollPosition, ref int itemsPerPage, ref int currentPage, Action onAddItem, Action<T> onDrawItem, string searchString = "")
		{
			int listSize = objList.Length;

			EditorGUILayout.BeginVertical();

			// Make it scrollable
			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, false, false, GUILayout.ExpandWidth(true));

			int oldItemsPerPage = itemsPerPage;
			int numberOfPages = Mathf.CeilToInt((float)listSize / itemsPerPage);
			if (numberOfPages == 0)
			{
				numberOfPages = 1;
			}

			// Make sure we don't show a page that isn't there(might happen when deleting items causes page count to decrease)
			if (currentPage > numberOfPages)
			{
				currentPage = numberOfPages;
			}

			int oldCurrentPage = currentPage;

			bool searching = false;
			// If list has something
			if (listSize > 0)
			{
				// If we are searching, then we change these values to get a dirty way of searching the whole category
				if (searchString.Trim() != string.Empty && searchString.Length >= 3)
				{
					searching = true;
					currentPage = 1;
					itemsPerPage = 1000000;
				}

				EditorGUILayout.Space();

				//Loop and show all buttons in page
				for (int elementIndex = (currentPage - 1) * itemsPerPage;
					elementIndex < Mathf.Clamp(currentPage * itemsPerPage, 0, listSize);
					elementIndex++)
				{
					// Draw Element
					onDrawItem(objList[elementIndex]);
				}
			}

			EditorGUILayout.Space();
			// End here so the page buttons always stay where they are
			EditorGUILayout.EndScrollView();

			itemsPerPage = EditorGUILayout.IntSlider("Items Per Page:", itemsPerPage, 10, 200);
			// Update number of pages since we might have added/removed an item
			numberOfPages = Mathf.CeilToInt((float)listSize / itemsPerPage);
			if (numberOfPages == 0)
			{
				numberOfPages = 1;
			}

			currentPage = EditorGUILayout.IntSlider("Current Page: ", currentPage, 1, numberOfPages);
			EditorGUILayout.BeginHorizontal();

			//Previous page button
			if (GUILayout.Button("Previous", EditorStyles.miniButtonLeft))
			{
				currentPage = Mathf.Clamp(--currentPage, 1, numberOfPages);
			}

			//Next page button
			if (GUILayout.Button("Next", EditorStyles.miniButtonRight))
			{
				currentPage = Mathf.Clamp(++currentPage, 1, numberOfPages);
			}

			EditorGUILayout.EndHorizontal();

			//Add Item Button
			Color previousGUIColor = GUI.color;
			GUI.color = Color.green;
			if (GUILayout.Button("Add New Item"))
			{
				onAddItem();
			}
			GUI.color = previousGUIColor;

			//Update item name enum buttons
			//GUI.color = Color.yellow;
			//if (GUILayout.Button("Update Names Enum For This Type"))
			//	UpdateItemNamesEnum(false);
			//if (GUILayout.Button("Update Names Enum For All Types"))
			//	UpdateItemNamesEnum(true);
			//GUI.color = Color.white;

			//Show page count
			EditorGUILayout.LabelField("Page: " + currentPage + "/" + numberOfPages);
			EditorGUILayout.EndVertical();

			//If we were searching make sure we return these values to what they were
			if (searching)
			{
				itemsPerPage = oldItemsPerPage;
				currentPage = oldCurrentPage;
			}
		}
		#endregion

		#region View Functions
		public static void DrawTitle(string title)
		{
			EditorGUILayout.PrefixLabel(title);	
		}

		public static void DrawLine()
		{
			DrawLine(EditorGUIUtility.isProSkin ? ScribeGUIStyle.collapsibleHeaderOpenColor : ScribeGUIStyle.collapsibleHeaderClosedColor);
		}

		public static void DrawLine(Color color)
		{
			GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });
		}
		#endregion

		#region Menu Functions
		public static int MenuBar(GenericMenu[] menus, int showMenuIndex, int rowSize, GUIContent[] labels)
		{
			EditorGUILayout.BeginVertical();

			Event currentEvent = Event.current;

			GUILayout.BeginHorizontal();

			for (int i = 0; i < menus.Length; i++)
			{
				GenericMenu menu = menus[i];
				bool showMenu = false;

				if (i == showMenuIndex)
				{
					showMenu = true;
				}

				EditorGUI.BeginChangeCheck();
				showMenu = GUILayout.Toggle(showMenu, labels[i], "Button");

				Rect buttonRect = GUILayoutUtility.GetLastRect();

				if (EditorGUI.EndChangeCheck())
				{
					if (showMenu)
					{
						
						Rect menuRect = new Rect(buttonRect.x, buttonRect.y + buttonRect.height, buttonRect.width, menu.GetItemCount() * 25);

						showMenuIndex = i;
						menu.DropDown(menuRect);
						currentEvent.Use();
					}
					else if (i == showMenuIndex)
					{
						showMenuIndex = -1;
					}
				}
				else if ((Event.current.button == 0 || Event.current.button == 1) && !buttonRect.Contains(currentEvent.mousePosition))
				{
					showMenu = false;
					showMenuIndex = -1;
				}
			}

			GUILayout.BeginHorizontal();

			GUILayout.EndVertical();

			return showMenuIndex;
		}
		#endregion


		#region Padding
		public static void BeginVerticalPadded(float padding, Color backgroundColor)
		{
			GUI.color = backgroundColor;
			GUILayout.BeginHorizontal(EditorStyles.textField);
			GUI.color = Color.white;

			GUILayout.Space(padding);
			GUILayout.BeginVertical();
			GUILayout.Space(padding);
		}

		public static void EndVerticalPadded(float padding)
		{
			GUILayout.Space(padding);
			GUILayout.EndVertical();
			GUILayout.Space(padding);
			GUILayout.EndHorizontal();
		}

		public static void BeginVerticalIndented(float indentation, Color backgroundColor)
		{
			GUI.color = backgroundColor;
			GUILayout.BeginHorizontal();
			GUILayout.Space(indentation);
			GUI.color = Color.white;
			GUILayout.BeginVertical();
		}

		public static void EndVerticalIndented()
		{
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		}

		public static void BeginHorizontalPadded(float padding, Color backgroundColor)
		{
			GUI.color = backgroundColor;
			GUILayout.BeginVertical(EditorStyles.textField);
			GUI.color = Color.white;

			GUILayout.Space(padding);
			GUILayout.BeginHorizontal();
			GUILayout.Space(padding);
		}

		public static void EndHorizontalPadded(float padding)
		{
			GUILayout.Space(padding);
			GUILayout.EndHorizontal();
			GUILayout.Space(padding);
			GUILayout.EndVertical();
		}
		#endregion

		#region Object Field Functions

		/// <summary>
		/// A generic version of EditorGUILayout.ObjectField.
		/// Allows objects to be drag and dropped or picked.
		/// This version defaults to 'allowSceneObjects = true'.
		/// 
		/// Instead of this:
		///     var script = (MyScript)target;
		///     script.transform = (Transform)EditorGUILayout.ObjectField("My Transform", script.transform, typeof(Transform), true);        
		/// 
		/// Do this:    
		///     var script = (MyScript)target;
		///     script.transform = EditorGUILayout.ObjectField<Transform>("My Transform", script.transform);        
		/// </summary>
		/// <typeparam name="T">The type of object to use</typeparam>
		/// <param name="label">The label (text) to show to the left of the field</param>
		/// <param name="obj">The obj variable of the script this GUI field is for</param>
		/// <returns>A reference to what is in the field. An object or null.</returns>
		public static T ObjectField<T>(string label, T obj) where T : UnityEngine.Object
		{
			return ObjectField<T>(label, obj, true);
		}

		/// <summary>
		/// A generic version of EditorGUILayout.ObjectField.
		/// Allows objects to be drag and dropped or picked.
		/// </summary>
		/// <typeparam name="T">The type of object to use</typeparam>
		/// <param name="label">The label (text) to show to the left of the field</param>
		/// <param name="obj">The obj variable of the script this GUI field is for</param>
		/// <param name="allowSceneObjects">Allow scene objects. See Unity Docs for more.</param>
		/// <returns>A reference to what is in the field. An object or null.</returns>
		public static T ObjectField<T>(string label, T obj, bool allowSceneObjects)
				where T : UnityEngine.Object
		{
			return (T)EditorGUILayout.ObjectField(label, obj, typeof(T), allowSceneObjects);
		}

		/// <summary>
		/// A generic version of EditorGUILayout.ObjectField.
		/// Allows objects to be drag and dropped or picked.
		/// </summary>
		/// <typeparam name="T">The type of object to use</typeparam>
		/// <param name="label">The label (text) to show to the left of the field</param>
		/// <param name="obj">The obj variable of the script this GUI field is for</param>
		/// <param name="allowSceneObjects">Allow scene objects. See Unity docs for more.</param>
		/// <param name="options">Layout options. See Unity docs for more.</param>
		/// <returns>A reference to what is in the field. An object or null.</returns>
		public static T ObjectField<T>(string label, T obj, bool allowSceneObjects, GUILayoutOption[] options)
				where T : UnityEngine.Object
		{
			return (T)EditorGUILayout.ObjectField(label, obj, typeof(T), allowSceneObjects, options);
		}


		/// <summary>
		/// A toggle button for a Bool type SerializedProperty. Nothing is returned because the 
		/// property is set by reference.
		/// </summary>
		/// <param name='property'>
		/// SerializedProperty.
		/// </param>
		/// <param name='content'>
		/// GUIContent(label, tooltip)
		/// </param>
		/// <param name='width'>
		/// Width of the button
		/// </param>
		public static void ToggleButton(SerializedProperty property, GUIContent content, int width)
		{
			GUIStyle style = new GUIStyle(EditorStyles.miniButton);
			style.alignment = TextAnchor.MiddleCenter;
			style.fixedWidth = width;

			// Not sure why we need this return value. Just copied from the Unity docs.
			content = EditorGUI.BeginProperty(new Rect(0, 0, 0, 0), content, property);

			EditorGUI.BeginChangeCheck();
			bool newValue = GUILayout.Toggle(property.boolValue, content, style);

			// Only assign the value back if it was actually changed by the user.
			// Otherwise a single value will be assigned to all objects when multi-object editing,
			// even when the user didn't touch the control.
			if (EditorGUI.EndChangeCheck())
			{
				property.boolValue = newValue;
			}

			EditorGUI.EndProperty();
		}

		/// <summary>
		/// A toggle button for a Bool type SerializedProperty. Nothing is returned because the 
		/// property is set by reference.
		/// </summary>
		/// <param name='property'>
		/// SerializedProperty.
		/// </param>
		/// <param name='content'>
		/// GUIContent(label, tooltip)
		/// </param>
		/// <param name='width'>
		/// Width of the button
		/// </param>
		public static bool ToggleButton(bool isTrue, GUIContent content, int width)
		{
			GUIStyle style = new GUIStyle(EditorStyles.miniButton);
			style.alignment = TextAnchor.MiddleCenter;
			style.fixedWidth = width;

			// Not sure why we need this return value. Just copied from the Unity docs.
			// content = EditorGUI.BeginProperty(new Rect(0, 0, 0, 0), content, property);

			EditorGUI.BeginChangeCheck();
			bool newValue = GUILayout.Toggle(isTrue, content, style);

			// Only assign the value back if it was actually changed by the user.
			// Otherwise a single value will be assigned to all objects when multi-object editing,
			// even when the user didn't touch the control.
			if (EditorGUI.EndChangeCheck())
			{
				isTrue = newValue;
			}
			return isTrue;
			// EditorGUI.EndProperty();
		}

		/// <summary>
		/// A toggle button for a Bool type SerializedProperty. Nothing is returned because the 
		/// property is set by reference.
		/// </summary>
		/// <param name='property'>
		/// SerializedProperty.
		/// </param>
		/// <param name='content'>
		/// GUIContent(label, tooltip)
		/// </param>
		/// <param name='width'>
		/// Width of the button
		/// </param>
		public static bool ToggleButton(Rect rect, bool isTrue, GUIContent content, int width)
		{
			GUIStyle style = new GUIStyle(EditorStyles.miniButton);
			style.alignment = TextAnchor.MiddleCenter;
			style.fixedWidth = width;

			// Not sure why we need this return value. Just copied from the Unity docs.
			// content = EditorGUI.BeginProperty(new Rect(0, 0, 0, 0), content, property);

			EditorGUI.BeginChangeCheck();
			bool newValue = EditorGUI.Toggle(rect, content, isTrue, style);

			// Only assign the value back if it was actually changed by the user.
			// Otherwise a single value will be assigned to all objects when multi-object editing,
			// even when the user didn't touch the control.
			if (EditorGUI.EndChangeCheck())
			{
				isTrue = newValue;
			}
			return isTrue;
			// EditorGUI.EndProperty();
		}


		/// <summary>
		/// A generic version of EditorGUILayout.EnumPopup.
		/// Displays an enum as a pop-up list of options
		/// 
		/// Instead of this:
		///     var script = (MyScript)target;
		///     script.options = (MyScript.MY_ENUM_OPTIONS)EditorGUILayout.EnumPopup("Options", (System.Enum)script.options);       
		/// 
		/// Do this:    
		///     var script = (MyScript)target;
		///     script.options = EditorGUILayout.EnumPopup<MyScript.MY_ENUM_OPTIONS>("Options", script.options);        
		/// </summary>
		/// <typeparam name="T">The enum type</typeparam>
		/// <param name="label">The label (text) to show to the left of the field</param>
		/// <param name="enumVar">The enum variable of the script this GUI field is for</param>
		/// <returns>The chosen option</returns>
		public static T EnumPopup<T>(string label, T enumVal)
				where T : struct, IFormattable, IConvertible, IComparable
		{
			Enum e;
			if ((e = enumVal as Enum) == null)
				throw new ArgumentException("value must be an Enum");
			object res = EditorGUILayout.EnumPopup(label, e);
			return (T)res;
		}


		/// <summary>
		/// See EnumPopup<T>(string label, T enumVal).
		/// This enables label-less GUI fields.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="enumVal"></param>
		/// <returns></returns>
		public static T EnumPopup<T>(T enumVal)
				where T : struct, IFormattable, IConvertible, IComparable
		{
			Enum e;
			if ((e = enumVal as Enum) == null)
				throw new ArgumentException("value must be an Enum");
			object res = EditorGUILayout.EnumPopup(e);
			return (T)res;
		}

		/// <summary>
		/// Add a LayerMash field. This hasn't been made available by Unity even though
		/// it exists in the automated version of the inspector (when no custom editor).
		/// </summary>
		/// <param name="label">The string to display to the left of the control</param>
		/// <param name="selected">The LayerMask variable</param>
		/// <returns>A new LayerMask value</returns>
		public static LayerMask LayerMaskField(string label, LayerMask selected)
		{
			return LayerMaskField(label, selected, true);
		}

		/// <summary>
		/// Add a LayerMash field. This hasn't been made available by Unity even though
		/// it exists in the automated version of the inspector (when no custom editor).
		/// Contains code from: 
		///     http://answers.unity3d.com/questions/60959/mask-field-in-the-editor.html
		/// </summary>
		/// <param name="label">The string to display to the left of the control</param>
		/// <param name="selected">The LayerMask variable</param>
		/// <param name="showSpecial">True to display "Nothing" & "Everything" options</param>
		/// <returns>A new LayerMask value</returns>
		public static LayerMask LayerMaskField(string label, LayerMask selected, bool showSpecial)
		{
			string selectedLayers = "";
			for (int i = 0; i < 32; i++)
			{
				string layerName = LayerMask.LayerToName(i);
				if (layerName == "") continue;  // Skip empty layers

				if (selected == (selected | (1 << i)))
				{
					if (selectedLayers == "")
					{
						selectedLayers = layerName;
					}
					else
					{
						selectedLayers = "Mixed";
						break;
					}
				}
			}

			List<string> layers = new List<string>();
			List<int> layerNumbers = new List<int>();
			if (Event.current.type != EventType.MouseDown &&
				Event.current.type != EventType.ExecuteCommand)
			{
				if (selected.value == 0)
					layers.Add("Nothing");
				else if (selected.value == -1)
					layers.Add("Everything");
				else
					layers.Add(selectedLayers);

				layerNumbers.Add(-1);
			}

			string check = "[x] ";
			string noCheck = "     ";
			if (showSpecial)
			{
				layers.Add((selected.value == 0 ? check : noCheck) + "Nothing");
				layerNumbers.Add(-2);

				layers.Add((selected.value == -1 ? check : noCheck) + "Everything");
				layerNumbers.Add(-3);
			}

			// A LayerMask is based on a 32bit field, so there are 32 'slots' max available
			for (int i = 0; i < 32; i++)
			{
				string layerName = LayerMask.LayerToName(i);
				if (layerName != "")
				{
					// Add a check box to the left of any selected layer's names
					if (selected == (selected | (1 << i)))
						layers.Add(check + layerName);
					else
						layers.Add(noCheck + layerName);

					layerNumbers.Add(i);
				}
			}

			bool preChange = GUI.changed;
			GUI.changed = false;

			int newSelected = 0;
			if (Event.current.type == EventType.MouseDown) newSelected = -1;

			newSelected = EditorGUILayout.Popup(label,
												newSelected,
												layers.ToArray(),
												EditorStyles.layerMaskField);

			if (GUI.changed && newSelected >= 0)
			{
				if (showSpecial && newSelected == 0)
					selected = 0;
				else if (showSpecial && newSelected == 1)
					selected = -1;
				else
				{
					if (selected == (selected | (1 << layerNumbers[newSelected])))
						selected &= ~(1 << layerNumbers[newSelected]);
					else
						selected = selected | (1 << layerNumbers[newSelected]);
				}
			}
			else
			{
				GUI.changed = preChange;
			}

			return selected;
		}
		#endregion

		#region Layout Utilities
		/// <summary>
		/// These are being deprecated now that we can release for specific versions of Unity.
		/// </summary>
		public static void SetLabelWidth()
		{
			EditorGUIUtility.labelWidth = CONTROLS_DEFAULT_LABEL_WIDTH;
		}

		public static void SetLabelWidth(int width)
		{
			EditorGUIUtility.labelWidth = width;
		}

		// For backwards compatability.
		public static void LookLikeControls()
		{
			SetLabelWidth();
		}

		public static void StartIndentedSection()
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(string.Empty, GUILayout.Width(8));
			EditorGUILayout.BeginVertical();
		}

		public static void EndIndentedSection()
		{
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
		}
		#endregion Layout Utilities

		#region Foldout Fields and Utilities
		/// <summary>
		/// Adds a fold-out list GUI from a generic list of any serialized object type
		/// </summary>
		/// <param name="list">A generic List</param>
		/// <param name="expanded">A bool to determine the state of the primary fold-out</param>
		public static bool FoldOutTextList(string label, List<string> list, bool expanded)
		{
			// Store the previous indent and return the flow to it at the end
			int indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			// A copy of toolbarButton with left alignment for foldouts
			var foldoutStyle = new GUIStyle(EditorStyles.toolbarButton);
			foldoutStyle.alignment = TextAnchor.MiddleLeft;

			expanded = AddFoldOutListHeader<string>(label, list, expanded, indent);

			// START. Will consist of one row with two columns. 
			//        The second column has the content
			EditorGUILayout.BeginHorizontal();

			// SPACER COLUMN / INDENT
			EditorGUILayout.BeginVertical();
			EditorGUILayout.BeginHorizontal(GUILayout.MinWidth((indent + 3) * 9));
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();

			// CONTENT COLUMN...
			EditorGUILayout.BeginVertical();

			// Use a for, instead of foreach, to avoid the iterator since we will be
			//   be changing the loop in place when buttons are pressed. Even legal
			//   changes can throw an error when changes are detected
			for (int i = 0; i < list.Count; i++)
			{
				string item = list[i];

				EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

				// FIELD...
				if (item == null) item = "";
				list[i] = EditorGUILayout.TextField(item);

				LIST_BUTTONS listButtonPressed = AddFoldOutListItemButtons();
				EditorGUILayout.EndHorizontal();
				GUILayout.Space(2);
				UpdateFoldOutListOnButtonPressed<string>(list, i, listButtonPressed);
			}

			EditorGUILayout.EndVertical();

			EditorGUILayout.EndHorizontal();

			EditorGUI.indentLevel = indent;

			return expanded;
		}

		/// <summary>
		/// Adds a fold-out list GUI from a generic list of any serialized object type
		/// </summary>
		/// <param name="list">A generic List</param>
		/// <param name="expanded">A bool to determine the state of the primary fold-out</param>
		public static bool FoldOutObjList<T>(string label, List<T> list, bool expanded) where T : UnityEngine.Object
		{
			// Store the previous indent and return the flow to it at the end
			int indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;  // Space will handle this for the header

			// A copy of toolbarButton with left alignment for foldouts
			var foldoutStyle = new GUIStyle(EditorStyles.toolbarButton);
			foldoutStyle.alignment = TextAnchor.MiddleLeft;

			if (!AddFoldOutListHeader<T>(label, list, expanded, indent))
				return false;


			// Use a for, instead of foreach, to avoid the iterator since we will be
			//   be changing the loop in place when buttons are pressed. Even legal
			//   changes can throw an error when changes are detected
			for (int i = 0; i < list.Count; i++)
			{
				T item = list[i];

				EditorGUILayout.BeginHorizontal();

				GUILayout.Space((indent + 3) * 6); // Matches the content indent

				// OBJECT FIELD...
				// Count is always in sync bec
				T fieldVal = (T)EditorGUILayout.ObjectField(item, typeof(T), true);


				// This is weird but have to replace the item with the new value, can't
				//   find a way to set in-place in a more stable way
				list.RemoveAt(i);
				list.Insert(i, fieldVal);

				LIST_BUTTONS listButtonPressed = AddFoldOutListItemButtons();

				EditorGUILayout.EndHorizontal();

				GUILayout.Space(2);


				#region Process List Changes

				// Don't allow 'up' presses for the first list item
				switch (listButtonPressed)
				{
					case LIST_BUTTONS.None: // Nothing was pressed, do nothing
						break;

					case LIST_BUTTONS.Up:
						if (i > 0)
						{
							T shiftItem = list[i];
							list.RemoveAt(i);
							list.Insert(i - 1, shiftItem);
						}
						break;

					case LIST_BUTTONS.Down:
						// Don't allow 'down' presses for the last list item
						if (i + 1 < list.Count)
						{
							T shiftItem = list[i];
							list.RemoveAt(i);
							list.Insert(i + 1, shiftItem);
						}
						break;

					case LIST_BUTTONS.Remove:
						list.RemoveAt(i);
						break;

					case LIST_BUTTONS.Add:
						list.Insert(i, null);
						break;
				}
				#endregion Process List Changes

			}

			EditorGUI.indentLevel = indent;

			return true;
		}

		/// <summary>
		/// Adds a fold-out list GUI from a generic list of any serialized object type.
		/// Uses System.Reflection to add all fields for a passed serialized object
		/// instance. Handles most basic types including automatic naming like the 
		/// inspector does by default
		/// </summary>
		/// <param name="label"> The field label</param>
		/// <param name="list">A generic List</param>
		/// <param name="expanded">A bool to determine the state of the primary fold-out</param>
		/// <param name="foldOutStates">Dictionary<object, bool> used to track list item states</param>
		/// <returns>The new foldout state from user input. Just like Unity's foldout</returns>
		public static bool FoldOutSerializedObjList(string label,
													   /*List<T> list,*/
													   SerializedProperty list,
													   Action<int, SerializedProperty> renderContents,
													   bool expanded,
													   ref Dictionary<object, bool> foldOutStates)
		{
			return SerializedObjFoldOutList(label, list, renderContents, expanded, ref foldOutStates, false);
		}

		/// <summary>
		/// Adds a fold-out list GUI from a generic list of any serialized object type.
		/// Uses System.Reflection to add all fields for a passed serialized object
		/// instance. Handles most basic types including automatic naming like the 
		/// inspector does by default
		/// 
		/// Adds collapseBools (see docs below)
		/// </summary>
		/// <param name="label"> The field label</param>
		/// <param name="list">A generic List</param>
		/// <param name="expanded">A bool to determine the state of the primary fold-out</param>
		/// <param name="foldOutStates">Dictionary<object, bool> used to track list item states</param>
		/// <param name="collapseBools">
		/// If true, bools on list items will collapse fields which follow them
		/// </param>
		/// <returns>The new foldout state from user input. Just like Unity's foldout</returns>
		public static bool SerializedObjFoldOutList(string label,
													   /*List<T> list,*/
													   SerializedProperty list,
													   Action<int, SerializedProperty> renderContents,
													   bool expanded,
													   ref Dictionary<object, bool> foldOutStates,
													   bool collapseBools)
		{
			// Store the previous indent and return the flow to it at the end
			int indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			int buttonSpacer = 6;

			#region Header Foldout
			// Use a Horizanal space or the toolbar will extend to the left no matter what
			EditorGUILayout.BeginHorizontal();
			EditorGUI.indentLevel = 0;  // Space will handle this for the header
			GUILayout.Space(indent * 6); // Matches the content indent

			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

			expanded = Foldout(expanded, label);
			if (!expanded)
			{
				// Don't add the '+' button when the contents are collapsed. Just quit.
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.EndHorizontal();
				EditorGUI.indentLevel = indent;  // Return to the last indent
				return expanded;
			}

			// BUTTONS...
			EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(100));

			// Add expand/collapse buttons if there are items in the list
			bool masterCollapse = false;
			bool masterExpand = false;
			if (list.arraySize > 0)
			{
				GUIContent content;
				var collapseIcon = '\u2261'.ToString();
				content = new GUIContent(collapseIcon, "Click to collapse all");
				masterCollapse = GUILayout.Button(content, EditorStyles.toolbarButton);

				var expandIcon = '\u25A1'.ToString();
				content = new GUIContent(expandIcon, "Click to expand all");
				masterExpand = GUILayout.Button(content, EditorStyles.toolbarButton);
			}
			else
			{
				GUILayout.FlexibleSpace();
			}

			EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(50));
			// A little space between button groups
			GUILayout.Space(buttonSpacer);

			// Main Add button
			if (GUILayout.Button(new GUIContent("+", "Click to add"), EditorStyles.toolbarButton))
			{
				list.arraySize++; // .Add(new T());
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndHorizontal();
			#endregion Header Foldout


			#region List Items
			// Use a for, instead of foreach, to avoid the iterator since we will be
			//   be changing the loop in place when buttons are pressed. Even legal
			//   changes can throw an error when changes are detected
			for (int i = 0; i < list.arraySize; i++)
			{
				//T item = list[i];
				SerializedProperty item = list.GetArrayElementAtIndex(i);

				#region Section Header
				// If there is a field with the name 'name' use it for our label
				string itemLabel = item.displayName; // GetSerializedObjFieldName<T>(item);
				if (itemLabel == "")
				{
					itemLabel = $"Element {i}";
				}


				// Get the foldout state. 
				//   If this item is new, add it too (singleton)
				//   Singleton works better than multiple Add() calls because we can do 
				//   it all at once, and in one place.
				bool foldOutState;
				if (!foldOutStates.TryGetValue(item.displayName, out foldOutState))
				{
					foldOutStates[item.displayName] = true;
					foldOutState = true;
				}

				// Force states if master buttons were pressed
				if (masterCollapse) foldOutState = false;
				if (masterExpand) foldOutState = true;

				// Use a Horizanal space or the toolbar will extend to the start no matter what
				EditorGUILayout.BeginHorizontal();
				EditorGUI.indentLevel = 0;  // Space will handle this for the header
				GUILayout.Space((indent + 3) * 6); // Matches the content indent

				EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
				// Display foldout with current state
				foldOutState = Foldout(foldOutState, itemLabel);
				foldOutStates[item.displayName] = foldOutState;  // Used again below

				LIST_BUTTONS listButtonPressed = AddFoldOutListItemButtons();

				EditorGUILayout.EndHorizontal();

				EditorGUILayout.EndHorizontal();

				#endregion Section Header


				// If folded out, display all serialized fields
				if (foldOutState == true)
				{
					EditorGUI.indentLevel = indent + 3;

					// Display Fields for the list instance
					//SerializedObjectFields<T>(item, collapseBools);
					if (renderContents != null)
					{
						renderContents(i, item);
					}
					GUILayout.Space(2);
				}



				#region Process List Changes
				// Don't allow 'up' presses for the first list item
				switch (listButtonPressed)
				{
					case LIST_BUTTONS.None: // Nothing was pressed, do nothing
						break;

					case LIST_BUTTONS.Up:
						if (i > 0)
						{
							//T shiftItem = list[i];
							//list.RemoveAt(i);
							//list.Insert(i - 1, shiftItem);

							list.MoveArrayElement(i, i - 1);
						}
						break;

					case LIST_BUTTONS.Down:
						// Don't allow 'down' presses for the last list item
						if (i + 1 < list.arraySize)
						{
							//T shiftItem = list[i];
							//list.RemoveAt(i);
							//list.Insert(i + 1, shiftItem);

							list.MoveArrayElement(i, i + 1);
						}


						break;

					case LIST_BUTTONS.Remove:
						if (EditorUtility.DisplayDialog($"Remove {itemLabel}?", $"Are you sure you want to remove {itemLabel}", "Yes", "No"))
						{
							list.DeleteArrayElementAtIndex(i);
							//list.RemoveAt(i);
							foldOutStates.Remove(item);  // Clean-up
						}
						break;

					case LIST_BUTTONS.Add:
						//list.Insert(i, new T());
						list.arraySize++;
						list.MoveArrayElement(list.arraySize - 1, i);
						break;
				}
				#endregion Process List Changes

			}
			#endregion List Items


			EditorGUI.indentLevel = indent;

			return expanded;
		}


		/// <summary>
		/// Searches a serialized object for a field matching "name" (not case-sensitve),
		/// and if found, returns the value
		/// </summary>
		/// <param name="instance">
		/// An instance of the given type. Must be System.Serializable.
		/// </param>
		/// <returns>The name field's value or ""</returns>
		public static string GetSerializedObjFieldName<T>(T instance)
		{
			// get all public properties of T to see if there is one called 'name'
			System.Reflection.FieldInfo[] fields = typeof(T).GetFields();

			// If there is a field with the name 'name' return its value
			foreach (System.Reflection.FieldInfo fieldInfo in fields)
				if (fieldInfo.Name.ToLower() == "name")
					return ((string)fieldInfo.GetValue(instance)).DeCamel();

			// If a field type is a UnityEngine object, return its name
			//   This is done in a second loop because the first is fast as is
			foreach (System.Reflection.FieldInfo fieldInfo in fields)
			{
				try
				{
					var val = (UnityEngine.Object)fieldInfo.GetValue(instance);
					return val.name.DeCamel();
				}
				catch { }
			}

			return "";
		}

		/// <summary>
		/// Uses System.Reflection to add all fields for a passed serialized object
		/// instance. Handles most basic types including automatic naming like the 
		/// inspector does by default
		/// 
		/// Optionally, this will make a bool switch collapse the following members if they 
		/// share the first 4 characters in their name or are not a bool (will collapse from
		/// bool until it finds another bool that doesn't share the first 4 characters)
		/// </summary>
		/// <param name="instance">
		/// An instance of the given type. Must be System.Serializable.
		/// </param>
		public static void SerializedObjectFields<T>(T instance)
		{
			SerializedObjectFields<T>(instance, false);
		}

		public static void SerializedObjectFields<T>(T instance, bool collapseBools)
		{
			// get all public properties of T to see if there is one called 'name'
			FieldInfo[] fields = typeof(T).GetFields();

			bool boolCollapseState = false;  // False until bool is found
			string boolCollapseName = "";    // The name of the last bool member
			string currentMemberName = "";   // The name of the member being processed

			// Display Fields Dynamically
			foreach (FieldInfo fieldInfo in fields)
			{
				if (!collapseBools)
				{
					FieldInfoField<T>(instance, fieldInfo);
					continue;
				}

				// USING collapseBools...
				currentMemberName = fieldInfo.Name;

				// If this is a bool. Add the field and set collapse to true until  
				//   the end or until another bool is hit
				if (fieldInfo.FieldType == typeof(bool))
				{
					// If the first 4 letters of this bool match the last one, include this
					//   in the collapse group, rather than starting a new one.
					if (boolCollapseName.Length > 4 &&
						currentMemberName.StartsWith(boolCollapseName.Substring(0, 4)))
					{
						if (!boolCollapseState) FieldInfoField<T>(instance, fieldInfo);
						continue;
					}

					FieldInfoField<T>(instance, fieldInfo);


					boolCollapseName = currentMemberName;
					boolCollapseState = !(bool)fieldInfo.GetValue(instance);
				}
				else
				{
					// Add the field unless collapse is true
					if (!boolCollapseState)
					{
						FieldInfoField<T>(instance, fieldInfo);
					}
				}

			}
		}

		/// <summary>
		/// Uses a System.Reflection.FieldInfo to add a field
		/// Handles most built-in types and components
		/// includes automatic naming like the inspector does 
		/// by default
		/// </summary>
		/// <param name="fieldInfo"></param>
		public static void FieldInfoField<T>(T instance, System.Reflection.FieldInfo fieldInfo)
		{
			Debug.Log("Field is " + fieldInfo.Name);

			string label = fieldInfo.Name.DeCamel();

			#region Built-in Data Types
			if (fieldInfo.FieldType == typeof(string))
			{
				var val = (string)fieldInfo.GetValue(instance);
				val = EditorGUILayout.TextField(label, val);
				fieldInfo.SetValue(instance, val);
				return;
			}
			else if (fieldInfo.FieldType == typeof(int))
			{
				var val = (int)fieldInfo.GetValue(instance);
				val = EditorGUILayout.IntField(label, val);
				fieldInfo.SetValue(instance, val);
				return;
			}
			else if (fieldInfo.FieldType == typeof(float))
			{
				var val = (float)fieldInfo.GetValue(instance);
				val = EditorGUILayout.FloatField(label, val);
				fieldInfo.SetValue(instance, val);
				return;
			}
			else if (fieldInfo.FieldType == typeof(bool))
			{
				var val = (bool)fieldInfo.GetValue(instance);
				val = EditorGUILayout.Toggle(label, val);
				fieldInfo.SetValue(instance, val);
				return;
			}
			#endregion Built-in Data Types

			#region Basic Unity Types
			else if (fieldInfo.FieldType == typeof(GameObject))
			{
				var val = (GameObject)fieldInfo.GetValue(instance);
				val = ObjectField<GameObject>(label, val);
				fieldInfo.SetValue(instance, val);
				return;
			}
			else if (fieldInfo.FieldType == typeof(Transform))
			{
				Debug.Log(" Found transform");

				var val = (Transform)fieldInfo.GetValue(instance);
				val = ObjectField<Transform>(label, val);
				fieldInfo.SetValue(instance, val);
				return;
			}
			else if (fieldInfo.FieldType == typeof(Rigidbody))
			{
				var val = (Rigidbody)fieldInfo.GetValue(instance);
				val = ObjectField<Rigidbody>(label, val);
				fieldInfo.SetValue(instance, val);
				return;
			}
			else if (fieldInfo.FieldType == typeof(Renderer))
			{
				var val = (Renderer)fieldInfo.GetValue(instance);
				val = ObjectField<Renderer>(label, val);
				fieldInfo.SetValue(instance, val);
				return;
			}
			else if (fieldInfo.FieldType == typeof(Mesh))
			{
				var val = (Mesh)fieldInfo.GetValue(instance);
				val = ObjectField<Mesh>(label, val);
				fieldInfo.SetValue(instance, val);
				return;
			}
			#endregion Basic Unity Types

			#region Unity Collider Types
			else if (fieldInfo.FieldType == typeof(BoxCollider))
			{
				var val = (BoxCollider)fieldInfo.GetValue(instance);
				val = ObjectField<BoxCollider>(label, val);
				fieldInfo.SetValue(instance, val);
				return;
			}
			else if (fieldInfo.FieldType == typeof(SphereCollider))
			{
				var val = (SphereCollider)fieldInfo.GetValue(instance);
				val = ObjectField<SphereCollider>(label, val);
				fieldInfo.SetValue(instance, val);
				return;
			}
			else if (fieldInfo.FieldType == typeof(CapsuleCollider))
			{
				var val = (CapsuleCollider)fieldInfo.GetValue(instance);
				val = ObjectField<CapsuleCollider>(label, val);
				fieldInfo.SetValue(instance, val);
				return;
			}
			else if (fieldInfo.FieldType == typeof(MeshCollider))
			{
				var val = (MeshCollider)fieldInfo.GetValue(instance);
				val = ObjectField<MeshCollider>(label, val);
				fieldInfo.SetValue(instance, val);
				return;
			}
			else if (fieldInfo.FieldType == typeof(WheelCollider))
			{
				var val = (WheelCollider)fieldInfo.GetValue(instance);
				val = ObjectField<WheelCollider>(label, val);
				fieldInfo.SetValue(instance, val);
				return;
			}
			#endregion Unity Collider Types

			#region Other Unity Types
			else if (fieldInfo.FieldType == typeof(CharacterController))
			{
				var val = (CharacterController)fieldInfo.GetValue(instance);
				val = ObjectField<CharacterController>(label, val);
				fieldInfo.SetValue(instance, val);
				return;
			}
			#endregion Other Unity Types
		}


		/// <summary>
		/// Adds the GUI header line which contains the label and add buttons.
		/// </summary>
		/// <param name="label">The visible label in the GUI</param>
		/// <param name="list">Needed to add a new item if count is 0</param>
		/// <param name="expanded"></param>
		/// <param name="lastIndent"></param>
		private static bool AddFoldOutListHeader<T>(string label, List<T> list, bool expanded, int lastIndent)
		{
			int buttonSpacer = 6;

			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
			expanded = Scribe.Foldout(expanded, label);
			if (!expanded)
			{
				// Don't add the '+' button when the contents are collapsed. Just quit.
				EditorGUILayout.EndHorizontal();
				EditorGUI.indentLevel = lastIndent;  // Return to the last indent
				return expanded;
			}

			EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(50));   // 1/2 the item button width
			GUILayout.Space(buttonSpacer);

			// Master add at end button. List items will insert
			if (GUILayout.Button(new GUIContent("+", "Click to add"), EditorStyles.toolbarButton))
				list.Add(default(T));

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndHorizontal();

			return expanded;
		}

		/// <summary>
		/// Used by AddFoldOutListItemButtons to return which button was pressed, and by 
		/// UpdateFoldOutListOnButtonPressed to process the pressed button for regular lists
		/// </summary>
		enum LIST_BUTTONS { None, Up, Down, Add, Remove }

		/// <summary>
		/// Adds the buttons which control a list item
		/// </summary>
		/// <returns>LIST_BUTTONS - The LIST_BUTTONS pressed or LIST_BUTTONS.None</returns>
		private static LIST_BUTTONS AddFoldOutListItemButtons()
		{
			#region Layout
			int buttonSpacer = 6;

			EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(100));
			// The up arrow will move things towards the beginning of the List
			var upArrow = '\u25B2'.ToString();
			bool upPressed = GUILayout.Button(new GUIContent(upArrow, "Click to shift up"),
											  EditorStyles.toolbarButton);

			// The down arrow will move things towards the end of the List
			var dnArrow = '\u25BC'.ToString();
			bool downPressed = GUILayout.Button(new GUIContent(dnArrow, "Click to shift down"),
												EditorStyles.toolbarButton);

			// A little space between button groups
			GUILayout.Space(buttonSpacer);

			// Remove Button - Process presses later
			bool removePressed = GUILayout.Button(new GUIContent("-", "Click to remove"),
												  EditorStyles.toolbarButton);

			// Add button - Process presses later
			bool addPressed = GUILayout.Button(new GUIContent("+", "Click to insert new"),
											   EditorStyles.toolbarButton);

			EditorGUILayout.EndHorizontal();
			#endregion Layout

			// Return the pressed button if any
			if (upPressed == true) return LIST_BUTTONS.Up;
			if (downPressed == true) return LIST_BUTTONS.Down;
			if (removePressed == true) return LIST_BUTTONS.Remove;
			if (addPressed == true) return LIST_BUTTONS.Add;

			return LIST_BUTTONS.None;
		}

		/// <summary>
		/// Used by basic foldout lists to process any list item button presses which will alter
		/// the order or members of the ist
		/// </summary>
		/// <param name="listButtonPressed"></param>
		private static void UpdateFoldOutListOnButtonPressed<T>(List<T> list, int currentIndex, LIST_BUTTONS listButtonPressed)
		{
			// Don't allow 'up' presses for the first list item
			switch (listButtonPressed)
			{
				case LIST_BUTTONS.None: // Nothing was pressed, do nothing
					break;

				case LIST_BUTTONS.Up:
					if (currentIndex > 0)
					{
						T shiftItem = list[currentIndex];
						list.RemoveAt(currentIndex);
						list.Insert(currentIndex - 1, shiftItem);
					}
					break;

				case LIST_BUTTONS.Down:
					// Don't allow 'down' presses for the last list item
					if (currentIndex + 1 < list.Count)
					{
						T shiftItem = list[currentIndex];
						list.RemoveAt(currentIndex);
						list.Insert(currentIndex + 1, shiftItem);
					}
					break;

				case LIST_BUTTONS.Remove:
					list.RemoveAt(currentIndex);
					break;

				case LIST_BUTTONS.Add:
					list.Insert(currentIndex, default(T));
					break;
			}
		}



		/// <summary>
		/// Adds a foldout in 'LookLikeInspector' which has a full bar to click on, not just
		/// the little triangle. It also adds a default tool-tip.
		/// </summary>
		/// <param name="expanded"></param>
		/// <param name="label"></param>
		/// <returns></returns>
		public static bool Foldout(bool expanded, string label)
		{
			var content = new GUIContent(label, FOLD_OUT_TOOL_TIP);
			expanded = EditorGUILayout.Foldout(expanded, content);
			SetLabelWidth();

			return expanded;
		}
		#endregion Foldout Fields and Utilities

		#region Standardized GUI Functios

		public static void EditorGUILayoutBegingGroup()
		{
			GUILayout.BeginHorizontal();
			EditorGUILayout.BeginHorizontal(ScribeGUIStyle.GroupBoxStyle, GUILayout.MinHeight(EditorGUIUtility.singleLineHeight));
			GUILayout.BeginVertical();
			GUILayout.Space(2);
		}

		public static void EditorGUILayoutEndGroup()
		{
			try
			{
				GUILayout.Space(3);
				GUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();
				GUILayout.EndHorizontal();
				GUILayout.Space(3);
			}
			catch (ArgumentException)
			{
				// If Unity opens a popup bwindow such as a color picker, it raises an exception.
			}
		}

		public static void EditorGUILayoutBeginIndent()
		{
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(18);
			EditorGUILayout.BeginVertical();
		}

		public static void EditorGUILayoutEndIndent()
		{
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
		}

		public static void EditorGUILayoutVerticalSpace(float pixels)
		{
			EditorGUILayout.BeginVertical();
			GUILayout.Space(pixels);
			EditorGUILayout.EndVertical();
		}

		public static bool EditorGUILayoutFoldout(string label, string tooltip, bool foldout, bool topLevel = true)
		{
			return EditorGUILayoutFoldout(label, tooltip, foldout, ScribeGUIStyle.collapsibleHeaderOpenColor, ScribeGUIStyle.collapsibleHeaderClosedColor, topLevel);
		}

		public static bool EditorGUILayoutFoldout(string label, string tooltip, bool foldout, Color openColor, bool topLevel = true)
		{
			return EditorGUILayoutFoldout(label, tooltip, foldout, openColor, ScribeGUIStyle.collapsibleHeaderClosedColor, topLevel);
		}

		public static bool EditorGUILayoutFoldout(string label, string tooltip, bool foldout, Color openColor, Color closedColor, bool topLevel = true)
		{
			try
			{
				GUILayout.BeginHorizontal();
				GUI.backgroundColor = foldout ? openColor : closedColor;
#if UNITY_2019_1_OR_NEWER
				var text = label;
#else
                var text = topLevel ? ("<b>" + label + "</b>") : label;
#endif
				var guiContent = new GUIContent((foldout ? ScribeGUIStyle.FoldoutOpenArrow : ScribeGUIStyle.FoldoutClosedArrow) + text, tooltip);
				var guiStyle = topLevel ? ScribeGUIStyle.CollapsibleHeaderButtonStyleName : ScribeGUIStyle.CollapsibleSubheaderButtonStyleName;
				if (!GUILayout.Toggle(true, guiContent, guiStyle))
				{
					foldout = !foldout;
				}
				GUI.backgroundColor = Color.white;
			}
			finally
			{
				EditorGUILayout.EndHorizontal();
			}
			return foldout;
		}

		public static bool EditorGUIFoldout(Rect rect, string label, string tooltip, bool foldout, bool topLevel = true)
		{
			return EditorGUIFoldout(rect, label, tooltip, foldout, ScribeGUIStyle.collapsibleHeaderOpenColor, ScribeGUIStyle.collapsibleHeaderClosedColor, topLevel);
		}

		public static bool EditorGUIFoldout(Rect rect, string label, string tooltip, bool foldout, Color openColor, Color closedColor, bool topLevel = true)
		{
			GUI.backgroundColor = foldout ? openColor : closedColor;
#if UNITY_2019_1_OR_NEWER
			var text = label;
#else
            var text = topLevel ? ("<b>" + label + "</b>") : label;
#endif
			var guiContent = new GUIContent((foldout ? ScribeGUIStyle.FoldoutOpenArrow : ScribeGUIStyle.FoldoutClosedArrow) + text, tooltip);
			var guiStyle = topLevel ? ScribeGUIStyle.CollapsibleHeaderButtonStyleName : ScribeGUIStyle.CollapsibleSubheaderButtonStyleName;
			if (!GUI.Toggle(rect, true, guiContent, guiStyle))
			{
				foldout = !foldout;
			}
			GUI.backgroundColor = Color.white;
			return foldout;
		}
		#endregion

		#region Next Gen Utility Functions
		/// <summary>
		/// Get all subtypes that inherit from type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="limitToNameSpace">Limit to namespace eg. "PixelCrushers.QuestMachine"</param>
		/// <returns></returns>
		public static List<Type> GetSubtypes<T>(string limitToNameSpace) where T : class
		{
			var subtypes = GetAllSubtypes<T>();
			subtypes.RemoveAll(x => HasWrapperType(x, limitToNameSpace));
			return subtypes;
		}

		/// <summary>
		/// Get all subtypes that inherit from type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="limitToNameSpace">Limit to namespace eg. "PixelCrushers.QuestMachine"</param>
		/// <returns></returns>
		public static List<Type> GetSubtypes(Type type, string limitToNameSpace)
		{
			var subtypes = GetAllSubtypes(type);
			subtypes.RemoveAll(x => HasWrapperType(x, limitToNameSpace));
			return subtypes;
		}

		public static bool HasWrapperType(System.Type type, string limitToNameSpace)
		{
			return GetWrapperType(type, limitToNameSpace) != null;
		}

		public static System.Type GetWrapperType(System.Type type, string limitToNameSpace)
		{
			try
			{
				if (string.Equals(type.Namespace, limitToNameSpace)) //
				{
					bool useNamespace = string.IsNullOrEmpty(limitToNameSpace);
					var wrapperName = useNamespace ? limitToNameSpace + "." + type.Name : type.Name;
					var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(p => !(p.ManifestModule is System.Reflection.Emit.ModuleBuilder)); // Exclude dynamic assemblies.
					var wrapperList = (from domainAssembly in assemblies
									   from assemblyType in domainAssembly.GetExportedTypes()
									   where string.Equals(useNamespace ? assemblyType.Name : assemblyType.FullName, wrapperName)
									   select assemblyType).ToArray();
					if (wrapperList.Length > 0) return wrapperList[0];
				}
			}
			catch (NotSupportedException)
			{
				// If an assembly complains, ignore it and move on.
			}
			return null;
		}

		/// <summary>
		/// Gets all non-abstract subtypes of a specified type.
		/// </summary>
		/// <typeparam name="T">Parent type.</typeparam>
		/// <returns>List of all non-abstract subtypes descended from the parent type.</returns>
		private static List<Type> GetAllSubtypes<T>() where T : class
		{
			return GetAllSubtypes(typeof(T));
		}

		/// <summary>
		/// Gets all non-abstract subtypes of a specified type.
		/// </summary>
		/// <typeparam name="T">Parent type.</typeparam>
		/// <returns>List of all non-abstract subtypes descended from the parent type.</returns>
		private static List<Type> GetAllSubtypes(Type type)
		{
			var subtypes = new List<Type>();
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (assembly.FullName.StartsWith("Mono.Cecil")) continue;
				if (assembly.FullName.StartsWith("UnityScript")) continue;
				if (assembly.FullName.StartsWith("Boo.Lan")) continue;
				if (assembly.FullName.StartsWith("System")) continue;
				if (assembly.FullName.StartsWith("I18N")) continue;
				if (assembly.FullName.StartsWith("UnityEngine")) continue;
				if (assembly.FullName.StartsWith("UnityEditor")) continue;
				if (assembly.FullName.StartsWith("mscorlib")) continue;
				foreach (Type assemblyType in assembly.GetTypes())
				{
					if (!assemblyType.IsClass) continue;
					if (assemblyType.IsAbstract) continue;
					if (!assemblyType.IsSubclassOf(type))
					{
						continue;
					}
					subtypes.Add(assemblyType);
				}
			}
			return subtypes;
		}
		#endregion

		/// <summary>
		/// Converts a string from camel-case to seperate words that start with 
		/// capital letters. Also removes leading underscores.
		/// </summary>
		/// <returns>string</returns>
		public static string DeCamel(this string s)
		{
			if (string.IsNullOrEmpty(s)) return string.Empty;

			System.Text.StringBuilder newStr = new System.Text.StringBuilder();

			char c;
			for (int i = 0; i < s.Length; i++)
			{
				c = s[i];

				// Handle spaces and underscores. 
				//   Do not keep underscores
				//   Only keep spaces if there is a lower case letter next, and 
				//       capitalize the letter
				if (c == ' ' || c == '_')
				{
					// Only check the next character is there IS a next character
					if (i < s.Length - 1 && char.IsLower(s[i + 1]))
					{
						// If it isn't the first character, add a space before it
						if (newStr.Length != 0)
						{
							newStr.Append(' ');  // Add the space
							newStr.Append(char.ToUpper(s[i + 1]));
						}
						else
						{
							newStr.Append(s[i + 1]);  // Stripped if first char in string
						}

						i++;  // Skip the next. We already used it
					}
				}  // Handle uppercase letters
				else if (char.IsUpper(c))
				{
					// If it isn't the first character, add a space before it
					if (newStr.Length != 0)
					{
						newStr.Append(' ');
						newStr.Append(c);
					}
					else
					{
						newStr.Append(c);
					}
				}
				else  // Normal character. Store and move on.
				{
					newStr.Append(c);
				}
			}

			return newStr.ToString();
		}
	}
}

			
