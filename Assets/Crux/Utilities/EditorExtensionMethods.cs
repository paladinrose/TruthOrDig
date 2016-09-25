using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Runtime.CompilerServices;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
public static class EditorExtensionMethods{
	#if UNITY_EDITOR
	public static System.Object GetPropertyFromFullPath(this SerializedProperty p, FieldInfo f)
	{
		MemberInfo pathMember;
		//Debug.Log (p.propertyPath);
		System.String[] path = p.ConvertedPath();
		System.String workString;
		System.Object currentObject =p.serializedObject.targetObject;
		System.Type currentType = currentObject.GetType ();
		//Debug.Log (currentObject.ToString ());
		IEnumerable arrayHolder;
		IEnumerator arraySifter;
		int workInt;
		for (int i = 0; i < path.Length; i++) {
			if (path [i].EndsWith ("]")) {
				int fieldArrayID = System.Convert.ToInt32 (path [i].Substring (path [i].LastIndexOf ("[" [0])).Replace ("[", "").Replace ("]", ""));

				workString = path [i].Substring (0, path [i].LastIndexOf ("["));
				pathMember = currentType.GetMember (workString) [0];
				if (pathMember != null) {
					if (fieldArrayID >= 0) {
						arrayHolder = null;
						switch (pathMember.MemberType) {
						case MemberTypes.Field:
							arrayHolder = ((FieldInfo)pathMember).GetValue (currentObject) as IEnumerable;
							break;
						case MemberTypes.Property:
							arrayHolder = ((PropertyInfo)pathMember).GetValue (currentObject, null) as IEnumerable;
							break;
						}
						if (arrayHolder != null) { 
							arraySifter = arrayHolder.GetEnumerator ();
							workInt = 0;
							while (workInt <= fieldArrayID) {
								arraySifter.MoveNext ();
								workInt++;
							}
							currentObject = arraySifter.Current;
							if (currentObject != null) {
								currentType = currentObject.GetType ();
							} else {
								currentType = null;
							}
						} else {
							currentType = null;
						}
						//testString = currentType.ToString () + fieldArrayID.ToString ();
						pathMember = null;
					} else {
						switch (pathMember.MemberType) {
						case MemberTypes.Field:
							currentObject = ((FieldInfo)pathMember).GetValue (currentObject);
							if (currentObject != null) {
								currentType = currentObject.GetType ();
							} else {
								currentType = null;
							}
							break;
						case MemberTypes.Property:
							currentObject = ((PropertyInfo)pathMember).GetValue (currentObject, null);
							if (currentObject != null) {
								currentType = currentObject.GetType ();
							} else {
								currentType = null;
							}
							break;
						}
					}
				} else {
					currentType = null; 
					break;
				}
			} else {

				pathMember = currentType.GetMember (path [i]) [0];
				if (pathMember != null) {
					switch (pathMember.MemberType) {
					case MemberTypes.Field:
						currentObject = ((FieldInfo)pathMember).GetValue (currentObject);
						if (currentObject != null) {
							currentType = currentObject.GetType ();
						} else {
							currentType = null;
						}
						break;
					case MemberTypes.Property:
						currentObject = ((PropertyInfo)pathMember).GetValue (currentObject, null);
						if (currentObject != null) {
							currentType = currentObject.GetType ();
						} else {
							currentType = null;
						}
						break;
					}
					pathMember = null;
				} else {
					currentType = null;
				}
			}
		}
		//Debug.Log(currentType.ToString());
		return currentObject;
	}


	public static System.String[] ConvertedPath(this SerializedProperty p){
		System.String[] path = p.propertyPath.Split ("."[0]);
		List<System.String> newPath = new List<System.String> ();
		for(int i = 0; i < path.Length; i++){
			if (i < path.Length - 2 && path [i + 1] == "Array") {
				newPath.Add (path [i] + path [i + 2].Substring (path [i + 2].IndexOf ("[" [0])));
				i += 2;
			} else {
				if (path [i] != "Array" && !path [i].StartsWith ("data[")) {
					newPath.Add (path [i]);
				}
			}
		}

		return newPath.ToArray ();
	}

