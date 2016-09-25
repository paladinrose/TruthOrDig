using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(LayoutTransform))]
public class LayoutTransformEditor : Editor {

	bool showTransform, showLocalBounds, showGlobalBounds, showSides, holding, waitForOneFrame;
	ExtensionMethods.BoundSides currentSide;
	float dist;
	Vector3[] globalLines = new Vector3[8], localLines = new Vector3[26];

	LayoutTransform layoutTransform;
	Transform transform;
	public void OnEnable()
	{
		layoutTransform = (LayoutTransform)target;
		transform = layoutTransform.t;
		layoutTransform.binder.Connect ();
	}
	public override void OnInspectorGUI()
	{
		serializedObject.Update ();
		EditorGUI.BeginChangeCheck ();
		showTransform = EditorGUILayout.ToggleLeft ("Transform Values", showTransform);

		if (showTransform) {
			
			transform.position = EditorGUILayout.Vector3Field (new GUIContent ("Position"), transform.position);
			transform.localPosition = EditorGUILayout.Vector3Field (new GUIContent ("Local Position"), transform.localPosition);
			EditorGUILayout.Space ();

			transform.eulerAngles = EditorGUILayout.Vector3Field (new GUIContent ("Rotation"), transform.eulerAngles);
			transform.localEulerAngles = EditorGUILayout.Vector3Field (new GUIContent ("Local Rotation"), transform.localEulerAngles);
			EditorGUILayout.Space ();

			layoutTransform.localBounds = transform.LocalBounds (layoutTransform.includeChildrenInLocalBounds);
			layoutTransform.bounds = transform.GlobalBounds (layoutTransform.includeChildrenInGlobalBounds);
			UpdateLocalBounds ();
			UpdateGlobalBounds ();

			Vector3 scale = EditorGUILayout.Vector3Field (new GUIContent ("Scale"), layoutTransform.localBounds.size);

			if (scale != layoutTransform.localBounds.size) {
				transform.localScale = transform.LossyScale (scale, layoutTransform.includeChildrenInLocalBounds);
			}

			transform.localScale = EditorGUILayout.Vector3Field (new GUIContent ("Local Scale"), transform.localScale);
			EditorGUILayout.Space ();

			showGlobalBounds = EditorGUILayout.ToggleLeft ("Bounds", showGlobalBounds);
			if (showGlobalBounds) {
				layoutTransform.includeChildrenInGlobalBounds = EditorGUILayout.Toggle (new GUIContent("Include Children"),layoutTransform.includeChildrenInGlobalBounds);
				EditorGUILayout.BoundsField (GUIContent.none, layoutTransform.bounds); 
				EditorGUILayout.Space ();
			}


			showLocalBounds = EditorGUILayout.ToggleLeft ("Local Bounds", showLocalBounds);
			if (showLocalBounds) {
				layoutTransform.includeChildrenInLocalBounds = EditorGUILayout.Toggle (new GUIContent("Include Children"),layoutTransform.includeChildrenInLocalBounds);
				EditorGUILayout.BoundsField (GUIContent.none, layoutTransform.localBounds);
				EditorGUILayout.Space ();
			}

			showSides = EditorGUILayout.ToggleLeft ("Local Sides", showSides);
			if (showSides) {
				float sideDist;
				EditorGUIUtility.labelWidth = EditorGUIUtility.labelWidth * 0.5f;

				GUILayout.BeginHorizontal ();
				//localLines[20].z = 
					EditorGUILayout.FloatField (new GUIContent("Front"), localLines [20].z);
				if (localLines[20] != localLines [8]) {
					currentSide = ExtensionMethods.BoundSides.Front;
					sideDist = Vector3.Distance (localLines [8], localLines[20]);
					sideDist *= Vector3.Dot((localLines [20] - localLines [8]).normalized, localLines[14]);

					Bounds b = ExtensionMethods.SetSide (layoutTransform.localBounds, currentSide, sideDist);
					transform.SetLocalBounds (b, layoutTransform.includeChildrenInLocalBounds);
					layoutTransform.localBounds = b;
					layoutTransform.bounds = transform.GlobalBounds (layoutTransform.includeChildrenInGlobalBounds);
					UpdateGlobalBounds ();
					UpdateLocalBounds ();
					serializedObject.Update ();
					Repaint ();
					HandleUtility.Repaint ();
				}

				//localLines[21].z = 
					EditorGUILayout.FloatField ("Back",localLines [21].z);
				if (localLines[21] != localLines [9]) {
					currentSide = ExtensionMethods.BoundSides.Back;
					sideDist = Vector3.Distance (localLines [9], localLines[21]);
					sideDist *= Vector3.Dot((localLines [21] - localLines [9]).normalized, localLines[15]);

					Bounds b = ExtensionMethods.SetSide (layoutTransform.localBounds, currentSide, sideDist);
					transform.SetLocalBounds (b, layoutTransform.includeChildrenInLocalBounds);
					layoutTransform.localBounds = b;
					layoutTransform.bounds = transform.GlobalBounds (layoutTransform.includeChildrenInGlobalBounds);
					UpdateGlobalBounds ();
					UpdateLocalBounds ();
					serializedObject.Update ();
					Repaint ();
					HandleUtility.Repaint ();
				}
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				//localLines[22].x = 
					EditorGUILayout.FloatField ("Left",localLines [22].x);
				if (localLines[22] != localLines [10]) {
					currentSide = ExtensionMethods.BoundSides.Left;
					sideDist = Vector3.Distance (localLines [10], localLines[22]);
					sideDist *= Vector3.Dot((localLines [22] - localLines [10]).normalized, localLines[16]);

					Bounds b = ExtensionMethods.SetSide (layoutTransform.localBounds, currentSide, sideDist);
					transform.SetLocalBounds (b, layoutTransform.includeChildrenInLocalBounds);
					layoutTransform.localBounds = b;
					layoutTransform.bounds = transform.GlobalBounds (layoutTransform.includeChildrenInGlobalBounds);
					UpdateGlobalBounds ();
					UpdateLocalBounds ();
					serializedObject.Update ();
					Repaint ();
					HandleUtility.Repaint ();
				}

				//localLines[23].x = 
					EditorGUILayout.FloatField ("Right",localLines [23].x);
				if (localLines[23] != localLines [11]) {
					currentSide = ExtensionMethods.BoundSides.Right;
					//sideDist = Vector3.Distance (localLines [11], localLines[23]);
					//sideDist *= Vector3.Dot((localLines [23] - localLines [11]).normalized, localLines[17]);
					sideDist = layoutTransform.localBounds.center.x + localLines[23].x;

					Bounds b = ExtensionMethods.SetSide (layoutTransform.localBounds, currentSide, sideDist);
					transform.SetLocalBounds (b, layoutTransform.includeChildrenInLocalBounds);
					layoutTransform.localBounds = b;
					layoutTransform.bounds = transform.GlobalBounds (layoutTransform.includeChildrenInGlobalBounds);
					UpdateGlobalBounds ();
					UpdateLocalBounds ();
					serializedObject.Update ();
					Repaint ();
					HandleUtility.Repaint ();
				}
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				//localLines[24].y = 
					EditorGUILayout.FloatField ("Top",localLines [24].y);
				if (localLines[24] != localLines [12]) {
					currentSide = ExtensionMethods.BoundSides.Top;
					sideDist = Vector3.Distance (localLines [12], localLines[24]);
					sideDist *= Vector3.Dot((localLines [24] - localLines [12]).normalized, localLines[18]);

					Bounds b = ExtensionMethods.SetSide (layoutTransform.localBounds, currentSide, sideDist);
					transform.SetLocalBounds (b, layoutTransform.includeChildrenInLocalBounds);
					layoutTransform.localBounds = b;
					layoutTransform.bounds = transform.GlobalBounds (layoutTransform.includeChildrenInGlobalBounds);
					UpdateGlobalBounds ();
					UpdateLocalBounds ();
					serializedObject.Update ();
					Repaint ();
					HandleUtility.Repaint ();
				}

				//localLines[25].y = 
					EditorGUILayout.FloatField ("Bottom",localLines [25].y);
				if (localLines[25] != localLines [13]) {
					currentSide = ExtensionMethods.BoundSides.Bottom;
					sideDist = Vector3.Distance (localLines [13], localLines[25]);
					sideDist *= Vector3.Dot((localLines [25] - localLines [13]).normalized, localLines[19]);

					Bounds b = ExtensionMethods.SetSide (layoutTransform.localBounds, currentSide, sideDist);
					transform.SetLocalBounds (b, layoutTransform.includeChildrenInLocalBounds);
					layoutTransform.localBounds = b;
					layoutTransform.bounds = transform.GlobalBounds (layoutTransform.includeChildrenInGlobalBounds);
					UpdateGlobalBounds ();
					UpdateLocalBounds ();
					serializedObject.Update ();
					Repaint ();
					HandleUtility.Repaint ();
				}
				GUILayout.EndHorizontal ();
				EditorGUIUtility.labelWidth = 0;
			}
			EditorGUILayout.Space ();
		

		}
		layoutTransform.bindInEditor = EditorGUILayout.ToggleLeft ("Bind In Editor", layoutTransform.bindInEditor);
		DrawPropertiesExcluding (serializedObject, new string[]{"m_Script"});
		serializedObject.ApplyModifiedProperties ();

		if (EditorGUI.EndChangeCheck () && layoutTransform.bindInEditor) {
			for (int ls = 0; ls < layoutTransform.layoutSets.Length; ls++) {
				for (int lss = 0; lss < layoutTransform.layoutSets[ls].bindings.Length; lss++) {
					layoutTransform.binder.Bind (layoutTransform.layoutSets[ls].bindings [lss]);
				}
			}
			layoutTransform.UpdateTransform ();
			HandleUtility.Repaint ();
		}
	}
	void UpdateGlobalBounds(){
		
		globalLines [0] = layoutTransform.bounds.min; 
		globalLines [1] = new Vector3 (layoutTransform.bounds.max.x, layoutTransform.bounds.min.y, layoutTransform.bounds.min.z);

		globalLines [2] = new Vector3 (layoutTransform.bounds.max.x, layoutTransform.bounds.max.y, layoutTransform.bounds.min.z); 
		globalLines [3] = new Vector3 (layoutTransform.bounds.min.x, layoutTransform.bounds.max.y, layoutTransform.bounds.min.z);


		globalLines [4] = new Vector3 (layoutTransform.bounds.min.x, layoutTransform.bounds.min.y, layoutTransform.bounds.max.z); 
		globalLines [5] = new Vector3 (layoutTransform.bounds.max.x, layoutTransform.bounds.min.y, layoutTransform.bounds.max.z);


		globalLines [6] = layoutTransform.bounds.max; 
		globalLines [7] = new Vector3 (layoutTransform.bounds.min.x, layoutTransform.bounds.max.y, layoutTransform.bounds.max.z);

	}

