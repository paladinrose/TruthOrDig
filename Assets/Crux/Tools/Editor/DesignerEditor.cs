using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Crux{
[CustomEditor(typeof(Designer))]
	public class DesignerEditor : Editor {
		
		Designer designer;

		GUIStyle buttonStyle;
		GUIStyle toggleStyle;
		OmniRoutine activeRoutine, updateSceneInfo;
		int currentArea = -1, currentSelection = -1, areaOffset, selectedTab = -1, selectedArea = -1, currentPalletItem=-1, paintMode, selectedTool;


		string[] paintModes = new string[] {"Free", "Tile" };
		static Texture[] icons;
		Texture[] paintPreview = new Texture[0];
		float pointSnap = 10, areaScrollHeight, posScrollHeight, negScrollHeight, splineScrollHeight, tileScrollHeight, palScrollHeight, splineSegScrollHeight;
		Vector2 areaScroll, posScroll, negScroll, splineScroll, tileScroll, palScroll, splineSegScroll;
		bool showShapes, showHoles, showSplines, showTiles, showSplineSegments, showPallet, toolInUse ,updateCurrentAreaEditor;
		TileGroup.WallpaperGroup selectedWallpaper;

		DesignAreaEditor currentAreaEditor;

		int key = 0, holdingID = -1;
		float lineHeight;
		double updateTime;
		DesignArea currentPaintTiling = null;

		[InitializeOnLoadMethod]
		static void GetUIIcons(){
			string uiIconPath = "Assets/Crux/UI/Textures/";
			DirectoryInfo dir = new DirectoryInfo (uiIconPath);
			FileInfo[] files = dir.GetFiles ("*.png");
			icons = new Texture[files.Length];
			//Debug.Log ("Loaded " + icons.Length + " icons.");
			for (int i = 0; i < files.Length; i++) {
				icons [i] = (Texture)AssetDatabase.LoadMainAssetAtPath (uiIconPath + files[i].Name);
				//Debug.Log (files [i].Name);
			}

		}
		public void OnEnable(){
			designer = (Designer)target;
			if (designer.areas.Length < 6) {
				areaScrollHeight = (designer.areas.Length + 1) * EditorGUIUtility.singleLineHeight;
			} else {
				areaScrollHeight = 6 * EditorGUIUtility.singleLineHeight;
			}

			GetPaintPreviews ();
			updateTime = EditorApplication.timeSinceStartup + 1.5d;
			EditorApplication.update += TimedRedraw;


		}
		public void OnDisable(){
			EditorApplication.update -= TimedRedraw;
		}
		public void TimedRedraw(){
			if (EditorApplication.timeSinceStartup >= updateTime) {
				RedrawSceneView ();
			}
		}
		public void RedrawSceneView(){
			SceneView.RepaintAll ();
			updateTime = EditorApplication.timeSinceStartup + 1.5d;
		}
		public void GetPaintPreviews(){
            if (paintLoader == null)
            {
                paintPreview = new Texture[designer.painter.pallet.Length];
                for (int i = designer.painter.pallet.Length - 1; i >= 0; i--)
                {
                    paintPreview[i] = icons[11];
                }
                paintLoader = LoadPaintPreview();
                EditorApplication.update += PaintPreviewLoader;
            }
		}
        IEnumerator paintLoader;
        void PaintPreviewLoader()
        {
            if (!paintLoader.MoveNext())
            {
                paintLoader = null;
                EditorApplication.update -= PaintPreviewLoader;
            }
        }
        IEnumerator LoadPaintPreview() {
            
           // List<Texture> pP = new List<Texture>();
            int dif = designer.painter.pallet.Length;
            
            for (int i = 0; i < dif; i++)
            {

                if (designer.painter.pallet[i].baseItem != null)
                {
                    paintPreview[i] = AssetPreview.GetAssetPreview(designer.painter.pallet[i].baseItem.gameObject);
                
                    while (AssetPreview.IsLoadingAssetPreviews())
                    {
                         
                        yield return null;
                        Repaint();
                    }
                }
                yield return null;
            }
            yield return null;
           // Debug.Log("Loading asset previews complete!");
           // paintPreview = pP.ToArray();
        }
		public override void OnInspectorGUI(){

			if (buttonStyle == null) {
				buttonStyle = new GUIStyle (GUI.skin.button);
				buttonStyle.font = GUI.skin.font;
				//buttonStyle.fontSize = 10;
				buttonStyle.fontStyle = FontStyle.Bold;
				buttonStyle.normal.textColor = Color.white;
			}

			serializedObject.Update (); 

			if (paintPreview.Length != designer.painter.pallet.Length) {
				GetPaintPreviews ();
			}

			EditorGUI.BeginChangeCheck ();
			Color c = GUI.backgroundColor, cc = GUI.contentColor, r = new Color (0.85f, 0, 0, 1);

			lineHeight = EditorGUIUtility.singleLineHeight;
			areaScroll = GUILayout.BeginScrollView (areaScroll, GUI.skin.box, GUILayout.Height (areaScrollHeight));
			int dar = designer.areas.Length, rem = -1;
			for (int i = 0; i < dar; i++) {
				 if (designer.areas [i].draw) {
					GUI.backgroundColor = designer.areas [i].color;
				} 

				GUILayout.BeginHorizontal (GUI.skin.box);
				GUI.backgroundColor = Color.gray;

				//Is this area visible?
                /*
				if(designer.areas [i].draw){
					if (GUILayout.Button (icons [6], GUI.skin.box,GUILayout.Width (lineHeight),GUILayout.Height (lineHeight))) {
						designer.areas [i].draw = false;
					}
				}else {
					if (GUILayout.Button (icons [7], GUI.skin.box, GUILayout.Width (lineHeight),GUILayout.Height (lineHeight))) {
						designer.areas [i].draw = true;
					}
				} 
                */
				GUI.backgroundColor = c;

				//Is this area the currently selected area?
				if (currentArea != i && GUILayout.Button (designer.areas [i].areaName)) {
					selectedArea = i;
					if (designer.areas [i].shapes.Length < 6) {
						posScrollHeight = (designer.areas [i].shapes.Length + 1) * lineHeight;
					} else {
						posScrollHeight = 6 * lineHeight;
					}
					if (designer.areas [i].holes.Length < 6) {
						negScrollHeight = (designer.areas [i].holes.Length + 1) * lineHeight;
					} else {
						negScrollHeight = 6 * lineHeight;
					}
					if (designer.areas [i].splines.Length < 6) {
						splineScrollHeight = (designer.areas [i].splines.Length + 1) * lineHeight;
					} else {
						splineScrollHeight = 6 * lineHeight;
					}
					if (designer.areas [i].tileGroups.Length < 6) {
						tileScrollHeight = (designer.areas [i].tileGroups.Length + 1) * lineHeight;
					} else {
						tileScrollHeight = 6 * lineHeight;
					}

				} else if (currentArea == i) {
					GUILayout.Label (designer.areas [i].areaName);
				}	

				//What properties does this area contain?
				GUI.backgroundColor = Color.gray;

				//Shapes and holes?
				if (designer.areas [i].shapes.Length > 0) {
					GUILayout.Box (icons [1], GUILayout.Width (lineHeight),GUILayout.Height (lineHeight));
				}
				//Splines?
				if (designer.areas [i].splines.Length > 0) {
					GUILayout.Box (icons [2], GUILayout.Width (lineHeight),GUILayout.Height (lineHeight));
				}
				//Tiles?
				if (designer.areas [i].tileGroups.Length > 0) {
					GUILayout.Box (icons [3], GUILayout.Width (lineHeight),GUILayout.Height (lineHeight));
				}
				//Colliders?
				if (designer.areas [i].colliders.Length >0 || designer.areas[i].colliders2D.Length >0) {
					GUILayout.Box (icons [8], GUILayout.Width (lineHeight),GUILayout.Height (lineHeight));
				}

				//Finally, a Remove button.
				GUI.backgroundColor = r;
				GUI.contentColor = Color.white;
				if (GUILayout.Button ("X", buttonStyle, GUILayout.Width (lineHeight),GUILayout.Height (lineHeight))) {
					rem = i;
				}
				GUI.backgroundColor = c;
				GUI.contentColor = cc;

				GUILayout.EndHorizontal ();
			}
			GUILayout.EndScrollView ();

			if (rem >= 0) {
				if (currentArea >= rem) {
					selectedArea = currentArea - 1;
					currentArea = selectedArea;
					currentSelection = -1;
				}
				List<DesignArea> arList = new List<DesignArea> (designer.areas);
				DesignArea desToRem = designer.areas [rem];
				arList.RemoveAt (rem);
				designer.areas = arList.ToArray ();
				DestroyImmediate (desToRem.gameObject);

				if (designer.areas.Length < 6) {
					areaScrollHeight = (designer.areas.Length + 1) * lineHeight;
				} else {
					areaScrollHeight = 6 * lineHeight;
				}
				return;
			}
			//Button to add a brand new area.
			if (GUILayout.Button ("New Area")) {
				List<DesignArea> arList = new List<DesignArea> (designer.areas);
				DesignArea desToAdd = new GameObject ("Area " + (arList.Count + 1).ToString()).AddComponent<DesignArea> ();
				desToAdd.transform.parent = designer.transform;
				desToAdd.areaName = desToAdd.gameObject.name;
				arList.Add (desToAdd);
				designer.areas = arList.ToArray ();
				if (designer.areas.Length < 6) {
					areaScrollHeight = (designer.areas.Length + 1) * lineHeight;
				} else {
					areaScrollHeight = 6 * lineHeight;
				}
				currentArea = designer.areas.Length - 1;
				currentSelection = -1;
				RedrawSceneView ();
			}


			EditorGUILayout.Space ();
			EditorGUILayout.Space ();


			if (currentArea >= 0) {
				GUILayout.BeginHorizontal ();
				//designer.areas [currentArea].draw = EditorGUILayout.Toggle (GUIContent.none, designer.areas [currentArea].draw, GUILayout.Width (lineHeight));
				designer.areas [currentArea].areaName = designer.areas [currentArea].gameObject.name = EditorGUILayout.TextField ("Current Area: ", designer.areas [currentArea].areaName);
				designer.areas [currentArea].color = EditorGUILayout.ColorField (GUIContent.none, designer.areas [currentArea].color, GUILayout.Width (lineHeight * 2));
				GUILayout.EndHorizontal ();

				//designer.areas [currentArea].startingPoint = EditorGUILayout.Vector3Field ("Starting Point", designer.areas [currentArea].startingPoint);
				//designer.areas [currentArea].wallpaper = (TileGroup.WallpaperGroup)EditorGUILayout.EnumPopup ("Tiling", designer.areas [currentArea].wallpaper);
				EditorGUILayout.Space ();

				showShapes = GUILayout.Toggle (showShapes, "Shapes");
				if (showShapes) {
					posScroll = GUILayout.BeginScrollView (posScroll, GUI.skin.box, GUILayout.Height (posScrollHeight));
					dar = designer.areas [currentArea].shapes.Length;
					rem = -1;
					for (int i = 0; i < dar; i++) {
						GUILayout.BeginHorizontal ();
						designer.areas [currentArea].shapes [i].color = currentAreaEditor.shapes [i].color = EditorGUILayout.ColorField (GUIContent.none, designer.areas [currentArea].shapes [i].color, GUILayout.Width (lineHeight * 2));
						if (currentSelection != i && GUILayout.Button (designer.areas [currentArea].shapes [i].name)) {
							currentSelection = i;
						} else if (currentSelection == i) {
							currentAreaEditor.shapes[i].name = designer.areas [currentArea].shapes [i].name = GUILayout.TextField (designer.areas [currentArea].shapes [i].name);
						}	
						GUI.backgroundColor = r;
						GUI.contentColor = Color.white;
						if (GUILayout.Button ("X", buttonStyle, GUILayout.Width (lineHeight))) {
							rem = i;
						}
						GUI.backgroundColor = c;
						GUI.contentColor = cc;
						GUILayout.EndHorizontal ();
					}
					GUILayout.EndScrollView ();
					if (rem >= 0) {
						if (currentSelection >= rem) {
							currentSelection--;
						}
						List<Polygon> polList = new List<Polygon> (designer.areas [currentArea].shapes);
						polList.RemoveAt (rem);
						designer.areas [currentArea].shapes = polList.ToArray ();
						selectedArea = currentArea;
						if (designer.areas [currentArea].shapes.Length < 6) {
							posScrollHeight = (designer.areas [currentArea].shapes.Length + 1) * lineHeight;
						} else {
							posScrollHeight = 6 * lineHeight;
						}
					}
					if (GUILayout.Button ("New Shape")) {
						List<Polygon> polList = new List<Polygon> (designer.areas [currentArea].shapes);
						polList.Add (new Polygon (designer.areas [currentArea].areaName + " " + ((int)(polList.Count + 1)).ToString ()));
						designer.areas [currentArea].shapes = polList.ToArray ();
						selectedArea = currentArea;
						currentSelection = -1;
						if (designer.areas [currentArea].shapes.Length < 6) {
							posScrollHeight = (designer.areas [currentArea].shapes.Length + 1) * lineHeight;
						} else {
							posScrollHeight = 6 * lineHeight;
						}
						RedrawSceneView ();
					}

				}

				EditorGUILayout.Space ();
				if (designer.areas [currentArea].shapes.Length > 0) {
					showHoles = GUILayout.Toggle (showHoles, "Holes");
					if (showHoles) {
						currentSelection -= designer.areas [currentArea].shapes.Length;
						negScroll = GUILayout.BeginScrollView (negScroll, GUI.skin.box, GUILayout.Height (negScrollHeight));
						dar = designer.areas [currentArea].holes.Length;
						rem = -1;
						for (int i = 0; i < dar; i++) {
							GUILayout.BeginHorizontal ();
							designer.areas [currentArea].holes [i].color = currentAreaEditor.holes [i].color = EditorGUILayout.ColorField (GUIContent.none, designer.areas [currentArea].holes [i].color, GUILayout.Width (lineHeight * 2));

							if (currentSelection != i && GUILayout.Button (designer.areas [currentArea].holes [i].name)) {
								currentSelection = i;
							} else if (currentSelection == i) {
								currentAreaEditor.holes [i].name = designer.areas [currentArea].holes [i].name = GUILayout.TextField (designer.areas [currentArea].holes [i].name);
							}	
							GUI.backgroundColor = r;
							GUI.contentColor = Color.white;
							if (GUILayout.Button ("X", buttonStyle, GUILayout.Width (lineHeight))) {
								rem = i;
							}
							GUI.backgroundColor = c;
							GUI.contentColor = cc;
							GUILayout.EndHorizontal ();
						}
						GUILayout.EndScrollView ();
						if (rem >= 0) {
							if (currentSelection >= rem) {
								currentSelection--;
							}
							List<Polygon> polList = new List<Polygon> (designer.areas [currentArea].holes);
							polList.RemoveAt (rem);
							designer.areas [currentArea].holes = polList.ToArray ();
							if (designer.areas [currentArea].holes.Length < 6) {
								negScrollHeight = (designer.areas [currentArea].holes.Length + 1) * lineHeight;
							} else {
								negScrollHeight = 6 * lineHeight;
							}
						}
						if (GUILayout.Button ("New Hole")) {
							List<Polygon> polList = new List<Polygon> (designer.areas [currentArea].holes);
							polList.Add (new Polygon (designer.areas [currentArea].areaName + " " + ((int)(polList.Count + 1)).ToString ()));
							designer.areas [currentArea].holes = polList.ToArray ();
							selectedArea = currentArea;
							currentSelection = -1;
							if (designer.areas [currentArea].holes.Length < 6) {
								negScrollHeight = (designer.areas [currentArea].holes.Length + 1) * lineHeight;
							} else {
								negScrollHeight = 6 * lineHeight;
							}
							RedrawSceneView ();
						}
						currentSelection += designer.areas [currentArea].shapes.Length;
						EditorGUILayout.Space ();
					}
				}

				EditorGUILayout.Space ();
				showSplines = GUILayout.Toggle (showSplines, "Splines");
				if (showSplines) {
					designer.splineSmoothing = EditorGUILayout.IntField ("Spline Smoothing", designer.splineSmoothing);
					currentSelection -= designer.areas [currentArea].shapes.Length + designer.areas [currentArea].holes.Length;
					splineScroll = GUILayout.BeginScrollView (splineScroll, GUI.skin.box, GUILayout.Height (splineScrollHeight));
					dar = designer.areas [currentArea].splines.Length;
					rem = -1;
					for (int i = 0; i < dar; i++) {
						GUILayout.BeginHorizontal ();
						//designer.areas [currentArea].splines [i].color = currentAreaEditor.splines [i].color = EditorGUILayout.ColorField (GUIContent.none, designer.areas [currentArea].splines [i].color, GUILayout.Width (lineHeight * 2));
						designer.areas [currentArea].splines [i].color = EditorGUILayout.ColorField (GUIContent.none, designer.areas [currentArea].splines [i].color, GUILayout.Width (lineHeight * 2));
						if (currentSelection != i && GUILayout.Button (designer.areas [currentArea].splines [i].name)) {
							currentSelection = i;
						} else if (currentSelection == i) {
							currentAreaEditor.splines [i].name = designer.areas [currentArea].splines [i].name = GUILayout.TextField (designer.areas [currentArea].splines [i].name);
						}	
						GUI.backgroundColor = r;
						GUI.contentColor = Color.white;
						if (GUILayout.Button ("X", buttonStyle, GUILayout.Width (lineHeight))) {
							rem = i;
						}
						GUI.backgroundColor = c;
						GUI.contentColor = cc;
						GUILayout.EndHorizontal ();
					}
					GUILayout.EndScrollView ();
					if (rem >= 0) {
						if (currentSelection >= rem) {
							currentSelection--;
						}
						List<Spline> sList = new List<Spline> (designer.areas [currentArea].splines);
						sList.RemoveAt (rem);
						designer.areas [currentArea].splines = sList.ToArray ();

						if (designer.areas [currentArea].splines.Length < 6) {
							splineScrollHeight = (designer.areas [currentArea].splines.Length + 1) * lineHeight;
						} else {
							splineScrollHeight = 6 * lineHeight;
						}
					}

					currentSelection += designer.areas [currentArea].shapes.Length + designer.areas[currentArea].holes.Length;
					if (GUILayout.Button ("New Spline")) {
						List<Spline> sList = new List<Spline> (designer.areas [currentArea].splines);
						sList.Add (new Spline (2));
						sList [sList.Count - 1].name = "Spline #" + sList.Count;
						designer.areas [currentArea].splines = sList.ToArray ();
						selectedArea = currentArea;
						currentSelection = -1;
						if (designer.areas [currentArea].splines.Length < 6) {
							splineScrollHeight = (designer.areas [currentArea].splines.Length + 1) * lineHeight;
						} else {
							splineScrollHeight = 6 * lineHeight;
						}
						RedrawSceneView ();
					}

				}

				EditorGUILayout.Space ();
				showTiles = GUILayout.Toggle (showTiles, "Tile Groups");
				if (showTiles) {
					currentSelection -= designer.areas [currentArea].shapes.Length + designer.areas [currentArea].holes.Length + designer.areas[currentArea].splines.Length;
					tileScroll = GUILayout.BeginScrollView (tileScroll, GUI.skin.box, GUILayout.Height (tileScrollHeight));
					dar = designer.areas [currentArea].tileGroups.Length;
					rem = -1;
					for (int i = 0; i < dar; i++) {
						GUILayout.BeginHorizontal ();
						Color tc = designer.areas[currentArea].tileGroups[i].color;
						tc = EditorGUILayout.ColorField (GUIContent.none, designer.areas [currentArea].tileGroups [i].color, GUILayout.Width (lineHeight * 2));
						if (tc != designer.areas [currentArea].tileGroups [i].color) {
							designer.areas [currentArea].tileGroups [i].color = currentAreaEditor.tileGroups [i].color = tc;
							Color sc = tc;
							Color.RGBToHSV (tc, out sc.r, out sc.g, out sc.b);

							if (sc.g >= 0.35f) {
								sc.g -= 0.25f;
							} else {
								sc.g += 0.15f;
							}
							sc = Color.HSVToRGB (sc.r, sc.g, sc.b);
							designer.areas [currentArea].tileGroups [i].sColor = sc;
						}
						if (currentSelection != i && GUILayout.Button (designer.areas [currentArea].tileGroups [i].name)) {
							currentSelection = i;
						} else if (currentSelection == i) {
							currentAreaEditor.tileGroups[i].name = designer.areas [currentArea].tileGroups [i].name = GUILayout.TextField (designer.areas [currentArea].tileGroups [i].name);
						}	
						GUI.backgroundColor = r;
						GUI.contentColor = Color.white;
						if (GUILayout.Button ("X", buttonStyle, GUILayout.Width (lineHeight))) {
							rem = i;
						}
						GUI.backgroundColor = c;
						GUI.contentColor = cc;
						GUILayout.EndHorizontal ();
					}
					GUILayout.EndScrollView ();
					if (rem >= 0) {
						if (currentSelection >= rem) {
							currentSelection--;
						}
						List<TileGroup> sList = new List<TileGroup> (designer.areas [currentArea].tileGroups);
						sList.RemoveAt (rem);
						designer.areas [currentArea].tileGroups = sList.ToArray ();

						if (designer.areas [currentArea].tileGroups.Length < 6) {
							tileScrollHeight = (designer.areas [currentArea].tileGroups.Length + 1) * lineHeight;
						} else {
							tileScrollHeight = 6 * lineHeight;
						}
					}

					currentSelection += designer.areas [currentArea].shapes.Length + designer.areas[currentArea].holes.Length + designer.areas[currentArea].splines.Length;
					if (GUILayout.Button ("New Tile Group")) {
						List<TileGroup> sList = new List<TileGroup> (designer.areas [currentArea].tileGroups);
						sList.Add (new TileGroup ());
						sList [sList.Count - 1].name = "Tile Group #" + sList.Count;
						designer.areas [currentArea].tileGroups = sList.ToArray ();
						selectedArea = currentArea;
						currentSelection = -1;
						if (designer.areas [currentArea].tileGroups.Length < 6) {
							tileScrollHeight = (designer.areas [currentArea].tileGroups.Length + 1) * lineHeight;
						} else {
							tileScrollHeight = 6 * lineHeight;
						}
						RedrawSceneView ();
					}
				}


				//Now to draw whatever my current tool requires
				switch(selectedTab){
				case 0:
					//Polygons
					switch (selectedTool) {
					case 2:
						//Tile
						selectedWallpaper = (TileGroup.WallpaperGroup)EditorGUILayout.EnumPopup ("Wallpaper", selectedWallpaper); 

						break;
					case 3:
                        //Paint
                        //Pallet items
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        followPaintDirection = GUILayout.Toggle(followPaintDirection, "Follow Brush Direction");
                        DrawPaintParameters(r, c, cc);
                        break;
					case 4:
                        //Fill
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        designer.fillDistance = EditorGUILayout.FloatField("Fill Spacing", designer.fillDistance);
                        designer.fillOverlapTolerance = EditorGUILayout.FloatField("Overlap Tolerance", designer.fillOverlapTolerance);
                        designer.fillSpread = EditorGUILayout.IntField("Fill Spread", designer.fillSpread);
                        DrawPaintParameters(r, c, cc);
                        break;
					case 5:
                        //outline
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        DrawPaintParameters(r, c, cc);
                        break;
					}
					break;
				case 1:
                    //Splines
                    
                    showSplineSegments = GUILayout.Toggle(showSplineSegments, "Spline Segments");
                    if (showSplineSegments)
                    {
                        currentSelection -= designer.areas[currentArea].shapes.Length + designer.areas[currentArea].holes.Length;
                        splineSegScroll = GUILayout.BeginScrollView(splineSegScroll, GUI.skin.box, GUILayout.Height(splineSegScrollHeight));
                        dar = designer.areas[currentArea].splines[currentSelection].Sections;
                        rem = -1;
                        for (int i = 0; i < dar; i++)
                        {
                            GUILayout.BeginHorizontal();
                            int seg = EditorGUILayout.IntPopup(designer.areas[currentArea].splines[currentSelection].sectionTypes[i],
                            new string[] { "CatmullRom", "Linear", "Cubic Bezier" }, new int[] {0,1,2});
                            if(seg != designer.areas[currentArea].splines[currentSelection].sectionTypes[i])
                            {
                                designer.areas[currentArea].splines[currentSelection].sectionTypes[i] = seg;
                                updateCurrentAreaEditor = true;
                                Repaint();
                                RedrawSceneView();
                            }

                            float wgt = EditorGUILayout.FloatField(designer.areas[currentArea].splines[currentSelection].sectionWeights[i]);
                            if (wgt != designer.areas[currentArea].splines[currentSelection].sectionWeights[i])
                            {
                                designer.areas[currentArea].splines[currentSelection].sectionWeights[i] = wgt;
                                updateCurrentAreaEditor = true;
                                Repaint();
                                RedrawSceneView();
                            }
                            GUI.backgroundColor = r;
                            GUI.contentColor = Color.white;
                            if (GUILayout.Button("X", buttonStyle, GUILayout.Width(lineHeight)))
                            {
                                rem = i;
                            }
                            GUI.backgroundColor = c;
                            GUI.contentColor = cc;
                            GUILayout.EndHorizontal();
                        }
                        GUILayout.EndScrollView();
                        if (rem >= 0)
                        {
                           
                            List<int> st = new List<int>(designer.areas[currentArea].splines[currentSelection].sectionTypes);
                            List<float> wt = new List<float>(designer.areas[currentArea].splines[currentSelection].sectionWeights);
                            st.RemoveAt(rem); wt.RemoveAt(rem);
                            designer.areas[currentArea].splines[currentSelection].sectionTypes = st.ToArray();
                            designer.areas[currentArea].splines[currentSelection].sectionWeights = wt.ToArray();
                            if (st.Count < 6)
                            {
                                splineSegScrollHeight = (st.Count + 1) * lineHeight;
                            }
                            else
                            {
                                splineSegScrollHeight = 6 * lineHeight;
                            }
                        }

                        currentSelection += designer.areas[currentArea].shapes.Length + designer.areas[currentArea].holes.Length;
                        
                    }


                    switch (selectedTool) {
					case 2:
						//Tile
						selectedWallpaper = (TileGroup.WallpaperGroup)EditorGUILayout.EnumPopup("Wallpaper",selectedWallpaper); 
						break;
					case 3:
                        //Paint
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        DrawPaintParameters(r, c, cc);
                        break;
					}
					break;
				case 2:
					//Tiles
					switch (selectedTool) {
					case 1:
						//Manipulate Tiles
						break;
					case 3:
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        DrawPaintParameters(r, c, cc);
                        break;
					}
					break;
				}
			}
			if (EditorGUI.EndChangeCheck ()) {
				//serializedObject.ApplyModifiedProperties ();
				EditorUtility.SetDirty (designer);
				EditorSceneManager.MarkSceneDirty (SceneManager.GetActiveScene ());
			}
			serializedObject.ApplyModifiedProperties ();
		}

        public void DrawPaintParameters(Color r, Color c, Color cc) {
            int rem, dar;
            designer.brushSize = EditorGUILayout.Vector3Field("Brush Size", designer.brushSize);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("painter"), true);

            showPallet = GUILayout.Toggle(showPallet, "Pallet");
            if (showPallet)
            {
                palScroll = GUILayout.BeginScrollView(palScroll, GUI.skin.box, GUILayout.Height(palScrollHeight));
                dar = designer.painter.pallet.Length;
                rem = -1;
                for (int i = 0; i < dar; i++)
                {
                    GUILayout.BeginHorizontal(GUILayout.Height(lineHeight * 2));
                    if (currentPalletItem != i && GUILayout.Button(paintPreview[i], GUI.skin.box, GUILayout.Height(lineHeight * 2)))
                    {
                        currentPalletItem = i;
                    }
                    else if (currentPalletItem == i)
                    {
                        GUILayout.Box(paintPreview[i], GUILayout.Height(lineHeight * 2));
                    }
                    GUI.backgroundColor = r;
                    GUI.contentColor = Color.white;
                    if (GUILayout.Button("X", buttonStyle, GUILayout.Width(lineHeight * 2), GUILayout.Height(lineHeight * 2)))
                    {
                        rem = i;
                    }
                    GUI.backgroundColor = c;
                    GUI.contentColor = cc;
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
                if (rem >= 0)
                {
                    if (currentPalletItem >= rem)
                    {
                        currentPalletItem--;
                    }
                    List<PalletItem> palList = new List<PalletItem>(designer.painter.pallet);
                    palList.RemoveAt(rem);
                    designer.painter.pallet = palList.ToArray();

                    if (designer.painter.pallet.Length < 6)
                    {
                        palScrollHeight = (designer.painter.pallet.Length + 1) * lineHeight * 2;
                    }
                    else
                    {
                        palScrollHeight = 6 * lineHeight * 2;
                    }
                }
                if (GUILayout.Button("New Pallet Item"))
                {
                    List<PalletItem> palList = new List<PalletItem>(designer.painter.pallet);
                    palList.Add(new PalletItem());
                    designer.painter.pallet = palList.ToArray();
                    if (designer.painter.pallet.Length < 6)
                    {
                        palScrollHeight = (designer.painter.pallet.Length + 1) * lineHeight * 2;
                    }
                    else
                    {
                        palScrollHeight = 6 * lineHeight * 2;
                    }
                    RedrawSceneView();
                }
                EditorGUILayout.Space();
                if (currentPalletItem >= 0)
                {
                    PaintSample baseIt = (PaintSample)EditorGUILayout.ObjectField(new GUIContent("Pallet Item"), designer.painter.pallet[currentPalletItem].baseItem, typeof(PaintSample), true);
                    if (baseIt != designer.painter.pallet[currentPalletItem].baseItem)
                    {
                        designer.painter.pallet[currentPalletItem].baseItem = baseIt;
                        GetPaintPreviews();
                    }
                    designer.painter.pallet[currentPalletItem].maxInstances = EditorGUILayout.IntField("Max Instances", designer.painter.pallet[currentPalletItem].maxInstances);
                    designer.painter.pallet[currentPalletItem].recycleIfMaxed = GUILayout.Toggle(designer.painter.pallet[currentPalletItem].recycleIfMaxed, "Recycle if Maxed");
                }
            }
        }

		public void OnSceneGUI(){
            
			camera = SceneView.currentDrawingSceneView.camera;
			HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
			key = 0;

			if (Event.current.control) {
				//Delete
				key = 1;
			} 
			if (Event.current.alt) {
				//Snap to Grid
				key = 2;
			}
			if (Event.current.shift) {
				//Add
				key = 3;

			}
            
            if (selectedArea != -1){
				if (currentArea != selectedArea) {
					currentArea = selectedArea;
					currentSelection = -1;
				}
				holdingID = -1;
				if (currentArea >= 0) {
					//Debug.Log (currentArea + " " + designer.areas.Length);
					currentAreaEditor = new DesignAreaEditor (designer.areas [currentArea], designer.splineSmoothing);
                    updateCurrentAreaEditor = false;
				}
				selectedArea = -1;
				selectedTab = -1;
				RedrawSceneView ();
			}
            if (updateCurrentAreaEditor)
            {
                currentAreaEditor = new DesignAreaEditor(designer.areas[currentArea], designer.splineSmoothing);
                updateCurrentAreaEditor = false;
            }
            if (currentArea >= 0 && currentArea < designer.areas.Length) {
				if (currentSelection >= 0) {
					int selC = designer.areas [currentArea].shapes.Length + currentAreaEditor.holes.Length;
					if (currentSelection < selC) {
						selectedTab = 0;
					} else if (currentSelection < (selC + currentAreaEditor.splines.Length)) {
						selectedTab = 1;
					} else if (currentSelection < (selC + currentAreaEditor.splines.Length + currentAreaEditor.tileGroups.Length)) {
						selectedTab = 2;
					}
				} else {
					selectedTab = -1;
				}

				switch (selectedTab) {
				case 0:
					PolygonView ();
					break;

				case 1:
					SplineView ();
					break;

				case 2:
					TileView ();
					break;

				}

				//Draw our tool bar.
				if (selectedTab >= 0) {
					Color bg = Color.gray;
					GUI.backgroundColor = bg;
					Rect screenRect = new Rect (0, 0, camera.pixelWidth, lineHeight*2);
					Handles.BeginGUI ();
					GUILayout.BeginArea (screenRect);
					GUILayout.BeginHorizontal ();
                    int selt = selectedTool;
					switch (selectedTab) {
					case 0:
						
						if (selectedTool != 0) {
							if (GUILayout.Button (new GUIContent (icons [0], "Select shape."), GUI.skin.box, GUILayout.Width (lineHeight * 2))) {
								selectedTool = 0;
							} 
						} else {
							GUI.backgroundColor = Color.white;
							GUILayout.Box (icons [0], GUILayout.Width (lineHeight * 2));
							GUI.backgroundColor = bg;
						}
						if (selectedTool != 1) {
							if (GUILayout.Button (new GUIContent (icons [1], "Manipulate shape."), GUI.skin.box, GUILayout.Width (lineHeight * 2))) {
								selectedTool = 1;
							}
						} else {
							GUI.backgroundColor = Color.white;
							GUILayout.Box (icons [1], GUILayout.Width (lineHeight * 2));
							GUI.backgroundColor = bg;
						}
						if (selectedTool != 2){
							if (GUILayout.Button (new GUIContent (icons [3], "Tile shape."), GUI.skin.box, GUILayout.Width (lineHeight * 2))) {
								selectedTool = 2;
							}
						}else {
							GUI.backgroundColor = Color.white;
							GUILayout.Box (icons [3], GUILayout.Width (lineHeight * 2));
							GUI.backgroundColor = bg;
						}
						if (selectedTool != 3 ) { 
							if (GUILayout.Button (new GUIContent (icons [9], "Paint within shape."), GUI.skin.box, GUILayout.Width (lineHeight * 2))) {
								selectedTool = 3;
							}
						}else {
							GUI.backgroundColor = Color.white;
							GUILayout.Box (icons [9], GUILayout.Width (lineHeight * 2));
							GUI.backgroundColor = bg;
						}
                            
                            if (selectedTool != 4 ){
                                if (GUILayout.Button (new GUIContent (icons [10], "Fill shape with paint."), GUI.skin.box, GUILayout.Width (lineHeight * 2))) {
                                    selectedTool = 4;
                                }
                            }else {
                                GUI.backgroundColor = Color.white;
                                GUILayout.Box (icons [10], GUILayout.Width (lineHeight * 2));
                                GUI.backgroundColor = bg;
                            }
                            if (selectedTool != 5 ){
                                if (GUILayout.Button (new GUIContent (icons [9], "Paint shape outline."), GUI.skin.box, GUILayout.Width (lineHeight * 2))) {
                                    selectedTool = 5;
                                }
                            }else {
                                GUI.backgroundColor = Color.white;
                                GUILayout.Box (icons [9], GUILayout.Width (lineHeight * 2));
                                GUI.backgroundColor = bg;
                            }
                            if (selectedTool != 6 ){
                                if (GUILayout.Button (new GUIContent (icons [8], "Create collider from shape."), GUI.skin.box, GUILayout.Width (lineHeight * 2))) {
                                    selectedTool = 6;
                                }
                            }else {
                                GUI.backgroundColor = Color.white;
                                GUILayout.Box (icons [8], GUILayout.Width (lineHeight * 2));
                                GUI.backgroundColor = bg;
                            }
                            if (selectedTool != 7 ){
                                if (GUILayout.Button (new GUIContent (icons [2], "Create Spline from shape."), GUI.skin.box, GUILayout.Width (lineHeight * 2))) {
                                    selectedTool = 7;
                                }
                            }else {
                                GUI.backgroundColor = Color.white;
                                GUILayout.Box (icons [2], GUILayout.Width (lineHeight * 2));
                                GUI.backgroundColor = bg;
                            }
                            if (selectedTool != 8 ){
                                if (GUILayout.Button (new GUIContent (icons [10], "Create Sprite from shape."), GUI.skin.box, GUILayout.Width (lineHeight * 2))) {
                                    selectedTool = 8;
                                }
                            }else {
                                GUI.backgroundColor = Color.white;
                                GUILayout.Box (icons [10], GUILayout.Width (lineHeight * 2));
                                GUI.backgroundColor = bg;
                            }
                            
                            /*
                            GUI.backgroundColor = Color.red;
                            GUILayout.Box(icons[10], GUILayout.Width(lineHeight * 2));
                            GUILayout.Box(icons[9], GUILayout.Width(lineHeight * 2));
                            GUILayout.Box(icons[8], GUILayout.Width(lineHeight * 2));
                            GUILayout.Box(icons[2], GUILayout.Width(lineHeight * 2));
                            GUILayout.Box(icons[10], GUILayout.Width(lineHeight * 2));
                            GUI.backgroundColor = bg;
                            */

                            break;

					case 1:
						if (selectedTool != 0) {
							if (GUILayout.Button (new GUIContent (icons [0], "Select Spline."), GUI.skin.box, GUILayout.Width (lineHeight * 2))) {
								selectedTool = 0;
							} 
						} else {
							GUI.backgroundColor = Color.white;
							GUILayout.Box (icons [0], GUILayout.Width (lineHeight * 2));
							GUI.backgroundColor = bg;
						}

						if (selectedTool != 1) {
							if (GUILayout.Button (new GUIContent(icons [2],"Manipulate spline."),GUI.skin.box, GUILayout.Width (lineHeight*2))) {
								selectedTool = 1;
							}
						} else {
							GUI.backgroundColor = Color.white;
							GUILayout.Box (icons [2], GUILayout.Width (lineHeight * 2));
							GUI.backgroundColor = bg;
						}


						if (selectedTool != 2) {
							if (GUILayout.Button (new GUIContent(icons [3],"Tile spline."), GUI.skin.box,GUILayout.Width (lineHeight*2))) {
								selectedTool = 2;
							}
						} else {
							GUI.backgroundColor = Color.white;
							GUILayout.Box (icons [3], GUILayout.Width (lineHeight * 2));
							GUI.backgroundColor = bg;
						}

                        
						if (selectedTool != 3) {
							if (GUILayout.Button (new GUIContent(icons [9],"Paint along spline."),GUI.skin.box, GUILayout.Width (lineHeight*2))) {
								selectedTool = 3;
							}
						} else {
							GUI.backgroundColor = Color.white;
							GUILayout.Box (icons [9], GUILayout.Width (lineHeight * 2));
							GUI.backgroundColor = bg;
						}

                            
                        if (selectedTool != 4) {
                            if (GUILayout.Button (new GUIContent(icons [1],"Create shape from spline."),GUI.skin.box, GUILayout.Width (lineHeight*2))) {
                                selectedTool = 4;
                            }
                        } else {
                            GUI.backgroundColor = Color.white;
                            GUILayout.Box (icons [1], GUILayout.Width (lineHeight * 2));
                            GUI.backgroundColor = bg;
                        }
                        break;

					case 2:

						if (selectedTool != 0) {
							if (GUILayout.Button (new GUIContent(icons [0],"Select tile group."),GUI.skin.box, GUILayout.Width (lineHeight*2))) {
								selectedTool = 0;
							}
						} else {
							GUI.backgroundColor = Color.white;
							GUILayout.Box (icons [0], GUILayout.Width (lineHeight * 2));
							GUI.backgroundColor = bg;
						}


						if (selectedTool != 1) {
							if (GUILayout.Button (new GUIContent(icons [3],"Manipulate tiles."),GUI.skin.box, GUILayout.Width (lineHeight*2))) {
								selectedTool = 1;
							}
						} else {
							GUI.backgroundColor = Color.white;
							GUILayout.Box (icons [3], GUILayout.Width (lineHeight * 2));
							GUI.backgroundColor = bg;
						}


						if (selectedTool != 2) {
							if (GUILayout.Button (new GUIContent(icons [1],"Create shape from tiles."),GUI.skin.box, GUILayout.Width (lineHeight*2))) {
								selectedTool = 2;
							}
						} else {
							GUI.backgroundColor = Color.white;
							GUILayout.Box (icons [1], GUILayout.Width (lineHeight * 2));
							GUI.backgroundColor = bg;
						}


						if (selectedTool != 3) {
							if (GUILayout.Button (new GUIContent(icons [9],"Paint tiles."),GUI.skin.box, GUILayout.Width (lineHeight*2))) {
								selectedTool = 3;
							}
						} else {
							GUI.backgroundColor = Color.white;
							GUILayout.Box (icons [9], GUILayout.Width (lineHeight * 2));
							GUI.backgroundColor = bg;
						}

						break;
					}
                    if(selt != selectedTool)
                    {
                        Repaint();
                    }
					GUILayout.EndHorizontal ();
					GUILayout.EndArea ();
					Handles.EndGUI ();
				}
			}
		}

		public void DrawPolygon(int id){
			Color hand = Handles.color;
			if (id >= 0 && id < (currentAreaEditor.shapes.Length + currentAreaEditor.holes.Length)) {
				
				if (id < currentAreaEditor.shapes.Length) {
					if (id != currentSelection) {
						Handles.color = currentAreaEditor.shapes [id].color;
					} else {
						Handles.color = Color.white;
					}
					Handles.DrawPolyLine (currentAreaEditor.shapes [id].points);
					Handles.DrawLine (currentAreaEditor.shapes [id].points [currentAreaEditor.shapes [id].points.Length - 1], currentAreaEditor.shapes [id].points [0]);

				} else {
					id -= currentAreaEditor.shapes.Length;
					if (id != currentSelection) {
						Handles.color = currentAreaEditor.holes [id].color;
					} else {
						Handles.color = Color.white;
					}
					Handles.DrawPolyLine (currentAreaEditor.holes [id].points);
					Handles.DrawLine (currentAreaEditor.holes [id].points [currentAreaEditor.holes [id].points.Length - 1], currentAreaEditor.holes [id].points [0]);

				}
				Handles.color = hand;
			}
		}

		public void DrawSpline(int id){
			if (id == currentSelection) {
				Handles.color = Color.white;
			} else {
				Handles.color = currentAreaEditor.splines [id - currentAreaEditor.shapes.Length - currentAreaEditor.holes.Length].color;
			}
			id -= currentAreaEditor.shapes.Length + currentAreaEditor.holes.Length;
			if (id >= 0 && id < currentAreaEditor.splines.Length) {
				Handles.DrawAAPolyLine (currentAreaEditor.splines [id].points);
			}
		}

		public void DrawTileGroup(int id, bool justOutline = false){
			//Debug.Log ("Drawing tile group " + currentAreaEditor.tileGroups[id].name + ". Points:" + currentAreaEditor.tileGroups[id].points.Length+
			//	" / " + designer.areas[currentArea].tileGroups[id].cellPoints.Length);
			//Debug.Log("TileGroup: " + id);
			bool selected = false;
			Color hand = Handles.color, primary = Color.green, secondary= new Color(0.25f, 0.85f, 0.25f, 1);
			if ((id + currentAreaEditor.shapes.Length + currentAreaEditor.holes.Length + currentAreaEditor.splines.Length) == currentSelection) {
				selected = true;
			}

			if (id >= 0 && id < currentAreaEditor.tileGroups.Length) {
				if (!selected) {
					
					primary = currentAreaEditor.tileGroups [id].color;
					secondary = designer.areas[currentArea].tileGroups [id].sColor;
				}

				switch (designer.areas [currentArea].canvasType) {
				case Designer.DesignCanvasType.Mesh:
					
					for (int i = designer.areas [currentArea].tileGroups [id].edges.Length - 1; i >= 0; i--) {
						if (!designer.areas [currentArea].tileGroups [id].edges [i].isBorder) {
							Handles.color = secondary;
						} else {
							Handles.color = primary;
						}
						//Now I need to find PAIRS of edge ids... jebus...

						bool allUsed = false;
						while (!allUsed) {
							int currentA = -1, currentB = -1;
							for (int j = 0; j < currentAreaEditor.tileGroups [id].points.Length; j++) {
							
							}
							//If we've got a currentA and B, draw a line.
							if (currentA >= 0 && currentB >= 0) {
							
							}
						}
					}
					break;
				default:
					
					for (int i = designer.areas [currentArea].tileGroups [id].edges.Length - 1; i >= 0; i--) {

						if (!designer.areas [currentArea].tileGroups [id].edges [i].isBorder && !justOutline) {
							Handles.color = secondary;
							Handles.DrawLine (currentAreaEditor.tileGroups [id].points [designer.areas [currentArea].tileGroups [id].edges [i].pointA],
								currentAreaEditor.tileGroups [id].points [designer.areas [currentArea].tileGroups [id].edges [i].pointB]);
						} else {
							Handles.color = primary;
							Handles.DrawLine (currentAreaEditor.tileGroups [id].points [designer.areas [currentArea].tileGroups [id].edges [i].pointA],
								currentAreaEditor.tileGroups [id].points [designer.areas [currentArea].tileGroups [id].edges [i].pointB]);
						}

					}
					break;

				}
				Handles.color = hand;
			}
		}

		public void PolygonView(){
			
			int i, j, k;
			Color handlesColor = Handles.color, guiColor = GUI.color;
			mousePos = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition).GetPoint (Vector3.Distance (camera.transform.position, designer.areas[currentArea].transform.position));

			switch(selectedTool){
			case 0:
				//Select Shape / Hole
				for (i = currentAreaEditor.shapes.Length+currentAreaEditor.holes.Length - 1; i >= 0; i--) {
					DrawPolygon (i);
				}
				Handles.BeginGUI ();

				for (i = currentAreaEditor.shapes.Length - 1; i >= 0; i--) {
					
					if (i != currentSelection) {
						GUI.color = currentAreaEditor.shapes [i].color;
						if (GUI.Button (currentAreaEditor.shapes [i].labelRect, currentAreaEditor.shapes [i].name)) {
							currentSelection = i;
						}
					} else {
						GUI.color = Color.white;
						GUI.Label (currentAreaEditor.shapes [i].labelRect, currentAreaEditor.shapes [i].name, GUI.skin.box);
					}
				}
				currentSelection -= currentAreaEditor.shapes.Length;
				for (i = currentAreaEditor.holes.Length - 1; i >= 0; i--) {
					
					if (i != currentSelection) {
						GUI.color = currentAreaEditor.holes[i].color;
						if (GUI.Button (currentAreaEditor.holes [i].labelRect, currentAreaEditor.holes [i].name)) {
							currentSelection = i;
						}
					} else {
						GUI.color = Color.white;
						GUI.Label (currentAreaEditor.holes [i].labelRect, currentAreaEditor.holes [i].name, GUI.skin.box);
					}
				}
				currentSelection += currentAreaEditor.shapes.Length;
				Handles.EndGUI ();

				break;

			case 1:
				// Manipulate selected polygon.
				RedrawSceneView ();
				/*
				 * Show only currently selected shape, with handles.
				*/
				float hs;
				// -1 is neutral
				// 0 and greater is our current shape point id.
				// -2 is holding area point
				// -3 or lower is our shape id point (Mathf.Abs(holdingID+3);
				Vector3 vec1;

				if (holdingID != -1 && currentArea >= 0) {
					if (Event.current.isMouse && Event.current.type == EventType.MouseUp) {
						holdingID = -1;
						currentAreaEditor.UpdateShapes ();
						EditorUtility.SetDirty (designer);
						EditorSceneManager.MarkSceneDirty (SceneManager.GetActiveScene ());
					} 
				}

				i = currentSelection;
				if(i < currentAreaEditor.shapes.Length){
					j = currentAreaEditor.shapes[i].points.GetClosestPoint (mousePos);
					hs = HandleUtility.GetHandleSize (currentAreaEditor.shapes[i].centerPoint) * 0.1f;
					switch (key) {
					case 0:
						Handles.color = Color.white;
						vec1 = Handles.FreeMoveHandle (currentAreaEditor.shapes[i].centerPoint, Quaternion.identity, hs, Vector3.zero, Handles.DotCap);
						if (vec1 != currentAreaEditor.shapes[i].centerPoint) { 
							holdingID = (i+3)*-1;
							//Get the difference between new and old. Add it to each of the points in each of my currentAreaEditor.shapes.
							vec1 -= currentAreaEditor.shapes[i].centerPoint;
							currentAreaEditor.shapes [i].Offset (vec1);
							RedrawSceneView ();
						}



						for (k = currentAreaEditor.shapes [i].points.Length - 1; k >= 1; k--) {
							Handles.DrawLine (currentAreaEditor.shapes [i].points [k], currentAreaEditor.shapes [i].points [k - 1]);
						}
						Handles.DrawLine (currentAreaEditor.shapes [i].points [0], currentAreaEditor.shapes [i].points [currentAreaEditor.shapes [i].points.Length - 1]);

						Handles.color = Color.green;
						vec1 = Handles.FreeMoveHandle (currentAreaEditor.shapes[i].points [j], Quaternion.identity, HandleUtility.GetHandleSize (currentAreaEditor.shapes[currentSelection].points [j]) * 0.1f, Vector3.zero, Handles.CubeCap);
						if (vec1 != currentAreaEditor.shapes[i].points [j]) {
							holdingID = j;
							currentAreaEditor.shapes[i].points[j] = vec1;
							currentAreaEditor.shapes [i].pointsDirty[j] = true;
						}
						break;

					case 1:
						Handles.color = Color.red;
						if(Handles.Button(currentAreaEditor.shapes[i].centerPoint, Quaternion.identity, hs, hs*1.25f,Handles.DotCap)){
							SerializedProperty areas = serializedObject.FindProperty ("areas");
							SerializedProperty cShapes = areas.GetArrayElementAtIndex (currentArea).FindPropertyRelative("shapes");
							cShapes.DeleteArrayElementAtIndex (i);
							if (currentSelection >= i) {
								currentSelection--;
							}
							holdingID = -99;
							return;
						}


						for (k = currentAreaEditor.shapes [i].points.Length - 1; k >= 1; k--) {
							if (k == j || k - 1 == j) {
								Handles.color = Color.red;
							} else {
								Handles.color = Color.white;
							}
							Handles.DrawLine (currentAreaEditor.shapes [i].points [k], currentAreaEditor.shapes [i].points [k - 1]);
						}
						if (j == 0 || i == j) {
							Handles.color = Color.red;
						} else {
							Handles.color = Color.white;
						}
						Handles.DrawLine (currentAreaEditor.shapes [i].points [0], currentAreaEditor.shapes [i].points [currentAreaEditor.shapes[i].points.Length-1]);

						Handles.color = Color.red;
						hs = HandleUtility.GetHandleSize (currentAreaEditor.shapes [i].points [j]) * 0.1f;
						if (Handles.Button (currentAreaEditor.shapes [i].points [j], Quaternion.identity, hs, hs * 1.25f, Handles.CubeCap)) {
							//Delete this point
							List<Vector3> newPoints = new List<Vector3> (currentAreaEditor.shapes [i].points);
							List<bool> newBools = new List<bool> (currentAreaEditor.shapes [i].pointsDirty);
							newBools.RemoveAt (j);
							newPoints.RemoveAt (j);
							currentAreaEditor.shapes [i].points = newPoints.ToArray ();
							currentAreaEditor.shapes [i].pointsDirty = newBools.ToArray ();
							currentAreaEditor.shapes [i].dirty = true;
							holdingID = -99;
						}

						break;

					case 2:
						Handles.color = Color.white;
						vec1 = Handles.FreeMoveHandle (currentAreaEditor.shapes[i].centerPoint, Quaternion.identity, hs, Vector3.one*designer.snap, Handles.DotCap);
						if (vec1 != currentAreaEditor.shapes[i].centerPoint) { 
							holdingID = (i+3)*-1;
							//Get the difference between new and old. Add it to each of the points in each of my currentAreaEditor.shapes.
							vec1 -= currentAreaEditor.shapes[i].centerPoint;
							currentAreaEditor.shapes [i].Offset (vec1);
							RedrawSceneView ();
						}


						for (k = currentAreaEditor.shapes [i].points.Length - 1; k >= 1; k--) {
							Handles.DrawLine (currentAreaEditor.shapes [i].points [k], currentAreaEditor.shapes [i].points [k - 1]);
						}
						Handles.DrawLine (currentAreaEditor.shapes [i].points [0], currentAreaEditor.shapes [i].points [currentAreaEditor.shapes [i].points.Length - 1]);

						Handles.color = Color.green;
						vec1 = Handles.FreeMoveHandle (currentAreaEditor.shapes[i].points [j], Quaternion.identity, HandleUtility.GetHandleSize (currentAreaEditor.shapes[currentSelection].points [j]) * 0.1f, Vector3.zero, Handles.CubeCap);
						if (vec1 != currentAreaEditor.shapes[i].points [j]) {
							holdingID = j;
							currentAreaEditor.shapes[i].points[j] = vec1;
							currentAreaEditor.shapes [i].pointsDirty[j] = true;
						}
						break;

					case 3:
						Handles.color = Color.yellow;

						for (k = currentAreaEditor.shapes [i].points.Length - 1; k >= 1; k--) {
							Handles.DrawLine (currentAreaEditor.shapes [i].points [k], currentAreaEditor.shapes [i].points [k - 1]);
						}
						Handles.DrawLine (currentAreaEditor.shapes [i].points [0], currentAreaEditor.shapes [i].points [currentAreaEditor.shapes [i].points.Length - 1]);
						vec1 = currentAreaEditor.shapes [i].points.GetClosestPointOnPolygon (mousePos, 20, ref j);
						j++;
						hs = HandleUtility.GetHandleSize (vec1) * 0.1f;
						if (Handles.Button (vec1, Quaternion.identity, hs, hs*1.25f, Handles.CubeCap)) {
							List<Vector3> newPoints = new List<Vector3> (currentAreaEditor.shapes [i].points);
							List<bool> newBools = new List<bool> (currentAreaEditor.shapes [i].pointsDirty);
							newPoints.Insert (j, vec1);
							newBools.Insert (j, true);
							currentAreaEditor.shapes [i].points = newPoints.ToArray ();
							currentAreaEditor.shapes [i].pointsDirty = newBools.ToArray();
							currentAreaEditor.shapes [i].dirty = true;
							holdingID = -99;
						}
						break;
					}

				}else {

					i -= currentAreaEditor.shapes.Length;
					j = currentAreaEditor.shapes[i].points.GetClosestPoint (mousePos);
					hs = HandleUtility.GetHandleSize (currentAreaEditor.holes[i].centerPoint) * 0.1f;
					switch (key) {
					case 0:
						Handles.color = Color.black;
						vec1 = Handles.FreeMoveHandle (currentAreaEditor.holes[i].centerPoint, Quaternion.identity, hs, Vector3.zero, Handles.DotCap);
						if (vec1 != currentAreaEditor.holes[i].centerPoint) { 
							holdingID = (i+3)*-1;
							//Get the difference between new and old. Add it to each of the points in each of my currentAreaEditor.holes.
							vec1 -= currentAreaEditor.holes[i].centerPoint;
							currentAreaEditor.holes [i].Offset (vec1);

							RedrawSceneView ();
						}


						for (k = currentAreaEditor.holes [i].points.Length - 1; k >= 1; k--) {
							Handles.DrawLine (currentAreaEditor.holes [i].points [k], currentAreaEditor.holes [i].points [k - 1]);
						}
						Handles.DrawLine (currentAreaEditor.holes [currentSelection].points [0], currentAreaEditor.holes [currentSelection].points [k]);

						Handles.color = Color.green;
						vec1 = Handles.FreeMoveHandle (currentAreaEditor.holes[i].points [j], Quaternion.identity, HandleUtility.GetHandleSize (currentAreaEditor.holes[currentSelection].points [j]) * 0.1f, Vector3.zero, Handles.CubeCap);
						if (vec1 != currentAreaEditor.holes[i].points [j]) {
							holdingID = j;
							currentAreaEditor.holes[i].points[j] = vec1;
							currentAreaEditor.holes [i].pointsDirty[j] = true;
						}
						break;

					case 1:
						Handles.color = Color.red;
						if(Handles.Button(currentAreaEditor.holes[i].centerPoint, Quaternion.identity, hs, hs*1.25f,Handles.DotCap)){
							SerializedProperty areas = serializedObject.FindProperty ("areas");
							SerializedProperty choles = areas.GetArrayElementAtIndex (currentArea).FindPropertyRelative("holes");
							choles.DeleteArrayElementAtIndex (i);
							if (currentSelection >= i) {
								currentSelection--;
							}
							holdingID = -99;
							return;
						}

						for (k = currentAreaEditor.holes [i].points.Length - 1; k >= 1; k--) {
							if (k == j || k - 1 == j) {
								Handles.color = Color.red;
							} else {
								Handles.color = Color.black;
							}
							Handles.DrawLine (currentAreaEditor.holes [i].points [k], currentAreaEditor.holes [i].points [k - 1]);
						}
						if (j == 0 || i == j) {
							Handles.color = Color.red;
						} else {
							Handles.color = Color.green;
						}
						Handles.DrawLine (currentAreaEditor.holes [i].points [0], currentAreaEditor.holes [i].points [currentAreaEditor.holes[i].points.Length]);

						Handles.color = Color.red;
						hs = HandleUtility.GetHandleSize (currentAreaEditor.holes [i].points [j]) * 0.1f;
						if (Handles.Button (currentAreaEditor.holes [i].points [j], Quaternion.identity, hs, hs * 1.25f, Handles.CubeCap)) {
							//Delete this point
							List<Vector3> newPoints = new List<Vector3> (currentAreaEditor.holes [i].points);
							List<bool> newBools = new List<bool> (currentAreaEditor.holes [i].pointsDirty);
							newBools.RemoveAt (j);
							newPoints.RemoveAt (j);
							currentAreaEditor.holes [i].points = newPoints.ToArray ();
							currentAreaEditor.holes [i].pointsDirty = newBools.ToArray ();
							currentAreaEditor.holes [i].dirty = true;
							holdingID = -99;
						}
						break;

					case 2:
						Handles.color = Color.black;
						vec1 = Handles.FreeMoveHandle (currentAreaEditor.holes[i].centerPoint, Quaternion.identity, hs, Vector3.one*designer.snap, Handles.DotCap);
						if (vec1 != currentAreaEditor.holes[i].centerPoint) { 
							holdingID = (i+3)*-1;
							//Get the difference between new and old. Add it to each of the points in each of my currentAreaEditor.holes.
							vec1 -= currentAreaEditor.holes[i].centerPoint;
							currentAreaEditor.holes [i].Offset (vec1);

							RedrawSceneView ();
						}

						for (k = currentAreaEditor.holes [i].points.Length - 1; k >= 1; k--) {
							Handles.DrawLine (currentAreaEditor.holes [i].points [k], currentAreaEditor.holes [i].points [k - 1]);
						}
						Handles.DrawLine (currentAreaEditor.holes [currentSelection].points [0], currentAreaEditor.holes [currentSelection].points [k]);

						Handles.color = Color.green;
						vec1 = Handles.FreeMoveHandle (currentAreaEditor.holes[i].points [j], Quaternion.identity, HandleUtility.GetHandleSize (currentAreaEditor.holes[currentSelection].points [j]) * 0.1f, Vector3.zero, Handles.CubeCap);
						if (vec1 != currentAreaEditor.holes[i].points [j]) {
							holdingID = j;
							currentAreaEditor.holes[i].points[j] = vec1;
							currentAreaEditor.holes [i].pointsDirty[j] = true;
						}
						break;

					case 3:
						Handles.color = Color.yellow;

						for (k = currentAreaEditor.holes [i].points.Length - 1; k >= 1; k--) {
							Handles.DrawLine (currentAreaEditor.holes [i].points [k], currentAreaEditor.holes [i].points [k - 1]);
						}
						Handles.DrawLine (currentAreaEditor.holes [i].points [0], currentAreaEditor.holes [i].points [currentAreaEditor.holes [i].points.Length - 1]);
						vec1 = currentAreaEditor.holes [i].points.GetClosestPointOnPolygon (mousePos, 20, ref j);
						j++;
						hs = HandleUtility.GetHandleSize (vec1) * 0.1f;
						if (Handles.Button (vec1, Quaternion.identity, hs, hs*1.25f, Handles.CubeCap)) {
							List<Vector3> newPoints = new List<Vector3> (currentAreaEditor.holes [i].points);
							List<bool> newBools = new List<bool> (currentAreaEditor.holes [i].pointsDirty);
							newPoints.Insert (j, vec1);
							newBools.Insert (j, true);
							currentAreaEditor.holes [i].points = newPoints.ToArray ();
							currentAreaEditor.holes [i].pointsDirty = newBools.ToArray();
							currentAreaEditor.holes [i].dirty = true;
							holdingID = -99;
						}
						break;
					}

				}
				break;

			case 2:
				//Tile shape.
				/*
			 * Show only currently selected shape, no handles, and Tiling Preferences side window.
			 */
				if (!toolInUse) {
					//On mouse up, if we're inside the selected shape, tile from that point to the edges.
					switch (key) {
					case 0:
				//Fill selected shape or hole with tiles.
						DrawPolygon (currentSelection);
						if (Event.current.isMouse && Event.current.type == EventType.MouseUp && Event.current.button == 0) {
							if (designer.areas [currentArea].shapes [currentSelection].ContainsPoint (mousePos)) {

								tilePolyCheck = designer.areas [currentArea].TileArea (-1, currentSelection, selectedWallpaper, mousePos); 
								toolInUse = true;
								EditorApplication.update += TilePoly;
							}
						}
						break;
					case 1:
					//fill ALL shapes and no holes with tiles.
						for (i = currentAreaEditor.shapes.Length - 1; i >= 0; i--) {
							DrawPolygon (i);
						}
						if (Event.current.isMouse && Event.current.type == EventType.MouseUp && Event.current.button == 0) {
							if (designer.areas [currentArea].shapes [currentSelection].ContainsPoint (mousePos)) {

								tilePolyCheck = designer.areas [currentArea].TileArea (-1, -1, selectedWallpaper, mousePos); 
								toolInUse = true;
								EditorApplication.update += TilePoly;
							}
						}
						break;
					}
				}

				break;
			case 3:
                    //Free Paint
                    /*
                     * Currently selected shape, no handles, Free Paint side window
                     */
                    DrawPolygon(currentSelection);
                    if (!toolInUse) {
					switch (key) {
					case 0:
						
						//On mouse down, start free paint. Only apply paint if it's within the shape, and not overlapping any holes.
						if (Event.current.isMouse && Event.current.type == EventType.MouseDown) {
							if (designer.areas [currentArea].Contains (mousePos, currentSelection)) {
								toolInUse = true;
								paintPos = mousePos;
								currentRate = EditorApplication.timeSinceStartup + designer.painter.rate;
								EditorApplication.update += FreePaint;
							}
						}

						break;
					//Paint in all shapes.
					case 1:
                        for(i = currentAreaEditor.shapes.Length-1; i >=0; i--)
                        {
                            if(i != currentSelection) { DrawPolygon(i); }
                        }
                        if (Event.current.isMouse && Event.current.type == EventType.MouseDown)
                        {
                            if (designer.areas[currentArea].Contains(mousePos, -1))
                            {
                                toolInUse = true;
                                paintPos = mousePos;
                                currentRate = EditorApplication.timeSinceStartup + designer.painter.rate;
                                EditorApplication.update += FreePaint;
                            }
                        }
                        break;
					}
				} else {
					RedrawSceneView ();
					if (doPaint && designer.areas [currentArea].Contains (mousePos, currentSelection)) {
						PaintSample paintDab = null;
						switch(designer.areas[currentArea].canvasType){
						case Designer.DesignCanvasType.World: paintDab = designer.painter.Paint ((Vector2)mousePos); break;
						case Designer.DesignCanvasType.LocalObjective: paintDab = designer.painter.Paint ((Vector2)(designer.transform.InverseTransformDirection (mousePos - designer.transform.position))); break;
						case Designer.DesignCanvasType.LocalSubjective: paintDab = designer.painter.Paint ((Vector2)(designer.transform.InverseTransformPoint (mousePos))); break;
						case Designer.DesignCanvasType.Mesh:
							//Renderer r = canvas.GetComponent<Renderer> ();
							break;
						}
						if (paintDab == null) {
							paintPos = mousePos;
							currentRate = 0;
							return;
						}

						paintDab.transform.parent = designer.areas[currentArea].transform;

						if (followPaintDirection) {
							Vector3 direction = mousePos - paintPos;
							float angle = Mathf.Atan2 (direction.y, direction.x) * Mathf.Rad2Deg;
							direction = paintDab.transform.eulerAngles;
							direction.z = angle;
							paintDab.transform.eulerAngles = direction;
						}
						paintDab.transform.localScale = designer.brushSize;
						designer.painter.ApplyPaintProperties (paintDab.transform);
						paintPos = mousePos;
						currentRate = 0;
						doPaint = false;
					}

					if (Event.current.isMouse && Event.current.type == EventType.MouseUp) {
						toolInUse = false;
					}
				}
				break;
			case 4:
				//Paint Fill
				
                if (!toolInUse)
                {
                    DrawPolygon(currentSelection);
                    //On mouse up, start paint-filling from the current point outward using our paint settings. Don't leave the shape and don't fill holes.
                    if (Event.current.isMouse && Event.current.type == EventType.MouseUp && designer.areas[currentArea].Contains(mousePos, currentSelection))
                    {
                        // fillSpread -  is the radial number of Paint Samples that each paint sample will make.
                        // fillDistance - is the distance each sample will move radially outward when copying.

                        //Start at our mouse position, assuming it's inside the shape. Go outward by fillDistance in 
                        //fillSpread radial directions away from it.toolInUse = true;
                        toolInUse = true;
                        paintPos = mousePos;
                        currentRate = EditorApplication.timeSinceStartup + designer.painter.rate;
                        tilePolyCheck = designer.FillPolygon(currentArea, currentSelection, mousePos);
                        EditorApplication.update += FillPaint;
                    }
                }
				break;
            case 5:
                //Paint Outline
                if (!toolInUse)
                {
                    DrawPolygon(currentSelection);
                    //On mouse up, if within shape,outline it with paint. Check for Edge Holes and clip around them.
                    if (Event.current.isMouse && Event.current.type == EventType.MouseUp && designer.areas[currentArea].Contains(mousePos, currentSelection))
                    {
                            toolInUse = true;
                            tilePolyCheck = designer.PaintPolygon(currentArea, currentSelection);
                            EditorApplication.update += PaintSpline;
                    }
                }
                break;
            case 6:
				//Collider
				DrawPolygon (currentSelection);
				//On mouse up, if within shape, create a collider out of it. Check for Edge Holes and clip around them.
				if (Event.current.isMouse && Event.current.type == EventType.MouseUp && designer.areas[currentArea].Contains(mousePos, currentSelection)) {
                    designer.areas[currentArea].ConvertShapeToCollider(currentSelection);
                    updateCurrentAreaEditor = true;
                    Repaint();
                }
				break;
            case 7:
                //Spline
                DrawPolygon(currentSelection);
                //On mouse up, if within shape, create a spline out of it. Check for Edge Holes and clip around them.
                if (Event.current.isMouse && Event.current.type == EventType.MouseUp && designer.areas[currentArea].Contains(mousePos, currentSelection))
                {
                    designer.areas[currentArea].ConvertShapeToSpline(currentSelection);
                    updateCurrentAreaEditor = true;
                    currentSelection = currentAreaEditor.shapes.Length + currentAreaEditor.holes.Length + 
                    currentAreaEditor.splines.Length - 1;
                    selectedTool = 0;
                    Repaint();
                }
                break;
            case 8:
                //Sprite
                DrawPolygon(currentSelection);
                //On mouse up, if within shape, create a sprite out of it. Check for Edge Holes and clip around them.
                if (Event.current.isMouse && Event.current.type == EventType.MouseUp && designer.areas[currentArea].Contains(mousePos, currentSelection))
                {
                    designer.areas[currentArea].ConvertShapeToMesh(currentSelection);
                   
                }
                break;
            }

			Handles.color = handlesColor;
			GUI.color = guiColor;
		}

		public void SplineView(){
			int i, j, k;
			mousePos = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition).GetPoint (Vector3.Distance (camera.transform.position, designer.areas[currentArea].transform.position));

			switch (selectedTool) {
			case 0:
				//Select
				j = currentAreaEditor.shapes.Length + currentAreaEditor.holes.Length;
				for (i = j+ currentAreaEditor.splines.Length-1; i >= j; i--) {
					DrawSpline (i);
				}
				Handles.BeginGUI ();

				currentSelection -= j;
                    Color co = GUI.color;
				for (i = currentAreaEditor.splines.Length - 1; i >= 0; i--) {

					if (i != currentSelection) {
						GUI.color = currentAreaEditor.splines[i].color;
						if (GUI.Button (currentAreaEditor.splines [i].labelRect, currentAreaEditor.splines [i].name)) {
							currentSelection = i;
						}
					} else {
						GUI.color = Color.white;
						GUI.Label (currentAreaEditor.splines [i].labelRect, currentAreaEditor.splines [i].name, GUI.skin.box);
					}
				}
                    GUI.color = co;
				currentSelection += j;
				Handles.EndGUI ();
				break;
			case 1:
				//manipulate
				RedrawSceneView ();

				float hs;
				// -1 is neutral
				// 0 and greater is our current shape point id.
				// -2 is holding area point
				// -3 or lower is our shape id point (Mathf.Abs(holdingID+3);
				Vector3 vec1;

				if (holdingID != -1 && currentArea >= 0) {
					if (Event.current.isMouse && Event.current.type == EventType.MouseUp) {
						holdingID = -1;
						currentAreaEditor.UpdateSplines ();
						EditorUtility.SetDirty (designer);
						EditorSceneManager.MarkSceneDirty (SceneManager.GetActiveScene ());
					} 
				}

				i = currentSelection - currentAreaEditor.shapes.Length - currentAreaEditor.holes.Length;
				//Debug.Log (mousePos);
				j = currentAreaEditor.splineControls [i].points.GetClosestPoint (mousePos);

				int ii, jj = currentAreaEditor.splines [i].points.GetClosestPoint (currentAreaEditor.splineControls [i].points [j]);
				hs = HandleUtility.GetHandleSize (currentAreaEditor.splineControls [i].centerPoint) * 0.1f;
				switch (key) {
				case 0:
					Handles.color = Color.white;
					vec1 = Handles.FreeMoveHandle (currentAreaEditor.splineControls [i].centerPoint, Quaternion.identity, hs, Vector3.zero, Handles.DotCap);
					if (vec1 != currentAreaEditor.splineControls [i].centerPoint) { 
						holdingID = (i + 3) * -1;
						//Get the difference between new and old. Add it to each of the points in each of my currentAreaEditor.shapes.
						vec1 -= currentAreaEditor.splineControls [i].centerPoint;
						currentAreaEditor.splineControls [i].Offset (vec1);
                        //currentAreaEditor.splines[i].Offset(vec1);	
					}


					Handles.color = Color.green;
					for (k = currentAreaEditor.splines [i].points.Length - 1; k >= 1; k--) {
						Handles.DrawLine (currentAreaEditor.splines [i].points [k], currentAreaEditor.splines [i].points [k - 1]);
					}
					//Handles.DrawLine (currentAreaEditor.splines [i].points [0], currentAreaEditor.splines [i].points [currentAreaEditor.splines [i].points.Length - 1]);

					vec1 = Handles.FreeMoveHandle (currentAreaEditor.splineControls [i].points [j], Quaternion.identity, HandleUtility.GetHandleSize (currentAreaEditor.splineControls [i].points [j]) * 0.1f, Vector3.zero, Handles.CubeCap);
					if (vec1 != currentAreaEditor.splineControls [i].points [j]) {
						holdingID = j;
						currentAreaEditor.splineControls [i].points [j] = vec1;
						currentAreaEditor.splineControls [i].pointsDirty [j] = true;
                        currentAreaEditor.splineControls[i].dirty = true;
					}
					break;

				case 1:
					Handles.color = Color.red;
					if (Handles.Button (currentAreaEditor.splineControls [i].centerPoint, Quaternion.identity, hs, hs * 1.25f, Handles.DotCap)) {
						SerializedProperty areas = serializedObject.FindProperty ("areas");
						SerializedProperty cShapes = areas.GetArrayElementAtIndex (currentArea).FindPropertyRelative ("splines");
						cShapes.DeleteArrayElementAtIndex (i);
						i--;
						holdingID = -99;
						return;
					}


					for (k = currentAreaEditor.splineControls [i].points.Length - 1; k >= 1; k--) {
						if (k == j || k - 1 == j) {
							Handles.color = Color.red;
						} else {
							Handles.color = Color.white;
						}
						ii = currentAreaEditor.splines [i].points.GetClosestPoint (currentAreaEditor.splineControls [i].points [k]);
						if (jj>ii) {
							int kk = ii;
							ii = jj;
							jj = kk;
						}
						for (int kk = ii - 1; kk >= jj; kk--) {
							Handles.DrawLine (currentAreaEditor.splines [i].points [kk], currentAreaEditor.splines [i].points [kk - 1]);
						}
					}
					if (j == 0 || i == j) {
						Handles.color = Color.red;
					} else {
						Handles.color = Color.white;
					}
					//Handles.DrawLine (currentAreaEditor.splineControls [i].points [0], currentAreaEditor.splineControls [i].points [currentAreaEditor.splineControls [i].points.Length - 1]);
					ii = currentAreaEditor.splines [i].points.GetClosestPoint (currentAreaEditor.splineControls [i].points [0]);
					if (jj>ii) {
						int kk = ii;
						ii = jj;
						jj = kk;
					}
					for (int kk = ii - 1; kk >= jj; kk--) {
						Handles.DrawLine (currentAreaEditor.splines [i].points [kk], currentAreaEditor.splines [i].points [kk - 1]);
					}

					Handles.color = Color.red;
					hs = HandleUtility.GetHandleSize (currentAreaEditor.splineControls [i].points [j]) * 0.1f;
					if (Handles.Button (currentAreaEditor.splineControls [i].points [j], Quaternion.identity, hs, hs * 1.25f, Handles.CubeCap)) {
						//Delete this point
						List<Vector3> newPoints = new List<Vector3> (currentAreaEditor.splineControls [i].points);
						List<bool> newBools = new List<bool> (currentAreaEditor.splineControls [i].pointsDirty);
						newBools.RemoveAt (j);
						newPoints.RemoveAt (j);
						currentAreaEditor.splineControls [i].points = newPoints.ToArray ();
						currentAreaEditor.splineControls [i].pointsDirty = newBools.ToArray ();
						currentAreaEditor.splineControls [i].dirty = true;
						holdingID = -99;
					}

					break;

				case 2:
					Handles.color = Color.white;
					vec1 = Handles.FreeMoveHandle (currentAreaEditor.splineControls [i].centerPoint, Quaternion.identity, hs, Vector3.one * designer.snap, Handles.DotCap);
					if (vec1 != currentAreaEditor.splineControls [i].centerPoint) { 
						holdingID = (i + 3) * -1;
						//Get the difference between new and old. Add it to each of the points in each of my currentAreaEditor.shapes.
						vec1 -= currentAreaEditor.splineControls [i].centerPoint;
						currentAreaEditor.splineControls [i].Offset (vec1);
						RedrawSceneView ();
					}



					Handles.color = Color.green;
					for (k = currentAreaEditor.splines [i].points.Length - 1; k >= 1; k--) {
						Handles.DrawLine (currentAreaEditor.splines [i].points [k], currentAreaEditor.splines [i].points [k - 1]);
					}
					Handles.DrawLine (currentAreaEditor.splines [i].points [0], currentAreaEditor.splines [i].points [currentAreaEditor.splines [i].points.Length - 1]);

					vec1 = Handles.FreeMoveHandle (currentAreaEditor.splineControls [i].points [j], Quaternion.identity, HandleUtility.GetHandleSize (currentAreaEditor.shapes [currentSelection].points [j]) * 0.1f, Vector3.zero, Handles.CubeCap);
					if (vec1 != currentAreaEditor.splineControls [i].points [j]) {
						holdingID = j;
						currentAreaEditor.splineControls [i].points [j] = vec1;
						currentAreaEditor.splineControls [i].pointsDirty [j] = true;
					}
					break;

				case 3:
					Handles.color = Color.yellow;
					for (k = currentAreaEditor.splines [i].points.Length - 1; k >= 1; k--) {
						Handles.DrawLine (currentAreaEditor.splines [i].points [k], currentAreaEditor.splines [i].points [k - 1]);
					}
					//Handles.DrawLine (currentAreaEditor.splines [i].points [0], currentAreaEditor.splines [i].points [currentAreaEditor.splines [i].points.Length - 1]);


					vec1 = currentAreaEditor.splineControls [i].points.GetClosestPointOnPolygon (mousePos, 20, ref j);
					j++;
					hs = HandleUtility.GetHandleSize (vec1) * 0.1f;
					if (Handles.Button (vec1, Quaternion.identity, hs, hs * 1.25f, Handles.CubeCap)) {
						List<Vector3> newPoints = new List<Vector3> (currentAreaEditor.splineControls [i].points);
						List<bool> newBools = new List<bool> (currentAreaEditor.splineControls [i].pointsDirty);
						newPoints.Insert (j, vec1);
						newBools.Insert (j, true);
						currentAreaEditor.splineControls [i].points = newPoints.ToArray ();
						currentAreaEditor.splineControls [i].pointsDirty = newBools.ToArray ();
						currentAreaEditor.splineControls [i].dirty = true;
						holdingID = -99;
					}
					break;
				}

				currentSelection = i + currentAreaEditor.shapes.Length + currentAreaEditor.holes.Length;
				break;
			case 2:
				if(!toolInUse){
					//tile along spline
					
					i = currentSelection -currentAreaEditor.shapes.Length - currentAreaEditor.holes.Length;
					DrawSpline(currentSelection);

					if (Handles.Button (currentAreaEditor.splineControls [i].centerPoint, Quaternion.identity, 0.1f,  0.125f, Handles.DotCap)) {

						toolInUse = true;
						Rect splineBounds = new Rect ();
						for (j = currentAreaEditor.splines [i].points.Length - 1; j >= 0; j--) {
							if (currentAreaEditor.splines [i].points [j].x > splineBounds.xMax) {
								splineBounds.xMax = currentAreaEditor.splines [i].points [j].x;
							}
							if (currentAreaEditor.splines [i].points [j].x < splineBounds.xMin) {
								splineBounds.xMin = currentAreaEditor.splines [i].points [j].x;
							}
							if (currentAreaEditor.splines [i].points [j].y > splineBounds.yMax) {
								splineBounds.yMax = currentAreaEditor.splines [i].points [j].y;
							}
							if (currentAreaEditor.splines [i].points [j].y < splineBounds.yMin) {
								splineBounds.yMin = currentAreaEditor.splines [i].points [j].y;
							}
						}
						//Create a polygon from that rect. Add it to our current area.
						Polygon tempPoly = new Polygon (currentAreaEditor.splines [i].name);
						Vector2[] ppoints = new Vector2[4];
						ppoints [0] = new Vector2 (splineBounds.xMin, splineBounds.yMax);
						ppoints [1] = new Vector2 (splineBounds.xMax, splineBounds.yMax);
						ppoints [2] = new Vector2 (splineBounds.xMax, splineBounds.yMin);
						ppoints [3] = new Vector2 (splineBounds.xMin, splineBounds.yMin);
                        tempPoly.points = ppoints;
						List<Polygon> polys = new List<Polygon> (designer.areas [currentArea].shapes);
						polys.Add (tempPoly);
						//Tile that shape.
						designer.areas [currentArea].shapes = polys.ToArray ();
                        tilePolyCheck = designer.areas[currentArea].TileArea(-1, designer.areas[currentArea].shapes.Length - 1, selectedWallpaper, splineBounds.center);
                        tsc = false;
						EditorApplication.update += TileSpline;
					}
				} 
				break;
			case 3:
				//paint along spline
				if(!toolInUse){
					i = currentSelection -currentAreaEditor.shapes.Length - currentAreaEditor.holes.Length;
					DrawSpline(currentSelection);

					if (Handles.Button (currentAreaEditor.splineControls [i].centerPoint, Quaternion.identity, 0.1f,  0.125f, Handles.DotCap)) {

						toolInUse = true;
						tilePolyCheck = designer.PaintSpline (currentArea, i);
						EditorApplication.update += PaintSpline;
					}
				}
				break;
			case 4:
                //make shape
                i = currentSelection - currentAreaEditor.shapes.Length - currentAreaEditor.holes.Length;
                DrawSpline(currentSelection);

                if (Handles.Button(currentAreaEditor.splineControls[i].centerPoint, Quaternion.identity, 0.1f, 0.125f, Handles.DotCap))
                {
                        designer.areas[currentArea].ConvertSplineToShape(i);
                        updateCurrentAreaEditor = true;
                        currentSelection = currentAreaEditor.shapes.Length - 1;
                }
                break;
			}
		}

		public void TileView(){
			int cellID, cs,
            tp = currentAreaEditor.shapes.Length+ currentAreaEditor.holes.Length + currentAreaEditor.splines.Length;

            switch (selectedTool) {
			case 0:
				//Select Tilegroup
                
				for(int i = currentAreaEditor.tileGroups.Length-1; i >=0; i--){
					
					if (i+tp != currentSelection) {
						DrawTileGroup (i, true);
						Handles.BeginGUI ();
						GUI.color = currentAreaEditor.tileGroups [i].color;
						if (GUI.Button (currentAreaEditor.tileGroups [i].labelRect, currentAreaEditor.tileGroups [i].name)) {
							currentSelection = i;
						}
						Handles.EndGUI ();
					} else {
						DrawTileGroup (i);
						Handles.BeginGUI ();
						GUI.color = Color.white;
						GUI.Label (currentAreaEditor.tileGroups [i].labelRect, currentAreaEditor.tileGroups [i].name, GUI.skin.box);
						Handles.EndGUI ();
					}
				}

				//currentSelection += tp;


				break;
			case 1:
                    //Manipulate tiles.
                    cs = currentSelection - tp;
				DrawTileGroup(cs);
				
				switch(key){
				case 0:
                            //Put a button in the middle of each cell. Clicking it will select it and open up a properties window for it?
                    //Debug.Log(currentSelection + "," + currentAreaEditor.tileGroups.Length);
                    for (int i = currentAreaEditor.tilePoints[cs].points.Length-1; i >=0; i--){
					    Handles.color = Color.green;
					    if (Handles.Button (currentAreaEditor.tilePoints [cs].points [i], Quaternion.identity, 0.1f, 0.15f, Handles.CubeCap)) {
							
					    }
					}
					break;
				case 1:
					//Ctrl will turn them red. Clicking will delete the cell.
					for(int i = currentAreaEditor.tilePoints[cs].points.Length-1; i >=0; i--){
						Handles.color = Color.red;
						if (Handles.Button (currentAreaEditor.tilePoints [cs].points [i], Quaternion.identity, 0.1f, 0.15f, Handles.CubeCap)) {
							designer.areas [currentArea].tileGroups [cs].RemoveCell (i, true);
							selectedArea = currentArea;
							EditorUtility.SetDirty (designer);
							EditorSceneManager.MarkSceneDirty (SceneManager.GetActiveScene ());
						}
					}
					break;
				case 2:
					//Snap. Use?
					for(int i = currentAreaEditor.tilePoints[currentSelection].points.Length-1; i >=0; i--){
						Handles.color = Color.green;
						if (Handles.Button (currentAreaEditor.tilePoints [currentSelection].points [i], Quaternion.identity, 0.1f, 0.15f, Handles.CubeCap)) {

						}
					}
					break;
				case 3:
					//Shift highlights edges, instead. Clicking them adds a cell on that side?
					
					for(int i = designer.areas[currentArea].tileGroups[currentSelection].edges.Length-1; i >=0; i--){
						if (designer.areas [currentArea].tileGroups [currentSelection].edges [i].isBorder) {
							Vector3 p = Vector3.Lerp (currentAreaEditor.tileGroups [currentSelection].points [designer.areas [currentArea].tileGroups [currentSelection].edges [i].pointA],
								           currentAreaEditor.tileGroups [currentSelection].points [designer.areas [currentArea].tileGroups [currentSelection].edges [i].pointB],
								           0.5f);
							Handles.color = Color.yellow;
							if (Handles.Button (p, Quaternion.identity, 0.1f, 0.15f, Handles.CubeCap)) {
								//Find which cell uses this edge. Try to make a neighbor that shares that edge.
								for(int j = designer.areas[currentArea].tileGroups[currentSelection].cells.Length-1; j>=0;j--){
									for (int k = designer.areas [currentArea].tileGroups [currentSelection].cells [j].edges.Length - 1; k >= 0; k--) {
										if (designer.areas [currentArea].tileGroups [currentSelection].cells [j].edges [k] == i) {
											//designer.areas [currentArea].tileGroups [currentSelection].AddCell (i, j);
										}
									}
								}
							}
						}
					}

					break;
				}
				break;
			
			case 2:
                    //Create shape
                    cs = currentSelection - tp;
                    DrawTileGroup(cs);
                    switch (key){
				case 0:
					if (Event.current.isMouse && Event.current.type == EventType.MouseUp
						&& designer.areas [currentArea].Contains (mousePos, currentSelection)) {
						designer.areas [currentArea].ConvertTilesToShapes (currentSelection-currentAreaEditor.shapes.Length-
							currentAreaEditor.holes.Length - currentAreaEditor.splines.Length);
						selectedArea = currentArea;
						EditorUtility.SetDirty (designer);
						EditorSceneManager.MarkSceneDirty (SceneManager.GetActiveScene ());

					}
					//Creates shapes from the whole group. Shapes and holes.

					break;
				case 1:
					//Ctrl creates a hole from the whole group. Holes become shapes?
					//Do the same as above, but keep track of the number of shapes and holes we have, to start.
					//After we're done, move any new ones from one to the other.
					if (Event.current.isMouse && Event.current.type == EventType.MouseUp
					    && designer.areas [currentArea].Contains (mousePos, currentSelection)) {
						int sC = currentAreaEditor.shapes.Length, hC = currentAreaEditor.holes.Length;
						designer.areas [currentArea].ConvertTilesToShapes (currentSelection-currentAreaEditor.shapes.Length-
							currentAreaEditor.holes.Length - currentAreaEditor.splines.Length);
						List<Polygon> newShapes = new List<Polygon> ();
						for (int i = 0; i < sC; i++) {
							newShapes.Add(designer.areas[currentArea].shapes[i]);
						}
						int sD = designer.areas [currentArea].shapes.Length - sC;
						List<Polygon> newHoles = new List<Polygon> ();
						for (int i = 0; i < hC; i++) {
							newHoles.Add(designer.areas[currentArea].holes[i]);
						}
						int hD = designer.areas [currentArea].shapes.Length - hC;

						for (int i = 0; i < sD; i++) {
							newHoles.Add(designer.areas[currentArea].shapes[i+sC]);
						}
						for (int i = 0; i < hD; i++) {
							newShapes.Add(designer.areas[currentArea].holes[i+hC]);
						}
						designer.areas [currentArea].shapes = newShapes.ToArray ();
						designer.areas [currentArea].holes = newHoles.ToArray ();
						selectedArea = currentArea;
						EditorUtility.SetDirty (designer);
						EditorSceneManager.MarkSceneDirty (SceneManager.GetActiveScene ());
					}
					break;
				case 2:
					//Snap. Use?

					break;
				case 3:
					//Shift creates a shape from just the cell clicked.
					if (Event.current.isMouse && Event.current.type == EventType.MouseUp
					    && designer.areas [currentArea].Contains (mousePos, currentSelection)) {
						cellID = -1;
						if (designer.areas [currentArea].tileGroups [currentSelection].ContainsPoint (mousePos, ref cellID)) {
							Polygon cellShape = new Polygon ("Cell " + cellID + " Shape");
							Vector2[] cellP = new Vector2[designer.areas [currentArea].tileGroups [currentSelection].cells [cellID].points.Length];
							for (int cp = cellP.Length - 1; cp >= 0; cp--) {
								cellP [cp] = designer.areas [currentArea].tileGroups [currentSelection].cellPoints [
									designer.areas [currentArea].tileGroups [currentSelection].cells [cellID].points [cp]];
							}
							cellShape.points = cellP;
							List<Polygon> newS = new List<Polygon> (designer.areas [currentArea].shapes);
							newS.Add (cellShape);
							designer.areas [currentArea].shapes = newS.ToArray ();
							selectedArea = currentArea;
							EditorUtility.SetDirty (designer);
							EditorSceneManager.MarkSceneDirty (SceneManager.GetActiveScene ());
						}
					}
					break;
				}
				break;

			case 3:
                    //Paint tiles
                    cs = currentSelection - tp;
                    DrawTileGroup(cs);
                    mousePos = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).GetPoint(Vector3.Distance(camera.transform.position, designer.areas[currentArea].transform.position));

                    if (!toolInUse) {
					switch (key) {
					case 0:
                                //Paint whatever current cell we're over.
                        if (Event.current.isMouse && Event.current.type == EventType.MouseDown) {
                            //Debug.Log("Clicked");
                            if(designer.areas[currentArea].Contains(mousePos, currentSelection)) {
                                //Debug.Log("      within.");
                                paintPos = mousePos;
                                EditorApplication.update += FreePaint;
                                paintMode = 0;
                                toolInUse = true;
                            }
                        }
						break;
					case 1:
						//Ctrl will clear the contents of the cell we're over.
						if (Event.current.isMouse && Event.current.type == EventType.MouseDown
							&& designer.areas [currentArea].Contains (mousePos, currentSelection)) {
                            paintPos = mousePos;
                            EditorApplication.update += FreePaint;
							paintMode = 1;
							toolInUse = true;
						}
						break;
					case 2:
						//Snap. Use?
						break;
					case 3:
						//Shift fill paints the whole group.
						if (Event.current.isMouse && Event.current.type == EventType.MouseDown
							&& designer.areas [currentArea].Contains (mousePos, currentSelection)) {
                            paintPos = mousePos;
                            //Start our designer fill process.
                            tilePolyCheck = designer.FillTileGroup(currentArea, currentSelection);
							EditorApplication.update += FillTiles;
							doPaint = true;
							paintMode = 3;
							toolInUse = true;
						}
						break;
					}
				} else {
					
					if (doPaint) {
						switch (paintMode) {
						case 0:
							if (Event.current.isMouse && Event.current.type == EventType.MouseUp){
								EditorApplication.update -= FreePaint;
								toolInUse = false;
								paintMode = -1;
								return;
							}
							cellID = -1;
							if (designer.areas [currentArea].tileGroups [cs].ContainsPoint (mousePos, ref cellID)) {
								if (designer.areas [currentArea].tileGroups [cs].cells [cellID].content == null) {
									PaintSample p = designer.painter.Paint (designer.areas [currentArea].tileGroups [cs].cells [cellID].point);
									if (p != null) {
										designer.areas [currentArea].tileGroups [cs].cells [cellID].content = p.gameObject;
									}
								}
							}
							doPaint = false;
							break;

						case 1:
							if (Event.current.isMouse && Event.current.type == EventType.MouseUp){
								EditorApplication.update -= FreePaint;
								toolInUse = false;
								paintMode = -1;
								return;
							}
							cellID = -1;
							if (designer.areas [currentArea].tileGroups [currentSelection].ContainsPoint (mousePos, ref cellID)) {
								if (designer.areas [currentArea].tileGroups [currentSelection].cells [cellID].content != null) {
									PaintSample p = designer.areas [currentArea].tileGroups [currentSelection].cells [cellID].content.GetComponent<PaintSample> ();
									if (p) {
										designer.painter.ClearPaint (p);
									}
								}
							}
							doPaint = false;
							break;
						}
					}
				}
				break;
			}
		}

		public void PaintSpline(){
			if (!tilePolyCheck.MoveNext ()) {
				toolInUse = false;
				EditorApplication.update -= PaintSpline;
				tilePolyCheck = null;
			}
		}
        
		public void TilePoly(){
			if (!tilePolyCheck.MoveNext ()) {
				toolInUse = false;
				EditorApplication.update -= TilePoly;
				tilePolyCheck = null;
                updateCurrentAreaEditor = true;
				currentSelection = currentAreaEditor.shapes.Length + currentAreaEditor.holes.Length + 
				currentAreaEditor.splines.Length + currentAreaEditor.tileGroups.Length - 1;
                selectedTool = 0;
				if (designer.areas [currentArea].tileGroups.Length < 6) {
					tileScrollHeight = (designer.areas [currentArea].tileGroups.Length + 1) * lineHeight;
				} else {
					tileScrollHeight = 6 * lineHeight;
				}
			}
		}
        bool tsc;
		public void TileSpline(){
			if (!tilePolyCheck.MoveNext ()) {
                if (!tsc)
                {
                    //Debug.Log("Starting spline check.");
                    tilePolyCheck = SplineTiling();
                    tsc = true;
                }
                else {
                    //Debug.Log("Finished spline check.");
                    toolInUse = false;
                    EditorApplication.update -= TileSpline;
                    tilePolyCheck = null;
                }
			}
		}

        IEnumerator SplineTiling()
        {
            List<Polygon> polys = new List<Polygon>(designer.areas[currentArea].shapes);
            polys.RemoveAt(polys.Count - 1);
            designer.areas[currentArea].shapes = polys.ToArray();
            updateCurrentAreaEditor = true;
            RedrawSceneView();
            yield return null;
            //Check which tiles intersect with the spline points.
            List<int> intersectedTiles = new List<int>();
            int k = designer.areas[currentArea].tileGroups.Length-1, m = currentSelection - currentAreaEditor.shapes.Length - currentAreaEditor.holes.Length;
            Debug.Log(designer.areas[currentArea].tileGroups[k].cells.Length.ToString()
                + "," + designer.areas[currentArea].tileGroups[k].extents.ToString());
            //Go through each spline point on our smoothed spline.
            for (int i = currentAreaEditor.splines[m].points.Length - 1; i >= 0; i--)
            {
                //Check to see if any tile contains that point.

                int l = -1;
                designer.areas[currentArea].tileGroups[k].ContainsPoint(currentAreaEditor.splines[m].points[i], ref l);

                if (l >= 0)
                {
                    bool alreadyHave = false;
                    for (int j = intersectedTiles.Count - 1; j >= 0; j--)
                    {
                        if (intersectedTiles[j] == l)
                        {
                            alreadyHave = true;
                            break;
                        }
                    }
                    if (!alreadyHave)
                    {
                        Debug.Log("Cell#" + l.ToString() + " intersects spline.");
                        intersectedTiles.Add(l);
                    }
                }

            }
            //Remove all tiles that don't.
            for (int i = designer.areas[currentArea].tileGroups[k].cells.Length - 1; i >= 0; i--)
            {
                bool removeThis = true;
                for (int j = intersectedTiles.Count - 1; j >= 0; j--)
                {
                    if (intersectedTiles[j] == i)
                    {
                        removeThis = false;
                        break;
                    }
                }
                if (removeThis)
                {
                    designer.areas[currentArea].tileGroups[k].RemoveCell(i);
                }
            }
            updateCurrentAreaEditor = true;
            yield return null;
            currentSelection = currentAreaEditor.shapes.Length + currentAreaEditor.holes.Length +
            currentAreaEditor.splines.Length + currentAreaEditor.tileGroups.Length - 1;
            
        }
		public void FillTiles(){
			
			if (!tilePolyCheck.MoveNext ()) {
				toolInUse = false;
				EditorApplication.update -= FillTiles;
				tilePolyCheck = null;
				paintMode = -1;
			}

		}

		/*
		public void PaintView(){
			//On mouse down, start our mouse check as an editor update function.

			Ray mouseRay;

			switch(paintMode){
			case 0:
				if (designer.status == Designer.DesignerStatus.Painting || designer.status == Designer.DesignerStatus.Tiling) {
					SceneView.RepaintAll ();
					mouseRay = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);
					mousePos = mouseRay.GetPoint (Vector3.Distance (camera.transform.position, designer.transform.position));
				}
				if (Event.current.isMouse && Event.current.type == EventType.MouseDown && Event.current.button == 0) {
					designer.status = Designer.DesignerStatus.Painting;
					doPaint = false;
					paintPos = mousePos = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition).GetPoint (Vector3.Distance (camera.transform.position, designer.transform.position));
					currentRate = EditorApplication.timeSinceStartup + designer.painter.rate;
					EditorApplication.update += FreePaint;
				}
				if (Event.current.isMouse && Event.current.type == EventType.MouseUp && Event.current.button == 0) {
					designer.status = Designer.DesignerStatus.Neutral;
					EditorApplication.update -= FreePaint;
					doPaint = false;
				}

				if (doPaint) {
					PaintSample paintDab = null;
					switch(designer.canvasType){
					case Designer.DesignCanvasType.World: paintDab = designer.painter.Paint ((Vector2)mousePos); break;
					case Designer.DesignCanvasType.LocalObjective: paintDab = designer.painter.Paint ((Vector2)(designer.transform.InverseTransformDirection (mousePos - designer.transform.position))); break;
					case Designer.DesignCanvasType.LocalSubjective: paintDab = designer.painter.Paint ((Vector2)(designer.transform.InverseTransformPoint (mousePos))); break;
					case Designer.DesignCanvasType.Mesh:
						//Renderer r = canvas.GetComponent<Renderer> ();
						break;
					}
					if (paintDab == null) {
						paintPos = mousePos;
						currentRate = 0;
						return;
					}
						
					paintDab.transform.parent = designer.areas[currentArea].transform;
					
					if (followPaintDirection) {
						Vector3 direction = mousePos - paintPos;
						float angle = Mathf.Atan2 (direction.y, direction.x) * Mathf.Rad2Deg;
						direction = paintDab.transform.eulerAngles;
						direction.z = angle;
						paintDab.transform.eulerAngles = direction;
					}
					paintDab.transform.localScale = designer.brushSize;
					designer.painter.ApplyPaintProperties (paintDab.transform);
					paintPos = mousePos;
					currentRate = 0;
					doPaint = false;
				}

				break;
			case 1:
				
				//Check to see if we've moved enough to check
				if (currentArea >= 0) {
					if (designer.status == Designer.DesignerStatus.Painting || designer.status == Designer.DesignerStatus.Tiling) {
						
						mouseRay = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);
						mousePos = mouseRay.GetPoint (Vector3.Distance (camera.transform.position, designer.transform.position));

						Vector3 vec = mousePos - lastMousePos;
						lastMousePos = mousePos;
						currentAreaEditor.areaPoint += vec;
						//currentAreaEditor.dirty = true;
						int j;
						Handles.color = Color.green;
						for (j = currentAreaEditor.shapes.Length - 1; j >= 0; j--) {
							currentAreaEditor.shapes [j].Offset (vec);

							Handles.DrawPolyLine (currentAreaEditor.shapes [j].points);
							Handles.DrawLine (currentAreaEditor.shapes [j].points [currentAreaEditor.shapes [j].points.Length - 1], currentAreaEditor.shapes [j].points [0]);
						}
						Handles.color = Color.red;
						for (j = currentAreaEditor.holes.Length - 1; j >= 0; j--) {
							currentAreaEditor.holes [j].Offset (vec);

							Handles.DrawPolyLine (currentAreaEditor.holes [j].points);
							Handles.DrawLine (currentAreaEditor.holes [j].points [currentAreaEditor.holes [j].points.Length - 1], currentAreaEditor.holes [j].points [0]);
						}
						SceneView.RepaintAll ();
					}
					if (Event.current.isMouse && Event.current.type == EventType.MouseDown && Event.current.button == 0) {

						switch(key){
						case 0:
							if (currentPaintTiling == null) {
								currentPaintTiling = new DesignArea ();
							}
							break;
						case 2:
							//Create a new area. 
							//Start by saving old one as new area.
							if(currentPaintTiling != null){
								currentPaintTiling.tileCells = designer.tiler.cells;
								currentPaintTiling.tilePoints = designer.tiler.cellPoints.ConvertToVec3();
								currentPaintTiling.tileEdges = designer.tiler.edges;
								List<DesignArea> newAreas = new List<DesignArea> (designer.areas);
								newAreas.Add (currentPaintTiling);
								designer.areas = newAreas.ToArray ();
								currentPaintTiling = null;
							}
							currentPaintTiling = new DesignArea ();

							break;
						}
						designer.status = Designer.DesignerStatus.Painting;
						doPaint = false;
						lastMousePos = paintPos = mousePos = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition).GetPoint (
							Vector3.Distance (camera.transform.position, designer.transform.position));
						designer.tiler.startingPoint = mousePos;
						int j, k;
						for (j = currentAreaEditor.shapes.Length - 1; j >= 0; j--) {
							//currentAreaEditor.shapes [j].dirty = true;
							for (k = currentAreaEditor.shapes [j].points.Length - 1; k >= 0; k--) {
								currentAreaEditor.shapes [j].points [k] = mousePos + (currentAreaEditor.shapes [j].points [k] - currentAreaEditor.areaPoint);
							}
						}

						for (j = currentAreaEditor.holes.Length - 1; j >= 0; j--) {
							//currentAreaEditor.holes [j].dirty = true;
							for (k = currentAreaEditor.holes [j].points.Length - 1; k >= 0; k--) {
								currentAreaEditor.holes [j].points [k] = mousePos + (currentAreaEditor.holes [j].points [k] - currentAreaEditor.areaPoint);
							}
						}
						tilePolyCheck = designer.TileArea (currentArea);
						EditorApplication.update += TilePaint;

					}

					if (Event.current.isMouse && Event.current.type == EventType.MouseUp && Event.current.button == 0) {
						currentAreaEditor.Update();
						EditorUtility.SetDirty (designer);
						EditorSceneManager.MarkSceneDirty (SceneManager.GetActiveScene ());
						EditorApplication.update -= TilePaint;
						tilePolyCheck = null;
						designer.status = Designer.DesignerStatus.Neutral;
					}

					if (doPaint) {
						PaintSample tP;
						switch(designer.canvasType){
						case Designer.DesignCanvasType.World:
							Vector3 vec = new Vector3 (designer.tiler.cells [currentCell].point.x, designer.tiler.cells [currentCell].point.y, 0);
							vec.Scale (designer.tiler.scale);
							tP = designer.painter.Paint ((Vector2)vec);
							if (tP) {
								designer.tiler.cells [currentCell].content = tP.gameObject;
							}
							if (designer.tiler.cells [currentCell].content != null) {
								designer.tiler.cells [currentCell].content.name = designer.tiler.name + " " + currentCell.ToString ();
								//designer.tiler.cells [currentCell].content.transform.parent = designer.tiler.areaContent.transform;
								designer.tiler.cells [currentCell].content.transform.eulerAngles = new Vector3 (0, 0, designer.tiler.cells [currentCell].point.z);
								designer.tiler.cells [currentCell].content.transform.localScale = designer.tiler.cells [currentCell].content.transform.LossyScale (
									new Vector3 (designer.tiler.cells [currentCell].scale.x, designer.tiler.cells [currentCell].scale.y, 1));
							}
							break;

						case Designer.DesignCanvasType.LocalObjective:

							vec = new Vector3 (designer.tiler.cells [currentCell].point.x, designer.tiler.cells [currentCell].point.y, 0);
							vec.Scale (designer.tiler.scale);
							if (designer.tiler.cells [currentCell].content == null) {
								tP = designer.painter.Paint (designer.canvas.t.position + designer.canvas.t.TransformDirection (vec));
								if (tP) {
									designer.tiler.cells [currentCell].content = tP.gameObject;
								}
							}
							if (designer.tiler.cells [currentCell].content != null) {
								//designer.tiler.cells[i].content = painter.Paint((Vector2)vec);
								designer.tiler.cells [currentCell].content.name = designer.tiler.name + " " + currentCell.ToString ();
								//designer.tiler.cells [currentCell].content.transform.parent = designer.tiler.areaContent.transform;
								vec = designer.tiler.cells [currentCell].content.transform.localEulerAngles;
								vec.z = designer.tiler.cells [currentCell].point.z;
								designer.tiler.cells [currentCell].content.transform.localEulerAngles = vec;
								designer.tiler.cells [currentCell].content.transform.localScale = designer.tiler.cells [currentCell].content.transform.LossyScale (
									new Vector3 (designer.tiler.cells [currentCell].scale.x, designer.tiler.cells [currentCell].scale.y, 1));
							}
							break;

						case Designer.DesignCanvasType.LocalSubjective:
							vec = new Vector3 (designer.tiler.cells [currentCell].point.x, designer.tiler.cells [currentCell].point.y, 0);
							vec.Scale (designer.tiler.scale);
							if (designer.tiler.cells [currentCell].content == null) {
								tP = designer.painter.Paint (designer.canvas.t.TransformPoint (vec));
								if (tP) {
									designer.tiler.cells [currentCell].content = tP.gameObject;
								}
							} 
							if (designer.tiler.cells [currentCell].content != null) {
								designer.tiler.cells [currentCell].content.name = designer.tiler.name + " " + currentCell.ToString ();
								//designer.tiler.cells [currentCell].content.transform.parent = designer.tiler.areaContent.transform;
								vec = designer.tiler.cells [currentCell].content.transform.localEulerAngles;
								vec.z = designer.tiler.cells [currentCell].point.z;
								designer.tiler.cells [currentCell].content.transform.localEulerAngles = vec;
								designer.tiler.cells [currentCell].content.transform.localScale = new Vector3 (designer.tiler.cells [currentCell].scale.x, designer.tiler.cells [currentCell].scale.y, 1);
							}
							break;

						case Designer.DesignCanvasType.Mesh:
							//Renderer r = canvas.GetComponent<Renderer> ();

							break;
						}
						doPaint = false;

					}

				}
				break;
			}

		}
		*/	
		double currentRate;
		Vector3 paintPos = Vector3.zero, mousePos, lastMousePos;
		bool followPaintDirection= false;
		Camera camera;
		Event mouseEvent;
		bool doPaint = false;
		int currentCell;

		public void FreePaint(){
			doPaint = false;
			if (designer.painter.rate >= 0) {
				if (EditorApplication.timeSinceStartup >= currentRate) {
					doPaint = true;
					currentRate = EditorApplication.timeSinceStartup + designer.painter.rate;
				}
			}
			if (designer.painter.minimumDistance >0 && Vector3.Distance(mousePos, paintPos) >= designer.painter.minimumDistance){doPaint = true;}
		}
        public void FillPaint() {
            if (!tilePolyCheck.MoveNext())
            {
                tilePolyCheck = null;
                EditorApplication.update -= FillPaint;
                toolInUse = false;
            }
        }
		float tilePaintMinimumDistance=1.75f;
		IEnumerator tilePolyCheck;
		/*
		public void TilePaint(){
			doPaint = false;
			int i, j;
			//First, independent of other parts, check to see if the mouse has moved more than minimumdistance. That's when we get new cells with our moved polygon.
			if (!tilePolyCheck.MoveNext ()) {
				if (Vector3.Distance (mousePos, paintPos) >= tilePaintMinimumDistance) {
					//Translate current area (our brush) back into its space, then run our tile area.
					paintPos = mousePos;
					for (i = currentAreaEditor.shapes.Length - 1; i >= 0; i--) {
						Vector3[] shPoints = new Vector3[currentAreaEditor.shapes [i].points.Length];
						for (j = shPoints.Length - 1; j >= 0; j--) {

							switch (designer.canvasType) {
							case Designer.DesignCanvasType.LocalObjective:
								shPoints [j] = designer.transform.InverseTransformDirection (currentAreaEditor.shapes [i].points [j] - designer.transform.position);
								break;
							case Designer.DesignCanvasType.LocalSubjective:
								shPoints [j] = designer.transform.InverseTransformPoint (currentAreaEditor.shapes [i].points [j]);
								break;
							case Designer.DesignCanvasType.World:
								shPoints [j] = currentAreaEditor.shapes [i].points [j];
								break;
							case Designer.DesignCanvasType.Mesh:
								shPoints [j] = currentAreaEditor.shapes [i].points [j];
								break;
							}

						}
						designer.areas [currentArea].shapes [i].points = shPoints.ConvertToVec2 ();
					}

					for (i = currentAreaEditor.holes.Length - 1; i >= 0; i--) {
						Vector3[] shPoints = new Vector3[currentAreaEditor.holes [i].points.Length];
						for (j = shPoints.Length - 1; j >= 0; j--) {

							switch (designer.canvasType) {
							case Designer.DesignCanvasType.LocalObjective:
								shPoints [j] = designer.transform.InverseTransformDirection (currentAreaEditor.holes [i].points [j] - designer.transform.position);
								break;
							case Designer.DesignCanvasType.LocalSubjective:
								shPoints [j] = designer.transform.InverseTransformPoint (currentAreaEditor.holes [i].points [j]);
								break;
							case Designer.DesignCanvasType.World:
								shPoints [j] = currentAreaEditor.holes [i].points [j];
								break;
							case Designer.DesignCanvasType.Mesh:
								shPoints [j] = currentAreaEditor.holes [i].points [j];
								break;
							}

						}
						designer.areas [currentArea].holes [i].points = shPoints.ConvertToVec2 ();
					}
					tilePolyCheck = null;
					tilePolyCheck = designer.areas[currentArea].TileArea (-1,-1);

				}
			}
			//Second, independent of first, we check to see which cell our mouse is in. If it doesn't have contents, we paint them.
			Vector3 modifiedMousePos = mousePos;

			switch (designer.canvasType) {
			case Designer.DesignCanvasType.LocalObjective:
				modifiedMousePos = designer.transform.InverseTransformDirection (mousePos - designer.transform.position);
				break;
			case Designer.DesignCanvasType.LocalSubjective:
				modifiedMousePos = designer.transform.InverseTransformPoint (mousePos);
				break;

			}
			i = -1;
			if (designer.tiler.ContainsPoint (modifiedMousePos, ref i)) {
				switch(key){
				case 0:
				case 2:
					if (designer.tiler.cells [i].content == null) {
						doPaint = true;
						currentCell = i;
					}
					break;
				case 1:
					if (designer.tiler.cells [i].content != null) {
						designer.painter.ClearPaint (designer.tiler.cells [i].content.GetComponent<PaintSample>());
					}
					break;
				}
			}
		}
		*/
	}



	[System.Serializable]
	public class DesignPointHelper
	{
		//For a tileGroup, we convert all cell points to points, flat out.
		//We then go through all edges. pointsDirty is set to edges.Length.
		//pointIDS is edges.Length*2
		public Vector3[] points;
		public bool[] pointsDirty;
		public int[] pointIDs;
		public Vector3[] pointNormals;
		public Vector3 centerPoint, centerPointNormal;
		public Rect labelRect;
		public string name;
		public Color color;
		public bool dirty;

		public void Offset(Vector3 o){
			for (int i = points.Length - 1; i >= 0; i--) {
				points [i] += o;
			}
			centerPoint += o;
			Vector2 lC = HandleUtility.WorldToGUIPoint (centerPoint);
			labelRect = new Rect (lC, GUI.skin.button.CalcSize (new GUIContent (name)));
			dirty = true;
		}

	}
		


	[System.Serializable]
	public class DesignAreaEditor{
		
		public DesignArea area;
		public DesignPointHelper[] shapes;
		public DesignPointHelper[] holes;
		public DesignPointHelper[] splines;
		public DesignPointHelper[] splineControls;
		public DesignPointHelper[] tileGroups;
		public DesignPointHelper[] tilePoints;
		public Vector3 areaPoint;
		public Rect labelRect;
		public bool dirty;
		int splineSmoothing;

		public DesignAreaEditor(DesignArea source, int sSmoothing = 5){

			splineSmoothing = sSmoothing;
			area = source;
			shapes = new DesignPointHelper[source.shapes.Length];
			holes = new DesignPointHelper[source.holes.Length];
			splines = new DesignPointHelper[source.splines.Length];
			splineControls = new DesignPointHelper[source.splines.Length];
			tileGroups = new DesignPointHelper[source.tileGroups.Length];
			tilePoints = new DesignPointHelper[source.tileGroups.Length];

			Vector2 vec;
			switch (source.canvasType) {
			case Designer.DesignCanvasType.World:
				areaPoint = source.center;
				vec = HandleUtility.WorldToGUIPoint (areaPoint);
				labelRect = new Rect (vec, GUI.skin.button.CalcSize (new GUIContent (source.areaName)));
				for (int i = shapes.Length - 1; i >= 0; i--) {

					shapes [i] = new DesignPointHelper ();
					shapes [i].name = source.shapes [i].name;
					shapes [i].color = source.shapes [i].color;
					shapes [i].points = source.shapes[i].points.ConvertToVec3();
					shapes [i].pointsDirty = new bool[shapes [i].points.Length];

					shapes [i].centerPoint = source.shapes [i].extents.center;

					vec = HandleUtility.WorldToGUIPoint (shapes[i].centerPoint);
					shapes [i].labelRect = new Rect (vec, GUI.skin.button.CalcSize (new GUIContent (shapes[i].name)));
				}

				for (int i = holes.Length - 1; i >= 0; i--) {
					holes [i] = new DesignPointHelper ();
					holes [i].name = source.holes [i].name;
					holes [i].color = source.holes [i].color;
					holes [i].points = source.holes[i].points.ConvertToVec3();
					holes [i].pointsDirty = new bool[holes [i].points.Length];

					holes [i].centerPoint = source.holes [i].extents.center;

					vec = HandleUtility.WorldToGUIPoint (holes[i].centerPoint);
					holes [i].labelRect = new Rect (vec, GUI.skin.button.CalcSize (new GUIContent (holes[i].name)));
				}

				for (int i = splines.Length - 1; i >= 0; i--) {
					splines [i] = new DesignPointHelper ();
					splineControls [i] = new DesignPointHelper ();
					splines [i].name = source.splines [i].name;
					splines [i].color = source.splines [i].color;
					Spline smoothed = new Spline (source.splines[i], source.splines[i].Length*splineSmoothing); 
					splines [i].points = new Vector3[smoothed.Length];
					for (int j = smoothed.Length - 1; j >= 0; j--) {
						splines [i].points [j] = smoothed[j];
					}


					splineControls [i].points = new Vector3[source.splines [i].Length];
					splineControls [i].pointsDirty = new bool[splineControls [i].points.Length];
					for (int j = source.splines[i].Length - 1; j >= 0; j--) {
						splineControls [i].points [j] = source.splines [i] [j];
						splineControls [i].centerPoint += splineControls [i].points [j];
					}
					splineControls [i].centerPoint /= splineControls[i].points.Length;


					vec = HandleUtility.WorldToGUIPoint (splineControls[i].centerPoint);
					splines [i].labelRect = new Rect (vec, GUI.skin.button.CalcSize (new GUIContent (splines[i].name)));
				}

				for (int i = tileGroups.Length - 1; i >= 0; i--) {

					tileGroups [i] = new DesignPointHelper ();

					tileGroups [i].name = source.tileGroups [i].name;
					tileGroups [i].color = source.tileGroups [i].color;
					tileGroups [i].points = source.tileGroups[i].cellPoints.ConvertToVec3();
					tileGroups [i].pointsDirty = new bool[tileGroups [i].points.Length];

					tileGroups [i].centerPoint = source.tileGroups [i].extents.center;

					vec = HandleUtility.WorldToGUIPoint (tileGroups[i].centerPoint);
					tileGroups [i].labelRect = new Rect (vec, GUI.skin.button.CalcSize (new GUIContent (tileGroups[i].name)));

					tilePoints [i] = new DesignPointHelper ();
					tilePoints [i].points = new Vector3[source.tileGroups [i].cells.Length];
					for (int j = tilePoints [i].points.Length - 1; j >= 0; j--) {
						tilePoints [i].points [j] = source.tileGroups [i].cells [j].point;
					}
				}
				break;
			case Designer.DesignCanvasType.LocalObjective:
				areaPoint = source.transform.LocalObjective(source.extents.center);
				vec = HandleUtility.WorldToGUIPoint (areaPoint);
				labelRect = new Rect (vec, GUI.skin.button.CalcSize (new GUIContent (source.areaName)));

				for (int i = shapes.Length - 1; i >= 0; i--) {
					shapes [i] = new DesignPointHelper ();

					shapes [i].name = source.shapes [i].name;
					shapes [i].color = source.shapes [i].color;
					shapes [i].points = source.shapes[i].points.ConvertToVec3();
					shapes [i].pointsDirty = new bool[shapes [i].points.Length];

					for (int j = shapes [i].points.Length - 1; j >= 0; j--) {
						shapes [i].points[j] = source.transform.LocalObjective (source.shapes[i].points[j]);
						shapes [i].pointsDirty [j] = false;
					}
						

					shapes [i].centerPoint = source.transform.LocalObjective(source.shapes [i].extents.center);
					vec = HandleUtility.WorldToGUIPoint (shapes[i].centerPoint);
					shapes [i].labelRect = new Rect (vec, GUI.skin.button.CalcSize (new GUIContent (shapes[i].name)));
				}

				for (int i = holes.Length - 1; i >= 0; i--) {
					holes [i] = new DesignPointHelper ();
					holes [i].name = source.holes [i].name;
					holes [i].color = source.holes [i].color;
					holes [i].points = source.holes[i].points.ConvertToVec3();
					holes [i].pointsDirty = new bool[holes [i].points.Length];

					for (int j = holes [i].points.Length - 1; j >= 0; j--) {
						holes [i].points[j] = source.transform.LocalObjective (source.holes[i].points[j]);
						holes [i].pointsDirty [j] = false;
					}
						
					holes [i].centerPoint = source.transform.LocalObjective(source.holes [i].extents.center);
					vec = HandleUtility.WorldToGUIPoint (holes[i].centerPoint);
					holes [i].labelRect = new Rect (vec, GUI.skin.button.CalcSize (new GUIContent (holes[i].name)));
				}

				for (int i = splines.Length - 1; i >= 0; i--) {
					splines [i] = new DesignPointHelper ();
					splines [i].name = source.splines [i].name;
					splines [i].color = source.splines [i].color;
					Spline smoothed = new Spline (source.splines[i], source.splines[i].Length*splineSmoothing); 
					splines [i].points = new Vector3[smoothed.Length];
					for (int j = smoothed.Length - 1; j >= 0; j--) {
						splines [i].points [j] = source.transform.LocalObjective(smoothed [j]);

					}


					splineControls [i].points = new Vector3[source.splines [i].Length];
					splineControls [i].pointsDirty = new bool[splineControls [i].points.Length];
					for (int j = source.splines[i].Length - 1; j >= 0; j--) {
						splineControls [i].points [j] = source.transform.LocalObjective(source.splines [i] [j]);
						splineControls [i].centerPoint += splineControls [i].points [j];
					}
					splineControls [i].centerPoint /= splineControls[i].points.Length;

					vec = HandleUtility.WorldToGUIPoint (splineControls[i].centerPoint);
					splines [i].labelRect = new Rect (vec, GUI.skin.button.CalcSize (new GUIContent (splines[i].name)));
				}

				for (int i = tileGroups.Length - 1; i >= 0; i--) {

					tileGroups [i] = new DesignPointHelper ();
					tileGroups [i].name = source.tileGroups [i].name;
					tileGroups [i].color = source.tileGroups [i].color;
					tileGroups [i].points = new Vector3[source.tileGroups[i].cellPoints.Length];
					tileGroups [i].centerPoint = Vector3.zero;
					for (int j = source.tileGroups[i].cellPoints.Length - 1; j >= 0; j--) {
						tileGroups [i].points [j] = source.transform.LocalObjective(source.tileGroups[i].cellPoints[j]);
						tileGroups [i].centerPoint += splines [i].points [j];
					}
					tileGroups [i].centerPoint /= tileGroups[i].points.Length;
					tileGroups [i].pointsDirty = new bool[tileGroups [i].points.Length];

					vec = HandleUtility.WorldToGUIPoint (tileGroups[i].centerPoint);
					tileGroups [i].labelRect = new Rect (vec, GUI.skin.button.CalcSize (new GUIContent (tileGroups[i].name)));

					tilePoints [i] = new DesignPointHelper ();
					tilePoints [i].points = new Vector3[source.tileGroups [i].cells.Length];
					for (int j = tilePoints [i].points.Length - 1; j >= 0; j--) {
						tilePoints [i].points [j] = source.transform.LocalObjective(source.tileGroups [i].cells [j].point);
					}
				}

				break;
			case Designer.DesignCanvasType.LocalSubjective:
				areaPoint = source.transform.TransformPoint(source.extents.center);
				vec = HandleUtility.WorldToGUIPoint (areaPoint);
				labelRect = new Rect (vec, GUI.skin.button.CalcSize (new GUIContent (source.areaName)));

				for (int i = shapes.Length - 1; i >= 0; i--) {
					shapes [i] = new DesignPointHelper ();
					shapes [i].name = source.shapes [i].name;
					shapes [i].color = source.shapes [i].color;
					shapes [i].points = source.shapes[i].points.ConvertToVec3();
					shapes [i].pointsDirty = new bool[shapes [i].points.Length];

					for (int j = shapes [i].points.Length - 1; j >= 0; j--) {
						shapes [i].points[j] = source.transform.TransformPoint (source.shapes[i].points[j]);
						shapes [i].pointsDirty [j] = false;
					}
						
					shapes [i].centerPoint = source.transform.TransformPoint(source.shapes [i].extents.center);
					vec = HandleUtility.WorldToGUIPoint (shapes[i].centerPoint);
					shapes [i].labelRect = new Rect (vec, GUI.skin.button.CalcSize (new GUIContent (shapes[i].name)));
				}

				for (int i = holes.Length - 1; i >= 0; i--) {
					holes [i] = new DesignPointHelper ();
					holes [i].name = source.holes [i].name;
					holes [i].color = source.holes [i].color;
					holes [i].points = source.holes[i].points.ConvertToVec3();
					holes [i].pointsDirty = new bool[holes [i].points.Length];

					for (int j = holes [i].points.Length - 1; j >= 0; j--) {
						holes [i].points[j] = source.transform.TransformPoint (source.holes[i].points[j]);
						holes [i].pointsDirty [j] = false;
					}
						
					holes [i].centerPoint = source.transform.TransformPoint(source.holes [i].extents.center);
					vec = HandleUtility.WorldToGUIPoint (holes[i].centerPoint);
					holes [i].labelRect = new Rect (vec, GUI.skin.button.CalcSize (new GUIContent (holes[i].name)));
				}

				for (int i = splines.Length - 1; i >= 0; i--) {
					splines [i] = new DesignPointHelper ();
					splines [i].name = source.splines [i].name;
					splines [i].color = source.splines [i].color;
					Spline smoothed = new Spline (source.splines[i], source.splines[i].Length*splineSmoothing); 
					splines [i].points = new Vector3[smoothed.Length];
					for (int j = smoothed.Length - 1; j >= 0; j--) {
						splines [i].points [j] = source.transform.TransformPoint(smoothed [j]);

					}


					splineControls [i].points = new Vector3[source.splines [i].Length];
					splineControls [i].pointsDirty = new bool[splineControls [i].points.Length];
					for (int j = source.splines[i].Length - 1; j >= 0; j--) {
						splineControls [i].points [j] = source.transform.TransformPoint(source.splines [i] [j]);
						splineControls [i].centerPoint += splineControls [i].points [j];
					}
					splineControls [i].centerPoint /= splineControls[i].points.Length;

					vec = HandleUtility.WorldToGUIPoint (splineControls[i].centerPoint);
					splines [i].labelRect = new Rect (vec, GUI.skin.button.CalcSize (new GUIContent (splines[i].name)));
				}

				for (int i = tileGroups.Length - 1; i >= 0; i--) {

					tileGroups [i] = new DesignPointHelper ();
					tileGroups [i].name = source.tileGroups [i].name;
					tileGroups [i].color = source.tileGroups [i].color;
					tileGroups [i].points = new Vector3[source.tileGroups[i].cellPoints.Length];
					tileGroups [i].centerPoint = Vector3.zero;
					for (int j = source.tileGroups[i].cellPoints.Length - 1; j >= 0; j--) {
						tileGroups [i].points [j] = source.transform.TransformPoint(source.tileGroups[i].cellPoints[j]);
						tileGroups [i].centerPoint += splines [i].points [j];
					}
					tileGroups [i].centerPoint /= tileGroups[i].points.Length;
					tileGroups [i].pointsDirty = new bool[tileGroups [i].points.Length];


					vec = HandleUtility.WorldToGUIPoint (tileGroups[i].centerPoint);
					tileGroups [i].labelRect = new Rect (vec, GUI.skin.button.CalcSize (new GUIContent (tileGroups[i].name)));

					tilePoints [i] = new DesignPointHelper ();
					tilePoints [i].points = new Vector3[source.tileGroups [i].cells.Length];
					for (int j = tilePoints [i].points.Length - 1; j >= 0; j--) {
						tilePoints [i].points [j] = source.transform.TransformPoint(source.tileGroups [i].cells [j].point);
					}
				}
				break;

			case Designer.DesignCanvasType.Mesh:
				MeshFilter mesh = source.GetComponentInChildren<MeshFilter> ();
				if (mesh) {
					//So the issue with this whole approach is that tiles can appear multiple times, depending on the UV in question.
					//That means I'm going to have to handle each point on a per occurance basis. I'll also need normals for my points. For painting and possibly handles. 
					//1. Get all the points for each thing. Associate each with its original.
					//2. If any instances of the original is moved, edit ALL instances uvs thusly.
					areaPoint = mesh.UvToWorldPoints(source.extents.center)[0];
					vec = HandleUtility.WorldToGUIPoint (areaPoint);
					labelRect = new Rect (vec, GUI.skin.button.CalcSize (new GUIContent (source.areaName)));

					for (int i = shapes.Length - 1; i >= 0; i--) {
						shapes [i] = new DesignPointHelper ();
						shapes [i].name = source.shapes [i].name;
						shapes [i].color = source.shapes [i].color;
						shapes [i].points = source.shapes [i].points.ConvertToVec3();
						shapes [i].pointsDirty = new bool[shapes [i].points.Length];
						shapes [i].centerPoint = Vector3.zero;
						List<Vector3> revisedPoints = new List<Vector3> ();
						List<Vector3> revisedNormals = new List<Vector3> ();
						List<int> revisedPointIDs = new List<int> ();
						for (int j =0; j < shapes [i].points.Length;  j++) {
							Vector3[] meshPoints = mesh.UvToWorldPoints(shapes [i].points [j]);
							for (int k = 0; k < meshPoints.Length - 1; k += 2) {
								revisedPoints.Add (meshPoints [k]);
								shapes [i].centerPoint += meshPoints [k];
								revisedNormals.Add (meshPoints [k+ 1]);
								revisedPointIDs.Add (j);
							}
						}
						shapes [i].points = revisedPoints.ToArray ();
						shapes [i].pointNormals = revisedNormals.ToArray ();
						shapes [i].pointIDs = revisedPointIDs.ToArray ();
						shapes [i].centerPoint /= shapes [i].points.Length;

						vec = HandleUtility.WorldToGUIPoint (shapes [i].centerPoint);
						shapes [i].labelRect = new Rect (vec, GUI.skin.button.CalcSize (new GUIContent (shapes [i].name)));
					}

					for (int i = holes.Length - 1; i >= 0; i--) {
						holes [i] = new DesignPointHelper ();
						holes [i].name = source.holes [i].name;
						holes [i].color = source.holes [i].color;
						holes [i].points = source.holes [i].points.ConvertToVec3();
						holes [i].pointsDirty = new bool[holes [i].points.Length];
						holes [i].centerPoint = Vector3.zero;
						List<Vector3> revisedPoints = new List<Vector3> ();
						List<Vector3> revisedNormals = new List<Vector3> ();
						List<int> revisedPointIDs = new List<int> ();
						for (int j =0; j < holes [i].points.Length;  j++) {
							Vector3[] meshPoints = mesh.UvToWorldPoints(holes [i].points [j]);
							for (int k = 0; k < meshPoints.Length - 1; k += 2) {
								revisedPoints.Add (meshPoints [k]);
								holes [i].centerPoint += meshPoints [k];
								revisedNormals.Add (meshPoints [k+ 1] );
								revisedPointIDs.Add (j);
							}
						}
						holes [i].points = revisedPoints.ToArray ();
						holes [i].pointNormals = revisedNormals.ToArray ();
						holes [i].pointIDs = revisedPointIDs.ToArray ();
						holes [i].centerPoint /= holes [i].points.Length;

						vec = HandleUtility.WorldToGUIPoint (holes [i].centerPoint);
						holes [i].labelRect = new Rect (vec, GUI.skin.button.CalcSize (new GUIContent (holes [i].name)));
					}

					for (int i = splines.Length - 1; i >= 0; i--) {
						splines [i] = new DesignPointHelper ();
						splines [i].name = source.splines [i].name;
						splines [i].color = source.splines [i].color;
						Spline smoothed = new Spline (source.splines[i], source.splines[i].Length*splineSmoothing); 
						splines [i].points = new Vector3[smoothed.Length];
						List<Vector3> revisedPoints = new List<Vector3> ();
						List<Vector3> revisedNormals = new List<Vector3> ();
						List<int> revisedPointIDs = new List<int> ();
						for (int j =0; j < smoothed.Length;  j++) {
							Vector3[] meshPoints = mesh.UvToWorldPoints(smoothed [j]);
							for (int k = 0; k < meshPoints.Length - 1; k += 2) {
								revisedPoints.Add (meshPoints [k]);
								splines [i].centerPoint += meshPoints [k];
								revisedNormals.Add (meshPoints [k+ 1] );
								revisedPointIDs.Add (j);
							}
						}
						splines [i].points = revisedPoints.ToArray ();
						splines [i].pointNormals = revisedNormals.ToArray ();
						splines [i].pointIDs = revisedPointIDs.ToArray ();


						revisedPoints = new List<Vector3> ();
						revisedNormals = new List<Vector3> ();
						revisedPointIDs = new List<int> ();
						for (int j =0; j < source.splines[i].Length;  j++) {
							Vector3[] meshPoints = mesh.UvToWorldPoints(source.splines[i] [j]);
							for (int k = 0; k < meshPoints.Length - 1; k += 2) {
								revisedPoints.Add (meshPoints [k]);
								splineControls [i].centerPoint += meshPoints [k];
								revisedNormals.Add (meshPoints [k+ 1] );
								revisedPointIDs.Add (j);
							}
						}
						splineControls [i].points = revisedPoints.ToArray ();
						splineControls [i].pointNormals = revisedNormals.ToArray ();
						splineControls [i].pointIDs = revisedPointIDs.ToArray ();
						splineControls [i].centerPoint /= splineControls [i].points.Length;

						vec = HandleUtility.WorldToGUIPoint (splineControls [i].centerPoint);
						splines [i].labelRect = new Rect (vec, GUI.skin.button.CalcSize (new GUIContent (splines [i].name)));


					}

					for (int i = tileGroups.Length - 1; i >= 0; i--) {

						tileGroups [i] = new DesignPointHelper ();
						tileGroups [i].name = source.tileGroups [i].name;
						tileGroups [i].color = source.tileGroups [i].color;
						tileGroups [i].points = new Vector3[source.tileGroups[i].cellPoints.Length];
						tileGroups [i].centerPoint = Vector3.zero;

						List<Vector3> revisedPoints = new List<Vector3> ();
						List<Vector3> revisedNormals = new List<Vector3> ();
						List<int> revisedPointIDs = new List<int> ();
						//Okay, so our point ids are now three for each point. The original point ID, edge A, and edge B.
						for (int j =0; j < tileGroups [i].points.Length;  j++) {
							Vector3[] meshPoints = mesh.UvToWorldPoints(source.tileGroups [i].cellPoints [j]);
							for (int k = 0; k < meshPoints.Length - 1; k += 2) {
								revisedPoints.Add (meshPoints [k]);
								tileGroups [i].centerPoint += meshPoints [k];
								revisedNormals.Add (meshPoints [k+ 1] );
								revisedPointIDs.Add (j);

							}
						}
						tileGroups [i].points = revisedPoints.ToArray ();
						tileGroups [i].pointNormals = revisedNormals.ToArray ();
						tileGroups [i].pointIDs = revisedPointIDs.ToArray ();
						tileGroups [i].centerPoint /= tileGroups[i].points.Length;
						tileGroups [i].pointsDirty = new bool[tileGroups [i].points.Length];

						vec = HandleUtility.WorldToGUIPoint (tileGroups[i].centerPoint);
						tileGroups [i].labelRect = new Rect (vec, GUI.skin.button.CalcSize (new GUIContent (tileGroups[i].name)));

						tilePoints [i] = new DesignPointHelper ();
						tilePoints [i].points = new Vector3[source.tileGroups [i].cells.Length];
						revisedPoints = new List<Vector3> ();
						revisedNormals = new List<Vector3> ();
						revisedPointIDs = new List<int> ();
						//Okay, so our point ids are now three for each point. The original point ID, edge A, and edge B.
						for (int j =0; j < tilePoints [i].points.Length;  j++) {
							Vector3[] meshPoints = mesh.UvToWorldPoints(source.tileGroups [i].cells[j].point);
							for (int k = 0; k < meshPoints.Length - 1; k += 2) {
								revisedPoints.Add (meshPoints [k]);
								revisedNormals.Add (meshPoints [k+ 1] );
								revisedPointIDs.Add (j);
							}
						}
						tilePoints [i].points = revisedPoints.ToArray ();
						tilePoints [i].pointNormals = revisedNormals.ToArray ();
						tilePoints [i].pointIDs = revisedPointIDs.ToArray ();
					}

				} else {
					//Default to LocalObjective.
					areaPoint = source.transform.LocalObjective(source.extents.center);
					vec = HandleUtility.WorldToGUIPoint (areaPoint);
					labelRect = new Rect (vec, GUI.skin.button.CalcSize (new GUIContent (source.areaName)));

					for (int i = shapes.Length - 1; i >= 0; i--) {
						shapes [i] = new DesignPointHelper ();
						//Debug.Log (source.shapes [i].name);
						shapes [i].name = source.shapes [i].name;
						shapes [i].color = source.shapes [i].color;
						shapes [i].points = source.shapes[i].points.ConvertToVec3();
						shapes [i].pointsDirty = new bool[shapes [i].points.Length];

						for (int j = shapes [i].points.Length - 1; j >= 0; j--) {
							shapes [i].points[j] = source.transform.LocalObjective (source.shapes[i].points[j]);
							shapes [i].pointsDirty [j] = false;
						}


						shapes [i].centerPoint = source.transform.LocalObjective(source.shapes [i].extents.center);
						vec = HandleUtility.WorldToGUIPoint (shapes[i].centerPoint);
						shapes [i].labelRect = new Rect (vec, GUI.skin.button.CalcSize (new GUIContent (shapes[i].name)));
					}

					for (int i = holes.Length - 1; i >= 0; i--) {
						holes [i] = new DesignPointHelper ();
						holes [i].name = source.holes [i].name;
						holes [i].color = source.holes [i].color;
						holes [i].points = source.holes[i].points.ConvertToVec3();
						holes [i].pointsDirty = new bool[holes [i].points.Length];

						for (int j = holes [i].points.Length - 1; j >= 0; j--) {
							holes [i].points[j] = source.transform.LocalObjective (source.holes[i].points[j]);
							holes [i].pointsDirty [j] = false;
						}

						holes [i].centerPoint = source.transform.LocalObjective(source.holes [i].extents.center);
						vec = HandleUtility.WorldToGUIPoint (holes[i].centerPoint);
						holes [i].labelRect = new Rect (vec, GUI.skin.button.CalcSize (new GUIContent (holes[i].name)));
					}

					for (int i = splines.Length - 1; i >= 0; i--) {
						splines [i] = new DesignPointHelper ();
						splines [i].name = source.splines [i].name;
						splines [i].color = source.splines [i].color;
						Spline smoothed = new Spline (source.splines[i], source.splines[i].Length*splineSmoothing); 
						splines [i].points = new Vector3[smoothed.Length];
						for (int j = smoothed.Length - 1; j >= 0; j--) {
							splines [i].points [j] = source.transform.LocalObjective(smoothed[j]);

						}


						splineControls [i].points = new Vector3[source.splines [i].Length];
						splineControls [i].pointsDirty = new bool[splineControls [i].points.Length];
						for (int j = source.splines[i].Length - 1; j >= 0; j--) {
							splineControls [i].points [j] = source.transform.LocalObjective(source.splines [i] [j]);
							splineControls [i].centerPoint += splineControls [i].points [j];
						}
						splineControls [i].centerPoint /= splineControls[i].points.Length;

						vec = HandleUtility.WorldToGUIPoint (splineControls[i].centerPoint);
						splines [i].labelRect = new Rect (vec, GUI.skin.button.CalcSize (new GUIContent (splines[i].name)));
					}

					for (int i = tileGroups.Length - 1; i >= 0; i--) {

						tileGroups [i] = new DesignPointHelper ();
						tileGroups [i].name = source.tileGroups [i].name;
						tileGroups [i].color = source.tileGroups [i].color;
						tileGroups [i].points = new Vector3[source.tileGroups[i].cellPoints.Length];
						tileGroups [i].centerPoint = Vector3.zero;
						for (int j = source.tileGroups[i].cellPoints.Length - 1; j >= 0; j--) {
							tileGroups [i].points [j] = source.transform.LocalObjective(source.tileGroups[i].cellPoints[j]);
							tileGroups [i].centerPoint += splines [i].points [j];
						}
						tileGroups [i].centerPoint /= tileGroups[i].points.Length;
						tileGroups [i].pointsDirty = new bool[tileGroups [i].points.Length];


						vec = HandleUtility.WorldToGUIPoint (tileGroups[i].centerPoint);
						tileGroups [i].labelRect = new Rect (vec, GUI.skin.button.CalcSize (new GUIContent (tileGroups[i].name)));

						tilePoints [i] = new DesignPointHelper ();
						tilePoints [i].points = new Vector3[source.tileGroups [i].cells.Length];
						for (int j = tilePoints [i].points.Length - 1; j >= 0; j--) {
							tilePoints [i].points [j] = source.transform.LocalObjective(source.tileGroups [i].cells [j].point);
						}
					}
				}
				break;
			}
		}


		//Keep this, but make some sub functions: UpdateShapes, UpdateHoles, UpdateSplines, UpdateTileGroups
		public void UpdateShapes(){
			switch (area.canvasType) {
			case Designer.DesignCanvasType.World:
				for (int i = shapes.Length - 1; i >= 0; i--) {
					if (shapes [i].dirty) {
						area.shapes[i].points = shapes [i].points.ConvertToVec2();
						for (int j = shapes [i].points.Length - 1; j >= 0; j--) {
							shapes [i].pointsDirty [j] = false;
						}
						dirty = true;
					}
				}

				for (int i = holes.Length - 1; i >= 0; i--) {
					if (holes [i].dirty) {
						area.holes[i].points = holes [i].points.ConvertToVec2();
						for (int j = holes [i].points.Length - 1; j >= 0; j--) {
							holes [i].pointsDirty [j] = false;
						}
						dirty = true;
					}
				}



				break;

			case Designer.DesignCanvasType.LocalObjective:

				for (int i = shapes.Length - 1; i >= 0; i--) {
					if (shapes [i].dirty) {
						area.shapes[i].points = shapes [i].points.ConvertToVec2();
						for (int j = shapes [i].points.Length - 1; j >= 0; j--) {
							area.shapes[i].points [j] = area.transform.InverseTransformDirection ((Vector3)area.shapes [i].points [j] - area.transform.position);
							shapes [i].pointsDirty [j] = false;
						}
						dirty = true;
					}
				}

				for (int i = holes.Length - 1; i >= 0; i--) {
					if (holes [i].dirty) {
						area.holes[i].points = holes [i].points.ConvertToVec2();
						for (int j = holes [i].points.Length - 1; j >= 0; j--) {
							area.holes[i].points [j] = area.transform.InverseTransformDirection ((Vector3)area.holes [i].points [j] - area.transform.position);
							holes [i].pointsDirty [j] = false;
						}
						dirty = true;
					}
				}



				break;

			case Designer.DesignCanvasType.LocalSubjective:

				for (int i = shapes.Length - 1; i >= 0; i--) {
					if (shapes [i].dirty) {
						area.shapes[i].points = shapes [i].points.ConvertToVec2();
						for (int j = shapes [i].points.Length - 1; j >= 0; j--) {
							area.shapes[i].points [j] = area.transform.InverseTransformPoint (area.shapes [i].points [j]);
							shapes [i].pointsDirty [j] = false;
						}
						dirty = true;
					}
				}

				for (int i = holes.Length - 1; i >= 0; i--) {
					if (holes [i].dirty) {
						area.holes[i].points = holes [i].points.ConvertToVec2();
						for (int j = holes [i].points.Length - 1; j >= 0; j--) {
							area.holes[i].points [j] = area.transform.InverseTransformPoint (area.holes [i].points [j]);
							holes [i].pointsDirty [j] = false;
						}
						dirty = true;
					}
				}



				break;

			case Designer.DesignCanvasType.Mesh:

				for (int i = shapes.Length - 1; i >= 0; i--) {
					if (shapes [i].dirty) {
						Vector2[] meshPoints = new Vector2[area.shapes[i].points.Length];

						for (int j = meshPoints.Length - 1; j >= 0; j--) {
							for (int k = shapes [i].points.Length - 1; k >= 0; k--) {
								if (shapes [i].pointIDs [k] == j) {
									//Convert back into a UV point, then break our last loop.
									//TODO
								}
							}
							shapes [i].pointsDirty [j] = false;
						}
						dirty = true;
					}
				}

				for (int i = holes.Length - 1; i >= 0; i--) {
					if (holes [i].dirty) {
						Vector2[] meshPoints = new Vector2[area.holes[i].points.Length];

						for (int j = meshPoints.Length - 1; j >= 0; j--) {
							for (int k = holes [i].points.Length - 1; k >= 0; k--) {
								if (holes [i].pointIDs [k] == j) {

								}
							}
							holes [i].pointsDirty [j] = false;
						}
						dirty = true;
					}
				}


				break;
			}

			if (dirty) {

				switch (area.canvasType) {
				case Designer.DesignCanvasType.LocalObjective:
					areaPoint = area.transform.LocalObjective(area.center);
					break;
				case Designer.DesignCanvasType.LocalSubjective:
					areaPoint = area.transform.TransformPoint(area.extents.center);
					break;
				case Designer.DesignCanvasType.World:
					areaPoint = area.extents.center;
					break;
				case Designer.DesignCanvasType.Mesh:
					areaPoint = area.extents.center;
					break;
				}
				Vector3 vec = HandleUtility.WorldToGUIPoint (areaPoint);
				labelRect = new Rect (vec, GUI.skin.button.CalcSize (new GUIContent (area.areaName)));
				dirty = false;
			}
		}

		public void Update(){
			switch (area.canvasType) {
			case Designer.DesignCanvasType.World:
				for (int i = shapes.Length - 1; i >= 0; i--) {
					if (shapes [i].dirty) {
						area.shapes[i].points = shapes [i].points.ConvertToVec2();
						for (int j = shapes [i].points.Length - 1; j >= 0; j--) {
							shapes [i].pointsDirty [j] = false;
						}
						dirty = true;
					}
				}

				for (int i = holes.Length - 1; i >= 0; i--) {
					if (holes [i].dirty) {
						area.holes[i].points = holes [i].points.ConvertToVec2();
						for (int j = holes [i].points.Length - 1; j >= 0; j--) {
							holes [i].pointsDirty [j] = false;
						}
						dirty = true;
					}
				}

				for (int i = splines.Length - 1; i >= 0; i--) {
					if (splineControls [i].dirty) {
						area.splines[i].SetSplinePoints(splineControls [i].points.ConvertToVector());
						for (int j = splineControls [i].points.Length - 1; j >= 0; j--) {
							splines [i].pointsDirty [j] = false;
						}

						Spline smoothed = new Spline (area.splines[i], area.splines[i].Length*splineSmoothing); 
						splines [i].points = new Vector3[smoothed.Length];
						for (int j = smoothed.Length - 1; j >= 0; j--) {
							splines [i].points [j] = smoothed[j];

						}

						dirty = true;
					}
				}

				break;

			case Designer.DesignCanvasType.LocalObjective:

				for (int i = shapes.Length - 1; i >= 0; i--) {
					if (shapes [i].dirty) {
						area.shapes[i].points = shapes [i].points.ConvertToVec2();
						for (int j = shapes [i].points.Length - 1; j >= 0; j--) {
							area.shapes[i].points [j] = area.transform.InverseTransformDirection ((Vector3)area.shapes [i].points [j] - area.transform.position);
							shapes [i].pointsDirty [j] = false;
						}
						dirty = true;
					}
				}

				for (int i = holes.Length - 1; i >= 0; i--) {
					if (holes [i].dirty) {
						area.holes[i].points = holes [i].points.ConvertToVec2();
						for (int j = holes [i].points.Length - 1; j >= 0; j--) {
							area.holes[i].points [j] = area.transform.InverseTransformDirection ((Vector3)area.holes [i].points [j] - area.transform.position);
							holes [i].pointsDirty [j] = false;
						}
						dirty = true;
					}
				}

				for (int i = splines.Length - 1; i >= 0; i--) {
					if (splineControls [i].dirty) {
						Vector3[] splinePoints = splineControls [i].points;
						for (int j = splineControls [i].points.Length - 1; j >= 0; j--) {
							splinePoints [j] = area.transform.InverseTransformDirection (splinePoints [j] - area.transform.position);
							splineControls [i].pointsDirty [j] = false;
						}
						area.splines[i].SetSplinePoints(splinePoints.ConvertToVector());
						Spline smoothed = new Spline (area.splines[i], area.splines[i].Length*splineSmoothing); 
						splines [i].points = new Vector3[smoothed.Length];
						for (int j = smoothed.Length - 1; j >= 0; j--) {
							splines [i].points [j] = area.transform.LocalObjective(smoothed[j]);

						}
						dirty = true;
					}
				}

				break;

			case Designer.DesignCanvasType.LocalSubjective:

				for (int i = shapes.Length - 1; i >= 0; i--) {
					if (shapes [i].dirty) {
						area.shapes[i].points = shapes [i].points.ConvertToVec2();
						for (int j = shapes [i].points.Length - 1; j >= 0; j--) {
							area.shapes[i].points [j] = area.transform.InverseTransformPoint (area.shapes [i].points [j]);
							shapes [i].pointsDirty [j] = false;
						}
						dirty = true;
					}
				}

				for (int i = holes.Length - 1; i >= 0; i--) {
					if (holes [i].dirty) {
						area.holes[i].points = holes [i].points.ConvertToVec2();
						for (int j = holes [i].points.Length - 1; j >= 0; j--) {
							area.holes[i].points [j] = area.transform.InverseTransformPoint (area.holes [i].points [j]);
							holes [i].pointsDirty [j] = false;
						}
						dirty = true;
					}
				}

				for (int i = splines.Length - 1; i >= 0; i--) {
					if (splineControls [i].dirty) {
						Vector3[] splinePoints = splineControls [i].points;
						for (int j = splines [i].points.Length - 1; j >= 0; j--) {
							splinePoints [j] = area.transform.InverseTransformPoint (splinePoints [j]);
							splineControls [i].pointsDirty [j] = false;
						}
						area.splines[i].SetSplinePoints(splinePoints.ConvertToVector());

						Spline smoothed = new Spline (area.splines[i], area.splines[i].Length*splineSmoothing); 
						splines [i].points = new Vector3[smoothed.Length];
						for (int j = smoothed.Length - 1; j >= 0; j--) {
							splines [i].points [j] =area.transform.TransformPoint(smoothed [j]);

						}

						dirty = true;
					}
				}

				break;

			case Designer.DesignCanvasType.Mesh:

				for (int i = shapes.Length - 1; i >= 0; i--) {
					if (shapes [i].dirty) {
						Vector2[] meshPoints = new Vector2[area.shapes[i].points.Length];

						for (int j = meshPoints.Length - 1; j >= 0; j--) {
							for (int k = shapes [i].points.Length - 1; k >= 0; k--) {
								if (shapes [i].pointIDs [k] == j) {
									//Convert back into a UV point, then break our last loop.
									//TODO
								}
							}
							shapes [i].pointsDirty [j] = false;
						}
						dirty = true;
					}
				}

				for (int i = holes.Length - 1; i >= 0; i--) {
					if (holes [i].dirty) {
						Vector2[] meshPoints = new Vector2[area.holes[i].points.Length];

						for (int j = meshPoints.Length - 1; j >= 0; j--) {
							for (int k = holes [i].points.Length - 1; k >= 0; k--) {
								if (holes [i].pointIDs [k] == j) {

								}
							}
							holes [i].pointsDirty [j] = false;
						}
						dirty = true;
					}
				}

				for (int i = splines.Length - 1; i >= 0; i--) {
					if (splineControls [i].dirty) {
						Vector3[] splinePoints = new Vector3[area.splines[i].Length];
						for (int j = splinePoints.Length - 1; j >= 0; j--) {
							for (int k = splineControls [i].points.Length - 1; k >= 0; k--) {
								if (splines [i].pointIDs [k] == j) {

								}
							}
							splineControls [i].pointsDirty [j] = false;
						}
						area.splines[i].SetSplinePoints(splinePoints.ConvertToVector());
						dirty = true;
					}
				}
				break;
			}

			if (dirty) {

				switch (area.canvasType) {
				case Designer.DesignCanvasType.LocalObjective:
					areaPoint = area.transform.LocalObjective(area.center);
					break;
				case Designer.DesignCanvasType.LocalSubjective:
					areaPoint = area.transform.TransformPoint(area.extents.center);
					break;
				case Designer.DesignCanvasType.World:
					areaPoint = area.extents.center;
					break;
				case Designer.DesignCanvasType.Mesh:
					areaPoint = area.extents.center;
					break;
				}
				Vector3 vec = HandleUtility.WorldToGUIPoint (areaPoint);
				labelRect = new Rect (vec, GUI.skin.button.CalcSize (new GUIContent (area.areaName)));
				dirty = false;
			}
		}

		public void UpdateSplines(){
			switch (area.canvasType) {
			case Designer.DesignCanvasType.World:

				for (int i = splines.Length - 1; i >= 0; i--) {
					if (splineControls [i].dirty) {
						area.splines[i].SetSplinePoints(splineControls [i].points.ConvertToVector());
						for (int j = splineControls [i].points.Length - 1; j >= 0; j--) {
							splineControls [i].pointsDirty [j] = false;
						}

						Spline smoothed = new Spline (area.splines[i], area.splines[i].Length*splineSmoothing); 
						splines [i].points = new Vector3[smoothed.Length];
						for (int j = smoothed.Length - 1; j >= 0; j--) {
							splines [i].points [j] = smoothed[j];

						}

						dirty = true;
					}
				}

				break;

			case Designer.DesignCanvasType.LocalObjective:

				for (int i = splines.Length - 1; i >= 0; i--) {
					if (splineControls [i].dirty) {
						Vector3[] splinePoints = splineControls [i].points;
						for (int j = splineControls [i].points.Length - 1; j >= 0; j--) {
							splinePoints [j] = area.transform.InverseTransformDirection (splinePoints [j] - area.transform.position);
							splineControls [i].pointsDirty [j] = false;
						}
						area.splines[i].SetSplinePoints(splinePoints.ConvertToVector());
						Spline smoothed = new Spline (area.splines[i], area.splines[i].Length*splineSmoothing); 
						splines [i].points = new Vector3[smoothed.Length];
						for (int j = smoothed.Length - 1; j >= 0; j--) {
							splines [i].points [j] = area.transform.LocalObjective(smoothed[j]);

						}
						dirty = true;
					}
				}

				break;

			case Designer.DesignCanvasType.LocalSubjective:
				

				for (int i = splines.Length - 1; i >= 0; i--) {
					if (splineControls [i].dirty) {
						Vector3[] splinePoints = splineControls [i].points;
						for (int j = splines [i].points.Length - 1; j >= 0; j--) {
							splinePoints [j] = area.transform.InverseTransformPoint (splinePoints [j]);
							splineControls [i].pointsDirty [j] = false;
						}
						area.splines[i].SetSplinePoints(splinePoints.ConvertToVector());

						Spline smoothed = new Spline (area.splines[i], area.splines[i].Length*splineSmoothing); 
						splines [i].points = new Vector3[smoothed.Length];
						for (int j = smoothed.Length - 1; j >= 0; j--) {
							splines [i].points [j] =area.transform.TransformPoint(smoothed [j]);

						}

						dirty = true;
					}
				}

				break;

			case Designer.DesignCanvasType.Mesh:
				
				for (int i = splines.Length - 1; i >= 0; i--) {
					if (splineControls [i].dirty) {
						Vector3[] splinePoints = new Vector3[area.splines[i].Length];
						for (int j = splinePoints.Length - 1; j >= 0; j--) {
							for (int k = splineControls [i].points.Length - 1; k >= 0; k--) {
								if (splines [i].pointIDs [k] == j) {

								}
							}
							splineControls [i].pointsDirty [j] = false;
						}
						area.splines[i].SetSplinePoints(splinePoints.ConvertToVector());
						dirty = true;
					}
				}
				break;
			}

			if (dirty) {

				switch (area.canvasType) {
				case Designer.DesignCanvasType.LocalObjective:
					areaPoint = area.transform.LocalObjective(area.center);
					break;
				case Designer.DesignCanvasType.LocalSubjective:
					areaPoint = area.transform.TransformPoint(area.extents.center);
					break;
				case Designer.DesignCanvasType.World:
					areaPoint = area.extents.center;
					break;
				case Designer.DesignCanvasType.Mesh:
					areaPoint = area.extents.center;
					break;
				}
				Vector3 vec = HandleUtility.WorldToGUIPoint (areaPoint);
				labelRect = new Rect (vec, GUI.skin.button.CalcSize (new GUIContent (area.areaName)));
				dirty = false;
			}
		}
	}
}