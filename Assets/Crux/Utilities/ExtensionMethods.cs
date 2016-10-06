using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Crux;

#if UNITY_EDITOR
using UnityEditor;
#endif
public static class ExtensionMethods {

	public static Bounds GlobalBounds(this Transform t, bool includeKids = false)
	{
		Bounds bounds;
		RectTransform rt;
		Renderer renderer;

		if (t is RectTransform) {
			rt = t as RectTransform;
			bounds = new Bounds(rt.rect.center, rt.TransformVector (rt.rect.size));
		} else {
			renderer = t.gameObject.GetComponent<Renderer> ();
			if (renderer) {
				bounds = renderer.bounds;
			} else {
				bounds = new Bounds (t.position, Vector3.zero);
			}
		}

		if (includeKids) {
			Transform[] kids = t.gameObject.GetComponentsInChildren<Transform>();
			for (int i = 1; i < kids.Length; i++) {
				if (kids[i] is RectTransform) {
					rt = kids[i] as RectTransform;
					bounds.Encapsulate(new Bounds(rt.rect.center, rt.TransformVector (rt.rect.size)));
				} else {
					renderer = kids[i].gameObject.GetComponent<Renderer> ();
					if (renderer) {
						bounds.Encapsulate (renderer.bounds);
					}
				}
			}
		}
		return bounds;
	}

	public static Bounds LocalBounds(this Transform t, bool includeKids = false)
	{

		Bounds combinedBounds;
		Vector3 initRot = t.eulerAngles, initPos = t.position;
		Transform oldParent = t.parent;
		RectTransform rt;
		Renderer renderer;

		t.parent = null;
		t.eulerAngles = t.position = Vector3.zero;

		if (t is RectTransform) {
			rt = t as RectTransform;
			combinedBounds = new Bounds(rt.rect.center, rt.TransformVector (rt.rect.size));
		} else {
			renderer = t.gameObject.GetComponent<Renderer> ();
			if (renderer) {
				combinedBounds = renderer.bounds;
			} else {
				combinedBounds = new Bounds (Vector3.zero, Vector3.zero);
			}
		}

		if (includeKids) {
			Transform[] kids = t.gameObject.GetComponentsInChildren<Transform>();
			for (int i = 1; i < kids.Length; i++) {
				if (kids[i] is RectTransform) {
					rt = kids[i] as RectTransform;
					combinedBounds.Encapsulate(new Bounds(rt.rect.center, rt.TransformVector (rt.rect.size)));
				} else {
					renderer = kids[i].gameObject.GetComponent<Renderer> ();
					if (renderer) {
						combinedBounds.Encapsulate(renderer.bounds);
					} 
				}
			}
		}
		t.parent = oldParent;
		t.eulerAngles = initRot;
		t.position = initPos;

		Vector3 size = combinedBounds.size;
		if (t.localScale.x < 0) {
			size.x *= -1;
		} if (t.localScale.y < 0) {
			size.y *= -1;
		} if (t.localScale.z < 0) {
			size.z *= -1;
		}
		combinedBounds.size = size;

		return combinedBounds;
	}

	public static Vector3 BoundsAlign(this Transform targ, Transform reference,Vector3 targAlign,Vector3 refAlign,Vector3 targOffset,Vector3 refOffset, bool includeKids = false)
	{

		Bounds referenceBounds = reference.LocalBounds(includeKids);

		Bounds targetBounds = targ.LocalBounds(includeKids);

		Vector3 referenceHalfSize = referenceBounds.size*0.5f, targetHalfSize = targetBounds.size*0.5f;
		referenceHalfSize-=refOffset;
		targetHalfSize-=targOffset;
		Vector3 alignmentPos = referenceBounds.center;
		alignmentPos+= Vector3.Scale(refAlign,referenceHalfSize);	
		alignmentPos-= Vector3.Scale(targAlign,targetHalfSize);

		return alignmentPos;
	}