	void UpdateLocalBounds(){
		
		localLines [0] = transform.position + transform.TransformDirection(layoutTransform.localBounds.min); 
		localLines [1] = transform.position + transform.TransformDirection( new Vector3 (layoutTransform.localBounds.max.x, layoutTransform.localBounds.min.y, layoutTransform.localBounds.min.z)) ;

		localLines [2] = transform.position + transform.TransformDirection(new Vector3 (layoutTransform.localBounds.max.x, layoutTransform.localBounds.max.y, layoutTransform.localBounds.min.z)) ; 
		localLines [3] = transform.position + transform.TransformDirection(new Vector3 (layoutTransform.localBounds.min.x, layoutTransform.localBounds.max.y, layoutTransform.localBounds.min.z)) ;


		localLines [4] = transform.position + transform.TransformDirection(new Vector3 (layoutTransform.localBounds.min.x, layoutTransform.localBounds.min.y, layoutTransform.localBounds.max.z)) ; 
		localLines [5] = transform.position + transform.TransformDirection(new Vector3 (layoutTransform.localBounds.max.x, layoutTransform.localBounds.min.y, layoutTransform.localBounds.max.z)) ;


		localLines [6] = transform.position + transform.TransformDirection(layoutTransform.localBounds.max) ; 
		localLines [7] = transform.position + transform.TransformDirection(new Vector3 (layoutTransform.localBounds.min.x, layoutTransform.localBounds.max.y, layoutTransform.localBounds.max.z)) ;

		//Front, Back, Left, Right, Top, Bottom
		localLines[8] = localLines [20] = transform.position + transform.TransformDirection(new Vector3(layoutTransform.localBounds.center.x, layoutTransform.localBounds.center.y, layoutTransform.localBounds.max.z));
		localLines[9] = localLines [21] = transform.position + transform.TransformDirection(new Vector3(layoutTransform.localBounds.center.x, layoutTransform.localBounds.center.y, layoutTransform.localBounds.min.z));

		localLines[10] = localLines [22] = transform.position + transform.TransformDirection(new Vector3(layoutTransform.localBounds.min.x, layoutTransform.localBounds.center.y, layoutTransform.localBounds.center.z));
		localLines[11] = localLines [23] = transform.position + transform.TransformDirection(new Vector3(layoutTransform.localBounds.max.x, layoutTransform.localBounds.center.y, layoutTransform.localBounds.center.z));

		localLines[12] = localLines [24] = transform.position + transform.TransformDirection(new Vector3(layoutTransform.localBounds.center.x, layoutTransform.localBounds.max.y, layoutTransform.localBounds.center.z));
		localLines[13] = localLines [25] = transform.position + transform.TransformDirection(new Vector3(layoutTransform.localBounds.center.x, layoutTransform.localBounds.min.y, layoutTransform.localBounds.center.z));

		localLines [14] = (localLines [8] - layoutTransform.bounds.center).normalized;
		localLines [15] = (localLines [9] - layoutTransform.bounds.center).normalized;
		localLines [16] = (localLines [10] - layoutTransform.bounds.center).normalized;
		localLines [17] = (localLines [11] - layoutTransform.bounds.center).normalized;
		localLines [18] = (localLines [12] - layoutTransform.bounds.center).normalized;
		localLines [19] = (localLines [13] - layoutTransform.bounds.center).normalized;
	}
	public void OnSceneGUI()
	{
		Color g = Handles.color;
		float ringSize = 0.1f, ringGrowth = 0.02f; 
		for (int i = 0; i < layoutTransform.anchors.Length; i++) {
			Handles.CircleCap (0, layoutTransform.anchors [i], Quaternion.identity, ringSize);
			ringSize += ringGrowth;
		}
		if(showGlobalBounds){
			Handles.color = new Color (0.5f, 0.5f, 1, 0.75f);
			Handles.DrawLine (globalLines [0], globalLines [1]);
			Handles.DrawLine (globalLines [1], globalLines [2]);
			Handles.DrawLine (globalLines [2], globalLines [3]);
			Handles.DrawLine (globalLines [3], globalLines [0]);
			Handles.DrawLine (globalLines [0], globalLines [4]);
			Handles.DrawLine (globalLines [4], globalLines [5]);
			Handles.DrawLine (globalLines [1], globalLines [5]);
			Handles.DrawLine (globalLines [5], globalLines [6]);
			Handles.DrawLine (globalLines [2], globalLines [6]);
			Handles.DrawLine (globalLines [6], globalLines [7]);
			Handles.DrawLine (globalLines [3], globalLines [7]);
			Handles.DrawLine (globalLines [7], globalLines [4]);
		}
		if (showLocalBounds) {
			Handles.color = new Color (0.5f, 1f, 0.5f, 0.75f);
			Handles.DrawLine (localLines [0], localLines [1]);
			Handles.DrawLine (localLines [1], localLines [2]);
			Handles.DrawLine (localLines [2], localLines [3]);
			Handles.DrawLine (localLines [3], localLines [0]);
			Handles.DrawLine (localLines [0], localLines [4]);
			Handles.DrawLine (localLines [4], localLines [5]);
			Handles.DrawLine (localLines [1], localLines [5]);
			Handles.DrawLine (localLines [5], localLines [6]);
			Handles.DrawLine (localLines [2], localLines [6]);
			Handles.DrawLine (localLines [6], localLines [7]);
			Handles.DrawLine (localLines [3], localLines [7]);
			Handles.DrawLine (localLines [7], localLines [4]);

			Handles.color = new Color (0.75f, 1, 0.75f, 1);

			if (Event.current.type == EventType.mouseUp && holding) {

				holding = false;
				Bounds b = ExtensionMethods.MoveSide (layoutTransform.localBounds, currentSide, dist);
				transform.SetLocalBounds (b, layoutTransform.includeChildrenInLocalBounds);
				layoutTransform.localBounds = b;
				layoutTransform.bounds = transform.GlobalBounds (layoutTransform.includeChildrenInGlobalBounds);
				UpdateGlobalBounds ();
				UpdateLocalBounds ();
				serializedObject.Update ();
				Repaint ();
				HandleUtility.Repaint ();
			}

			localLines[20] = Handles.Slider (localLines [20], localLines[14]);

			if (localLines[20] != localLines [8]) {
				currentSide = ExtensionMethods.BoundSides.Front;
				holding = true;
				dist = Vector3.Distance (localLines [8], localLines[20]);
				dist *= Vector3.Dot((localLines [20] - localLines [8]).normalized, localLines[14]);
				if (transform.localScale.z < 0) {
					dist *= -1;
				}
			}

			localLines[21] = Handles.Slider (localLines [21], localLines[15]);

			if (localLines[21] != localLines [9]) {
				currentSide = ExtensionMethods.BoundSides.Back;
				holding = true;
				dist = Vector3.Distance (localLines [9], localLines[21]);
				dist *= Vector3.Dot((localLines [21] - localLines [9]).normalized, localLines[15]);
				if (transform.localScale.z < 0) {
					dist *= -1;
				}
			}

			localLines[22] = Handles.Slider (localLines [22], localLines[16]);

			if (localLines[22] != localLines [10]) {
				currentSide = ExtensionMethods.BoundSides.Left;
				holding = true;
				dist = Vector3.Distance (localLines [10], localLines[22]);
				dist *= Vector3.Dot((localLines [22] - localLines [10]).normalized, localLines[16]);
				if (transform.localScale.x < 0) {
					dist *= -1;
				}
			}

			localLines[23] = Handles.Slider (localLines [23], localLines[17]);

			if (localLines[23] != localLines [11]) {
				currentSide = ExtensionMethods.BoundSides.Right;
				holding = true;
				dist = Vector3.Distance (localLines [11], localLines[23]);
				dist *= Vector3.Dot((localLines [23] - localLines [11]).normalized, localLines[17]);
				if (transform.localScale.x < 0) {
					dist *= -1;
				}
			}

			localLines[24] = Handles.Slider (localLines [24], localLines[18]);

			if (localLines[24] != localLines [12]) {
				currentSide = ExtensionMethods.BoundSides.Top;
				holding = true;
				dist = Vector3.Distance (localLines [12], localLines[24]);
				dist *= Vector3.Dot((localLines [24] - localLines [12]).normalized, localLines[18]);
				if (transform.localScale.y < 0) {
					dist *= -1;
				}
			}

			localLines[25] = Handles.Slider (localLines [25], localLines[19]);

			if (localLines[25] != localLines [13]) {
				currentSide = ExtensionMethods.BoundSides.Bottom;
				holding = true;
				dist = Vector3.Distance (localLines [13], localLines[25]);
				dist *= Vector3.Dot((localLines [25] - localLines [13]).normalized, localLines[19]);
				if (transform.localScale.y < 0) {
					dist *= -1;
				}
			}


		}
		Handles.color = g;

	}

}
