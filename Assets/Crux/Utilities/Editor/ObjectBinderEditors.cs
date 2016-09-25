using UnityEngine;
using System;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

[CustomPropertyDrawer (typeof (ObjectBinder))]
public class ObjectBinderEditor : PropertyDrawer {

	ObjectBinder ob;


	string[] tabLabels = new string[]{"Targets", "Bindings"};

	public override float GetPropertyHeight (SerializedProperty prop, GUIContent label) {
		ob = (ObjectBinder)prop.GetPropertyFromFullPath(fieldInfo);

		ob.lineHeight = base.GetPropertyHeight (prop, label)*1.125f;
		if (ob.totalLineHeight == 0) {
			ob.totalLineHeight = ob.lineHeight * 2;
		}
		if (!ob.show) {
			return ob.lineHeight*2;
		}

		return ob.totalLineHeight;

	}


	public override void OnGUI (Rect pos, SerializedProperty prop, GUIContent label) {

		if (ob.primaryC != GUI.backgroundColor) {
			SetColors ();
		}

		EditorGUI.BeginProperty (pos, label, prop);

		int indent = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;
		Color b = GUI.backgroundColor;
		GUI.backgroundColor = ob.tirtiaryC;
		GUI.Box (pos, "");
		GUI.backgroundColor = b;
		ob.drawBox = new Rect (pos.x, pos.y, pos.width, ob.lineHeight);
		ob.drawBox.y += 2;
		SerializedProperty nam = prop.FindPropertyRelative ("name");
		nam.stringValue = EditorGUI.TextField (ob.drawBox, "Binder", nam.stringValue);
		ob.drawBox.width *= 0.5f;
		ob.drawBox.y += ob.lineHeight;

		SerializedProperty active = prop.FindPropertyRelative ("active");
		if (!ob.show) {
			ob.show  = EditorGUI.ToggleLeft (ob.drawBox, "Show", ob.show);
		} else {
			ob.show = EditorGUI.ToggleLeft (ob.drawBox, "Hide", ob.show);
		}
		ob.drawBox.x += ob.drawBox.width;
		active.boolValue = EditorGUI.Toggle (ob.drawBox, "Active", active.boolValue);

		if (ob.show) {
			int i, maxListSize;
			bool genericToggle = false;
			string workString = "";
			int workInt;

			ob.drawBox.y += ob.lineHeight;
			ob.drawBox.x = pos.x;
			ob.drawBox.width = pos.width;

			SerializedProperty conSuc = prop.FindPropertyRelative ("ConnectionSuccessCondition");
			if (conSuc.enumValueIndex == 1 || conSuc.enumValueIndex == 3 || conSuc.enumValueIndex == 4) {
				ob.drawBox.width *= 0.75f;
				EditorGUI.PropertyField (ob.drawBox, conSuc);
				ob.drawBox.x += ob.drawBox.width;
				ob.drawBox.width = pos.width * 0.25f;
				SerializedProperty conID = prop.FindPropertyRelative ("cscID");
				EditorGUI.PropertyField (ob.drawBox, conID, GUIContent.none);
				ob.drawBox.x = pos.x;
				ob.drawBox.width = pos.width;
			} else {
				EditorGUI.PropertyField (ob.drawBox, conSuc);
			}
			ob.drawBox.y += ob.lineHeight;

			SerializedProperty binSuc = prop.FindPropertyRelative ("BindAllSuccessCondition");
			if (binSuc.enumValueIndex == 1 || binSuc.enumValueIndex == 3 || binSuc.enumValueIndex == 4) {
				ob.drawBox.width *= 0.75f;
				EditorGUI.PropertyField (ob.drawBox, binSuc);
				ob.drawBox.x += ob.drawBox.width;
				ob.drawBox.width = pos.width * 0.25f;
				SerializedProperty binID = prop.FindPropertyRelative ("bascID");
				EditorGUI.PropertyField (ob.drawBox, binID, GUIContent.none);
				ob.drawBox.x = pos.x;
				ob.drawBox.width = pos.width;
			} else {
				EditorGUI.PropertyField (ob.drawBox, binSuc);
			}
			ob.drawBox.y += ob.lineHeight;

			ob.drawBox.width = pos.width * 0.5f;

			for (i = 0; i < tabLabels.Length; i++) {
				if (ob.currentTab != i) {
					GUI.backgroundColor = ob.secondaryC;
					if (GUI.Button (ob.drawBox, tabLabels [i])) {
						ob.targetListID = 0;
						ob.currentTab = i;
						SetStringLists (prop);
					}
					GUI.backgroundColor = ob.primaryC;
				} else {
					EditorGUI.LabelField (ob.drawBox, tabLabels [i], GUI.skin.box);
				}
				if (!genericToggle) {
					genericToggle = true;
					ob.drawBox.x += ob.drawBox.width;
				} else {
					ob.drawBox.x = pos.x;
					ob.drawBox.y += ob.lineHeight;
					genericToggle = false;
				}
			}
			if (!genericToggle) {
				ob.drawBox.y += ob.lineHeight * 0.5f;
				ob.drawBox.x = pos.x;
			} 
				
			genericToggle = false;

			ob.drawBox.x = pos.x;
			ob.drawBox.width = pos.width;

			switch (ob.currentTab) {

			case 0:
				//targets
				EditorGUI.BeginChangeCheck ();
				SerializedProperty targs = prop.FindPropertyRelative ("baseTargets");
				SerializedProperty paths = prop.FindPropertyRelative ("memberPaths");
				SerializedProperty labs = prop.FindPropertyRelative ("baseTargetLabels");
				SerializedProperty sucs = prop.FindPropertyRelative ("connectionSuccesses");
				SerializedProperty refs = prop.FindPropertyRelative ("refreshTarget");

				FieldInfo[] fieldI;
				PropertyInfo[] propI;
				MethodBase[] methI;
				string[] currentPath;
				IEnumerable arrayHolder;
				IEnumerator arraySifter;
				MemberInfo pathMember;

				float lineWidthCounter;
				float prd = GUI.skin.label.CalcSize (new GUIContent (".")).x;

				GUI.Box (ob.drawBox, "Targets");
				ob.drawBox.y += ob.lineHeight;


				ob.primaryScrollBar = new Rect (pos.x + ob.drawBox.width * 0.95f, ob.drawBox.y, ob.drawBox.width * 0.1f, 0);
				maxListSize = targs.arraySize;
				if (targs.arraySize != labs.arraySize || paths.arraySize != targs.arraySize || sucs.arraySize != targs.arraySize || refs.arraySize != targs.arraySize) {
					labs.arraySize = paths.arraySize = sucs.arraySize = refs.arraySize = targs.arraySize;
				}
				if (ob.currentTarget > labs.arraySize) {
					ob.currentTarget = labs.arraySize - 1;
				}
				if (ob.targetListID > targs.arraySize) {
					ob.targetListID = targs.arraySize - 1;
				}
				if (maxListSize > 10) {
					maxListSize = 10;
				} else {
					ob.targetListID = 0;
				}

				//Debug.Log (targs.arraySize.ToString () + "," + labs.arraySize.ToString () + "," + paths.arraySize.ToString ());
				for (i = 0; i < maxListSize; i++) {
					int r = ob.targetListID + i;

					GUI.backgroundColor = new Color(1, 0.5f, 0.5f, 1);
					if (GUI.Button (new Rect (ob.drawBox.x + ob.drawBox.width * 0.05f, ob.drawBox.y, ob.lineHeight, ob.lineHeight), "X")) {
						if (r <= ob.currentTarget) {
							ob.currentTarget--;
						} 
						targs.DeleteArrayElementAtIndex (r);
						labs.DeleteArrayElementAtIndex (r);
						paths.DeleteArrayElementAtIndex (r);
						sucs.DeleteArrayElementAtIndex (r);
						refs.DeleteArrayElementAtIndex (r);
						SetStringLists (prop, true);

						EditorGUI.indentLevel = indent;
						GUI.backgroundColor = b;
						EditorGUI.EndChangeCheck ();
						EditorGUI.EndProperty ();
						return;
					}
					if (genericToggle) {
						GUI.backgroundColor = ob.secondaryC;
						genericToggle = false;
					} else {
						GUI.backgroundColor = ob.primaryC;
						genericToggle = true;
					}

					if (r != ob.currentTarget) {
						if (GUI.Button (new Rect (ob.drawBox.x + ob.lineHeight + ob.drawBox.width * 0.05f, ob.drawBox.y, ob.drawBox.width * 0.9f - ob.lineHeight * 2, ob.lineHeight), labs.GetArrayElementAtIndex (r).stringValue)) {
							ob.currentTarget = r;
						}
					} else {
						EditorGUI.ObjectField (new Rect (ob.drawBox.x + ob.lineHeight + ob.drawBox.width * 0.05f, ob.drawBox.y, ob.drawBox.width * 0.9f - ob.lineHeight * 2, ob.lineHeight), 
							targs.GetArrayElementAtIndex (r), 
							new GUIContent (labs.GetArrayElementAtIndex (r).stringValue));
					}
					SerializedProperty connected = sucs.GetArrayElementAtIndex (r);
					if (connected.boolValue) {
						GUI.backgroundColor = new Color (0.5f, 1, 0.5f, 1);
					} else {
						GUI.backgroundColor = new Color (1, 0.5f, 0.5f, 1);
					}
					GUI.Toggle (new Rect (ob.drawBox.x + ob.drawBox.width - (ob.drawBox.width *0.05f) - ob.lineHeight, ob.drawBox.y, ob.lineHeight, ob.lineHeight), connected.boolValue, GUIContent.none);
					GUI.backgroundColor = ob.primaryC;
					ob.drawBox.y += ob.lineHeight;
				}
				if (targs.arraySize > 10) {
					ob.primaryScrollBar.height = ob.drawBox.y - ob.primaryScrollBar.y + ob.lineHeight;
					ob.targetListID = Mathf.RoundToInt (GUI.VerticalSlider (ob.primaryScrollBar, ob.targetListID, 0, targs.arraySize - 10, GUI.skin.verticalScrollbar, GUI.skin.verticalScrollbarThumb));
				}

				GUI.backgroundColor = ob.primaryC;
				if (GUI.Button (new Rect (ob.drawBox.x + ob.drawBox.width * 0.05f, ob.drawBox.y, ob.drawBox.width * 0.9f, ob.lineHeight), "Add Target")) {
					i = targs.arraySize + 1;
					targs.arraySize = i;
					labs.arraySize = i;
					paths.arraySize = i;
					refs.arraySize = i;
					sucs.arraySize = i;
				}
				ob.drawBox.y += ob.lineHeight;

				//Now we do our current target stuff.
				if (ob.currentTarget < targs.arraySize) {
					ob.drawBox.y += ob.lineHeight * 0.5f;
					GUI.Box (ob.drawBox, "Current Target");
					ob.drawBox.y += ob.lineHeight;
					EditorGUI.PropertyField (ob.drawBox, labs.GetArrayElementAtIndex (ob.currentTarget), new GUIContent ("Name"));
					ob.drawBox.y += ob.lineHeight;
					//We should get our target from base targets, here. If it's null, we don't want to proceed.
					SerializedProperty cT = targs.GetArrayElementAtIndex (ob.currentTarget);
					if (cT.objectReferenceValue == null) {
						//Display a label and return.
						GUI.Label (ob.drawBox, "No Target Object Set.");
						ob.totalLineHeight = ob.drawBox.y + ob.lineHeight - pos.y;
						EditorGUI.indentLevel = indent;
						GUI.backgroundColor = b;
						EditorGUI.EndChangeCheck ();
						EditorGUI.EndProperty ();
						return;
					}

					SerializedProperty refThis = refs.GetArrayElementAtIndex (ob.currentTarget);
					refThis.boolValue = GUI.Toggle (ob.drawBox, refThis.boolValue, "Refresh Target");
					ob.drawBox.y += ob.lineHeight;

					SerializedProperty pathProp = paths.GetArrayElementAtIndex (ob.currentTarget);
					workString = pathProp.stringValue;
					currentPath = workString.Split ("." [0]);

					System.Type pathWalker = cT.objectReferenceValue.GetType ();
					System.Object currentObject = cT.objectReferenceValue;

					lineWidthCounter = GUI.skin.box.CalcSize (new GUIContent ("Path: ")).x;

					ob.drawBox.width = lineWidthCounter;
					GUI.Box (ob.drawBox, "Path: ");
					ob.drawBox.x += lineWidthCounter;
					if (currentPath.Length > 1) {
						for (int sw = 0; sw < currentPath.Length - 1; sw++) {
							//Get the width of this path segment.
							float segWidth = GUI.skin.box.CalcSize (new GUIContent (currentPath [sw])).x;

							//If adding this segment and a period would take too much space...
							if ((segWidth + lineWidthCounter + prd) > pos.width) {
								//Move down to the next line.
								ob.drawBox.y += ob.lineHeight;
								ob.drawBox.x = pos.x;
								//If our segment width and a period are too much for a line, we abbreviate.
								if ((segWidth + prd) > pos.width) {
									ob.drawBox.width = pos.width - prd;
									lineWidthCounter = pos.width - prd;
								} else {
									ob.drawBox.width = segWidth;
									lineWidthCounter = segWidth;
								}
							} else {
								ob.drawBox.width = segWidth;
								lineWidthCounter += segWidth;
							}
							if (GUI.Button (ob.drawBox, currentPath [sw], GUI.skin.box)) {
								workString = "";
								for (int cp = 0; cp <= sw; cp++) {
									workString += currentPath [cp];
									if (cp < sw) {
										workString += ".";
									}
								}
								pathProp.stringValue = workString;
							}
							ob.drawBox.x += ob.drawBox.width;
							ob.drawBox.width = prd; 

							GUI.Label (ob.drawBox, ".");
							lineWidthCounter += prd;
							ob.drawBox.x += prd;
							if (currentPath [sw].EndsWith ("]")) {
								int fieldArrayID = Convert.ToInt32 (currentPath [sw].Substring (currentPath [sw].LastIndexOf ("[" [0])).Replace ("[", "").Replace ("]", ""));

								workString = currentPath [sw].Substring (0, currentPath [sw].LastIndexOf ("["));
								//testString = workString + fieldArrayID.ToString ();
								pathMember = pathWalker.GetMember (workString) [0];
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
												pathWalker = currentObject.GetType ();
											} else {
												pathWalker = null;
											}
										} else {
											pathWalker = null;
										}
										//testString = pathWalker.ToString () + fieldArrayID.ToString ();
										pathMember = null;
									} else {
										switch (pathMember.MemberType) {
										case MemberTypes.Field:
											currentObject = ((FieldInfo)pathMember).GetValue (currentObject);
											if (currentObject != null) {
												pathWalker = currentObject.GetType ();
											} else {
												pathWalker = null;
											}
											break;
										case MemberTypes.Property:
											currentObject = ((PropertyInfo)pathMember).GetValue (currentObject, null);
											if (currentObject != null) {
												pathWalker = currentObject.GetType ();
											} else {
												pathWalker = null;
											}
											break;
										}
									}
								} else {
									pathWalker = null; 
									break;
								}
							} else {

								pathMember = pathWalker.GetMember (currentPath [sw]) [0];
								if (pathMember != null) {
									switch (pathMember.MemberType) {
									case MemberTypes.Field:
										currentObject = ((FieldInfo)pathMember).GetValue (currentObject);
										if (currentObject != null) {
											pathWalker = currentObject.GetType ();
										} else {
											pathWalker = null;
										}
										break;
									case MemberTypes.Property:
										currentObject = ((PropertyInfo)pathMember).GetValue (currentObject, null);
										if (currentObject != null) {
											pathWalker = currentObject.GetType ();
										} else {
											pathWalker = null;
										}
										break;
									}
									pathMember = null;
								} else {
									pathWalker = null;
								}
							}
							if (pathWalker == null) {
								GUI.backgroundColor = Color.red;
								EditorGUI.LabelField (ob.drawBox, "X_X");
								GUI.backgroundColor = b;
								EditorGUI.indentLevel = indent;
								EditorGUI.EndChangeCheck ();
								EditorGUI.EndProperty ();
								ob.totalLineHeight = ob.drawBox.y + ob.lineHeight - pos.y;
								return;
							}
						}
						ob.drawBox.width = GUI.skin.box.CalcSize (new GUIContent (currentPath [currentPath.Length - 1])).x;
						GUI.Label (ob.drawBox, currentPath [currentPath.Length - 1], GUI.skin.box); 
					} else if (currentPath.Length == 1) {
						ob.drawBox.width = GUI.skin.box.CalcSize (new GUIContent (currentPath [0])).x;
						GUI.Label (ob.drawBox, currentPath [0], GUI.skin.box); 
					}

