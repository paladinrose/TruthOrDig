using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Crux{
	
	[System.Serializable]
	public class TileGroup {


		#if UNITY_EDITOR
		public Color32 color = Color.white, sColor = new Color(0.75f, 0.75f, 0.75f);
		public bool draw;
		#endif

		public enum WallpaperGroup {
			p1, p2, pm, pg, cm,
			p2mm, p2mg, p2gg, c2mm,
			p4, p4mm, p4mg, p3, p3m1,
			p31m, p6, p6mm, Penrose
		}



		bool dirty;

		public string name = "New Tile Group";

		public UnityEvent onTileStart = new UnityEvent(), onTileClear= new UnityEvent(), 
		onTileStep= new UnityEvent(), onTileCycle= new UnityEvent(), onTileComplete= new UnityEvent();
		public float cellPositionTolerance = 0.01f, cellRotationTolerance = 61, cellScaleTolerance = 0.01f;
		public int tilesPerCycle = 125;
		public TileCell metaCell = new TileCell ();
		public Polygon metaCellPoly = new Polygon ("Meta Cell");
		public bool setup, useMetaCell, recycleCells;
		public int maxCells = 0;
		public TileCell[] cells = new TileCell[0];
		public Vector2[] cellPoints = new Vector2[0];
		public TileEdge[] edges = new TileEdge[0];
		public WallpaperGroup wallpaper;
		public int[] palletPattern = new int[]{0};
		public Vector3 scale = Vector3.one;

		public Vector2 startingPoint = Vector2.zero;
		public GameObject areaContent;

		//Might not want this - at least, as it is. Should calculate based on cells.
		Rect _extents;

		public void ClearContent()
		{
			for (int i = cells.Length - 1; i >= 0; i--) {
				if (cells [i].content != null) {
					if (!Application.isEditor) {
						Object.Destroy (cells [i].content);
					} else {
						Object.DestroyImmediate (cells[i].content);
					}
					cells[i].content = null;
				}
			}
			if (areaContent != null) {
				if (!Application.isEditor) {
					Object.Destroy (areaContent);
				} else {
					Object.DestroyImmediate (areaContent);
				}
			}
			areaContent = null;
			onTileClear.Invoke ();
		}

		public Rect extents {
			get { 
				if (dirty) {
					_extents = new Rect ();
					for (int i = cellPoints.Length - 1; i >= 0; i--) {
						if (cellPoints [i].x < _extents.xMin) {
							_extents.xMin = cellPoints [i].x;
						} else if (cellPoints [i].x > _extents.xMax) {
							_extents.xMax = cellPoints [i].x;
						}

						if (cellPoints [i].y < _extents.yMin) {
							_extents.yMin = cellPoints [i].y;
						} else if (cellPoints [i].y > _extents.yMax) {
							_extents.yMax = cellPoints [i].y;
						}
					}
					dirty = false;
				}
				return _extents;
			}
		}

		//Should modify this to see if its within a cell.
		public bool ContainsPoint(Vector2 p, ref int id)
		{

			int i,j;
			for(i = cells.Length-1; i >=0; i--){
				Vector2[] pts = new Vector2[cells [i].points.Length];
				for (j = pts.Length - 1; j >= 0; j--) {
					pts [j] = cellPoints [cells [i].points [j]];
				}
				if (ExtensionMethods.PolyContainsPoint (pts, p)) {
					id = i;
					return true;
				}
			}
			id = -1;
			return false;
		}

		public bool WithinTolerance(TileCell a, TileCell b)
		{

			Vector3 diff = a.point - b.point;
			if (Mathf.Abs (diff.x) > cellPositionTolerance) {
				return false;
			}
			if (Mathf.Abs (diff.y) > cellPositionTolerance) {
				return false;
			}

			float da = a.point.z % 360, db = b.point.z % 360;

			if (da < 0) {
				da = 360 - (Mathf.Abs (da));
			} 
			if (db < 0) {
				db = 360 - (Mathf.Abs (db));
			}
			da = Mathf.Abs(da - db);
			if (da == 180) {
				if (a.scale.x == b.scale.x * -1 && a.scale.y == b.scale.y * -1) {
					return true;
				} else {
					return false;
				}
			} else if (da > cellRotationTolerance) {
				return false;
			}

			diff = a.scale - b.scale;
			if (Mathf.Abs (diff.x) > cellScaleTolerance) {
				return false;
			}
			if (Mathf.Abs (diff.y) > cellScaleTolerance) {
				return false;
			}
			if (Mathf.Abs (diff.z) > cellScaleTolerance) {
				return false;
			}

			return true;
		}

		public void SetCellPolyFromWallpaper(){
			Vector2[] pts;
			metaCell = new TileCell ();
			metaCell.scale = Vector3.one;
			metaCellPoly = new Polygon ("Cell Polygon");
			int i;
			switch (wallpaper) {
			case WallpaperGroup.p1:
			case WallpaperGroup.p2:
			case WallpaperGroup.p2gg:
			case WallpaperGroup.p2mg:
			case WallpaperGroup.p2mm:
			case WallpaperGroup.p4:
			case WallpaperGroup.pm:
			case WallpaperGroup.p4mg:
				//Square
				metaCell.neighbors = new int[4];
				metaCell.edges = new int[4];
				metaCell.points = new int[4];
				metaCell.edgeNeighbors = new int[4];
				for (i = 3; i >= 0; i--) {
					metaCell.neighbors[i] = metaCell.points[i] = metaCell.edges [i] = metaCell.edgeNeighbors [i] = -1;
				}
				pts = new Vector2[4];
				pts [0] = new Vector2 (-0.5f, -0.5f);
				pts [1] = new Vector2 (-0.5f, 0.5f);
				pts [2] = new Vector2 (0.5f, 0.5f);
				pts [3] = new Vector2 (0.5f, -0.5f);
				metaCellPoly.points = pts;
				break;
			case WallpaperGroup.pg:
				//Slightly offset square.
				metaCell.neighbors = new int[4];
				metaCell.edges = new int[6];
				metaCell.points = new int[6];
				metaCell.edgeNeighbors = new int[6];
				for (i = 5; i >= 0; i--) {
					metaCell.neighbors[i] = metaCell.points[i] = metaCell.edges [i] = metaCell.edgeNeighbors [i] = -1;
				}
				pts = new Vector2[6];
				pts [0] = new Vector2 (-0.5f, -0.5f);
				pts [1] = new Vector2 (-0.5f, 0.5f);
				pts [2] = new Vector2 (0, 0.5f);
				pts [3] = new Vector2 (0.5f, 0.5f);
				pts [4] = new Vector2 (0.5f, -0.5f);
				pts [5] = new Vector2 (0, -0.5f);
				metaCellPoly.points = pts;
				break;
			case WallpaperGroup.p3:
				//Rhombus
				metaCell.neighbors = new int[4];
				metaCell.edges = new int[4];
				metaCell.points = new int[4];
				metaCell.edgeNeighbors = new int[4];
				for (i = 3; i >= 0; i--) {
					metaCell.neighbors[i] = metaCell.points[i] = metaCell.edges [i] = metaCell.edgeNeighbors [i] = -1;
				}
				pts = new Vector2[4];
				pts [0] = new Vector2 (-0.5f, -0.28865f);
				pts [1] = new Vector2 (-0.16667f, 0.28865f);
				pts [2] = new Vector2 (0.5f, 0.28865f);
				pts [3] = new Vector2 (0.16667f, -0.28865f);
				metaCellPoly.points = pts;
				break;
			case WallpaperGroup.cm:
			case WallpaperGroup.p31m:
			case WallpaperGroup.p6:
				//equtriangles
				metaCell.neighbors = new int[3];
				metaCell.edges = new int[3];
				metaCell.points = new int[3];
				metaCell.edgeNeighbors = new int[3];
				for (i = 2; i >= 0; i--) {
					metaCell.neighbors[i] = metaCell.points[i] = metaCell.edges [i] = metaCell.edgeNeighbors [i] = -1;
				}
				pts = new Vector2[3];
				pts [0] = new Vector2 (-0.5f, -0.43301f);
				pts [1] = new Vector2 (0, 0.43301f);
				pts [2] = new Vector2 (0.5f, -0.43301f);
				metaCellPoly.points = pts;
				break;
			case WallpaperGroup.p3m1:
				//flat triangle
				metaCell.neighbors = new int[3];
				metaCell.edges = new int[3];
				metaCell.points = new int[3];
				metaCell.edgeNeighbors = new int[3];
				for (i = 2; i >= 0; i--) {
					metaCell.neighbors[i] = metaCell.points[i] = metaCell.edges [i] = metaCell.edgeNeighbors [i] = -1;
				}
				pts = new Vector2[3];
				pts [0] = new Vector2 (-0.5f, -0.14434f);
				pts [1] = new Vector2 (0, 0.14434f);
				pts [2] = new Vector2 (0.5f, -0.14434f);
				metaCellPoly.points = pts;
				break;
			case WallpaperGroup.p6mm:
				//Tall triangle
				metaCell.neighbors = new int[3];
				metaCell.edges = new int[3];
				metaCell.points = new int[3];
				metaCell.edgeNeighbors = new int[3];
				for (i = 2; i >= 0; i--) {
					metaCell.neighbors[i] = metaCell.points[i] = metaCell.edges [i] = metaCell.edgeNeighbors [i] = -1;
				}
				pts = new Vector2[3];
				pts [0] = new Vector2 (-0.28868f, -0.5f);
				pts [1] = new Vector2 (0.28868f, 0.5f);
				pts [2] = new Vector2 (0.28868f, -0.5f);
				metaCellPoly.points = pts;
				break;
			case WallpaperGroup.c2mm:
			case WallpaperGroup.p4mm:
				//right triangles
				metaCell.neighbors = new int[3];
				metaCell.edges = new int[3];
				metaCell.points = new int[3];
				metaCell.edgeNeighbors = new int[3];
				for (i = 2; i >= 0; i--) {
					metaCell.neighbors[i] = metaCell.points[i] = metaCell.edges [i] = metaCell.edgeNeighbors [i] = -1;
				}
				pts = new Vector2[3];
				pts [0] = new Vector2 (-0.5f, -0.5f);
				pts [1] = new Vector2 (0.5f, 0.5f);
				pts [2] = new Vector2 (0.5f, -0.5f);
				metaCellPoly.points = pts;
				break;
			}
		}


		public void RemoveCell(int id, bool clearContent = false){
			if (id >= 0 && id < cells.Length) {
                Debug.Log("Removing cell#" + id);
				TileCell cell = cells [id];
				//Go through each point in the cell and discover if it's used by any others.
				//If so, leave it alone.
				//If not, find any Edges that use it and remove them.
				List<int> pointsToRemove = new List<int>();
				for(int c = cell.points.Length-1; c >=0; c--){
					bool found = false;
					for (int oc = cells.Length - 1; oc >= 0; oc--) {
						if (oc != id) {
							for (int ocp = cells [oc].points.Length - 1; ocp >= 0; ocp--) {
								if (cells [oc].points [ocp] == cell.points [c]) {
									found = true;
									break;
								}
							}
						}
						if (found) {
							break;
						}
					}
					if (!found) {
						pointsToRemove.Add (cell.points [c]);
					}
				}
				List<int> edgesToRemove = new List<int> ();
				for (int e = cell.edges.Length - 1; e >= 0; e--) {
					//Go through the cells edges. If they're border edges, mark them for removal.
					if(edges[cell.edges[e]].isBorder){
						edgesToRemove.Add (cell.edges [e]);
					}
				}
			
				// In all of our other cells:
				for (int r = cells.Length - 1; r >= 0; r--) {
					if (r != id) {
						int tr, rm, current;
						//1. -if their neighbors or edgeNeighbors are ABOVE id, reduce them by 1
						for (tr = cells [r].neighbors.Length - 1; tr >= 0; tr--) {
							if (cells [r].neighbors [tr] > id) {
								cells [r].neighbors [tr]--;
							}
						}

						for (tr = cells [r].edgeNeighbors.Length - 1; tr >= 0; tr--) {
							if (cells [r].edgeNeighbors [tr] > id) {
								cells [r].edgeNeighbors [tr]--;
							}
						}

						//2. -for each of their points, go through all the points we're removing and, if they're greater than the current point, reduce it by 1.
						if (pointsToRemove.Count > 0) {
							for (tr = cells [r].points.Length - 1; tr >= 0; tr--) {
								current = cells [r].points [tr];
								for (rm = pointsToRemove.Count - 1; rm >= 0; rm--) {
									if (pointsToRemove [rm] > current) {
										cells [r].points [tr]--;
									}
								}
							}
						}

						//3. -for each of their edges, go through all the edges we're removing and, if they're greater than current, reduce by 1.
						if (edgesToRemove.Count > 0) {
							for (tr = cells [r].edges.Length - 1; tr >= 0; tr--) {
								current = cells [r].edges [tr];
								for (rm = edgesToRemove.Count - 1; rm >= 0; rm--) {
									if (edgesToRemove [rm] > current) {
										cells [r].edges [tr]--;
									}
								}
							}
						}
					}
				}

				//Now our edges!
				for (int e = edges.Length - 1; e >= 0; e--) {
					int currentA = edges [e].pointA, currentB = edges [e].pointB;
					for (int pr = pointsToRemove.Count - 1; pr >= 0; pr--) {
						if (pointsToRemove [pr] > currentA) {
							edges [e].pointA--;
						}
						if (pointsToRemove [pr] > currentB) {
							edges [e].pointB--;
						}
					}
				}

				//Lastly, get rid of our cell, points, and edges.
				List<TileCell> newCells = new List<TileCell>(cells);
				newCells.RemoveAt (id);
				cells = newCells.ToArray ();

				List<Vector2> newPoints = new List<Vector2> (cellPoints);
				for (int rp = cellPoints.Length - 1; rp >= 0; rp--) {
					bool drop = false;
					for (int dr = pointsToRemove.Count - 1; dr >= 0; dr--) {
						if (pointsToRemove [dr] == rp) {
							drop = true;
							break;
						}
					}
					if (drop) {
						newPoints.RemoveAt (rp);
					}
				}
				cellPoints = newPoints.ToArray ();

				List<TileEdge> newEdges = new List<TileEdge> (edges);
				for (int rp = edges.Length - 1; rp >= 0; rp--) {
					bool drop = false;
					for (int dr = edgesToRemove.Count - 1; dr >= 0; dr--) {
						if (edgesToRemove [dr] == rp) {
							drop = true;
							break;
						}
					}
					if (drop) {
						newEdges.RemoveAt (rp);
					}
				}
				edges = newEdges.ToArray ();

				if (clearContent) {
					if (cell.content != null) {
						if (!Application.isEditor) {
							Object.Destroy (cell.content);
						} else {
							Object.DestroyImmediate (cell.content);
						}

					}
				}
				cell = null;
			}
		}
	}

	[System.Serializable]
	public class TileCell {
		//X and Y are planar coords. Z is our angle of rotation.
		public Vector3 point;
		public Vector3 scale;
		public int[] neighbors;
		public int generation;
		public bool edgeCell;
		public int[] points, edges, edgeNeighbors;
		public GameObject content;

		public TileCell()
		{
			point = Vector3.zero;
			scale = Vector3.one;
			neighbors = new int[3];
			generation = 0;
			edgeCell = false;
			points = new int[3]; edges = new int[3];
			edgeNeighbors = new int[3];
			for (int i = 2; i >= 0; i--) {
				neighbors[i] = points [i] = edges [i] = edgeNeighbors [i] = -1;
			}


			content = null;
		}

		public TileCell(TileCell c)
		{
			point = c.point;
			scale = c.scale;
			neighbors = new int[c.neighbors.Length];
			for (int i = neighbors.Length - 1; i >= 0; i--) {
				neighbors [i] = -1;
			}
			generation = c.generation + 1;
			content = null;

			edges = new int[c.edges.Length];
			for (int i = edges.Length - 1; i >= 0; i--) {
				edges [i] = -1;
			}
			edgeNeighbors = new int[c.edgeNeighbors.Length];
			for (int i = edgeNeighbors.Length - 1; i >= 0; i--) {
				edgeNeighbors [i] = -1;
			}
			points = new int[c.points.Length];
			for (int i = points.Length - 1; i >= 0; i--) {
				points [i] = -1;
			}
		}


		public TileCell GetNeighbor( int i, int j, Transform tileTransform, TileGroup.WallpaperGroup wallpaper){

			tileTransform.position = Vector3.zero;
			tileTransform.eulerAngles = Vector3.zero;
			tileTransform.localScale = Vector3.one;

			TileCell currentTile = new TileCell (this);

			currentTile.scale = Vector3.one;
			Vector3 pos = point, scl = scale, rot = Vector3.zero;

			switch (wallpaper) {

			case TileGroup.WallpaperGroup.p1:
				switch (j) {
				case 0:
					pos.x += 1;
					currentTile.neighbors [1] = i;
					break;
				case 1:
					pos.x -= 1;
					currentTile.neighbors [0] = i;
					break;
				case 2:
					pos.y += 1;
					currentTile.neighbors [3] = i;
					break; 
				case 3:
					pos.y -= 1;
					currentTile.neighbors [2] = i;
					break;
				}
				currentTile.point.x = pos.x;
				currentTile.point.y = pos.y;
				break;

			case TileGroup.WallpaperGroup.p2:
				switch (j) {
				case 0:
					pos.x += 1;
					currentTile.neighbors [1] = i;
					break;
				case 1:
					pos.x -= 1;
					currentTile.neighbors [0] = i;
					break;
				case 2:
					pos.y += 1;
					pos.z += 180;
					currentTile.neighbors [3] = i;
					break; 
				case 3:
					pos.y -= 1;
					pos.z -= 180;
					currentTile.neighbors [2] = i;
					break;
				}
				currentTile.point = pos;
				break;

			case TileGroup.WallpaperGroup.pm:
				switch (j) {
				case 0:
					pos.y++;
					currentTile.neighbors [1] = i;
					break;
				case 1:
					pos.y--;
					currentTile.neighbors [0] = i;
					break;
				case 2:
					pos.x++;
					scl.x *= -1;
					currentTile.neighbors [3] = i;
					break; 
				case 3:
					pos.x--;
					scl.x *= -1;
					currentTile.neighbors [2] = i;
					break;
				}
				currentTile.point = pos;
				currentTile.scale = scl;
				break;

			case TileGroup.WallpaperGroup.pg:
				switch (j) {
				case 0:
					pos.x += 0.5f;
					pos.y++; 
					currentTile.neighbors [3] = i;
					break;
				case 1:
					pos.x -= 0.5f;
					pos.y++;
					currentTile.neighbors [2] = i;
					break;
				case 2:
					pos.x += 0.5f;
					pos.y--;
					currentTile.neighbors [1] = i;
					break; 
				case 3:
					pos.x -= 0.5f;
					pos.y--;
					currentTile.neighbors [0] = i;
					break;
				}
				scl.y *= -1;
				currentTile.point = pos;
				currentTile.scale = scl;
				break;

			case TileGroup.WallpaperGroup.cm:
				switch (j) {
				case 0:
					pos.x += 0.5f;
					currentTile.neighbors [1] = i;
					break;
				case 1:
					pos.x -= 0.5f;
					currentTile.neighbors [0] = i;
					break;
				case 2:
					if (scl.y >= 0) {
						pos.y -= 0.866f;
					} else {
						pos.y += 0.866f;
					}
					currentTile.neighbors [2] = i;
					break; 

				}
				scl.y *= -1;
				currentTile.point = pos;
				currentTile.scale = scl;
				break;

			case TileGroup.WallpaperGroup.p2mm:
				switch (j) {
				case 0:
					pos.y++;
					scl.y *= -1;
					currentTile.neighbors [1] = i;
					break;
				case 1:
					pos.y--;
					scl.y *= -1;
					currentTile.neighbors [0] = i;
					break;
				case 2:
					pos.x++;
					scl.x *= -1;
					currentTile.neighbors [3] = i;
					break; 
				case 3:
					pos.x--;
					scl.x *= -1;
					currentTile.neighbors [2] = i;
					break;
				}
				currentTile.point = pos;
				currentTile.scale = scl;
				break;

			case TileGroup.WallpaperGroup.p2mg:
				switch (j) {
				case 0:
					pos.y++;
					pos.z += 180;
					currentTile.neighbors [1] = i;
					break;
				case 1:
					pos.y--;
					pos.z -= 180;
					currentTile.neighbors [0] = i;
					break;
				case 2:
					pos.x++;
					scl.x *= -1;
					currentTile.neighbors [3] = i;
					break; 
				case 3:
					pos.x--;
					scl.x *= -1;
					currentTile.neighbors [2] = i;
					break;
				}
				currentTile.point = pos;
				currentTile.scale = scl;
				break;

			case TileGroup.WallpaperGroup.p2gg:
				switch (j) {
				case 0:
					pos.y++;
					pos.z += 180;
					currentTile.neighbors [1] = i;
					break;
				case 1:
					pos.y--;
					pos.z -= 180;
					currentTile.neighbors [0] = i;
					break;
				case 2:
					pos.x++;
					scl.y *= -1;
					currentTile.neighbors [3] = i;
					break; 
				case 3:
					pos.x--;
					scl.y *= -1;
					currentTile.neighbors [2] = i;
					break;
				}
				currentTile.point = pos;
				currentTile.scale = scl;
				break;

			case TileGroup.WallpaperGroup.c2mm:
				switch (j) {
				case 0:
					if (scl.y >= 0) {
						pos.x++;
					} else {
						pos.x--;
					}
					scl.x *= -1;
					currentTile.neighbors [0] = i;
					break;
				case 1:
					if (scl.x >= 0) {
						pos.y--;
					} else {
						pos.y++;
					}
					scl.y *= -1;
					currentTile.neighbors [1] = i;
					break;
				case 2:
					if (pos.z >= 0) {
						pos.z += 180;
					} else {
						pos.z -= 180;
					}
					currentTile.neighbors [2] = i;
					break; 

				}
				currentTile.point = pos;
				currentTile.scale = scl;
				break;

			case TileGroup.WallpaperGroup.p4:
				tileTransform.position = new Vector3 (pos.x, pos.y, 0);
				tileTransform.eulerAngles = new Vector3 (0, 0, pos.z);
				switch (j) {
				case 0:
					tileTransform.RotateAround (tileTransform.TransformPoint(new Vector3(0.5f, 0.5f, 0)), Vector3.forward, 90);
					pos.x = Mathf.RoundToInt(tileTransform.position.x);
					pos.y = Mathf.RoundToInt(tileTransform.position.y);
					pos.z = Mathf.RoundToInt(tileTransform.eulerAngles.z);
					break;
				case 1:
					tileTransform.RotateAround (tileTransform.TransformPoint(new Vector3(0.5f, 0.5f, 0)), Vector3.forward, 180);
					pos.x = Mathf.RoundToInt(tileTransform.position.x);
					pos.y = Mathf.RoundToInt(tileTransform.position.y);
					pos.z = Mathf.RoundToInt(tileTransform.eulerAngles.z);
					currentTile.neighbors [1] = i;
					break;
				case 2:
					tileTransform.RotateAround (tileTransform.TransformPoint(new Vector3(0.5f, 0.5f, 0)), Vector3.forward, 270);
					pos.x = Mathf.RoundToInt(tileTransform.position.x);
					pos.y = Mathf.RoundToInt(tileTransform.position.y);
					pos.z = Mathf.RoundToInt(tileTransform.eulerAngles.z);
					break; 
				case 3:
					tileTransform.RotateAround (tileTransform.TransformPoint(new Vector3(0.5f, -0.5f, 0)), Vector3.forward, 180);
					pos.x = Mathf.RoundToInt(tileTransform.position.x);
					pos.y = Mathf.RoundToInt(tileTransform.position.y);
					pos.z = Mathf.RoundToInt(tileTransform.eulerAngles.z);
					currentTile.neighbors [3] = i;
					break;
				}
				currentTile.point = pos;
				break;

			case TileGroup.WallpaperGroup.p4mm:
				tileTransform.position = new Vector3 (pos.x, pos.y, 0);
				tileTransform.eulerAngles = new Vector3 (0, 0, pos.z);
				switch (j) {
				case 0:
					scl.x *= -1;
					scl.y *= -1;
					currentTile.neighbors [0] = i;
					break;
				case 1:
					if (scl.y >= 0) {
						tileTransform.RotateAround (tileTransform.TransformPoint (new Vector3 (0.5f, -0.5f, 0)), Vector3.forward, 180);
					} else {
						tileTransform.RotateAround (tileTransform.TransformPoint (new Vector3 (-0.5f, 0.5f, 0)), Vector3.forward, 180);
					}
					pos.x = tileTransform.position.x;
					pos.y = tileTransform.position.y;
					pos.z = Mathf.RoundToInt (tileTransform.eulerAngles.z);
					currentTile.neighbors [1] = i;
					break;
				case 2:

					tileTransform.RotateAround (tileTransform.TransformPoint (new Vector3 (-0.5f, -0.5f, 0)), Vector3.forward, -90);
					pos.x = tileTransform.position.x;
					pos.y = tileTransform.position.y;
					pos.z = Mathf.RoundToInt (tileTransform.eulerAngles.z);
					break; 

				}
				currentTile.point = pos;
				currentTile.scale = scl;
				break;

			case TileGroup.WallpaperGroup.p4mg:
				tileTransform.position = new Vector3(pos.x, pos.y, 0);
				tileTransform.eulerAngles = new Vector3 (0, 0, pos.z);
				switch (j) {
				case 0:

					if (scl.x >= 0) {
						tileTransform.Translate (Vector3.right, Space.Self);
					} else {
						tileTransform.Translate (Vector3.left, Space.Self);
					}
					scl.x *= -1;
					pos.x = Mathf.RoundToInt (tileTransform.position.x);
					pos.y = Mathf.RoundToInt (tileTransform.position.y);
					currentTile.neighbors [0] = i;
					break;
				case 1:

					if (scl.y >= 0) {
						tileTransform.Translate (Vector3.up, Space.Self);
					} else {
						tileTransform.Translate (Vector3.down, Space.Self);
					}
					scl.y *= -1;
					pos.x = Mathf.RoundToInt (tileTransform.position.x);
					pos.y = Mathf.RoundToInt (tileTransform.position.y);
					currentTile.neighbors [1] = i;
					break;
				case 2:

					if (scl.x >= 0) {
						rot.x = -0.5f;
					} else {
						rot.x = 0.5f;
					}

					if (scl.y >= 0) {
						rot.y = -0.5f;
					} else {
						rot.y = 0.5f;
					}

					tileTransform.RotateAround (tileTransform.TransformPoint (rot), Vector3.forward, -90);
					pos.x = Mathf.RoundToInt (tileTransform.position.x);
					pos.y = Mathf.RoundToInt (tileTransform.position.y);
					pos.z = Mathf.RoundToInt (tileTransform.eulerAngles.z);
					currentTile.neighbors [3] = i;
					break; 
				case 3:

					if (scl.x >= 0) {
						rot.x = -0.5f;
					} else {
						rot.x = 0.5f;
					}

					if (scl.y >= 0) {
						rot.y = -0.5f;
					} else {
						rot.y = 0.5f;
					}

					tileTransform.RotateAround (tileTransform.TransformPoint (rot), Vector3.forward, 90);
					pos.x = Mathf.RoundToInt (tileTransform.position.x);
					pos.y = Mathf.RoundToInt (tileTransform.position.y);
					pos.z = Mathf.RoundToInt (tileTransform.eulerAngles.z);
					currentTile.neighbors [2] = i;
					break; 

				}
				currentTile.point = pos;
				currentTile.scale = scl;
				break;

			case TileGroup.WallpaperGroup.p3:
				tileTransform.position = new Vector3 (pos.x, pos.y, 0);
				tileTransform.eulerAngles = new Vector3 (0, 0, pos.z);
				switch (j) {
				case 0:
					tileTransform.RotateAround (tileTransform.TransformPoint (new Vector3(-0.5f, -0.2886f)), Vector3.forward, 120);
					pos.x = tileTransform.position.x;
					pos.y = tileTransform.position.y;
					pos.z = Mathf.RoundToInt (tileTransform.eulerAngles.z);
					break;
				case 1:

					tileTransform.RotateAround (tileTransform.TransformPoint (new Vector3(-0.16667f, 0.2886f)), Vector3.forward, 120);
					pos.x = tileTransform.position.x;
					pos.y = tileTransform.position.y;
					pos.z = Mathf.RoundToInt (tileTransform.eulerAngles.z);
					break;
				case 2:

					tileTransform.RotateAround (tileTransform.TransformPoint (new Vector3(0.16667f, -0.2886f)), Vector3.forward, 120);
					pos.x = tileTransform.position.x;
					pos.y = tileTransform.position.y;
					pos.z = Mathf.RoundToInt (tileTransform.eulerAngles.z);
					break; 
				case 3:

					tileTransform.RotateAround (tileTransform.TransformPoint (new Vector3(0.5f, 0.2886f)), Vector3.forward, 120);
					pos.x = tileTransform.position.x;
					pos.y = tileTransform.position.y;
					pos.z = Mathf.RoundToInt (tileTransform.eulerAngles.z);
					break; 
				}
				currentTile.point = pos;
				break;

			case TileGroup.WallpaperGroup.p3m1:
				tileTransform.position = new Vector3 (pos.x, pos.y, 0);
				tileTransform.eulerAngles = new Vector3 (0, 0, pos.z);

				switch (j) {
				case 0:
					if (scl.y >= 0) {
						tileTransform.Translate (Vector3.down*0.28868f, Space.Self);
					} else {
						tileTransform.Translate (Vector3.up*0.28868f, Space.Self);
					}
					pos.x = tileTransform.position.x;
					pos.y = tileTransform.position.y;
					scl.y *= -1;
					currentTile.neighbors [0] = i;
					break;
				case 1:

					if (scl.y >= 0) {
						tileTransform.RotateAround (tileTransform.TransformPoint (new Vector3 (0, 0.14434f, 0)), Vector3.forward, 120);
					} else {
						tileTransform.RotateAround (tileTransform.TransformPoint (new Vector3 (0, -0.14434f, 0)), Vector3.forward, 120);
					}
					pos.x = tileTransform.position.x;
					pos.y = tileTransform.position.y;
					pos.z = Mathf.RoundToInt (tileTransform.eulerAngles.z);
					break;
				case 2:
					if (scl.y >= 0) {
						tileTransform.RotateAround (tileTransform.TransformPoint (new Vector3 (0, 0.14434f, 0)), Vector3.forward, -120);
					} else {
						tileTransform.RotateAround (tileTransform.TransformPoint (new Vector3 (0, -0.14434f, 0)), Vector3.forward, -120);
					}
					pos.x = tileTransform.position.x;
					pos.y = tileTransform.position.y;
					pos.z = Mathf.RoundToInt (tileTransform.eulerAngles.z);
					break; 

				}
				currentTile.point = pos;
				currentTile.scale = scl;
				break;

			case TileGroup.WallpaperGroup.p31m:
				tileTransform.position = new Vector3 (pos.x, pos.y, 0);
				tileTransform.eulerAngles = new Vector3 (0, 0, pos.z);

				switch (j) {
				case 0:
					if (scl.y >= 0) {
						tileTransform.Translate (Vector3.down * 0.86602f, Space.Self);
					} else {
						tileTransform.Translate (Vector3.up * 0.86602f, Space.Self);
					}
					pos.x = tileTransform.position.x;
					pos.y = tileTransform.position.y;
					scl.y *= -1;
					currentTile.neighbors [0] = i;
					break;
				case 1:

					tileTransform.Translate (new Vector3 (-0.5f, 0, 0), Space.Self);
					scl.y *= -1;
					if (scl.y >= 0) {
						tileTransform.RotateAround (tileTransform.TransformPoint(new Vector3 (0, -0.14434f, 0)), Vector3.forward, -120);
					} else {
						tileTransform.RotateAround (tileTransform.TransformPoint(new Vector3 (0, 0.14434f, 0)), Vector3.forward, 120);
					}

					pos.x = tileTransform.position.x;
					pos.y = tileTransform.position.y;
					pos.z = tileTransform.eulerAngles.z;
					break;
				case 2:

					tileTransform.Translate (new Vector3 (0.5f, 0, 0), Space.Self);
					scl.y *= -1;

					if (scl.y >= 0) {
						tileTransform.RotateAround (tileTransform.TransformPoint (new Vector3 (0, -0.14434f, 0)), Vector3.forward, 120);
					} else {
						tileTransform.RotateAround (tileTransform.TransformPoint (new Vector3 (0, 0.14434f, 0)), Vector3.forward, -120);
					}

					pos.x = tileTransform.position.x;
					pos.y = tileTransform.position.y;
					pos.z = tileTransform.eulerAngles.z;
					break; 

				}
				currentTile.point = pos;
				currentTile.scale = scl;
				break;

			case TileGroup.WallpaperGroup.p6:
				tileTransform.position = new Vector3 (pos.x, pos.y, 0);
				tileTransform.eulerAngles = new Vector3 (0, 0, pos.z);

				switch (j) {
				case 0:
					tileTransform.RotateAround (tileTransform.TransformPoint (new Vector3 (0, -0.43301f, 0)), Vector3.forward, 180);
					pos.x = tileTransform.position.x;
					pos.y = tileTransform.position.y;
					pos.z = Mathf.RoundToInt (tileTransform.eulerAngles.z);
					currentTile.neighbors [0] = i;
					break;
				case 1:
					tileTransform.RotateAround (tileTransform.TransformPoint(new Vector3 (0, 0.43301f, 0)), Vector3.forward, 60);
					pos.x = tileTransform.position.x;
					pos.y = tileTransform.position.y;
					pos.z = Mathf.RoundToInt (tileTransform.eulerAngles.z);
					currentTile.neighbors [2] = i;
					break;
				case 2:
					tileTransform.RotateAround (tileTransform.TransformPoint(new Vector3 (0, 0.43301f, 0)), Vector3.forward, -60);
					pos.x = tileTransform.position.x;
					pos.y = tileTransform.position.y;
					pos.z = Mathf.RoundToInt (tileTransform.eulerAngles.z);
					currentTile.neighbors [1] = i;
					break; 

				}
				currentTile.point = pos;
				break;

			case TileGroup.WallpaperGroup.p6mm:
				tileTransform.position = new Vector3 (pos.x, pos.y, 0);
				tileTransform.eulerAngles = new Vector3 (0, 0, pos.z);
				switch (j) {
				case 0:
					rot = Vector3.zero;
					if (scl.x >= 0) {
						rot.x = -60;
					} else {
						rot.x = 60;

					}
					if (scl.y < 0) {
						rot.x *= -1;
					}
					scl.x *= -1;
					pos.z += rot.x;
					break;
				case 1:
					if (scl.y >= 0) {
						tileTransform.Translate (Vector3.down, Space.Self); 
					} else {
						tileTransform.Translate (Vector3.up, Space.Self);
					}
					scl.y *= -1;
					pos.x = tileTransform.position.x;
					pos.y = tileTransform.position.y;
					currentTile.neighbors [1] = i;
					break;
				case 2:
					if (scl.x >= 0) {
						tileTransform.Translate (Vector3.right * 0.57735f, Space.Self); 
					} else {
						tileTransform.Translate (Vector3.left * 0.57735f, Space.Self);
					}
					scl.x *= -1;
					pos.x = tileTransform.position.x;
					pos.y = tileTransform.position.y;
					currentTile.neighbors [2] = i;
					break; 

				}
				currentTile.point = pos;
				currentTile.scale = scl;
				break;

			case TileGroup.WallpaperGroup.Penrose:
				//WOW, so this will have to wait for a bit...
				break;
			}

			return currentTile;
		}


	}

	[System.Serializable]
	public struct TileEdge{
		public int pointA, pointB;
		public bool isBorder;
		public TileEdge(int a,int b){
			pointA = a; pointB = b; isBorder = false;
		}

		public bool Compare(TileEdge other){
			if ((pointA == other.pointA && pointB == other.pointB) || (pointA == other.pointB && pointB == other.pointA)) {
				return true;
			}
			return false;
		}
	}

}