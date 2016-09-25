using UnityEngine;
using UnityEditor;
using System.Collections;
[CustomEditor(typeof(EditorGUITest))]
public class EditorGUIEditor : Editor {

	InEditorUI editorUI;

	public override void OnInspectorGUI(){
		DrawDefaultInspector ();
		//DrawPropertiesExcluding (serializedObject, new string[]{ "m_Script" });
		if (GUILayout.Button ("Get events")) {
			EditorGUITest test = (EditorGUITest)target;
			editorUI = new InEditorUI ();
			editorUI.AddSelectable(test.testButton);
			editorUI.AddSelectable(test.testToggle);
			editorUI.AddSelectable(test.testField);
			editorUI.AddSelectable(test.testScroll);
			editorUI.AddSelectable(test.testSlider);
			editorUI.AddSelectable(test.testDrop);
		}
	}
}