					ob.drawBox.y += ob.lineHeight;
					ob.drawBox.x = pos.x;
					ob.drawBox.width = pos.width * 0.33f;
					string crPath;
					switch (ob.showTargetType) {

					case 0:
						GUI.backgroundColor = ob.primaryC;
						GUI.Box (ob.drawBox, "Fields"); 
						ob.drawBox.x += ob.drawBox.width;
						GUI.backgroundColor = ob.secondaryC;
						if (GUI.Button (ob.drawBox, "Properties")) {
							ob.showTargetType = 1;
							ob.secondaryListID = 0;
						}
						ob.drawBox.x += ob.drawBox.width;
						if (GUI.Button (ob.drawBox, "Methods")) {
							ob.showTargetType = 2;
							ob.secondaryListID = 0;
						}
						GUI.backgroundColor = ob.primaryC;
						ob.drawBox.x = pos.x;
						ob.drawBox.width = pos.width;
						ob.drawBox.y += ob.lineHeight;

						ob.fieldInfos = pathWalker.GetFields (BindingFlags.Public | BindingFlags.Instance);

						genericToggle = true;
						ob.secondaryScrollBar = new Rect (pos.x + ob.drawBox.width * 0.95f, ob.drawBox.y, pos.width * 0.1f, 0);
						maxListSize = ob.fieldInfos.Length;
						if (maxListSize > 10) {
							maxListSize = 10;
						} else {
							ob.secondaryListID = 0;
						}
						for (i = 0; i < maxListSize; i++) {
							
							int currentField = ob.secondaryListID + i;
							workString = ob.fieldInfos [currentField].FieldType.ToString ();

							if (genericToggle) {
								GUI.backgroundColor = ob.primaryC;
								genericToggle = false;
							} else {
								GUI.backgroundColor = ob.secondaryC; 
								genericToggle = true;
							}

							if (workString.EndsWith ("]")) {
								workString = workString.Substring (0, workString.Length - 2);
								//Get the array and make a button for every array item in it that I can select to move into that array thinger.
								//Debug.Log(workString);
								arrayHolder = ob.fieldInfos [currentField].GetValue (currentObject) as IEnumerable;
								arraySifter = arrayHolder.GetEnumerator ();
								int counter = 0;

								if (GUI.Button (new Rect (ob.drawBox.x + ob.drawBox.width * 0.05f, ob.drawBox.y, ob.drawBox.width * 0.9f - ob.lineHeight, ob.lineHeight), ob.fieldInfos [currentField].Name + " (" + workString + ")")) {
									if (currentPath.Length > 1) {
										currentPath [currentPath.Length - 1] = ob.fieldInfos [currentField].Name + "[]";
										crPath = "";
										for (int cp = 0; cp < currentPath.Length; cp++) {
											crPath += currentPath [cp];
											if (cp < currentPath.Length - 1) {
												crPath += ".";
											}
										}
										pathProp.stringValue = crPath;
									} else {
										pathProp.stringValue = ob.fieldInfos [currentField].Name + "[]";
									}
								}

								if (GUI.Button (new Rect (ob.drawBox.x + ob.drawBox.width * 0.95f - ob.lineHeight, ob.drawBox.y, ob.lineHeight, ob.lineHeight), "+")) {
									currentPath [currentPath.Length - 1] = ob.fieldInfos [currentField].Name + "[]";
									crPath = "";	
									if (currentPath.Length > 1) {
										for (int cp = 0; cp < currentPath.Length; cp++) {
											crPath += currentPath [cp] + ".";
										}

									} else {
										crPath = ob.fieldInfos [currentField].Name + "[].";
									}

									pathWalker = arrayHolder.GetType ();
									fieldI = pathWalker.GetFields (BindingFlags.Instance | BindingFlags.Public);
									if (fieldI.Length > 0) {
										crPath += fieldI [0].Name;
									} else {
										propI = pathWalker.GetProperties (BindingFlags.Instance | BindingFlags.Public);
										if (propI.Length > 0) {
											crPath += propI [0].Name;
										} else {
											methI = pathWalker.GetMethods (BindingFlags.Instance | BindingFlags.Public);
											if (methI.Length > 0) {
												crPath += methI [0].Name;
											}
										}
									}
									pathProp.stringValue = crPath;
								}
								ob.drawBox.y += ob.lineHeight;

								while (arraySifter.MoveNext ()) {
									if (genericToggle) {
										GUI.backgroundColor = ob.primaryC;
										genericToggle = false;
									} else {
										GUI.backgroundColor = ob.secondaryC; 
										genericToggle = true;
									}
									if (GUI.Button (new Rect (ob.drawBox.x + ob.drawBox.width * 0.05f, ob.drawBox.y, ob.drawBox.width * 0.9f - ob.lineHeight, ob.lineHeight),
										    ob.fieldInfos [currentField].Name + " (" + workString.Substring (0, workString.Length - 2) + "[" + counter.ToString () + "]" + ")")) {
										if (currentPath.Length > 1) {
											currentPath [currentPath.Length - 1] = ob.fieldInfos [currentField].Name + "[" + counter.ToString () + "]";
											crPath = "";
											for (int cp = 0; cp < currentPath.Length; cp++) {
												crPath += currentPath [cp];
												if (cp < currentPath.Length - 1) {
													crPath += ".";
												}
											}
											pathProp.stringValue = crPath;
										} else {
											pathProp.stringValue = ob.fieldInfos [currentField].Name + "[" + counter.ToString () + "]";
										}
									}

									if (GUI.Button (new Rect (ob.drawBox.x + ob.drawBox.width * 0.95f - ob.lineHeight, ob.drawBox.y, ob.lineHeight, ob.lineHeight), "+")) {
										currentPath [currentPath.Length - 1] = ob.fieldInfos [currentField].Name + "[" + counter.ToString () + "]";
										crPath = "";	
										if (currentPath.Length > 1) {
											for (int cp = 0; cp < currentPath.Length; cp++) {
												crPath += currentPath [cp] + ".";
											}

										} else {
											crPath = ob.fieldInfos [currentField].Name + "[" + counter.ToString () + "].";
										}

										pathWalker = arrayHolder.GetType ();
										fieldI = pathWalker.GetFields (BindingFlags.Instance | BindingFlags.Public);
										if (fieldI.Length > 0) {
											crPath += fieldI [0].Name;
										} else {
											propI = pathWalker.GetProperties (BindingFlags.Instance | BindingFlags.Public);
											if (propI.Length > 0) {
												crPath += propI [0].Name;
											} else {
												methI = pathWalker.GetMethods (BindingFlags.Instance | BindingFlags.Public);
												if (methI.Length > 0) {
													crPath += methI [0].Name;
												}
											}
										}
										pathProp.stringValue = crPath;
									}
									ob.drawBox.y += ob.lineHeight;
									counter++;
								}
							} else {
								
								if (GUI.Button (new Rect (ob.drawBox.x + ob.drawBox.width * 0.05f, ob.drawBox.y, ob.drawBox.width * 0.9f - ob.lineHeight - 2, ob.lineHeight), ob.fieldInfos [currentField].Name + " (" + workString + ")")) {
									if (currentPath.Length > 1) {
										currentPath [currentPath.Length - 1] = ob.fieldInfos [currentField].Name;
										crPath = "";
										for (int cp = 0; cp < currentPath.Length; cp++) {
											crPath += currentPath [cp];
											if (cp < currentPath.Length - 1) {
												crPath += ".";
											}
										}
										pathProp.stringValue = crPath;
									} else {
										pathProp.stringValue = ob.fieldInfos [currentField].Name;
									}
								}
								if (GUI.Button (new Rect (ob.drawBox.x + ob.drawBox.width * 0.95f - ob.lineHeight, ob.drawBox.y, ob.lineHeight, ob.lineHeight), "+")) {
									currentPath [currentPath.Length - 1] = ob.fieldInfos [currentField].Name;
									crPath = "";	
									if (currentPath.Length > 1) {
										for (int cp = 0; cp < currentPath.Length; cp++) {
											crPath += currentPath [cp] + ".";
										}

									} else {
										crPath = ob.fieldInfos [currentField].Name + ".";
									}
									pathWalker = currentObject.GetType ();
									fieldI = pathWalker.GetFields (BindingFlags.Instance | BindingFlags.Public);
									if (fieldI.Length > 0) {
										crPath += fieldI [0].Name;
									} else {
										propI = pathWalker.GetProperties (BindingFlags.Instance | BindingFlags.Public);
										if (propI.Length > 0) {
											crPath += propI [0].Name;
										} else {
											methI = pathWalker.GetMethods (BindingFlags.Instance | BindingFlags.Public);
											if (methI.Length > 0) {
												crPath += methI [0].Name;
											}
										}
									}
									pathProp.stringValue = crPath;
								}
								ob.drawBox.y += ob.lineHeight;
							}
						}
						if (ob.fieldInfos.Length > 10) {
							ob.secondaryScrollBar.height = ob.drawBox.y - ob.secondaryScrollBar.y;
							ob.secondaryListID = Mathf.RoundToInt (GUI.VerticalSlider (ob.secondaryScrollBar, ob.secondaryListID, 0, ob.fieldInfos.Length - 10, GUI.skin.verticalScrollbar, GUI.skin.verticalScrollbarThumb));
						}

						break;

					case 1:
						GUI.backgroundColor = ob.secondaryC;
						if (GUI.Button (ob.drawBox, "Fields")) {
							ob.showTargetType = 0;
							ob.secondaryListID = 0;
						}
						ob.drawBox.x += ob.drawBox.width;
						GUI.backgroundColor = ob.primaryC;
						GUI.Box (ob.drawBox, "Properties"); 
						ob.drawBox.x += ob.drawBox.width;
						GUI.backgroundColor = ob.secondaryC;
						if (GUI.Button (ob.drawBox, "Methods")) {
							ob.showTargetType = 2;
							ob.secondaryListID = 0;
						}
						GUI.backgroundColor = ob.primaryC;
						ob.drawBox.x = pos.x;
						ob.drawBox.width = pos.width;
						ob.drawBox.y += ob.lineHeight;

						ob.propertyInfos = pathWalker.GetProperties (BindingFlags.Public | BindingFlags.Instance);

						genericToggle = true;
						ob.secondaryScrollBar = new Rect (pos.x + ob.drawBox.width * 0.95f, ob.drawBox.y, pos.width * 0.1f, 0);
						maxListSize = ob.propertyInfos.Length;
						if (maxListSize > 10) {
							maxListSize = 10;
						} else {
							ob.secondaryListID = 0;
						}
						for (i = 0; i < maxListSize; i++) {

							int currentProperty = ob.secondaryListID + i;
							workString = ob.propertyInfos [currentProperty].PropertyType.ToString ();

							if (genericToggle) {
								GUI.backgroundColor = ob.primaryC;
								genericToggle = false;
							} else {
								GUI.backgroundColor = ob.secondaryC; 
								genericToggle = true;
							}

							if (workString.EndsWith ("]")) {
								workString = workString.Substring (0, workString.Length - 2);
								//Get the array and make a button for every array item in it that I can select to move into that array thinger.
								arrayHolder = ob.propertyInfos [currentProperty].GetValue (currentObject, null) as IEnumerable;
								arraySifter = arrayHolder.GetEnumerator ();
								int counter = 0;

								if (GUI.Button (new Rect (ob.drawBox.x + ob.drawBox.width * 0.05f, ob.drawBox.y, ob.drawBox.width * 0.9f - ob.lineHeight, ob.lineHeight), ob.propertyInfos [currentProperty].Name + " (" + workString + ")")) {
									if (currentPath.Length > 1) {
										currentPath [currentPath.Length - 1] = ob.propertyInfos [currentProperty].Name + "[]";
										crPath = "";
										for (int cp = 0; cp < currentPath.Length; cp++) {
											crPath += currentPath [cp];
											if (cp < currentPath.Length - 1) {
												crPath += ".";
											}
										}
										pathProp.stringValue = crPath;
									} else {
										pathProp.stringValue = ob.propertyInfos [currentProperty].Name + "[]";
									}
								}

								if (GUI.Button (new Rect (ob.drawBox.x + ob.drawBox.width * 0.95f - ob.lineHeight, ob.drawBox.y, ob.lineHeight, ob.lineHeight), "+")) {
									currentPath [currentPath.Length - 1] = ob.propertyInfos [currentProperty].Name + "[]";
									crPath = "";	
									if (currentPath.Length > 1) {
										for (int cp = 0; cp < currentPath.Length; cp++) {
											crPath += currentPath [cp] + ".";
										}

									} else {
										crPath = ob.propertyInfos [currentProperty].Name + "[].";
									}

									pathWalker = arrayHolder.GetType ();
									fieldI = pathWalker.GetFields (BindingFlags.Instance | BindingFlags.Public);
									if (fieldI.Length > 0) {
										crPath += fieldI [0].Name;
									} else {
										propI = pathWalker.GetProperties (BindingFlags.Instance | BindingFlags.Public);
										if (propI.Length > 0) {
											crPath += propI [0].Name;
										} else {
											methI = pathWalker.GetMethods (BindingFlags.Instance | BindingFlags.Public);
											if (methI.Length > 0) {
												crPath += methI [0].Name;
											}
										}
									}
									pathProp.stringValue = crPath;
								}
								ob.drawBox.y += ob.lineHeight;

								while (arraySifter.MoveNext ()) {
									if (genericToggle) {
										GUI.backgroundColor = ob.primaryC;
										genericToggle = false;
									} else {
										GUI.backgroundColor = ob.secondaryC; 
										genericToggle = true;
									}
									if (GUI.Button (new Rect (ob.drawBox.x + ob.drawBox.width * 0.05f, ob.drawBox.y, ob.drawBox.width * 0.9f - ob.lineHeight, ob.lineHeight),
										    ob.propertyInfos [currentProperty].Name + " (" + workString.Substring (0, workString.Length - 2) + "[" + counter.ToString () + "]" + ")")) {
										if (currentPath.Length > 1) {
											currentPath [currentPath.Length - 1] = ob.propertyInfos [currentProperty].Name + "[" + counter.ToString () + "]";
											crPath = "";
											for (int cp = 0; cp < currentPath.Length; cp++) {
												crPath += currentPath [cp];
												if (cp < currentPath.Length - 1) {
													crPath += ".";
												}
											}
											pathProp.stringValue = crPath;
										} else {
											pathProp.stringValue = ob.propertyInfos [currentProperty].Name + "[" + counter.ToString () + "]";
										}
									}

									if (GUI.Button (new Rect (ob.drawBox.x + ob.drawBox.width * 0.95f - ob.lineHeight, ob.drawBox.y, ob.lineHeight, ob.lineHeight), "+")) {
										currentPath [currentPath.Length - 1] = ob.propertyInfos [currentProperty].Name + "[" + counter.ToString () + "]";
										crPath = "";	
										if (currentPath.Length > 1) {
											for (int cp = 0; cp < currentPath.Length; cp++) {
												crPath += currentPath [cp] + ".";
											}

										} else {
											crPath = ob.propertyInfos [currentProperty].Name + "[" + counter.ToString () + "].";
										}

										pathWalker = arrayHolder.GetType ();
										fieldI = pathWalker.GetFields (BindingFlags.Instance | BindingFlags.Public);
										if (fieldI.Length > 0) {
											crPath += fieldI [0].Name;
										} else {
											propI = pathWalker.GetProperties (BindingFlags.Instance | BindingFlags.Public);
											if (propI.Length > 0) {
												crPath += propI [0].Name;
											} else {
												methI = pathWalker.GetMethods (BindingFlags.Instance | BindingFlags.Public);
												if (methI.Length > 0) {
													crPath += methI [0].Name;
												}
											}
										}
										pathProp.stringValue = crPath;
									}
									ob.drawBox.y += ob.lineHeight;
									counter++;
								}
							} else {

								if (GUI.Button (new Rect (ob.drawBox.x + ob.drawBox.width * 0.05f, ob.drawBox.y, ob.drawBox.width * 0.9f - ob.lineHeight, ob.lineHeight),
									    ob.propertyInfos [currentProperty].Name + " (" + workString + ")")) {
									if (currentPath.Length > 1) {
										currentPath [currentPath.Length - 1] = ob.propertyInfos [currentProperty].Name;
										crPath = "";
										for (int cp = 0; cp < currentPath.Length; cp++) {
											crPath += currentPath [cp];
											if (cp < currentPath.Length - 1) {
												crPath += ".";
											}
										}
										pathProp.stringValue = crPath;
									} else {
										pathProp.stringValue = ob.propertyInfos [currentProperty].Name;
									}
								}
								if (GUI.Button (new Rect (ob.drawBox.x + ob.drawBox.width * 0.95f - ob.lineHeight, ob.drawBox.y, ob.lineHeight, ob.lineHeight), "+")) {
									currentPath [currentPath.Length - 1] = ob.propertyInfos [currentProperty].Name;
									crPath = "";	
									if (currentPath.Length > 1) {
										for (int cp = 0; cp < currentPath.Length; cp++) {
											crPath += currentPath [cp] + ".";
										}

									} else {
										crPath = ob.propertyInfos [currentProperty].Name + ".";
									}
									pathWalker = currentObject.GetType ();
									fieldI = pathWalker.GetFields (BindingFlags.Instance | BindingFlags.Public);
									if (fieldI.Length > 0) {
										crPath += fieldI [0].Name;
									} else {
										propI = pathWalker.GetProperties (BindingFlags.Instance | BindingFlags.Public);
										if (propI.Length > 0) {
											crPath += propI [0].Name;
										} else {
											methI = pathWalker.GetMethods (BindingFlags.Instance | BindingFlags.Public);
											if (methI.Length > 0) {
												crPath += methI [0].Name;
											}
										}
									}
									pathProp.stringValue = crPath;
								}
								ob.drawBox.y += ob.lineHeight;
							}
						}
						if (ob.propertyInfos.Length > 10) {
							ob.secondaryScrollBar.height = ob.drawBox.y - ob.secondaryScrollBar.y;
							ob.secondaryListID = Mathf.RoundToInt (GUI.VerticalSlider (ob.secondaryScrollBar, ob.secondaryListID, 0, ob.propertyInfos.Length - 10, GUI.skin.verticalScrollbar, GUI.skin.verticalScrollbarThumb));
						}


						break;

					case 2:
						GUI.backgroundColor = ob.secondaryC;
						if (GUI.Button (ob.drawBox, "Fields")) {
							ob.showTargetType = 0;
							ob.secondaryListID = 0;
						}
						ob.drawBox.x += ob.drawBox.width;
						if (GUI.Button (ob.drawBox, "Properties")) {
							ob.showTargetType = 1;
							ob.secondaryListID = 0;
						}
						ob.drawBox.x += ob.drawBox.width;
						GUI.backgroundColor = ob.primaryC;
						GUI.Box (ob.drawBox, "Methods"); 

						ob.drawBox.x = pos.x;
						ob.drawBox.width = pos.width;
						ob.drawBox.y += ob.lineHeight;

						pathWalker = currentObject.GetType ();
						ob.methodInfos = pathWalker.GetMethods (BindingFlags.Instance | BindingFlags.Public);
						genericToggle = true;
						ob.secondaryScrollBar = new Rect (pos.x + ob.drawBox.width * 0.95f, ob.drawBox.y, pos.width * 0.1f, 0);
						maxListSize = ob.methodInfos.Length;
						if (maxListSize > 10) {
							maxListSize = 10;
						} else {
							ob.secondaryListID = 0;
						}
						for (i = 0; i < maxListSize; i++) {
							
							int currentMethod = ob.secondaryListID + i;
							if (GUI.Button (new Rect (ob.drawBox.x + ob.drawBox.width * 0.05f, ob.drawBox.y, ob.drawBox.width * 0.9f, ob.lineHeight), ob.methodInfos [currentMethod].Name)) {
								if (currentPath.Length > 1) {
									currentPath [currentPath.Length - 1] = ob.methodInfos [currentMethod].Name;
									crPath = "";
									for (int cp = 0; cp < currentPath.Length; cp++) {
										crPath += currentPath [cp];
										if (cp < currentPath.Length - 1) {
											crPath += ".";
										}
									}
									pathProp.stringValue = crPath;
								} else {
									pathProp.stringValue = ob.methodInfos [currentMethod].Name;
								}
							}
							ob.drawBox.y += ob.lineHeight;
						}
						if (ob.methodInfos.Length > 10) {
							ob.secondaryScrollBar.height = ob.drawBox.y - ob.secondaryScrollBar.y;
							ob.secondaryListID = Mathf.RoundToInt (GUI.VerticalSlider (ob.secondaryScrollBar, ob.secondaryListID, 0, ob.methodInfos.Length - 10, GUI.skin.verticalScrollbar, GUI.skin.verticalScrollbarThumb));
						}
						break;
					}
				} else {
					ob.currentTarget = 0;
				}
				if (EditorGUI.EndChangeCheck ()) {
					ObjectBinder baseObject = (ObjectBinder)fieldInfo.GetValue (prop.serializedObject.targetObject);
					baseObject.Connect ();
				}
				break;



			case 1:
				//bindings
				SerializedProperty binds = prop.FindPropertyRelative ("bindings");

				GUI.Box (ob.drawBox, "Bindings");
				ob.drawBox.y += ob.lineHeight;

				ob.primaryScrollBar = new Rect (pos.x + ob.drawBox.width * 0.95f, ob.drawBox.y, ob.drawBox.width * 0.1f, 0);
				maxListSize = binds.arraySize;
				if (maxListSize > 10) {
					maxListSize = 10;
				} else {
					ob.targetListID = 0;
				}
				for (i = 0; i < maxListSize; i++) {
					int r = ob.targetListID + i;
					GUI.backgroundColor = Color.red;
					if (GUI.Button (new Rect (ob.drawBox.x + ob.drawBox.width * 0.05f, ob.drawBox.y, ob.lineHeight, ob.lineHeight), "X")) {

						binds.DeleteArrayElementAtIndex (r);
						EditorGUI.indentLevel = indent;
						GUI.backgroundColor = b;
						EditorGUI.EndProperty ();
						return;
					}
					if (genericToggle) {
						GUI.backgroundColor = ob.secondaryC;
						genericToggle = false;
					} else {
						GUI.backgroundColor = ob.primaryC;
						genericToggle = true;
					}

					if (i != ob.currentBinding) {
						if (GUI.Button (new Rect (ob.drawBox.x + ob.lineHeight + ob.drawBox.width * 0.05f, ob.drawBox.y, ob.drawBox.width * 0.9f - ob.lineHeight, ob.lineHeight), 
							    binds.GetArrayElementAtIndex (r).FindPropertyRelative ("name").stringValue)) {
							ob.currentBinding = r;
						}
					} else {
						EditorGUI.LabelField (new Rect (ob.drawBox.x + ob.lineHeight + ob.drawBox.width * 0.05f, ob.drawBox.y, ob.drawBox.width * 0.9f - ob.lineHeight, ob.lineHeight), 
							binds.GetArrayElementAtIndex (r).FindPropertyRelative ("name").stringValue);
					}
					ob.drawBox.y += ob.lineHeight;
				}
				if (binds.arraySize > 10) {
					ob.primaryScrollBar.height = ob.drawBox.y - ob.primaryScrollBar.y + ob.lineHeight;
					ob.targetListID = Mathf.RoundToInt (GUI.VerticalSlider (ob.primaryScrollBar, ob.targetListID, 0, binds.arraySize - 10, GUI.skin.verticalScrollbar, GUI.skin.verticalScrollbarThumb));
				}
				GUI.backgroundColor = ob.primaryC;
				if (GUI.Button (new Rect (ob.drawBox.x + ob.drawBox.width * 0.05f, ob.drawBox.y, ob.drawBox.width * 0.9f, ob.lineHeight), "Add Binding")) {
					i = binds.arraySize + 1;
					binds.arraySize = i;
				}
				ob.drawBox.y += ob.lineHeight;

				//Now we do our current target stuff.
				if (ob.currentBinding < binds.arraySize) {
					ob.drawBox.y += ob.lineHeight * 0.5f;
					SerializedProperty cBind = binds.GetArrayElementAtIndex (ob.currentBinding);

					GUI.Box (ob.drawBox, "Current Binding");
					ob.drawBox.y += ob.lineHeight;
					EditorGUI.PropertyField (ob.drawBox, cBind.FindPropertyRelative ("name"), new GUIContent ("Name"));
					ob.drawBox.y += ob.lineHeight;



					SerializedProperty bindSuc = cBind.FindPropertyRelative ("successCondition");
					if (bindSuc.enumValueIndex == 1 || bindSuc.enumValueIndex == 3 || bindSuc.enumValueIndex == 4) {
						ob.drawBox.width *= 0.75f;
						EditorGUI.PropertyField (ob.drawBox, bindSuc);
						ob.drawBox.x += ob.drawBox.width;
						ob.drawBox.width = pos.width * 0.25f;
						SerializedProperty bindID = cBind.FindPropertyRelative ("scID");
						EditorGUI.PropertyField (ob.drawBox, bindID, GUIContent.none);
						ob.drawBox.x = pos.x;
						ob.drawBox.width = pos.width;
					} else {
						EditorGUI.PropertyField (ob.drawBox, bindSuc);
					}
					ob.drawBox.y += ob.lineHeight;

					SerializedProperty primID = cBind.FindPropertyRelative ("primaryID");
					if ((primID.intValue + 1) >= ob.targetNames.Length) {
						primID.intValue = ob.targetNames.Length - 2;
					}
					primID.intValue = EditorGUI.Popup (ob.drawBox, "Primary Target", primID.intValue + 1, ob.targetNames) - 1;
					ob.drawBox.y += ob.lineHeight;


					SerializedProperty primMod = cBind.FindPropertyRelative ("primaryModifier");
					if ((primMod.intValue + 1) >= ob.modifierNames.Length) {
						primMod.intValue = ob.modifierNames.Length - 2;
					}
					primMod.intValue = EditorGUI.Popup (ob.drawBox, "Primary Modifier", primMod.intValue + 1, ob.modifierNames) - 1;
					if (ob.modifierNames [primMod.intValue + 1] == "NA : Not a Modifier") {
						primMod.intValue = -1;
					}
					ob.drawBox.y += ob.lineHeight;

					SerializedProperty primCon = cBind.FindPropertyRelative ("converter");
					if ((primCon.intValue + 1) >= ob.converterNames.Length) {
						primCon.intValue = ob.converterNames.Length - 2;
					}
					primCon.intValue = EditorGUI.Popup (ob.drawBox, "Converter", primCon.intValue + 1, ob.converterNames) - 1;
					if (ob.converterNames [primCon.intValue + 1] == "NA : Not a Converter") {
						primCon.intValue = -1;
					}
					ob.drawBox.y += ob.lineHeight;

					SerializedProperty groupIDs = cBind.FindPropertyRelative ("groupIDs");
					SerializedProperty groupMods = cBind.FindPropertyRelative ("groupModifiers");

					ob.secondaryScrollBar = new Rect (pos.x + ob.drawBox.width * 0.95f, ob.drawBox.y, pos.width * 0.1f, 0);
					maxListSize = groupIDs.arraySize;
					if (maxListSize > 10) {
						maxListSize = 10;
					} else {
						ob.secondaryListID = 0;
					}

					ob.drawBox.width = (pos.width*0.9f-ob.lineHeight) * 0.5f;
					ob.drawBox.x = pos.x + pos.width * 0.05f;
					for (i = 0; i < maxListSize; i++) {
						int ii = i + ob.secondaryListID;

						if (genericToggle) {
							GUI.backgroundColor = ob.primaryC;
							genericToggle = false;
						} else {
							GUI.backgroundColor = ob.secondaryC; 
							genericToggle = true;
						}
						GUI.backgroundColor = Color.red;
						if (GUI.Button (new Rect(ob.drawBox.x, ob.drawBox.y, ob.lineHeight, ob.lineHeight),"X")) {
							groupIDs.DeleteArrayElementAtIndex (ii);
							groupMods.DeleteArrayElementAtIndex (ii);
							EditorGUI.indentLevel = indent;
							GUI.backgroundColor = b;
							EditorGUI.EndProperty ();
							return;
						}
						GUI.backgroundColor = b;
						EditorGUIUtility.labelWidth = ob.drawBox.width * 0.5f;
						SerializedProperty groupMem = groupIDs.GetArrayElementAtIndex (i);
						ob.drawBox.x += ob.lineHeight;
						if ((groupMem.intValue + 1) >= ob.targetNames.Length) {
							groupMem.intValue = ob.targetNames.Length - 2;
						}
						groupMem.intValue = EditorGUI.Popup (ob.drawBox, "#" + ii.ToString() + " Target", groupMem.intValue + 1, ob.targetNames) - 1;
						ob.drawBox.x += ob.drawBox.width;
						SerializedProperty groupMod = groupMods.GetArrayElementAtIndex (i);
						if ((groupMod.intValue + 1) >= ob.modifierNames.Length) {
							groupMod.intValue = ob.modifierNames.Length - 2;
						}
						groupMod.intValue= EditorGUI.Popup (ob.drawBox, "Modifier",groupMod.intValue + 1, ob.modifierNames) - 1;
						if (ob.modifierNames [groupMod.intValue + 1] == "NA : Not a Modifier") {
							groupMod.intValue = -1;
						}
						EditorGUIUtility.labelWidth = 0;
						ob.drawBox.x = pos.x + pos.width * 0.05f;
						ob.drawBox.y += ob.lineHeight;
					}
					if (groupIDs.arraySize > 10) {
						ob.secondaryScrollBar.height = ob.drawBox.y - ob.secondaryScrollBar.y + ob.lineHeight;
						ob.secondaryListID = Mathf.RoundToInt (GUI.VerticalSlider (ob.secondaryScrollBar, ob.secondaryListID, 0, groupIDs.arraySize - 10, GUI.skin.verticalScrollbar, GUI.skin.verticalScrollbarThumb));
					}
					ob.drawBox.width = pos.width * 0.9f;
					if (GUI.Button (ob.drawBox, "Add Group Target")) {
						i = groupIDs.arraySize + 1;
						groupIDs.arraySize = i;
						groupMods.arraySize = i;
					}
					ob.drawBox.y += ob.lineHeight;
					ob.drawBox.x = pos.x;
					ob.drawBox.width = pos.width;
				}
				break;
			}
			ob.drawBox.y += ob.lineHeight * 0.5f;
			GUI.backgroundColor = ob.primaryC;
			if(GUI.Button(ob.drawBox, "Done")){ob.show = false;}
			ob.totalLineHeight =  ob.drawBox.y + ob.lineHeight - pos.y;
		}
		EditorGUI.indentLevel = indent;
		GUI.backgroundColor = b;
		EditorGUI.EndProperty ();
	} 



	void SetColors()
	{
		ob.primaryC = GUI.backgroundColor;
		Color.RGBToHSV (GUI.backgroundColor, out ob.hsv.x, out ob.hsv.y, out ob.hsv.z);
		if (ob.hsv.z > 0.5f) {
			ob.hsv.z -= 0.225f;
			ob.secondaryC = Color.HSVToRGB(ob.hsv.x, ob.hsv.y, ob.hsv.z);
			ob.hsv.z -= 0.225f;
			ob.tirtiaryC = Color.HSVToRGB(ob.hsv.x, ob.hsv.y, ob.hsv.z);
		} else {
			ob.hsv.z += 0.225f;
			ob.secondaryC = Color.HSVToRGB(ob.hsv.x, ob.hsv.y, ob.hsv.z);
			ob.hsv.z += 0.225f;
			ob.tirtiaryC = Color.HSVToRGB(ob.hsv.x, ob.hsv.y, ob.hsv.z);
		}
	}

	void SetStringLists(SerializedProperty prop, bool remap = false)
	{
		 
		SerializedProperty targls = prop.FindPropertyRelative ("baseTargetLabels");
		SerializedProperty targs = prop.FindPropertyRelative ("baseTargets");
		SerializedProperty paths = prop.FindPropertyRelative("memberPaths");

		int i;

		List<string> sList = new List<string> ();
		sList.Add ("None");

		for (i = 0; i < targls.arraySize; i++) {
			sList.Add (targls.GetArrayElementAtIndex (i).stringValue);
		}
		ob.targetNames = sList.ToArray ();

		sList = new List<string> ();
		List<string> sList2 = new List<string> ();
		sList.Add ("None");
		sList2.Add ("None");

		for (i = 0; i < paths.arraySize; i++) {
			bool wasModifier = false, wasConverter = false;
			SerializedProperty targ = targs.GetArrayElementAtIndex (i);
			if (targ.objectReferenceValue != null) {
				SerializedProperty pathProp = paths.GetArrayElementAtIndex (i);
				string workString = pathProp.stringValue;
				string[] currentPath = workString.Split ("." [0]);

				System.Object currentObject = targ.objectReferenceValue;
				System.Type pathWalker = currentObject.GetType ();

				IEnumerable arrayHolder;
				IEnumerator arraySifter;
				MemberInfo pathMember;
				MemberInfo[] ms;
				for (int j = 0; j < currentPath.Length - 1; j++) {
					//Check to see if this field is an array. If so, it'll end with ].
					//In that case, check for an id. If we get one, we're getting a specific element in the array. If we don't, the field is the array itself.
					if (currentPath [j].EndsWith ("]")) {
						string interum = currentPath [j].Substring (currentPath [j].LastIndexOf ("[" [0])).Replace ("[", "").Replace ("]", "");
						int fieldArrayID = Convert.ToInt32 (interum);
						string pf = currentPath [j].Substring (0, currentPath [j].LastIndexOf ("["));
						ms = pathWalker.GetMember (pf);
						if (ms.Length > 0) {
							pathMember = ms [0];
							if (fieldArrayID >= 0) {
								switch (pathMember.MemberType) {
								case MemberTypes.Field:
									currentObject = RuntimeHelpers.GetObjectValue (((FieldInfo)pathMember).GetValue (currentObject));
									break;
								case MemberTypes.Property:
									currentObject = RuntimeHelpers.GetObjectValue (((PropertyInfo)pathMember).GetValue (currentObject, null));
									break;
								}
								arrayHolder = currentObject as IEnumerable;
								arraySifter = arrayHolder.GetEnumerator ();
								while (fieldArrayID-- >= 0) {
									arraySifter.MoveNext ();
								}
								currentObject = RuntimeHelpers.GetObjectValue (arraySifter.Current);
								pathWalker = currentObject.GetType ();
							} else {
								switch (pathMember.MemberType) {
								case MemberTypes.Field:
									currentObject = RuntimeHelpers.GetObjectValue (((FieldInfo)pathMember).GetValue (currentObject));
									pathWalker = currentObject.GetType ();
									break;
								case MemberTypes.Property:
									currentObject = RuntimeHelpers.GetObjectValue (((PropertyInfo)pathMember).GetValue (currentObject, null));
									pathWalker = currentObject.GetType ();
									break;
								}
							}
						} else {
							pathWalker = null; 
							break;
						}
					} else {
						//Debug.Log ("Getting " + currentPath [j]);
						ms = pathWalker.GetMember (currentPath [j]);
						if (ms.Length > 0) {
							//Debug.Log ("Found " + ms.Length + " members with that name.");
							pathMember = ms [0];
							switch (pathMember.MemberType) {
							case MemberTypes.Field:
									//Debug.Log ("It was a field.");
								currentObject = RuntimeHelpers.GetObjectValue (((FieldInfo)pathMember).GetValue (currentObject));
								pathWalker = currentObject.GetType ();
									//Debug.Log ("It was a field: " + currentObject.ToString());
								break;
							case MemberTypes.Property:
									//Debug.Log ("It was a property.");
								currentObject = RuntimeHelpers.GetObjectValue (((PropertyInfo)pathMember).GetValue (currentObject, null));
								pathWalker = currentObject.GetType ();
								break;
							}
						} else {
							//Debug.Log ("Cound not find any members with that name.");
							pathWalker = null; 
							break;
						}
					}
				}

				//Now, the last one:
				if (pathWalker != null) {
					//Debug.Log ("Getting " + path [path.Length-1]);
					ms = pathWalker.GetMember (currentPath [currentPath.Length - 1]);
					if (ms.Length > 0) {
						//Debug.Log ("Found " + ms.Length + " members with that name.");
						if(ms[0].MemberType == MemberTypes.Method){
							MethodInfo mb = (MethodInfo)ms [0];
							ParameterInfo[] pars = mb.GetParameters ();
							if (pars.Length == 1) {
								if (mb.ReturnType != typeof(void)) {
									sList.Add (mb.Name);
									wasModifier = true;
								}
							} else if (pars.Length == 2) {
								if (pars [0].ParameterType.IsByRef && mb.ReturnType == typeof(void) &&
									pars [1].ParameterType.GetInterface("IEnumerable") != null && pars [1].ParameterType.IsByRef) {
									sList2.Add (mb.Name);
									wasConverter = true;
								}
							}
						}
					} 
				} 
			}
			if (!wasModifier) {
				sList.Add ("NA : Not a Modifier");
			}
			if (!wasConverter) {
				sList2.Add ("NA : Not a Converter");
			}
		}
		ob.modifierNames = sList.ToArray ();
		ob.converterNames = sList2.ToArray ();
		//Debug.Log (ob.targetNames.Length.ToString () +" "+ ob.modifierNames.Length.ToString () +" "+ ob.converterNames.Length.ToString ());
	}

	public System.Object NullModifier(System.Object target){
		return target;
	}

	public void NullConverter (ref System.Object target, ref System.Object[] grouping){

		return;
	}

}

