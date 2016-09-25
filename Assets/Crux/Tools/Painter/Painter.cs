using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;

namespace Crux{
	[System.Serializable]
	public class Painter {
		//Paints assets, with several different Brush aspects that effect rotation, scale, etc, of the "paint" we're using.
		//Allows for FreePaint which just lets us paint using our settings on a given plane, in a given space.
		//Plane: XY, XZ, or YZ
		//Space: World, Screen
		[HideInInspector]
		public PalletItem[] pallet = new PalletItem[0];

		public PalletSet[] palletSets;
		int currentPalletSetID = 0, currentPalletSet = 0;
		public float rate =0.75f, minimumDistance = 0.1f;
		public enum PalletMode{
			Sequential,
			Random,
			SequentialFromSet,
			RandomFromSet
		}
		public PalletMode palletSelectionMode = PalletMode.Sequential;

		System.Random random;
		public Vector3 spread, angularDrift, scaleDrift;
		public enum CanvasPlane
		{
			xy, xz, yz
		}
		public CanvasPlane canvasPlane;

		public enum CanvasSpace
		{
			world,
			screen
		}
		public CanvasSpace canvasSpace;

		public PaintSample Paint(Vector2 p)
		{
			if (random == null) {
				random = new System.Random(System.DateTime.Now.DayOfYear*System.DateTime.Now.Millisecond);
			}

			Vector3 cP = Vector3.zero;
			switch (canvasPlane) {
			case CanvasPlane.xy:
				cP.x = p.x ;
				cP.y = p.y;
				break;
			case CanvasPlane.xz:
				cP.x = p.x;
				cP.z = p.y;
				break;
			case CanvasPlane.yz:
				cP.y = p.x;
				cP.z = p.y;
				break;
			}
			return Paint (cP);
		}
		public PaintSample Paint(Vector3 p)
		{
			if (random == null) {
				random = new System.Random(System.DateTime.Now.DayOfYear*System.DateTime.Now.Millisecond);
			}
			PalletItem dab = null; 
			PaintSample paint = null;
			switch (palletSelectionMode) {
			case PalletMode.Sequential:
				if(currentPalletSetID < pallet.Length){
					dab = pallet[currentPalletSetID]; 
					currentPalletSetID++;
				} else {
					dab = pallet[0]; currentPalletSetID = 1;
				}
				break;
			case PalletMode.Random:
				int r = random.Next(0, pallet.Length);
				dab = pallet[r];
				break;
			case PalletMode.SequentialFromSet:
				if(currentPalletSetID < palletSets[currentPalletSet].ids.Length) {
					dab = pallet[palletSets[currentPalletSet].ids[currentPalletSetID]];
					currentPalletSetID++;
				}else {
					dab = pallet[palletSets[currentPalletSet].ids[0]]; currentPalletSetID = 1;
				}
				break;
			case PalletMode.RandomFromSet:
				int rfp = random.Next(0, palletSets[currentPalletSet].ids.Length);
				dab = pallet[palletSets[currentPalletSet].ids[rfp]];
				break;
			}
			if (dab != null) {
				
				if (canvasSpace == CanvasSpace.screen) {
					if(!Application.isEditor){
						p = Camera.main.ScreenToWorldPoint(p);
					} else {
						#if UNITY_EDITOR
						Camera c = UnityEditor.SceneView.lastActiveSceneView.camera;
						if(c){p = c.ScreenToWorldPoint(p);}
						else {p = Camera.main.ScreenToWorldPoint(p);}
						#endif
					}
				}
				paint = dab.Paint (p);
			}
				
			return paint;	                              
		}

		public void ApplyPaintProperties(Transform g){
			Vector3 t;
			t = g.position;
			t.x +=  (spread.x-(spread.x * (float)random.NextDouble ()*2));
			t.y +=  (spread.y-(spread.y * (float)random.NextDouble ()*2));
			t.z +=  (spread.z-(spread.z * (float)random.NextDouble ()*2));
			g.position = t;

			t = g.eulerAngles;
			t.x +=  (angularDrift.x-(angularDrift.x * (float)random.NextDouble ()*2));
			t.y +=  (angularDrift.y-(angularDrift.y * (float)random.NextDouble ()*2));
			t.z +=  (angularDrift.z-(angularDrift.z * (float)random.NextDouble ()*2));
			g.eulerAngles = t;

			t = g.localScale;
			t.x +=  (scaleDrift.x-(scaleDrift.x * (float)random.NextDouble ()*2));
			t.y +=  (scaleDrift.y-(scaleDrift.y * (float)random.NextDouble ()*2));
			t.z +=  (scaleDrift.z-(scaleDrift.z * (float)random.NextDouble ()*2));
			g.localScale = t;

		}