	public static Vector3 LocalBoundsAlign(this Transform targ, Transform reference,Vector3 targAlign,Vector3 refAlign,Vector3 targOffset,Vector3 refOffset, bool includeKids = false)
	{

		Bounds targetBounds = targ.LocalBounds(includeKids);
		Bounds referenceBounds = reference.LocalBounds(includeKids);


		Vector3 referenceHalfSize = referenceBounds.size*0.5f, targetHalfSize = targetBounds.size*0.5f;
		referenceHalfSize-=refOffset;
		targetHalfSize-=targOffset;


		Vector3 currentPos = referenceBounds.center;
		Vector3 alignmentPos = Vector3.zero;

		referenceHalfSize-=refOffset;
		targetHalfSize-=targOffset;

		alignmentPos+= Vector3.Scale(refAlign,referenceHalfSize);	
		currentPos += reference.TransformDirection(alignmentPos);
		alignmentPos = Vector3.zero;
		alignmentPos-= Vector3.Scale(targAlign,targetHalfSize);
		currentPos += targ.TransformDirection(alignmentPos);
		return currentPos;
	}

	public static void SetLocalBounds(this Transform t, Bounds bounds, bool includeKids = false){
		//Get the bounds right now.
		//Compare to the local scale.
		//Figure out how to scale to bring our bounds to where they should be.
		Bounds originalBounds = t.LocalBounds(includeKids);
		if (bounds == originalBounds) {
			return;
		}
		//Vector3  relativePosition = t.InverseTransformPoint (originalBounds.center);

		//desiredScale is to bounds.size as localscale is to originalBounds.size
		Vector3 localScale = t.localScale;
		localScale.x = bounds.size.x * localScale.x / originalBounds.size.x;
		localScale.y = bounds.size.y * localScale.y / originalBounds.size.y;
		localScale.z = bounds.size.z * localScale.z / originalBounds.size.z;
		t.localScale = localScale;
		originalBounds = t.LocalBounds (includeKids);
		Vector3  relativePosition = t.TransformDirection (originalBounds.center), relativeNew =  t.TransformDirection(bounds.center);
		t.position += relativeNew - relativePosition;
		//t.position = relativeNew;
	}

	public enum BoundSides { Front, Back, Left, Right, Top, Bottom };

	public static Bounds MoveSide(Bounds b,BoundSides side, float dist)
	{
		Vector3 center = b.center, size = b.size;
		switch (side) {
		case BoundSides.Front:
			size.z = b.max.z + dist - b.min.z; 
			center.z = b.min.z + size.z*0.5f;
			break;
		case BoundSides.Back:
			size.z = b.max.z - b.min.z + dist; 
			center.z = b.max.z - size.z*0.5f;
			break;
		case BoundSides.Left:
			size.x = b.max.x - b.min.x + dist;
			center.x = b.max.x - size.x * 0.5f;
			break;
		case BoundSides.Right:
			//Debug.Log ("Local Right: " + b.max.x.ToString () + "  Moving by: " + dist.ToString ());
			size.x = b.max.x + dist - b.min.x;
			//Debug.Log ("Local Left: " + b.min.x.ToString () + "  Resultant size: " + size.x.ToString ());
			center.x = b.min.x + size.x * 0.5f;
			//Debug.Log ("Center Point: " + center.x.ToString () + "  Resultant Left Side: " + (new Bounds(center, size)).min.x.ToString());
			break;
		case BoundSides.Top:
			size.y = b.max.y + dist - b.min.y;
			center.y = b.min.y + size.y * 0.5f;
			break;
		case BoundSides.Bottom:
			size.y = b.max.y + dist - b.min.y;
			center.y = b.max.y - size.y * 0.5f;
			break;
		}
		return new Bounds(center, size);
	}

	public static Bounds SetSide(Bounds b, BoundSides side, float value)
	{
		Vector3 center = b.center, size = b.size;
		switch (side) {
		case BoundSides.Front:
			size.z = value - b.min.z; 
			center.z = b.min.z + size.z*0.5f;
			break;
		case BoundSides.Back:
			size.z = b.max.z - value; 
			center.z = b.max.z - size.z*0.5f;
			break;
		case BoundSides.Left:
			size.x = b.max.x - value;
			center.x = b.max.x - size.x * 0.5f;
			break;
		case BoundSides.Right:
			size.x = value - b.min.x;
			center.x = b.min.x + size.x * 0.5f;
			break;
		case BoundSides.Top:
			size.y = value - b.min.y;
			center.y = b.min.y + size.y * 0.5f;
			break;
		case BoundSides.Bottom:
			size.y = b.max.y - value;
			center.y = b.max.y - size.y * 0.5f;
			break;
		}
		return new Bounds(center, size);
	}

