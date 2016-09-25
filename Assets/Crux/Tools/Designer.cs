using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Crux{
	public class Designer : MonoBehaviour {
		
		
		public Painter painter = new Painter();
		public DesignArea[] areas = new DesignArea[0];

        public enum DesignCanvasType {World, LocalObjective, LocalSubjective, Mesh}
		public DesignCanvasType canvasType = DesignCanvasType.LocalObjective;

		public int maxCellsToPaintPerFrame = 5;
		public enum DesignerStatus {Neutral, Tiling, Painting, Edging}
		public DesignerStatus status = DesignerStatus.Neutral;

		#if UNITY_EDITOR
		public float snap = 0.5f,fillOverlapTolerance = 0.125f, fillDistance = 0.25f;
		public Vector3 brushSize = Vector3.one;
        public int splineSmoothing = 10, fillSpread = 4;
		#endif
        public IEnumerator FillPolygon(int areaID, int polyID, Vector3 startingPos)
        {
            Polygon p;
            if(polyID < areas[areaID].shapes.Length) { p = areas[areaID].shapes[polyID]; }
            else { p = areas[areaID].holes[polyID- areas[areaID].shapes.Length]; }
            //We'll keep two lists. One of established points that have already attempted to 
            //have kids. The next is the last generation of kids created.
            List<Vector3> establishedPoints = new List<Vector3>();
            List<Vector3> currentGeneration = new List<Vector3>();
            currentGeneration.Add(startingPos);
            while(currentGeneration.Count > 0)
            {
                List<Vector3> nextGeneration = new List<Vector3>();
                for(int i = currentGeneration.Count-1; i >=0; i--)
                {
                    float mop = 1f/fillSpread;
                    for(int j = fillSpread-1; j>=0; j--)
                    {
                        
                        Vector3 point = new Vector3(fillDistance* Mathf.Cos(2*Mathf.PI*j*mop)+currentGeneration[i].x,
                           fillDistance*Mathf.Sin(2*Mathf.PI*j*mop)+ currentGeneration[i].y, currentGeneration[i].z);
                        //Go through each Established Point, and all the OTHER current generation points.
                        //If any of them are within a tolerance to this proposed position, don't generate. 
                        bool addP = true;
                        for (int k = establishedPoints.Count-1; k>=0; k--)
                        {
                            if(establishedPoints[k].WithinTolerance(point, fillOverlapTolerance))
                            {
                                addP = false;
                                break;
                            }
                            
                        }
                        if (!addP)
                        {
                            for(int k = nextGeneration.Count -1; k >=0; k--)
                            {
                                if (nextGeneration[k].WithinTolerance(point, fillOverlapTolerance))
                                {
                                    addP = false;
                                    break;
                                }
                            }
                        }

                        if (addP) {
                            nextGeneration.Add(point);
                            //Paint asset.
                        }
                    }

                    establishedPoints.Add(currentGeneration[i]);
                }
                currentGeneration.Clear();
                currentGeneration = new List<Vector3>(nextGeneration.ToArray());

            }


            yield return null;
        }

		public IEnumerator FillTileGroup(int areaID, int tileGroupID){
			status = DesignerStatus.Painting;
			int m = 0, i;



			Vector3 vec;
			switch(canvasType){
			case DesignCanvasType.World:
				for (i = areas [areaID].tileGroups [tileGroupID].cells.Length - 1; i >= 0; i--) {
					vec = new Vector3(areas [areaID].tileGroups [tileGroupID].cells[i].point.x, areas [areaID].tileGroups [tileGroupID].cells[i].point.y, 0);
					vec.Scale (areas [areaID].tileGroups [tileGroupID].scale);
					if (areas [areaID].tileGroups [tileGroupID].cells [i].content == null) {
						PaintSample tP = painter.Paint ((Vector2)vec);
						if (tP != null) {
							areas [areaID].tileGroups [tileGroupID].cells [i].content = tP.gameObject;
						}
					}
					if(areas [areaID].tileGroups [tileGroupID].cells[i].content == null){continue;}
					areas [areaID].tileGroups [tileGroupID].cells [i].content.name = areas [areaID].tileGroups [tileGroupID].name + " " + i.ToString ();
					areas [areaID].tileGroups [tileGroupID].cells [i].content.transform.parent = areas [areaID].tileGroups [tileGroupID].areaContent.transform;
					areas [areaID].tileGroups [tileGroupID].cells [i].content.transform.eulerAngles = new Vector3 (0, 0, areas [areaID].tileGroups [tileGroupID].cells [i].point.z);
					areas [areaID].tileGroups [tileGroupID].cells [i].content.transform.localScale = areas [areaID].tileGroups [tileGroupID].cells [i].content.transform.LossyScale (new Vector3 (areas [areaID].tileGroups [tileGroupID].cells [i].scale.x, areas [areaID].tileGroups [tileGroupID].cells [i].scale.y, 1));
					m++;
					if (m >= maxCellsToPaintPerFrame) {
						m = 0;
						yield return null;
					}
				}
				break;

				case DesignCanvasType.LocalObjective:
				for (i = areas [areaID].tileGroups [tileGroupID].cells.Length - 1; i >= 0; i--) {
					vec = new Vector3(areas [areaID].tileGroups [tileGroupID].cells[i].point.x, areas [areaID].tileGroups [tileGroupID].cells[i].point.y, 0);
					vec.Scale (areas [areaID].tileGroups [tileGroupID].scale);
					if (areas [areaID].tileGroups [tileGroupID].cells [i].content == null) {
						PaintSample tP = painter.Paint (transform.position + transform.TransformDirection (vec));
						if (tP) {
							areas [areaID].tileGroups [tileGroupID].cells [i].content = tP.gameObject;
						}
					}
					if(areas [areaID].tileGroups [tileGroupID].cells[i].content == null){continue;}
					//areas [areaID].tileGroups [tileGroupID].cells[i].content = painter.Paint((Vector2)vec);
					areas [areaID].tileGroups [tileGroupID].cells [i].content.name = areas [areaID].tileGroups [tileGroupID].name + " " + i.ToString ();
					areas [areaID].tileGroups [tileGroupID].cells [i].content.transform.parent = areas [areaID].tileGroups [tileGroupID].areaContent.transform;
					vec = areas [areaID].tileGroups [tileGroupID].cells [i].content.transform.localEulerAngles;
					vec.z = areas [areaID].tileGroups [tileGroupID].cells [i].point.z;
					areas [areaID].tileGroups [tileGroupID].cells [i].content.transform.localEulerAngles= vec;
					areas [areaID].tileGroups [tileGroupID].cells [i].content.transform.localScale = areas [areaID].tileGroups [tileGroupID].cells [i].content.transform.LossyScale (new Vector3 (areas [areaID].tileGroups [tileGroupID].cells [i].scale.x, areas [areaID].tileGroups [tileGroupID].cells [i].scale.y, 1));
					m++;
					if (m >= maxCellsToPaintPerFrame) {
						m = 0;
						yield return null;
					}
				}
					break;
				case DesignCanvasType.LocalSubjective:
				for (i = areas [areaID].tileGroups [tileGroupID].cells.Length - 1; i >= 0; i--) {
					vec = new Vector3(areas [areaID].tileGroups [tileGroupID].cells[i].point.x, areas [areaID].tileGroups [tileGroupID].cells[i].point.y, 0);
					vec.Scale (areas [areaID].tileGroups [tileGroupID].scale);
					if (areas [areaID].tileGroups [tileGroupID].cells [i].content == null) {
						PaintSample tP = painter.Paint (transform.position + transform.TransformDirection (vec));
						if (tP) {
							areas [areaID].tileGroups [tileGroupID].cells [i].content = tP.gameObject;
						}
					} 
					if(areas [areaID].tileGroups [tileGroupID].cells[i].content == null){continue;}
					areas [areaID].tileGroups [tileGroupID].cells [i].content.name = areas [areaID].tileGroups [tileGroupID].name + " " + i.ToString ();
					areas [areaID].tileGroups [tileGroupID].cells [i].content.transform.parent = areas [areaID].tileGroups [tileGroupID].areaContent.transform;
					vec = areas [areaID].tileGroups [tileGroupID].cells [i].content.transform.localEulerAngles;
					vec.z = areas [areaID].tileGroups [tileGroupID].cells [i].point.z;
					areas [areaID].tileGroups [tileGroupID].cells [i].content.transform.localEulerAngles= vec;
					areas [areaID].tileGroups [tileGroupID].cells [i].content.transform.localScale = new Vector3 (areas [areaID].tileGroups [tileGroupID].cells [i].scale.x, areas [areaID].tileGroups [tileGroupID].cells [i].scale.y, 1);
					m++;
					if (m >= maxCellsToPaintPerFrame) {
						m = 0;
						yield return null;
					}
				}
					break;
			case DesignCanvasType.Mesh:
				//Renderer r = transform.GetComponent<Renderer> ();
				for (i = areas [areaID].tileGroups [tileGroupID].cells.Length - 1; i >= 0; i--) {
					
					m++;
					if (m >= maxCellsToPaintPerFrame) {
						m = 0;
						yield return null;
					}
				}
					break;
			}
			yield return null;
			status = DesignerStatus.Neutral;
		}

		public IEnumerator PaintSpline(int areaID, int splineID, int splineSamples = 20){
			status = DesignerStatus.Painting;

			int m = 0;
			GameObject splineLines = new GameObject (), line;
			int i;

			int smoothed = areas [areaID].splines [splineID].Length * splineSamples;
			float per = 1 / smoothed, currentP = 1;
			Vector current = areas[areaID].splines[splineID].GetPoint(currentP), last;
			for (i = smoothed-1; i >= 1; i--) {
				currentP -= per;
				last = current;
				current = areas [areaID].splines [splineID].GetPoint (currentP);
				line = painter.PaintLine (last, current);
				line.name = areas [areaID].splines [splineID].name + " " + i.ToString ();
				line.transform.parent = splineLines.transform;
				m++;
				if (m >= maxCellsToPaintPerFrame) {
					m = 0;
					yield return null;
				}

			}

			yield return null;
			status = DesignerStatus.Neutral;
		}

		public IEnumerator PaintPolygon(int areaID){
			status = DesignerStatus.Painting;
			
			int m = 0;
			GameObject areaLines = new GameObject (), shapeLines, line;
			int i, j;
			Vector3[] pt;
			for (i = areas [areaID].shapes.Length - 1; i >= 0; i--) {

				shapeLines = new GameObject (areas [areaID].shapes [i].name + " lines");
				shapeLines.transform.parent = areaLines.transform;

				pt = areas [areaID].shapes [i].points.ConvertToVec3();
				for (j = pt.Length - 1; j >= 0; j--) {
					
					if (j < pt.Length - 1) {
						line = painter.PaintLine (pt [j], pt [j + 1]);
					} else {
						line = painter.PaintLine (pt [j], pt [0]);
					}
					line.name = areas [areaID].shapes [i].name + " " + j.ToString ();
					line.transform.parent = shapeLines.transform;
					m++;
					if (m >= maxCellsToPaintPerFrame) {
						m = 0;
						yield return null;
					}
				}
			}

			for (i = areas [areaID].holes.Length - 1; i >= 0; i--) {

				shapeLines = new GameObject (areas [areaID].holes [i].name + " lines");
				shapeLines.transform.parent = areaLines.transform;

				pt = areas [areaID].holes [i].points.ConvertToVec3();
				for (j = pt.Length - 1; j >= 0; j--) {

					if (j < pt.Length - 1) {
						line = painter.PaintLine  (pt [j + 1],pt [j]);
					} else {
						line = painter.PaintLine (pt [0],pt [j]);
					}
					line.name = areas [areaID].holes [i].name + " " + j.ToString ();
					line.transform.parent = shapeLines.transform;
					m++;
					if (m >= maxCellsToPaintPerFrame) {
						m = 0;
						yield return null;
					}
				}
			}

			yield return null;
			status = DesignerStatus.Neutral;
		}
	}

	public class DesignArea : MonoBehaviour {

		public Designer.DesignCanvasType canvasType = Designer.DesignCanvasType.World;
		public Spline[] splines = new Spline[0];
		public Polygon[] shapes = new Polygon[0];
		public Polygon[] holes = new Polygon[0];
		public TileGroup[] tileGroups = new TileGroup[0];

		public Collider[] colliders = new Collider[0];
		public Collider2D[] colliders2D = new Collider2D[0];

		#if UNITY_EDITOR
		public string areaName;
		public Color color;
		public bool draw;
		#endif

		bool dirty;
		Rect _extents;
		public bool Contains(Vector2 point, int id = -1){
			bool has = false;
			int i;

			switch (canvasType) {
			case Designer.DesignCanvasType.LocalObjective:
				point = transform.InverseTransformDirection ((Vector3)point - transform.position);
				break;
			case Designer.DesignCanvasType.LocalSubjective:
				point = transform.InverseTransformPoint (point);
				break;
			case Designer.DesignCanvasType.Mesh:
				//TODO
				break;
			}
			if (id < 0) {
				for (i = shapes.Length - 1; i >= 0; i--) {
					if (shapes [i].ContainsPoint (point)) {
						has = true;
					}
				}
				for (i = holes.Length - 1; i >= 0; i--) {
					if (holes [i].ContainsPoint (point)) {
						has = false;
					}
				}
			} else {
				if (id < shapes.Length) {
					if (shapes [id].ContainsPoint (point)) {
						return true;
					}
				} else {
					if (holes [id-shapes.Length].ContainsPoint (point)) {
						return true;
					}
				}
			}
			return has;	
		}

		public Vector2 center {
			get {return extents.center; }
			set {
				Vector2 diff = value - _extents.center;
				for (int i = shapes.Length - 1; i >= 0; i--) {
					Vector2[] points = shapes [i].points;
					for (int j = points.Length - 1; j >= 0; j--) {
						points [j] += diff;
					}
					shapes [i].points = points;
				}
				for (int i = holes.Length - 1; i >= 0; i--) {
					Vector2[] points = holes [i].points;
					for (int j = points.Length - 1; j >= 0; j--) {
						points [j] += diff;
					}
					holes [i].points = points;
				}
				for (int i = splines.Length - 1; i >= 0; i--) {
					for (int j = splines [i].Length - 1; j >= 0; j--) {
						Vector sj = splines [i] [j];
						sj [0] += diff.x;
						if (sj.Length > 1) {
							sj [1] += diff.y;
						}
						splines [i] [j] = sj;
					}
				}
				dirty = true;
			}
		}

		public Rect extents{
			get{ 
				if (dirty) {
					_extents = new Rect ();
					for (int i = shapes.Length - 1; i >= 0; i--) {
						Rect sR = shapes [i].extents;
						if (sR.xMin < _extents.xMin) {
							_extents.xMin = sR.xMin;
						} 
						if (sR.xMax > _extents.xMax) {
							_extents.xMax = sR.xMax;
						}
						if (sR.yMin < _extents.yMin) {
							_extents.yMin = sR.yMin;
						} 
						if (sR.yMax > _extents.yMax) {
							_extents.yMax = sR.yMax;
						}
					}
					for (int i = splines.Length - 1; i >= 0; i--) {
						for (int j = splines [i].Length - 1; j >= 0; j--) {
							Vector sj = splines [i] [j];
							if (sj.Length > 1) {
								if (sj [0] < _extents.xMin) {
									_extents.xMin = sj [0];
								}
								if (sj [0] > _extents.xMax) {
									_extents.xMax = sj [0];
								}
								if (sj [1] > _extents.yMax) {
									_extents.yMax = sj [1];
								}
								if (sj [1] < _extents.yMin) {
									_extents.yMin = sj [1];
								}
							}
						}
					}
					dirty = false;
				}
				return _extents;
			}
		}



		/* Goals:
		 * 1. Paint along shape lines and splines
		 * 2. Fill shapes, excluding holes
		 * 3. Create, destroy, and modify Colliders.
		 * 4. Do all these things in real time, with memory management thought out.
		*/

		public IEnumerator TileArea(int tileGroupID, int graphicID = -1, TileGroup.WallpaperGroup wg = TileGroup.WallpaperGroup.p1, Vector3 start = new Vector3()){
			//status = DesignerStatus.Tiling;

			if(tileGroupID < 0 || tileGroupID > tileGroups.Length){
				System.Array.Resize (ref tileGroups, tileGroups.Length + 1);
				tileGroupID = tileGroups.Length - 1;
				tileGroups [tileGroupID] = new TileGroup ();
				tileGroups [tileGroupID].wallpaper = wg;
				tileGroups [tileGroupID].SetCellPolyFromWallpaper ();
                tileGroups[tileGroupID].startingPoint = start;
			}
		    
			Transform tileTransform = new GameObject ("TileTransform").transform;
			TileCell currentTile;
			Vector2[] pts;
			tileTransform.hideFlags = HideFlags.HideInHierarchy;
			int i, j, k, l,  b;

			List<TileCell> newCells = new List<TileCell> (tileGroups[tileGroupID].cells);
			List<Vector2> newCellPoints = new List<Vector2> (tileGroups[tileGroupID].cellPoints);

			Rect areaExtents = extents;
			Rect areaCoverage = new Rect (0,0,tileGroups[tileGroupID].scale.x,tileGroups[tileGroupID].scale.y);
			areaCoverage.center = areaExtents.center;
			int cGen = 0,tC = 0;

			if (newCells.Count == 0) {
				if (Contains (tileGroups[tileGroupID].startingPoint, graphicID)) {
					TileCell modifiedBase = new TileCell (tileGroups[tileGroupID].metaCell);
					modifiedBase.point = tileGroups[tileGroupID].startingPoint;

					//TileEdge edge;
					pts = tileGroups[tileGroupID].metaCellPoly.points;
					for (i = pts.Length - 1; i >= 0; i--) {
						pts [i].Scale (modifiedBase.scale);
						pts [i] += (Vector2)modifiedBase.point;
						//pts [i].z = 0;
						modifiedBase.points [i] = newCellPoints.Count;
						newCellPoints.Add (pts [i]);

					}
					modifiedBase.edgeCell = true;
					newCells.Add (modifiedBase);
				}
			} else {

				List<int> oldCellsToCheck = new List<int> ();
				//1. Do we have pre-existing cells? If so, create any neighbors they DON'T have (value -1) Add these to newCells. Move on.
				for (i = newCells.Count - 1; i >= 0; i--) {
					if (newCells [i].edgeCell) {
						for (j = newCells [i].edges.Length - 1; j >= 0; j--) {
							k = newCells [i].edges [j];
							if (tileGroups[tileGroupID].edges [k].isBorder && Contains (newCellPoints [tileGroups[tileGroupID].edges [k].pointA], graphicID) && Contains (newCellPoints [tileGroups[tileGroupID].edges [k].pointB], graphicID)) {
								oldCellsToCheck.Add (i);
							}
						}
					}
				}
				if (tileGroups[tileGroupID].tilesPerCycle > 0) {
					tC = 0;
					yield return null;
				}
				//Debug.Log (oldCellsToCheck.Count.ToString () + " old cells to check.");
				for (b = oldCellsToCheck.Count - 1; b >= 0; b--) {
					i = oldCellsToCheck [b];

					for (j = newCells [i].neighbors.Length - 1; j >= 0; j--) {
						if (newCells [i].neighbors [j] == -2) {
							currentTile = newCells [i].GetNeighbor (i, j, tileTransform, tileGroups[tileGroupID].wallpaper);

							tC++;
							if (tileGroups[tileGroupID].tilesPerCycle > 0 && tC >= tileGroups[tileGroupID].tilesPerCycle) {
								tC = 0;
								//tileCycle++;
								tileGroups[tileGroupID].onTileCycle.Invoke ();
								yield return null;
							}
							//Debug.Log ("Checking to see if we already have this cell.");
							bool alreadyHaveCell = false;
							for (k = newCells.Count - 1; k >= 0; k--) {
								if (tileGroups[tileGroupID].WithinTolerance (currentTile, newCells [k])) {
									alreadyHaveCell = true; 
									newCells [i].neighbors [j] = k;
									break;
								}
							}

							if (!alreadyHaveCell) {
								//Debug.Log ("Adding new cell from old.");
								Vector3 scaledPos = currentTile.point;
								scaledPos.Scale (tileGroups[tileGroupID].scale);
								scaledPos.z = 0;
								if (Contains (scaledPos, graphicID)) {
									//Debug.Log ("New cell is in area.");
									pts = tileGroups[tileGroupID].metaCellPoly.points;
									for (k = pts.Length - 1; k >= 0; k--) {
										pts [k].Scale (currentTile.scale);
										tileTransform.eulerAngles = Vector3.zero;
										tileTransform.position = pts [k]; 
										if (currentTile.point.z != 0) {
											tileTransform.RotateAround (Vector3.zero, Vector3.forward, currentTile.point.z); 
										}
										pts [k] = tileTransform.position + currentTile.point;
										bool hasPoint = false;
										for (l = newCellPoints.Count - 1; l >= 0; l--) {

											if (pts [k].InTolerance (newCellPoints [l], 0.1f)) {
												hasPoint = true;
												currentTile.points [k] = l;
												break;
											}
										}
										if (!hasPoint) {
											newCellPoints.Add (pts [k]);
											currentTile.points [k] = newCellPoints.Count - 1;
										}
									}
									newCells [i].neighbors [j] = newCells.Count;
									newCells.Add (currentTile); 


									if (scaledPos.x < areaCoverage.xMin) {
										areaCoverage.xMin = scaledPos.x;
									} else if (scaledPos.x > areaCoverage.xMax) {
										areaCoverage.xMax = scaledPos.x;
									}
									if (scaledPos.y < areaCoverage.yMin) {
										areaCoverage.yMin = scaledPos.y;
									} else if (scaledPos.y > areaCoverage.yMax) {
										areaCoverage.yMax = scaledPos.y;
									}
									tileGroups[tileGroupID].onTileStep.Invoke ();
								} else {
									newCells [i].neighbors [j] = -2;
									if (scaledPos.x - tileGroups[tileGroupID].scale.x <= areaExtents.xMin) {
										areaCoverage.xMin = areaExtents.xMin;
									}
									if (scaledPos.y - tileGroups[tileGroupID].scale.y <= areaExtents.yMin) {
										areaCoverage.yMin = areaExtents.yMin;
									}
									if (scaledPos.x + tileGroups[tileGroupID].scale.x >= areaExtents.xMax) {
										areaCoverage.xMax = areaExtents.xMax;
									}
									if (scaledPos.y + tileGroups[tileGroupID].scale.y >= areaExtents.yMax) {
										areaCoverage.yMax = areaExtents.yMax;
									}
								}
								if (tileGroups[tileGroupID].tilesPerCycle > 0) {
									tC = 0;
									yield return null;
								}
							} 
						}
					}
				}
			}


			if (newCells.Count == 0) {
				yield break;
			}
			//Debug.Log ("Filling area. We have " + newCells.Count.ToString () + " tiles that we're starting with.");
			bool incomplete = true;
			while (incomplete || (areaCoverage.xMin > areaExtents.xMin || areaCoverage.yMin > areaExtents.yMin ||
				areaCoverage.xMax < areaExtents.xMax || areaCoverage.yMax < areaExtents.yMax)) {
				incomplete = false;

				int c = newCells.Count;
				cGen++;
				for (i = 0; i < c; i++) {

					//For each cell we have, check each of its neighbors.
					for (j = 0; j < newCells [i].neighbors.Length; j++) {
						if (newCells [i].neighbors [j] == -1) {
							incomplete = true;

							currentTile = newCells [i].GetNeighbor (i,j,tileTransform, tileGroups[tileGroupID].wallpaper);

							tC++;
							if (tileGroups[tileGroupID].tilesPerCycle >0 && tC >= tileGroups[tileGroupID].tilesPerCycle) {
								tC = 0;
								//tileCycle++;
								tileGroups[tileGroupID].onTileCycle.Invoke ();
								yield return null;
							}

							bool alreadyHaveCell = false;
							for (k = newCells.Count - 1; k >= 0; k--) {
								if (tileGroups[tileGroupID].WithinTolerance (currentTile, newCells [k])) {
									alreadyHaveCell = true; 
									newCells [i].neighbors [j] = k;
									break;
								}
							}

							if (!alreadyHaveCell) {
								Vector3 scaledPos = currentTile.point;
								scaledPos.Scale (tileGroups[tileGroupID].scale);
								scaledPos.z = 0;
								if (Contains(scaledPos, graphicID)) {
									pts = tileGroups[tileGroupID].metaCellPoly.points;
									for (k = pts.Length - 1; k >= 0; k--) {
										pts [k].Scale (currentTile.scale);
										tileTransform.eulerAngles = Vector3.zero;
										tileTransform.position = pts [k]; 
										if (currentTile.point.z != 0) {
											tileTransform.RotateAround (Vector3.zero, Vector3.forward, currentTile.point.z); 
										}
										pts [k] = tileTransform.position + currentTile.point;
										bool hasPoint = false;
										for (l = newCellPoints.Count - 1; l >= 0; l--) {

											if(pts[k].InTolerance(newCellPoints[l], 0.1f)){
												hasPoint = true;
												currentTile.points [k] = l;
												break;
											}
										}
										if (!hasPoint) {
											currentTile.points [k] = newCellPoints.Count;
											newCellPoints.Add (pts [k]);
										}
									}

									newCells [i].neighbors [j] = newCells.Count;
									newCells.Add (currentTile); 


									if (scaledPos.x < areaCoverage.xMin) {
										areaCoverage.xMin = scaledPos.x;
									}else if (scaledPos.x > areaCoverage.xMax) {
										areaCoverage.xMax = scaledPos.x;
									}
									if (scaledPos.y < areaCoverage.yMin) {
										areaCoverage.yMin = scaledPos.y;
									}else if (scaledPos.y > areaCoverage.yMax) {
										areaCoverage.yMax = scaledPos.y;
									}
									tileGroups[tileGroupID].onTileStep.Invoke ();
								} else {
									newCells [i].neighbors [j] = -2;
									if (scaledPos.x - tileGroups[tileGroupID].scale.x <= areaExtents.xMin) {
										areaCoverage.xMin = areaExtents.xMin;
									}
									if (scaledPos.y - tileGroups[tileGroupID].scale.y <= areaExtents.yMin) {
										areaCoverage.yMin = areaExtents.yMin;
									}
									if (scaledPos.x + tileGroups[tileGroupID].scale.x >= areaExtents.xMax) {
										areaCoverage.xMax = areaExtents.xMax;
									}
									if (scaledPos.y + tileGroups[tileGroupID].scale.y >= areaExtents.yMax) {
										areaCoverage.yMax = areaExtents.yMax;
									}
								}
								if (tileGroups[tileGroupID].tilesPerCycle > 0) {
									tC = 0;
									yield return null;
								}
							} 
						}
					}
				}
			}

			int[] newPointIDs = new int[newCellPoints.Count];

			bool oneTimeAdjustment = true;
			//Debug.Log ("Syncing " + tileGroups[tileGroupID].cells.Length.ToString () + " old tiles with " + newCells.Count.ToString () + " new tiles.");
			if (tileGroups[tileGroupID].maxCells > 0 && newCells.Count > tileGroups[tileGroupID].maxCells) {
				//1. Go through and get rid of our differences worth from the start.
				//2. Check to see if there are any 0 gens left, etc. Move all back until our lowest is at 0.
				//3. Go through our points to find any that are no longer used, along with any edges that FEATURE them.
				int dif = newCells.Count - tileGroups[tileGroupID].maxCells;

				newCells.RemoveRange (0, dif);
				tC = 0;
				for (i = newCellPoints.Count - 1; i >= 0; i--) {
					newPointIDs [i] = i;
				}
				for (i = newCellPoints.Count - 1; i >= 0; i--) {

					bool stillHas = false;
					for (j = newCells.Count - 1; j >= 0; j--) {
						if (oneTimeAdjustment) {
							for (k = newCells [j].edgeNeighbors.Length - 1; k >= 0; k--) {
								newCells [j].edgeNeighbors [k] -= dif;
							}
						}
						for (k = newCells [j].points.Length - 1; k >= 0; k--) {
							if (newCells [j].points [k] == i) {
								stillHas = true;
								if (!oneTimeAdjustment) {
									break;
								}
							}
						}
						if (stillHas) {
							break;
						}
					}
					oneTimeAdjustment = false;
					if (!stillHas) {
						newCellPoints.RemoveAt (i);
						newPointIDs [i] = -1;
						for (j = i+1; j < newPointIDs.Length; j++) {
							newPointIDs [j]--;
						}
					}
					tC++;
					if (tileGroups[tileGroupID].tilesPerCycle >0 && tC >= tileGroups[tileGroupID].tilesPerCycle) {
						tC = 0;
						//tileCycle++;
						//tileGroups[tileGroupID].onTileCycle.Invoke ();
						yield return null;
					}
				}

				for (i = newCells.Count - 1; i >= 0; i--) {
					for (j = newCells [i].points.Length - 1; j >= 0; j--) {
						newCells [i].points [j] = newPointIDs [newCells [i].points [j]];
						if (newCells [i].points [j] <0) {
							//Debug.Log ("Well, this should not have happened: " + i.ToString() + "," + j.ToString ());
						}
					}
				}
				for (i = tileGroups[tileGroupID].edges.Length - 1; i >= 0; i--) {
					tileGroups[tileGroupID].edges [i].pointA = newPointIDs [tileGroups[tileGroupID].edges [i].pointA];
					tileGroups[tileGroupID].edges [i].pointB = newPointIDs [tileGroups[tileGroupID].edges [i].pointB];
				}
				if (tileGroups[tileGroupID].tilesPerCycle > 0) {
					tC = 0;
					yield return null;
				}

			}
			tileGroups[tileGroupID].cells = newCells.ToArray ();
			tileGroups[tileGroupID].cellPoints = newCellPoints.ToArray ();

			List<TileEdge> areaEdges = new List<TileEdge> (tileGroups[tileGroupID].edges);
			//Debug.Log ("Cell generation complete. We have " + tileGroups[tileGroupID].cells.Length.ToString () + " cells and " + tileGroups[tileGroupID].cellPoints.Length.ToString () + " cell points. Finding new edges on top of " +
			//	tileGroups[tileGroupID].edges.Length.ToString() + " old ones.");

			for (i = tileGroups[tileGroupID].cells.Length - 1; i >= 0; i--) {
				for (j = tileGroups[tileGroupID].cells [i].points.Length - 1; j >= 0; j--) {
					if(tileGroups[tileGroupID].cells[i].edgeNeighbors[j] <0 ){
						TileEdge t;
						if (j < tileGroups[tileGroupID].cells[i].points.Length-1) {
							t = new TileEdge (tileGroups[tileGroupID].cells [i].points [j], tileGroups[tileGroupID].cells [i].points [j + 1]);
						} else {
							t = new TileEdge (tileGroups[tileGroupID].cells [i].points [j], tileGroups[tileGroupID].cells [i].points [0]);
						}
						bool hasEdge = false;
						for (k = areaEdges.Count - 1; k >= 0; k--) {
							if (t.Compare (areaEdges [k])) {
								hasEdge = true;
								TileEdge ae = areaEdges [k]; ae.isBorder = false;
								areaEdges [k] = ae;
								tileGroups[tileGroupID].cells [i].edges [j] = k;
								break;
							}
						}
						if (!hasEdge) {
							t.isBorder = true;
							tileGroups[tileGroupID].cells [i].edges [j] = areaEdges.Count;
							areaEdges.Add (t);
						}
					}
				}
				tC++;
				if (tileGroups[tileGroupID].tilesPerCycle >0 && tC >= tileGroups[tileGroupID].tilesPerCycle) {
					tC = 0;
					yield return null;
				}
			}

			tileGroups[tileGroupID].edges = areaEdges.ToArray ();
			if (tileGroups[tileGroupID].tilesPerCycle > 0) {
				tC = 0;
				yield return null;
			}
			//Debug.Log ("We now have " + tileGroups[tileGroupID].edges.Length.ToString () + " edges. Finding shared edges.");
			for(i = tileGroups[tileGroupID].cells.Length-1; i >=0; i--){
				tileGroups[tileGroupID].cells [i].edgeCell = false;
				for (j = tileGroups[tileGroupID].cells [i].edges.Length - 1; j >= 0; j--) {
					if(tileGroups[tileGroupID].cells[i].edgeNeighbors[j] <0 ){
						if (!tileGroups[tileGroupID].edges [tileGroups[tileGroupID].cells [i].edges [j]].isBorder) {
							bool shared = false;
							for (k = tileGroups[tileGroupID].cells.Length - 1; k >= 0; k--) {
								if (i != k) {
									for (l = tileGroups[tileGroupID].cells [k].edges.Length - 1; l >= 0; l--) {
										if (tileGroups[tileGroupID].cells [i].edges [j] == tileGroups[tileGroupID].cells [k].edges [l]) {
											tileGroups[tileGroupID].cells [i].edgeNeighbors [j] = k;
											tileGroups[tileGroupID].cells [k].edgeNeighbors [l] = i; 
											shared = true;
											break;
										}
									}
								}
								if (shared) {
									break;
								}
							}
						} else {
							tileGroups[tileGroupID].cells [i].edgeCell = true;
						}
					}
				}
				tC++;
				if (tileGroups[tileGroupID].tilesPerCycle >0 && tC >= tileGroups[tileGroupID].tilesPerCycle) {
					tC = 0;
					yield return null;
				}
			}
			if (!Application.isEditor) {
				Object.Destroy (tileTransform.gameObject);
			} else {
				Object.DestroyImmediate (tileTransform.gameObject);
			}

			yield return null;
			tileGroups[tileGroupID].onTileComplete.Invoke ();
		}
			

		public IEnumerator ConvertTilesToShapes(int tileGroup){
			//Get just our perimeter points. 
			List<TileEdge> edges = new List<TileEdge>();
			int startEdge = -1, endEdge = -1;

			int startID = -1;
			Vector2 startPoint = tileGroups[tileGroup].extents.center;

			for (int i = tileGroups [tileGroup].edges.Length - 1; i >= 0; i--) {
				if (tileGroups [tileGroup].edges [i].isBorder) {
					TileEdge current = tileGroups [tileGroup].edges [i];

					edges.Add (current);
				}
			}

			List<Polygon> totalPolys = new List<Polygon> ();
			while(edges.Count >2){


				List<Vector2> thisShape = new List<Vector2> ();
				for (int i = edges.Count - 1; i >= 0; i--) {

					TileEdge current = edges [i];

					//Find the most upper leftist.
					Vector2 workVec;

					workVec = tileGroups [tileGroup].cellPoints [current.pointA];
					if (workVec.x <= startPoint.x && workVec.y > startPoint.y) {
						startPoint = workVec;
						startID = current.pointA;

					}

					workVec = tileGroups [tileGroup].cellPoints [current.pointB];
					if (workVec.x <= startPoint.x && workVec.y > startPoint.y) {
						startPoint = workVec;
						startID = current.pointB;
					}
				}

				Vector2 currentPoint = tileGroups [tileGroup].cellPoints [startID];
				thisShape.Add (currentPoint);

				//1. Find our startEdge and EndEdge. They both make use of our starting point.
				for (int i = edges.Count - 1; i >= 0; i--) {
					if (edges [i].pointA == startID || edges[i].pointB == startID) {
						if (startEdge == -1) {
							startEdge = i;
						} else if (endEdge == -1) {
							endEdge = i;
						} else {
							break;
						}
					} 
				}

				Vector2 startA = tileGroups [tileGroup].cellPoints [edges [startEdge].pointA],
				endA = tileGroups [tileGroup].cellPoints [edges [endEdge].pointA],
				startB = tileGroups [tileGroup].cellPoints [edges [startEdge].pointB],
				endB = tileGroups [tileGroup].cellPoints [edges [endEdge].pointB],
				diff1, diff2;

				if (startA != currentPoint) {
					if (endA != currentPoint) {
						//current, sa, ea
						diff1 = startA - currentPoint;
						diff2 = endA - currentPoint;
					} else {
						//current, sa, eb
						diff1 = startA - currentPoint;
						diff2 = endB - currentPoint;
					}
				} else {
					if (endA != currentPoint) {
						//current, sb, ea
						diff1 = startB - currentPoint;
						diff2 = endA - currentPoint;
					} else {
						//current, sb, eb
						diff1 = startB - currentPoint;
						diff2 = endB - currentPoint;
					}
				}
				if (diff2.x >= diff1.x && diff2.y > diff1.y) {
					//swap!
					int swapper = startEdge;
					startEdge = endEdge;
					endEdge = swapper;
				} 
				int currentEdge = startEdge;

				//Then we just go through all our remaining edges, point to point, until we arrive back at end. Each time we complete an edge, we remove it from consideration.
				while(currentEdge != endEdge){
					//First, find the point that's NOT currentPoint. Add it to our pointsList.
					if (tileGroups [tileGroup].cellPoints [edges [currentEdge].pointA] != currentPoint) {
						currentPoint = tileGroups [tileGroup].cellPoints [edges [currentEdge].pointA];
						thisShape.Add (currentPoint);
					} else {
						currentPoint = tileGroups [tileGroup].cellPoints [edges [currentEdge].pointB];
						thisShape.Add (currentPoint);
					}

					//Then, remove that Edge.
					edges.RemoveAt(currentEdge);
					if (endEdge > currentEdge) {
						endEdge--;
					}
					//Last, find our next edge.
					for (int i = edges.Count - 1; i >= 0; i--) {
						//Find another that has currentPoint. That's our next link.
						if(tileGroups [tileGroup].cellPoints [edges[i].pointA] == currentPoint|| tileGroups [tileGroup].cellPoints [edges[i].pointB] == currentPoint){
							currentEdge = i;
						}
					}
				}
				totalPolys.Add(new Polygon(thisShape.ToArray()));
				//When we've arrived back at the start, check to see if we've still got unused edges. They're part of another shape.
			}
			//Now we need to sort shapes from holes.
			List<Polygon> newShapes = new List<Polygon>(shapes);
			List<Polygon> newHoles = new List<Polygon> (holes);
			for (int i = 0; i < totalPolys.Count; i++) {
				bool isShape = false;
				for (int j = tileGroups [tileGroup].cells.Length - 1; j >= 0; j--) {
					if(totalPolys[i].ContainsPoint(tileGroups [tileGroup].cells[j].point)){
						isShape = true;
						break;
					}
				}
				if (isShape) {
					newShapes.Add (totalPolys [i]);
				} else {
					newHoles.Add (totalPolys [i]);
				}
			}
			shapes = newShapes.ToArray ();
			holes = newHoles.ToArray ();
			yield return null;
		}

		public IEnumerator ConvertShapeToCollider(int id){
			PolygonCollider2D collider = new GameObject(shapes[id].name).AddComponent<PolygonCollider2D>();
			collider.gameObject.hideFlags = HideFlags.HideInHierarchy;
			collider.transform.parent = transform;

			Polygon poly = new Polygon( shapes [id]);
			poly.ReducePoints (2, float.PositiveInfinity);
			collider.points = poly.points;
			yield return null;
		}

		public void ConvertPolygonCollidersToShapes(PolygonCollider2D[] polys){
			Polygon workPoly;
			List<Polygon> shaps = new List<Polygon> (shapes);
			for (int i = 0; i < polys.Length; i++) {
				workPoly = new Polygon (polys[i].gameObject.name);
				workPoly.points = polys [i].points;
				shaps.Add (workPoly);
			}
			shapes = shaps.ToArray ();
		}
	}
}