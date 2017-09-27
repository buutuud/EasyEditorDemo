using UnityEditor;
using UnityEngine;
using System.Collections;
using EasyEditor;
using EasyEditor.ReorderableList;
using System.Linq;
using System.Collections.Generic;

[Groups ("")]
[CustomEditor (typeof(LuaBehaver))]
public class LuaBehaverEditor : EasyEditorBase
{
	//ReorderableListRenderer reorderableList;
	private EESerializedPropertyAdaptor reorderableList;
	new public void OnEnable ()
	{
		base.OnEnable ();

		var nodesProperty = serializedObject.FindProperty ("m_BindObjList2");
		this.reorderableList = new EESerializedPropertyAdaptor (nodesProperty, false);
		this.reorderableList.OnDrawItem += OnDrawItem;
		//reorderableList = (ReorderableListRenderer)LookForRenderer ("m_BindObjList2");
		//reorderableList.OnItemInserted += HandleOnItemInserted;
		//reorderableList.OnItemBeingRemoved += HandleOnItemBeingRemoved;
		//reorderableList.OnDrawItem = OnDrawItem;
	}
	public override void OnInspectorGUI ()
	{
		//base.OnInspectorGUI ();
		this.serializedObject.Update ();
//		ReorderableListGUI.ListField (this.pProperty);
		ReorderableListGUI.ListField (this.reorderableList);

		//this.listControl.Draw (this.listAdaptor);
		this.serializedObject.ApplyModifiedProperties ();
	}

	new public void OnDisable ()
	{
		//reorderableList.OnItemBeingRemoved -= HandleOnItemBeingRemoved;
	}

	void OnDrawItem (Rect rect, int index)
	{
		var arrayProperty = this.reorderableList.arrayProperty;
		var element = arrayProperty.GetArrayElementAtIndex (index);
		var text = arrayProperty.GetArrayElementAtIndex (index).displayName;
		//Debug.LogFormat ("1.{0}", text);
		var content = new GUIContent (text);

		float widthA = rect.width * 0.25f;
		float widthB = rect.width * 0.75f * 0.5f;
		float widthC = rect.width * 0.75f * 0.5f;
		SerializedProperty keyProp = element.FindPropertyRelative ("key");
		SerializedProperty objectProp = element.FindPropertyRelative ("obj");

		EditorGUI.BeginChangeCheck ();

		EditorGUI.PropertyField (
			new Rect (rect.x, rect.y, widthA - 5, EditorGUIUtility.singleLineHeight),
			keyProp, GUIContent.none);

		if (EditorGUI.EndChangeCheck ()) {
			// Force the key to be a valid Lua variable name
			var luaBindings = target as LuaBehaver;
			keyProp.stringValue = GetUniqueKey (luaBindings, keyProp.stringValue, index);
		}

		//obj
		EditorGUI.BeginChangeCheck ();

		EditorGUI.PropertyField (new Rect (rect.x + widthA, rect.y, widthB - 5, EditorGUIUtility.singleLineHeight),objectProp, GUIContent.none);

		if (EditorGUI.EndChangeCheck ()) {
			// Use the object name as the key
			string keyName = objectProp.objectReferenceValue.name;
			var luaBindings = target as LuaBehaver;
			element.FindPropertyRelative ("key").stringValue = GetUniqueKey (luaBindings, keyName.ToLower (), index);

			// Auto select any Flowchart component in the object
			GameObject go = objectProp.objectReferenceValue as GameObject;
			if (go != null) {
				Component flowchart = go.GetComponent ("Fungus.Flowchart");
				if (flowchart != null) {
					SerializedProperty componentProp = element.FindPropertyRelative ("component");
					componentProp.objectReferenceValue = flowchart;
				}
			}
		}

		//
		//EditorGUI.PropertyField (new Rect (rect.x + widthA, rect.y, widthB - 5, EditorGUIUtility.singleLineHeight),objectProp, GUIContent.none);

		if (objectProp.objectReferenceValue != null) {         
			GameObject go = objectProp.objectReferenceValue as GameObject;
			if (go != null) {
				SerializedProperty componentProp = element.FindPropertyRelative ("component");

				int selected = 0;
				List<string> options = new List<string> ();
				options.Add ("<GameObject>");

				int count = 1;
				Component[] componentList = go.GetComponents<Component> ();
				foreach (Component component in componentList) {
					if (componentProp.objectReferenceValue == component) {
						selected = count;
					}

					if (component == null ||
						component.GetType () == null) {
						// Missing script?
						continue;
					}

					string componentName = component.GetType ().ToString ().Replace ("UnityEngine.", "");
					options.Add (componentName);

					count++;
				}

				int i = EditorGUI.Popup (new Rect (rect.x + widthA + widthB, rect.y, widthC, EditorGUIUtility.singleLineHeight),selected,options.ToArray ());
				if (i == 0) {
					componentProp.objectReferenceValue = null;
				} else {
					componentProp.objectReferenceValue = componentList [i - 1];
				}
			}                            
		}
	}

	void HandleOnItemInserted (int index, SerializedProperty list)
	{
		list.GetArrayElementAtIndex (index).boundsValue = new Bounds (Vector3.one, Vector3.zero);
	}

	void HandleOnItemBeingRemoved (int index, SerializedProperty list)
	{
		Debug.Log ("Bounds being removed\n" + list.GetArrayElementAtIndex (index));
	}

	/// <summary>
	/// Returns a new binding key that is guaranteed to be a valid Lua variable name and
	/// not to clash with any existing binding in the list.
	/// </summary>
	protected virtual string GetUniqueKey (LuaBehaver luaBindings, string originalKey, int ignoreIndex = -1)
	{
		string baseKey = originalKey;

		// Only letters and digits allowed
		char[] arr = baseKey.Where (c => (char.IsLetterOrDigit (c) || c == '_')).ToArray (); 
		baseKey = new string (arr);

		// No leading digits allowed
		baseKey = baseKey.TrimStart ('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');

		// No empty keys allowed
		if (baseKey.Length == 0) {
			baseKey = "object";
		}

		// Build a hash of all keys currently in use
		HashSet<string> keyhash = new HashSet<string> ();
		for (int i = 0; i < luaBindings.m_BindObjList2.Count; ++i) {
			if (i == ignoreIndex) {
				continue;
			}

			keyhash.Add (luaBindings.m_BindObjList2 [i].key);
		}

		// Append a suffix to make the key unique
		string key = baseKey;
		int suffix = 0;
		while (keyhash.Contains (key)) {
			suffix++;
			key = baseKey + suffix;
		}

		return key;
	}
}