	public static Vector3 LocalObjective(this Transform obj, Vector3 p)
	{
		return obj.position + obj.TransformDirection (p);
	}
	public static Vector3 LossyScale(this Transform obj, Vector3 size, bool includeKids = false)
	{
		Vector3 origScale = obj.localScale, newSize = Vector3.zero;
		Bounds bBounds = obj.LocalBounds(includeKids);
		//OriginalScale / bounds.size = newScale / newSize;
		//newScale = originalScale / bounds.size * newSize;
		float sideLength = origScale.x*size.x;
		if(sideLength == 0f || bBounds.size.x == 0f){newSize.x = origScale.x;}
		else{newSize.x = sideLength/bBounds.size.x;}

		sideLength = origScale.y*size.y;
		if(sideLength == 0f || bBounds.size.y == 0f){newSize.y = origScale.y;}
		else{newSize.y = sideLength/bBounds.size.y;}

		sideLength = origScale.z*size.z;
		if(sideLength == 0f || bBounds.size.z == 0f){newSize.z = origScale.z;}
		else{newSize.z = sideLength/bBounds.size.z;}
		
		return newSize;
	}

	//I think this is more technically Pitch, Yaw, or Roll. I need to figure out which and rename it. 
	//I also need to change up the X, Y, and Z vals and create whichever other two I don't have.
	public static float PlanarAngle(this Transform t, Transform d)
	{
		Vector3 vec = Vector3.zero;
		vec = d.position - t.position;
		vec = d.InverseTransformDirection (vec);
		return Mathf.Atan2 (vec.y, vec.x) * Mathf.Rad2Deg;
	}

	public static Vector3 ConvertEulerLocalToGlobal(this Transform targ, Vector3 localEulers)
	{
		Vector3 r = targ.eulerAngles;
		targ.localEulerAngles = targ.localEulerAngles + localEulers;
		Vector3 ret = targ.eulerAngles - r;
		targ.eulerAngles = r;
		return ret;
	}


	public enum PrivacyLevel { All, Public, Private, Static, PublicAndStatic, PublicAndPrivate, PrivateAndStatic }

	public static BindingFlags Privacy(PrivacyLevel p)
	{

		switch (p) {
		case PrivacyLevel.All:
			return BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

		case PrivacyLevel.Public:
			return BindingFlags.Instance | BindingFlags.Public;

		case PrivacyLevel.Private:
			return BindingFlags.Instance | BindingFlags.NonPublic;

		case PrivacyLevel.Static:
			return BindingFlags.Instance | BindingFlags.Static;

		case PrivacyLevel.PublicAndPrivate:
			return BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		case PrivacyLevel.PublicAndStatic:
			return BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;

		case PrivacyLevel.PrivateAndStatic:
			return BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;

		}
		return BindingFlags.Default;
	}

	public enum TaskSuccess{Any, Just, All, AllBut,  AnyBut, None}

	public static bool CheckTaskSuccess(bool[] taskStatus, TaskSuccess t, int id=0){
		int i;
		switch (t) {
		case TaskSuccess.Any:
			for (i = taskStatus.Length - 1; i >= 0; i--) {
				if (taskStatus [i]) {
					return true;
				}
			}
			return false;
		case TaskSuccess.Just:
			if (taskStatus [id]) {
				return true;
			}
			return false;
		case TaskSuccess.All:
			for (i = taskStatus.Length - 1; i >= 0; i--) {
				if (!taskStatus [i]) {
					return false;
				}
			}
			return true;
		case TaskSuccess.AllBut:
			if (taskStatus [id]) {
				return false;
			}
			for (i = taskStatus.Length - 1; i >= 0; i--) {
				if (i != id && !taskStatus [i]) {
					return false;
				}
			}

			return true;
		case TaskSuccess.AnyBut:
			if (taskStatus [id]) {
				return false;
			}
			for (i = taskStatus.Length - 1; i >= 0; i--) {
				if (i != id && taskStatus [i]) {
					return true;
				}
			}

			return false;
		case TaskSuccess.None:
			for (i = taskStatus.Length - 1; i >= 0; i--) {
				if (taskStatus [i]) {
					return false;
				}
			}
			return true;
		}
		return false;
	}


