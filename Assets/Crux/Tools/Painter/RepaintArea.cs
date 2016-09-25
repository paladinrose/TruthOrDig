using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace Crux{
	public class RepaintArea : MonoBehaviour {

		[System.Serializable]
		public class RepaintZoneBase {
			public string name;
			public PaintTarget[] repaintTargets;
			public UnityEvent onEnter, onExit;
			public RepaintArea area;
			public virtual void GetAllPaintTargets(PaintTarget[] masterTargetList){
				repaintTargets = new PaintTarget[0];
			}

			public void DeactivateAllPaintSamples(){
				for (int i = repaintTargets.Length - 1; i >= 0; i--) {
					if (repaintTargets [i].paint != null) {
						repaintTargets [i].paint.gameObject.SetActive (false);
					}
				}
			}

			public void ActivateAllPaintSamples(){
				for (int i = repaintTargets.Length - 1; i >= 0; i--) {
					if (repaintTargets [i].paint != null) {
						repaintTargets [i].paint.gameObject.SetActive (true);
					}
				}
			}
		}
		public class RepaintZone : RepaintZoneBase {
			public Collider collider;

			override public void GetAllPaintTargets(PaintTarget[] masterTargetList){
				List<PaintTarget> targs = new List<PaintTarget> ();
				for (int i = 0; i < masterTargetList.Length; i++) {
					if(collider.Contains(masterTargetList[i].position)){
						targs.Add(masterTargetList[i]);
					}
				}
				repaintTargets = targs.ToArray ();
			}
		}
		public class RepaintZone2D : RepaintZoneBase {
			public Collider2D collider;

			override public void GetAllPaintTargets(PaintTarget[] masterTargetList){
				List<PaintTarget> targs = new List<PaintTarget> ();
				for (int i = 0; i < masterTargetList.Length; i++) {
					if (collider.OverlapPoint (masterTargetList [i].position)) {
						targs.Add (masterTargetList [i]);
					}
				}
				repaintTargets = targs.ToArray ();
			}
		}

		public Painter painter;
		public RepaintZoneBase[] zones;
		public PaintTarget[] targets;
		public Camera detailViewer;
		public int viewLayer, maximumRenderObjects;
		public float renderSliceDistance;

		public void ZoneEnter(int id){
			if (id >= 0 && id < zones.Length) {
				zones [id].onEnter.Invoke ();
			}
		}

		public void ZoneExit(int id){
			if (id >= 0 && id < zones.Length) {
				zones [id].onExit.Invoke ();
			}
		}
		public void GetAllPaintTargets(){
			for (int i = 0; i < zones.Length; i++) {
				zones [i].GetAllPaintTargets (targets);
			}
		}

		public void GetPaintTargetsIn(int id){
			zones [id].GetAllPaintTargets (targets);
		}

		public delegate void DetailViewResults (RenderTexture[] renders, float[] renderDistances);

		public IEnumerator RenderDetailView(Vector3 position, Vector3 rotation, float startRange, float endRange, DetailViewResults results, RenderTexture texFormat = null){
			//1. Create a camera that will only render the layer with my paint details.
			//2. Position it at startPosition with start and end ranges.
			//3. Tell all paint details in that view to turn on and go to their highest LOD.
			if(!detailViewer){
				GameObject viewCam = new GameObject ("Detail Viewer");
				viewCam.hideFlags = HideFlags.HideInHierarchy;
				detailViewer = viewCam.AddComponent<Camera> ();
				detailViewer.cullingMask = 1 << viewLayer;
				detailViewer.backgroundColor = Color.clear;
				detailViewer.clearFlags = CameraClearFlags.Nothing;
			}

			detailViewer.gameObject.SetActive (true);

			detailViewer.transform.position = position;
			detailViewer.transform.eulerAngles = rotation;
			detailViewer.nearClipPlane = startRange;
			detailViewer.farClipPlane = endRange;
			//detailViewer.targetTexture = tex;

			Plane[] planes = GeometryUtility.CalculateFrustumPlanes (detailViewer);
			List<RenderTexture> textures = new List<RenderTexture> ();
			List<float> sliceDistances = new List<float> ();

			List<PaintSample> seenDetails = new List<PaintSample> ();
			List<float> detailDistances = new List<float> ();
			for (int i = 0; i < zones.Length; i++) {
				if (zones [i] is RepaintZone) {
					RepaintZone z = (RepaintZone)zones [i];
				
					z.ActivateAllPaintSamples ();
					if (maximumRenderObjects > 0) {
						yield return null;
					}
					if (GeometryUtility.TestPlanesAABB (planes, z.collider.bounds)) {
						for (int j = 0; j < z.repaintTargets.Length; j++) {
							Bounds paintBounds = z.repaintTargets [j].paint.GetComponentInChildren<Renderer> ().bounds;
							if (z.repaintTargets [j].paint != null && GeometryUtility.TestPlanesAABB (planes, paintBounds)) {
							
								//z.repaintTargets [j].paint.levelsOfDetail.ForceLOD (z.repaintTargets [j].paint.levelsOfDetail.lodCount);
								float distance = Vector3.Distance (detailViewer.transform.position, paintBounds.center);
								//Okay, I want to sort them by distance here. We start by assuming that, since this process will work, the list, such as it is, will always be in order.
								//Given that it's in order, we can use a better sorting method. Check first, midpoint and last. If it's smaller than first, or larger than last, we know where to add it.
								//If it's neither, check it against the middle. We now know that it's either:
								//A. Bigger than the smallest, and smaller than the middle 
								//or
								//B. Smaller than the biggest, and bigger than the middle.
								//We check midway between the two until there's no gap between them anymore and that's where we add it.
								//If it ever ties with a result, we add it AFTER the one that already exists.
								int sD = seenDetails.Count;
								switch (sD) {
								case 0:
									seenDetails.Add (z.repaintTargets [j].paint);
									detailDistances.Add (distance);
									break;
								case 1:
									if (distance >= detailDistances [0]) {
										seenDetails.Add (z.repaintTargets [j].paint);
										detailDistances.Add (distance);
									} else {
										seenDetails.Insert (0, z.repaintTargets [j].paint);
										detailDistances.Insert (0, distance);
									}
									break;
								case 2:
									if (distance < detailDistances [0]) {
										seenDetails.Insert (0, z.repaintTargets [j].paint);
										detailDistances.Insert (0, distance);
									} else if (distance >= detailDistances [1]) {
										seenDetails.Add (z.repaintTargets [j].paint);
										detailDistances.Add (distance);
									} else {
										seenDetails.Insert (1, z.repaintTargets [j].paint);
										detailDistances.Insert (1, distance);
									}
									break;
								default:
									int a = 0, b = seenDetails.Count - 1, diff = b - a;
									while (diff > 1) {
										if (distance < detailDistances [a]) {
											b = a;
											diff = 0;
										} else if (distance >= detailDistances [b]) {
											diff = 0;
											b++;
										} else {
											int c = diff / 2;
											if (distance >= detailDistances [c]) {
												a = c;

											} else {
												b = c;
											}
											diff = b - a;
										}
									}
									seenDetails.Insert (b, z.repaintTargets [j].paint);
									detailDistances.Insert (b, distance);
									break;
								}
							}
						}
					}
					z.DeactivateAllPaintSamples ();
				}
			}

			//BAM! Now, our targets are all definitely seen and in order from closest to farthest away.
			if (maximumRenderObjects > 0) {
				yield return null;
			}

			//Now I want to go through by slices of depth. Within each slice, I turn on and render up to maximumRenderObjects, then render. Next frame, I write the new values directly over the old ones.
			//Each slice gets its own renderTextuer.
			float currentDistance = detailDistances[detailDistances.Count-1], targetDistance = currentDistance - renderSliceDistance;

			int currentObjectCount = 0, startObjectID = seenDetails.Count-1;
			RenderTexture tex;
			if (texFormat != null) {
				tex = new RenderTexture (texFormat.width, texFormat.height, texFormat.depth, texFormat.format);
			} else {
				tex = new RenderTexture (1024, 1024, 24, RenderTextureFormat.Default);
			}
			tex.Create ();	

			for (int i = seenDetails.Count - 1; i >= 0; i--) {
				currentObjectCount++;
				if (detailDistances [i] >= targetDistance) {
					seenDetails [i].gameObject.SetActive (true);
					seenDetails [i].onPaint.Invoke ();
					seenDetails [i].levelsOfDetail.ForceLOD (seenDetails [i].levelsOfDetail.lodCount);
				} else {
					//Next slice!
					//Render whatever objects are currently active. Go back through to startObjectID and deactivate each along the way.
					RenderTexture current = RenderTexture.active;
					RenderTexture.active = tex;
					detailViewer.Render();
					//Add this rendertexture to our list. Create a new RenderTexture.
					textures.Add(tex);
					sliceDistances.Add (targetDistance);
					RenderTexture.active = current;

					for (int c = i; c <= startObjectID; c++) {
						seenDetails [c].gameObject.SetActive (false);
					}
					startObjectID = i;
					currentDistance = detailDistances [i]; targetDistance = currentDistance - renderSliceDistance;
					seenDetails [i].gameObject.SetActive (true);
					seenDetails [i].onPaint.Invoke ();
					seenDetails [i].levelsOfDetail.ForceLOD (seenDetails [i].levelsOfDetail.lodCount);

					if (texFormat != null) {
						tex = new RenderTexture (texFormat.width, texFormat.height, texFormat.depth, texFormat.format);
					} else {
						tex = new RenderTexture (1024, 1024, 24, RenderTextureFormat.Default);
					}
					tex.Create ();
					detailViewer.targetTexture = tex;
				}

				if (maximumRenderObjects > 0 && currentObjectCount >= maximumRenderObjects) {
					//Render and deactivate back through maxinmumRenderObjects worth of objects.

					RenderTexture current = RenderTexture.active;
					RenderTexture.active = tex;
					detailViewer.Render();
					RenderTexture.active = current;
					for (currentObjectCount = 0; currentObjectCount < maximumRenderObjects; currentObjectCount++) {
						seenDetails [i + currentObjectCount].levelsOfDetail.ForceLOD (-1);
						seenDetails [i + currentObjectCount].onRemove.Invoke ();
						seenDetails [i + currentObjectCount].gameObject.SetActive (false);
					}
					currentObjectCount = 0;
					startObjectID = i;
				}

			}

			RenderTexture currentRT = RenderTexture.active;
			RenderTexture.active = tex;
			detailViewer.Render ();
			RenderTexture.active = currentRT;
			textures.Add (tex);
			//Texture2D modTex = Texture2D.CreateExternalTexture (tex.width, tex.height, tex.format, true, true, tex.GetNativeTexturePtr ());

			sliceDistances.Add (targetDistance);
			
			detailViewer.gameObject.SetActive (false);
			results.Invoke(textures.ToArray(), sliceDistances.ToArray());
		}


		public void TestDetailRender(){
		}
		public void TestReturn(RenderTexture[] texs, float[] dists){
			
		}
	}
}