	public static void SetPropertyAtFullPath(this SerializedProperty p, FieldInfo f, System.Object value)
	{
		MemberInfo pathMember;
		//Debug.Log (p.propertyPath);
		System.String[] path = p.ConvertedPath();
		for (int i = 0; i < path.Length; i++) {
			Debug.Log (path [i]);
		}
		System.String workString;
		System.Object currentObject =p.serializedObject.targetObject;
		System.Type currentType = currentObject.GetType ();
		Debug.Log (currentObject.ToString ());
		IEnumerable arrayHolder;
		IEnumerator arraySifter;
		int workInt;
		for (int i = 0; i < path.Length; i++) {
			if (path [i].EndsWith ("]")) {
				int fieldArrayID = System.Convert.ToInt32 (path [i].Substring (path [i].LastIndexOf ("[" [0])).Replace ("[", "").Replace ("]", ""));

				workString = path [i].Substring (0, path [i].LastIndexOf ("["));
				pathMember = currentType.GetMember (workString) [0];
				if (pathMember != null) {
					if (fieldArrayID >= 0) {
						arrayHolder = null;
						switch (pathMember.MemberType) {
						case MemberTypes.Field:
							arrayHolder = ((FieldInfo)pathMember).GetValue (currentObject) as IEnumerable;
							break;
						case MemberTypes.Property:
							arrayHolder = ((PropertyInfo)pathMember).GetValue (currentObject, null) as IEnumerable;
							break;
						}
						if (arrayHolder != null) { 
							arraySifter = arrayHolder.GetEnumerator ();
							workInt = 0;
							while (workInt <= fieldArrayID) {
								arraySifter.MoveNext ();
								workInt++;
							}
							currentObject = arraySifter.Current;
							if (currentObject != null) {
								currentType = currentObject.GetType ();
							} else {
								currentType = null;
							}
						} else {
							currentType = null;
						}
						//testString = currentType.ToString () + fieldArrayID.ToString ();
						pathMember = null;
					} else {
						switch (pathMember.MemberType) {
						case MemberTypes.Field:
							currentObject = ((FieldInfo)pathMember).GetValue (currentObject);
							if (currentObject != null) {
								currentType = currentObject.GetType ();
							} else {
								currentType = null;
							}
							break;
						case MemberTypes.Property:
							currentObject = ((PropertyInfo)pathMember).GetValue (currentObject, null);
							if (currentObject != null) {
								currentType = currentObject.GetType ();
							} else {
								currentType = null;
							}
							break;
						}
					}
				} else {
					currentType = null; 
					break;
				}
			} else {
				pathMember = currentType.GetMember (path [i]) [0];
				if (pathMember != null) {
					switch (pathMember.MemberType) {
					case MemberTypes.Field:
						currentObject = ((FieldInfo)pathMember).GetValue (currentObject);
						if (currentObject != null) {
							currentType = currentObject.GetType ();
						} else {
							currentType = null;
						}
						break;
					case MemberTypes.Property:
						currentObject = ((PropertyInfo)pathMember).GetValue (currentObject, null);
						if (currentObject != null) {
							currentType = currentObject.GetType ();
						} else {
							currentType = null;
						}
						break;
					}
					pathMember = null;
				} else {
					currentType = null;
				}
			}
		}

		int pl = path.Length - 1;

		Debug.Log (path [pl] + " " + currentType.ToString ());
		if (path [pl].EndsWith ("]")) {
			//Get the core things as a collection and 
			int fieldArrayID = System.Convert.ToInt32 (path [pl].Substring (path [pl].LastIndexOf ("[" [0])).Replace ("[", "").Replace ("]", ""));

			workString = path [pl].Substring (0, path [pl].LastIndexOf ("["));
			Debug.Log (workString);
			pathMember = currentType.GetMember (workString) [0];
			IList col;
			if (pathMember != null) {
				if (fieldArrayID >= 0) {
					col = null;
					switch (pathMember.MemberType) {
					case MemberTypes.Field:
						col = ((FieldInfo)pathMember).GetValue (currentObject) as IList;
						if (col != null) { 
							if (col [fieldArrayID].GetType () == value.GetType ()) {
								col [fieldArrayID] = value;

							}
						}
						((FieldInfo)pathMember).SetValue (currentObject, col);
						break;
					case MemberTypes.Property:
						col = ((PropertyInfo)pathMember).GetValue (currentObject, null) as IList;
						if (col != null) { 
							if (col [fieldArrayID].GetType () == value.GetType ()) {
								col [fieldArrayID] = value;

							}
						}
						((PropertyInfo)pathMember).SetValue (currentObject, col, null);
						break;
					}

				}
			}
		} else {
			currentType.GetField (path [pl]).SetValue (currentObject, value);
		}
	}
	#endif
}