	public static bool InTolerance(this Vector3 a, Vector3 b, float tol){
		Vector3 d = b - a;
		if (Mathf.Abs (d.x) > tol) {
			return false;
		}
		if (Mathf.Abs (d.y) > tol) {
			return false;
		}
		if (Mathf.Abs (d.z) > tol) {
			return false;
		}
		return true;
	}

	public static bool InTolerance(this Vector2 a, Vector2 b, float tol){
		Vector2 d = b - a;
		if (Mathf.Abs (d.x) > tol) {
			return false;
		}
		if (Mathf.Abs (d.y) > tol) {
			return false;
		}
		return true;
	}

    public static Vector[] ConvertToVector(this Vector2[] value)
    {
        Vector[] v = new Vector[value.Length];
        for (int i = v.Length - 1; i >= 0; i--)
        {
            v[i] = value[i];
        }
        return v;
    }
    public static Vector[] ConvertToVector(this Vector3[] value){
		Vector[] v = new Vector[value.Length];
		for (int i = v.Length - 1; i >= 0; i--) {
			v [i] = value [i];
		}
		return v;
	}

	public static Vector2[] ConvertToVec2(this Vector3[] value){
		Vector2[] v = new Vector2[value.Length];
		for (int i = v.Length - 1; i >= 0; i--) {
			v [i] = value [i];
		}
		return v;
	}
    public static Vector2[] ConvertToVec2(this Vector[] value)
    {
        Vector2[] v = new Vector2[value.Length];
        for (int i = v.Length - 1; i >= 0; i--)
        {
            v[i] = value[i];
        }
        return v;
    }
    public static Vector3[] ConvertToVec3 (this Vector2[] value){
		Vector3[] v = new Vector3[value.Length];
		for (int i = v.Length - 1; i >= 0; i--) {
			v [i] = value [i];
		}
		return v;
	} 


	public static Vector3[] UvToWorldPoints(this MeshFilter m, Vector2 uv) {
		List<Vector3> hits = new List<Vector3> ();
		Mesh mesh = m.mesh;
		int[] tris = mesh.triangles;
		Vector2[] uvs = mesh.uv;
		Vector3[] verts = mesh.vertices;
		for (int i = 0; i < tris.Length; i += 3){
			
			Vector2 u1 = uvs[tris[i]];
			Vector2 u2 = uvs[tris[i+1]];
			Vector2 u3 = uvs[tris[i+2]];
			// calculate triangle area - if zero, skip it
			float a = UVTriangleArea(u1, u2, u3); if (a == 0) {continue;}
			// calculate barycentric coordinates of u1, u2 and u3
			// if anyone is negative, point is outside the triangle: skip it
			float a1  = UVTriangleArea(u2, u3, uv)/a; if (a1 < 0) {continue;}
			float a2 = UVTriangleArea(u3, u1, uv)/a; if (a2 < 0) {continue;}
			float a3 = UVTriangleArea(u1, u2, uv)/a; if (a3 < 0) {continue;}
			// point inside the triangle - find mesh position by interpolation...
			Vector3 p3D= a1*verts[tris[i]]+a2*verts[tris[i+1]]+a3*verts[tris[i+2]];
			// and return it in world coordinates:
			hits.Add(m.transform.TransformPoint(p3D));
			//Now, add the normal of the triangle in question.
			hits.Add(m.mesh.normals[i/3]);
		}

		return hits.ToArray();
	}