		public GameObject PaintLine(Vector2 a, Vector2 b){
			if (random == null) {
				random = new System.Random(System.DateTime.Now.DayOfYear*System.DateTime.Now.Millisecond);
			}
			Vector3 cP = Vector3.zero, cP2 = Vector3.zero;
			Vector2 d = b - a;

			float angle = Mathf.Atan2 (d.y, d.x) * Mathf.Rad2Deg;

			switch (canvasPlane) {
			case CanvasPlane.xy:
				cP.x = a.x;
				cP.y = a.y;
				cP2.x = b.x;
				cP2.y = b.y;
				break;
			case CanvasPlane.xz:
				cP.x = a.x;
				cP.z = a.y;
				cP2.x = b.x;
				cP2.z = b.y;
				break;
			case CanvasPlane.yz:
				cP.y = a.x;
				cP.z = a.y;
				cP2.y = b.x;
				cP2.z = b.y;
				break;
			}

			Transform p = Paint (Vector3.Lerp (cP, cP2, 0.5f)).transform;
			float lSX = p.LossyScale (new Vector3 (Vector3.Distance (cP, cP2) * 1.05f, p.lossyScale.y, p.lossyScale.z)).x;
			p.localScale = new Vector3(lSX, p.localScale.y, p.localScale.z);
			switch (canvasPlane) {
			case CanvasPlane.xy:
				p.eulerAngles = new Vector3 (p.eulerAngles.x, p.eulerAngles.y, angle);
				break;
			case CanvasPlane.xz:
				p.eulerAngles = new Vector3 (p.eulerAngles.x, angle, p.eulerAngles.z);
				break;
			case CanvasPlane.yz:
				p.eulerAngles = new Vector3 (angle, p.eulerAngles.y, p.eulerAngles.z);
				break;
			}
			return p.gameObject;
		}

		public void ClearPaint(PaintSample o){
			bool doDestroy = true;
			for (int i = pallet.Length - 1; i >= 0; i--) {
				bool found = false;
				for (int j = pallet [i].instances.Count - 1; j >= 0; j--) {
					if (pallet [i].instances [j] == o){
						pallet [i].Remove (j);
						found = true; 
						doDestroy = false;
						break;
					}
				}
				if (found) {
					break;
				}
			}

			if (doDestroy) {
				if (!Application.isEditor) {
					Object.Destroy (o);
				} else {
					#if UNITY_EDITOR
					Object.DestroyImmediate(o);
					#endif
				}
			}
		}
	}

	[System.Serializable]
	public class PalletItem{
		public PaintSample baseItem;
		public List<PaintSample> instances = new List<PaintSample>();
		public List<bool> instanceIsInPlay = new List<bool>();
		public int maxInstances, currentInstance;
		public bool recycleIfMaxed = true;

		public PaintSample Paint(Vector3 p){
			GameObject paintObj;
			PaintSample paint;
			int id = CheckForInactive ();

			if (id >= 0) {
				paint = instances [id];
				instanceIsInPlay [id] = true;

			} else if (maxInstances > 0) {
				if (instances.Count >= maxInstances) {
					//If we're recycling, give it our oldest, using currentInstance as our metric.
					if (recycleIfMaxed) {
						paint = instances [currentInstance];
						instanceIsInPlay [currentInstance] = true;
						currentInstance++;
						if (currentInstance >= maxInstances) {
							currentInstance = 0;
						}
					} else {
						//...and if we're not recycling, return null. We have no paint to give. Out of paint.
						return null;
					}
				} else {
					//Otherwise, add a new instance.
					if (!Application.isEditor) {
						paintObj = (GameObject)Object.Instantiate (baseItem.gameObject, p, baseItem.transform.rotation);
						paintObj.hideFlags = HideFlags.HideInHierarchy;
						paint = paintObj.GetComponent<PaintSample> ();
                    } else {
						#if UNITY_EDITOR
						paintObj = (GameObject)PrefabUtility.InstantiatePrefab (baseItem.gameObject, UnityEngine.SceneManagement.SceneManager.GetActiveScene ());
						paint = paintObj.GetComponent<PaintSample> ();
                        #endif
                    }
					instances.Add (paint);
					instanceIsInPlay.Add (true);
				}
			} else {
				id = instances.Count;
				if (!Application.isEditor) {
					paintObj = (GameObject)Object.Instantiate (baseItem.gameObject, p, baseItem.transform.rotation);
					//paintObj.hideFlags = HideFlags.HideInHierarchy;
					paint = paintObj.GetComponent<PaintSample> ();
                } else {
					#if UNITY_EDITOR
					paintObj = (GameObject)PrefabUtility.InstantiatePrefab (baseItem.gameObject, UnityEngine.SceneManagement.SceneManager.GetActiveScene ());
					paint = paintObj.GetComponent<PaintSample> ();
                   
					#endif
				}
				instances.Add (paint);
				instanceIsInPlay.Add (true);
			}
			paint.transform.position = p;
            //Debug.Log(baseItem.transform.localScale);
            //paint.transform.localScale = baseItem.transform.localScale;
            paint.sampleID = id;
			paint.onPaint.Invoke ();
			return paint;
		}

		public void Remove(int id){
			instanceIsInPlay [id] = false;
			instances [id].transform.parent = null;
			instances [id].gameObject.SetActive (false);
		}
		public int CheckForInactive(){
			for (int i = 0; i < instances.Count; i++) {
				if (!instanceIsInPlay [i]) {
					return i;
				}
			}
			return -1;
		}
	}

	[System.Serializable]
	public class PalletSet{
		public string name;
		public int[] ids;
	}

	[System.Serializable]
	public class PaintTarget {

		public Vector3 position, rotation, scale;

		public PaintSample paint;
	}
}