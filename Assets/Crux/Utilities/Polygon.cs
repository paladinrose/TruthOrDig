using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Crux{
	
	[System.Serializable]
	public class Polygon {

		[SerializeField]
		Rect _extents;

		[SerializeField]
		Vector2[] _points;

		bool dirty;
		public string name;
		 public Vector2[] points {
			get { 
				Vector2[] nP = new Vector2[_points.Length];
				for (int i = nP.Length - 1; i >= 0; i--) {
					nP [i] = _points [i];
				}
				return nP;
			}
			set {
				if (value.Length >= 3) {
					dirty = true;
					_points = value;
				}
			}
		}

		#if UNITY_EDITOR
		public bool show, showPoints;
		public Rect drawBox;
		public Color32 color;
		public float totalLineHeight;
		#endif

		public Polygon(string n){
			Vector2[] pp  = new Vector2[3];
			pp [0] = Vector2.zero;
			pp [1] = new Vector2 (1, 2);
			pp [2] = new Vector2 (2, 0);
			_points = points =pp;
			dirty = true;
			_extents = new Rect();
			name = n;
			#if UNITY_EDITOR
			show = false;
			showPoints = false;
			drawBox = new Rect();
			color = Color.white;
			totalLineHeight = 0;
			#endif
		}

		public Polygon(Vector2[] pts){
			if (pts.Length > 2) {
				Vector2[] plts = new Vector2[pts.Length];
				for (int i = plts.Length - 1; i >= 0; i--) {
					plts [i] = pts [i];
				}
				_points = points = plts;

			} else {
				_points = new Vector2[3];
			}
			dirty = true;
			_extents = new Rect();
			name = "New Polygon";
			#if UNITY_EDITOR
			show = false;
			showPoints = false;
			drawBox = new Rect();
			color = Color.white;
			totalLineHeight = 0;
			#endif
		}
		public Polygon(Polygon p){
			Vector2[] pp = p.points, np ;
			if (pp.Length >= 3) {
				np = new Vector2[pp.Length];
				for (int i = pp.Length - 1; i >= 0; i--) {
					np [i] = pp [i];
				}
			} else {
				np = new Vector2[3];
				np [1] = new Vector2 (1, 1.72f);
				np [2] = new Vector2 (2, 0);
			}
			name = "New " + p.name;
			_points = points = np;
			dirty = true;
			_extents = new Rect();
			#if UNITY_EDITOR
			show = false;
			showPoints = false;
			drawBox = new Rect();
			color = Color.white;
			totalLineHeight = 0;
			#endif

		}
		public enum ClippingMethod{BFromA, AFromB, Intersecting, NotIntersecting};
		public static Polygon Clip(Polygon a, Polygon b, ClippingMethod clippingMethod){
			Polygon clipped = new Polygon ("Clipped Polygon");

			return clipped;
		}

		public bool ContainsPoint ( Vector2 p) { 
			int j = _points.Length-1; 
			bool inside = false; 
			//Go through each dimension of our source vector and ignore any destination points that have fewer dimensions.

			for (int i = 0; i < _points.Length; j = i++) { 
			Vector2 pi = _points [i];
			Vector2 pj = _points [j];
				
				if (((pi.y <= p.y && p.y < pj.y) || (pj.y <= p.y && p.y < pi.y)) &&
				   (p.x < (pj.x - pi.x) * (p.y - pi.y) / (pj.y - pi.y) + pi.x)) {
					inside = !inside;
				}
			}

			return inside; 
		}

		public Rect extents
		{
			get {
				if (dirty) {
					dirty = false;
					if (_points.Length < 3) {
						_points  = new Vector2[3];
						_points [0] = Vector2.zero;
						_points [1] = new Vector2 (1, 2);
						_points [2] = new Vector2 (2, 0);
					}
					_extents = new Rect ((Vector2)_points[0],Vector2.zero);

					for (int i = _points.Length - 1; i >= 0; i--) {
						Vector2 pi = _points [i];
						if (pi.x < extents.xMin) {
							_extents.xMin = pi.x;
						}
						if (pi.y < extents.yMin) {
							_extents.yMin = pi.y;
						}
						if (pi.x > extents.xMax) {
							_extents.xMax = pi.x;
						}
						if (pi.y > extents.yMax) {
							_extents.yMax = pi.y;
						}
					}
				}
				return _extents;
			}
		}

		public int GetClosestPoint(Vector2 p)
		{
			float lowestDist = float.PositiveInfinity;
			int id = -1;
			for(int i = _points.Length-1; i >=0; i--)
			{
				
				float d = Vector2.Distance (p, (Vector2)_points [i]);
				if (d< lowestDist) {
					id = i;
					lowestDist = d;
				}
			}
			return id;
		}

		public Vector2 GetClosestPointOnPolygon(Vector2 p, int samples, ref int cls)
		{
			int closestPolyPoint = GetClosestPoint (p), left = -1, right = -1;
			Vector2 ret = Vector2.one * -1, a, b;
			int l = _points.Length - 1;
			float dist;

			if (closestPolyPoint == 0) {
				left = l;
				right = closestPolyPoint + 1;
			} else if (closestPolyPoint == l) {
				right = 0;
				left = closestPolyPoint - 1;
			} else {
				right = closestPolyPoint + 1;
				left = closestPolyPoint - 1;
			}
			a = _points [closestPolyPoint];

			dist = Vector2.Distance (p, _points [left]);
			if (Vector2.Distance (p, _points [right]) > dist) {
				b = a;
				a = _points [left];
				cls = left;
			} else {
				b = _points [right];
				cls = closestPolyPoint;
			}

			for (int s = samples; s > 0; s--) {
				ret = Vector2.Lerp (a, b, 0.5f);
				dist = Vector2.Distance (a, p);
				if (Vector2.Distance (b, p) < dist) {
					a = ret;
				} else {
					b = ret;
				}
			}

			dist = Vector2.Distance (a, p);
			if (dist < Vector2.Distance (b, p)) {
				ret = a;
			} else {
				ret = b;
			}
				
			return ret;
		}

		public void ReducePoints(float angleTolerance, float minimumDeviation){
			Vector2[] pts = points;
			//Get the angle between our first two points.
			//Start going through from that point. Check the angle of the current against the next in line.
			//As long as it's the same, we don't record it. Once it's different, we record the last one, then start checking against this NEW angle.
			//We don't include our first one, since we check it with the last for a run: If the angle between the last and the first is the same as between the first and second, we're done, because the
			//last point we recorded started this run, and the first point we recorded ends it.
			float angle, currentAngle, angleDiff;
			Vector2 d;
			List<Vector2> newPoints = new List<Vector2> ();
			d = pts [1] - pts [0];
			angle = Mathf.Atan2 (d.y, d.x) * Mathf.Rad2Deg;

			for (int i = 1; i < pts.Length; i++) {
				if (i < pts.Length - 1) {
					d = pts [i + 1] - pts [i];
					currentAngle = Mathf.Atan2 (d.y, d.x) * Mathf.Rad2Deg;
					angleDiff = angle - currentAngle;
					if (Mathf.Abs (angleDiff) > angleTolerance) {
						//Check the next point. If it's back on angle, and the distance from our last
						//to our next is less than minimumOffsetSize, skip straight on by this one.
						if (i < pts.Length - 2) {
							d = pts [i + 2] - pts [i - 1];
							angleDiff = angle - (Mathf.Atan2 (d.y, d.x) * Mathf.Rad2Deg);
							if (Mathf.Abs(angleDiff) < angleTolerance) {
								if (Vector2.Distance (pts [i + 2], pts [i-1]) >= minimumDeviation) {
									newPoints.Add (pts [i]);
									angle = currentAngle;
								} else {
									i++;
								}
							} else {
								newPoints.Add (pts [i]);
								angle = currentAngle;
							}

						} else {
							//Check against 0
							d= pts[0] - pts[i];
							angleDiff = angle - (Mathf.Atan2 (d.y, d.x) * Mathf.Rad2Deg);
							if (Mathf.Abs(angleDiff) < angleTolerance) {
								if (Vector2.Distance (pts [0], pts [i-1]) >= minimumDeviation) {
									newPoints.Add (pts [i]);
									angle = currentAngle;
								} else {
									i++;
									d = pts [1] - pts [0];
									angleDiff = currentAngle - (Mathf.Atan2 (d.y, d.x) * Mathf.Rad2Deg);
									if (Mathf.Abs(angleDiff) > angleTolerance) {
										newPoints.Add (pts [0]);
									}
								}
							}else {
								newPoints.Add (pts [i]);
								angle = currentAngle;
							}
						}
					}
				} else {
					d = pts [0] - pts [i];
					currentAngle = Mathf.Atan2 (d.y, d.x) * Mathf.Rad2Deg;
					angleDiff = angle - currentAngle;
					if (Mathf.Abs(angleDiff) >= angleTolerance) {
						newPoints.Add (pts [i]);
					}
					d = pts [1] - pts [0];
					angleDiff = currentAngle - (Mathf.Atan2 (d.y, d.x) * Mathf.Rad2Deg);
					if (Mathf.Abs(angleDiff) > angleTolerance) {
						newPoints.Add (pts [0]);
					}
				}
			}
			points = newPoints.ToArray ();
		}

        
        public int[] triangles;
        /*
        public void Triangulate()
        {
            List<int> finalTris = new List<int>();
            bool[] removed = new bool[points.Length];
            int counter = points.Length;
            while(counter > 2)
            {
               
                
                float closestAngle = 120;
                int a, b, c, sA, sB, sC;
                Vector2[] abc = new Vector2[3];
                a = b = c = sA = sB = sC = -1;
                 for(int i = 0; i < points.Length; i++)
                {
                    if (!removed[i])
                    {
                        b = i;
                        for(int j = 1; j < points.Length; j++)
                        {
                            int jj = i - j;
                            if(jj < 0) { jj += points.Length; }
                            if (!removed[jj])
                            {
                                a = jj;
                                break;
                            }
                        }
                        for (int j = 1; j < points.Length; j++)
                        {
                            int jj = i + j;
                            if (jj >=points.Length) { jj -= points.Length; }
                            if (!removed[jj])
                            {
                                c = jj;
                                break;
                            }
                        }
                    }
                    if(a >=0 && b >=0 && c >= 0)
                    {
                        abc[0] = points[a];
                        abc[1] = points[b];
                        abc[2] = points[c];
                        Vector2 center = abc[0] + abc[1] + abc[2];
                        center /= 3;
                        if (ContainsPoint(center))
                        {
                            float distAB = Vector2.Distance(points[a], points[b]), distBC = Vector2.Distance(points[b], points[c]),
                                distAC = Vector2.Distance(points[a], points[c]);
                            float angle = Mathf.Acos(((distAB * distAB) + (distBC * distBC) - (distAC * distAC)) /
                                (2 * distAB * distBC));

                            angle *= Mathf.Rad2Deg;
                            angle = Mathf.Abs(angle - 60);
                            if (angle < closestAngle)
                            {
                                bool validTri = true;
                                for (int j = points.Length - 1; j >= 0; j--)
                                {
                                    if (j != a && j != b && j != c && abc.PolyContainsPoint(points[j]))
                                    {
                                        validTri = false;
                                        break;

                                    }
                                }
                                if (validTri)
                                {
                                    closestAngle = angle;
                                    sA = a; sB = b; sC = c;
                                }
                            }
                        }
                    }
                }
                counter = 0;
                if (sA >= 0)
                {
                    finalTris.Add(sA); finalTris.Add(sB); finalTris.Add(sC);
                    removed[sB] = true;
                    for (int i = points.Length - 1; i >= 0; i--)
                    {
                        if (!removed[i]) { counter++; }
                    }
                }
               
                
            }
            triangles = finalTris.ToArray();   
        }
        */

        //Add more functions here.
        
        private List<Vector2> m_points = new List<Vector2>();
        
        public void Triangulate()
        {
            m_points = new List<Vector2>(points);
            List<int> indices = new List<int>();

            int n = m_points.Count;
            if (n < 3)
            {
                triangles = indices.ToArray();
                return;
            }

            int[] V = new int[n];
            if (Area() > 0)
            {
                for (int v = 0; v < n; v++)
                    V[v] = v;
            }
            else
            {
                for (int v = 0; v < n; v++)
                    V[v] = (n - 1) - v;
            }

            int nv = n;
            int count = 2 * nv;
            for (int m = 0, v = nv - 1; nv > 2;)
            {
                if ((count--) <= 0)
                { triangles = indices.ToArray(); return; }

                int u = v;
                if (nv <= u)
                    u = 0;
                v = u + 1;
                if (nv <= v)
                    v = 0;
                int w = v + 1;
                if (nv <= w)
                    w = 0;

                if (Snip(u, v, w, nv, V))
                {
                    int a, b, c, s, t;
                    a = V[u];
                    b = V[v];
                    c = V[w];
                    indices.Add(a);
                    indices.Add(b);
                    indices.Add(c);
                    m++;
                    for (s = v, t = v + 1; t < nv; s++, t++)
                        V[s] = V[t];
                    nv--;
                    count = 2 * nv;
                }
            }

            indices.Reverse();
            triangles= indices.ToArray();
        }

        private float Area()
        {
            int n = m_points.Count;
            float A = 0.0f;
            for (int p = n - 1, q = 0; q < n; p = q++)
            {
                Vector2 pval = m_points[p];
                Vector2 qval = m_points[q];
                A += pval.x * qval.y - qval.x * pval.y;
            }
            return (A * 0.5f);
        }

        private bool Snip(int u, int v, int w, int n, int[] V)
        {
            int p;
            Vector2 A = m_points[V[u]];
            Vector2 B = m_points[V[v]];
            Vector2 C = m_points[V[w]];
            if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
                return false;
            for (p = 0; p < n; p++)
            {
                if ((p == u) || (p == v) || (p == w))
                    continue;
                Vector2 P = m_points[V[p]];
                if (InsideTriangle(A, B, C, P))
                    return false;
            }
            return true;
        }

        private bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
        {
            float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
            float cCROSSap, bCROSScp, aCROSSbp;

            ax = C.x - B.x; ay = C.y - B.y;
            bx = A.x - C.x; by = A.y - C.y;
            cx = B.x - A.x; cy = B.y - A.y;
            apx = P.x - A.x; apy = P.y - A.y;
            bpx = P.x - B.x; bpy = P.y - B.y;
            cpx = P.x - C.x; cpy = P.y - C.y;

            aCROSSbp = ax * bpy - ay * bpx;
            cCROSSap = cx * apy - cy * apx;
            bCROSScp = bx * cpy - by * cpx;

            return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
        }
        
    }
}