	// calculate signed triangle area using a kind of "2D cross product":
	public static float UVTriangleArea(Vector2 p1, Vector2 p2, Vector2 p3){
		Vector2 v1 = p1 - p3;
		Vector2 v2 = p2 - p3;
		return ((v1.x * v2.y - v1.y * v2.x)*0.5f);
	}


	public static int GetClosestPoint(this Vector3[] _points, Vector3 p)
	{
		float lowestDist = float.PositiveInfinity;
		int id = -1;
		for(int i = _points.Length-1; i >=0; i--)
		{
			float d = Vector3.Distance (p, _points [i]);
			if (d< lowestDist) {
				id = i;
				lowestDist = d;
			}
		}
		return id;
	}

	public static Vector3 GetClosestPointOnPolygon(this Vector3[] _points, Vector3 p, int samples, ref int cls)
	{
		int closestPolyPoint =_points.GetClosestPoint (p), left = -1, right = -1;
		Vector3 ret = Vector3.one * -1, a, b;
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

		dist = Vector3.Distance (p, _points [left]);
		if (Vector3.Distance (p, _points [right]) > dist) {
			b = a;
			a = _points [left];
			cls = left;
		} else {
			b = _points [right];
			cls = closestPolyPoint;
		}

		for (int s = samples; s > 0; s--) {
			ret = Vector3.Lerp (a, b, 0.5f);
			dist = Vector3.Distance (a, p);
			if (Vector3.Distance (b, p) < dist) {
				a = ret;
			} else {
				b = ret;
			}
		}

		dist = Vector3.Distance (a, p);
		if (dist < Vector3.Distance (b, p)) {
			ret = a;
		} else {
			ret = b;
		}

		return ret;
	}

	public static OmniRoutine StartOmniRoutine(this MonoBehaviour target, IEnumerator routine)
	{
		OmniRoutine omni = new OmniRoutine();

		omni.routines.Add(routine);
		if(!omni.isRunning){
			omni.isRunning = true;
			if(!Application.isEditor){
				target.StartCoroutine(omni.UpdateRoutine());
			} else {
				#if UNITY_EDITOR
				EditorApplication.update += omni.Update;
				#endif
			}

		}
		return omni;
	}

	public static bool PolyContainsPoint (this Vector2[] points, Vector2 p) { 
		int j = points.Length-1; 
		bool inside = false; 
		for (int i = 0; i < points.Length; j = i++) { 
			if ( ((points[i].y <= p.y && p.y < points[j].y) || (points[j].y <= p.y && p.y < points[i].y)) && 
				(p.x < (points[j].x - points[i].x) * (p.y - points[i].y) / (points[j].y - points[i].y) + points[i].x)) 
				inside = !inside; 
		} 
		return inside; 
	}

	public static bool Contains(this Collider c, Vector3 point){

		if (c.bounds.Contains (point)) {
			Vector3 start = c.bounds.max * 2, direction = point - start;
			direction.Normalize ();
			int i = 0;
			Vector3 currentPoint = start;
			while (currentPoint != point) {
				RaycastHit hit;
				if (c.Raycast (new Ray (currentPoint, direction), out hit, 1000)) {
					i++;
					currentPoint = hit.point + (direction * 0.0001f);
				} else {
					currentPoint = point;
				}
			}

			while (currentPoint != start) {
				RaycastHit hit;
				if (c.Raycast (new Ray (currentPoint, -direction), out hit, 1000)) {
					i++;
					currentPoint = hit.point + (-direction * 0.0001f);
				} else {
					currentPoint = start;
				}
			}

			if (i % 2 == 1) {
				return true;
			}
		}
		return false;
	}

    public static bool WithinTolerance(this Vector3 v,Vector3 b, float tol)
    {
        bool r = false;
        Vector3 low = new Vector3(v.x - tol, v.y - tol, v.z - tol), high = new Vector3(v.x + tol, v.y + tol, v.z + tol);
        if(b.x <= high.x && b.x >= low.x &&
            b.y <= high.y && b.y >= low.y &&
            b.z <= high.z && b.z >= low.z)
        {
            r = true;
        }
        return r;
    }
}
