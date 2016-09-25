using UnityEngine;
using System;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace Crux{
	[CustomPropertyDrawer (typeof (Polygon))]
	public class PolygonEditor : PropertyDrawer {
		float lineHeight;
		SerializedProperty show, db, color, totalLineHeight, showPoints;

		public override float GetPropertyHeight (SerializedProperty prop, GUIContent label) {
			show = prop.FindPropertyRelative ("show");
			totalLineHeight = prop.FindPropertyRelative ("totalLineHeight");
			lineHeight = base.GetPropertyHeight (prop, label)*1.125f;
			if (totalLineHeight.floatValue == 0) {
				totalLineHeight.floatValue = lineHeight;
			}
			if (!show.boolValue) {
				return lineHeight;
			}

			return totalLineHeight.floatValue;

		}


		public override void OnGUI (Rect pos, SerializedProperty prop, GUIContent label) {
			
			show = prop.FindPropertyRelative ("show");
			showPoints = prop.FindPropertyRelative ("showPoints");
			db= prop.FindPropertyRelative ("drawBox");
			color = prop.FindPropertyRelative ("color");
			totalLineHeight = prop.FindPropertyRelative ("totalLineHeight");
			EditorGUI.BeginProperty (pos, label, prop);
			Rect drawBox = new Rect (pos.x, pos.y, pos.width, lineHeight);

			SerializedProperty n = prop.FindPropertyRelative ("name");
			if (show.boolValue) {
				drawBox.width *= 0.5f;
				show.boolValue = EditorGUI.ToggleLeft (drawBox, "Hide " + n.stringValue, show.boolValue);
				drawBox.x += drawBox.width;
				color.colorValue = EditorGUI.ColorField (drawBox, color.colorValue);
				drawBox.x = pos.x;
				drawBox.width = pos.width;
				drawBox.y += lineHeight;
				int indent = EditorGUI.indentLevel;
				EditorGUI.indentLevel = 0;

				EditorGUI.PropertyField (drawBox, n);
				drawBox.y += lineHeight;

				Polygon p = (Polygon)prop.GetPropertyFromFullPath(fieldInfo);
				Vector3 currentPoint;
				Vector3 [] points = p.points.ConvertToVec3();
				bool dirty = false;
				int pointCount = points.Length;
				int pc = pointCount;
				if (showPoints.boolValue) {
					drawBox.width *= 0.5f;
					showPoints.boolValue = EditorGUI.ToggleLeft (drawBox, new GUIContent ("Points"), showPoints.boolValue);

					drawBox.x += drawBox.width;
					pc = EditorGUI.IntField (drawBox, GUIContent.none, pointCount);
					drawBox.x -= drawBox.width;
					drawBox.width *= 2;

				} else {
					showPoints.boolValue = EditorGUI.ToggleLeft (drawBox, new GUIContent ("Points"),showPoints.boolValue);

				}
				drawBox.y += lineHeight;
				if (pc < 0) {
					pc = 0;
				}

				if (pc != pointCount) {
					
					dirty = true;
					List<Vector3> newPoints = new List<Vector3> (pc);

					int c = pointCount;
					if (pc < pointCount) {
						c = pc;
					}
					for (int cc = 0; cc < c; cc++) {
						newPoints.Add (points [cc]);
					}
					if (pointCount == 0) {
						newPoints.Add (Vector3.zero);
						pointCount++;
					}
					if (pc > pointCount) {
						c = pc - pointCount;
						for (int cc = 0; cc < c; cc++) {
							Vector3 nP = Vector3.Lerp (newPoints [newPoints.Count - 1], newPoints [0], 0.5f);
							newPoints.Add (nP);
						}

					}
					points = newPoints.ToArray ();
				}
				if (p.showPoints) {
					for (int i = 0; i < points.Length; i++) {
						currentPoint = EditorGUI.Vector3Field (drawBox, GUIContent.none, points [i]);
						if (currentPoint != points [i]) {
							dirty = true;
							points [i] = currentPoint;
						}
						drawBox.y += lineHeight;
					}
				}
				//drawBox.y += lineHeight;
				if (dirty) {
					p.points = points.ConvertToVec2();
					//prop.SetPropertyAtFullPath (fieldInfo, p);
					prop.serializedObject.Update ();
				}
				totalLineHeight.floatValue = drawBox.y   - pos.y;
				db.rectValue = drawBox;
				EditorGUI.indentLevel = indent;

			} else {
				show.boolValue = EditorGUI.ToggleLeft (drawBox, "Show " + n.stringValue, show.boolValue);
			}
			EditorGUI.EndProperty ();
		}

	}
}