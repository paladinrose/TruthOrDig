using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LayoutTransform : MonoBehaviour {

	[HideInInspector]
	public Transform t;

	[HideInInspector]
	public bool includeChildrenInLocalBounds, includeChildrenInGlobalBounds, bindInEditor;
	public Vector3[] anchors = new Vector3[0];

	public LayoutSet[] layoutSets;

	public float defaultWeight = 1, defaultType = 1;
	public ObjectBinder binder;

	[HideInInspector]
	public Bounds bounds, localBounds;
	//Each Vector3 contains the following values:
	//x = Value
	//y = Value Weight
	//z = Value Type (Multiplyer is <=0, Additive is >0) 
	List<Vector3> positionX = new List<Vector3>(), positionY = new List<Vector3>(), positionZ = new List<Vector3>(), 
	rotationX = new List<Vector3>(), rotationY = new List<Vector3>(), rotationZ = new List<Vector3>(), 
	scaleX = new List<Vector3>(), scaleY = new List<Vector3>(), scaleZ = new List<Vector3>(),
	localPositionX = new List<Vector3>(), localPositionY = new List<Vector3>(), localPositionZ = new List<Vector3>(), 
	localRotationX = new List<Vector3>(), localRotationY = new List<Vector3>(), localRotationZ = new List<Vector3>(), 
	localScaleX = new List<Vector3>(), localScaleY = new List<Vector3>(), localScaleZ = new List<Vector3>();

	bool posX, posY, posZ, rotX, rotY, rotZ, sclX, sclY, sclZ;

	void Reset()
	{
		t = transform;
		t.hideFlags = HideFlags.HideInInspector;
		SetupBindings ();
	}
	void Start()
	{
		binder.Connect ();
	}
		
	[ContextMenu("Setup")]
	public void SetupBindings()
	{
		if (binder == null) {
			binder = new ObjectBinder ();
		}
		binder.name = "Layout Bindings";
		binder.baseTargets = new Object[25];
		binder.baseTargetLabels = new string[25];
		binder.memberPaths = new string[25];
		for (int i = 24; i >= 0; i--) {
			binder.baseTargets [i] = this;
		}
		binder.baseTargetLabels [0] = "Position X";
		binder.memberPaths [0] = "SetPositionX";

		binder.baseTargetLabels [1] = "Position Y";
		binder.memberPaths [1] = "SetPositionY";

		binder.baseTargetLabels [2] = "Position Z";
		binder.memberPaths [2] = "SetPositionZ";

		binder.baseTargetLabels [3] = "Rotation X";
		binder.memberPaths [3] = "SetRotationX";

		binder.baseTargetLabels [4] = "Rotation Y";
		binder.memberPaths [4] = "SetRotationY";

		binder.baseTargetLabels [5] = "Rotation Z";
		binder.memberPaths [5] = "SetRotationZ";

		binder.baseTargetLabels [6] = "Scale X";
		binder.memberPaths [6] = "SetScaleX";

		binder.baseTargetLabels [7] = "Scale Y";
		binder.memberPaths [7] = "SetScaleY";

		binder.baseTargetLabels [8] = "Scale Z";
		binder.memberPaths [8] = "SetScaleZ";

		binder.baseTargetLabels [9] = "Local Position X";
		binder.memberPaths [9] = "SetLocalPositionX";

		binder.baseTargetLabels [10] = "Local Position Y";
		binder.memberPaths [10] = "SetLocalPositionY";

		binder.baseTargetLabels [11] = "Local Position Z";
		binder.memberPaths [11] = "SetLocalPositionZ";

		binder.baseTargetLabels [12] = "Local Rotation X";
		binder.memberPaths [12] = "SetLocalRotationX";

		binder.baseTargetLabels [13] = "Local Rotation Y";
		binder.memberPaths [13] = "SetLocalRotationY";

		binder.baseTargetLabels [14] = "Local Rotation Z";
		binder.memberPaths [14] = "SetLocalRotationZ";

		binder.baseTargetLabels [15] = "Local Scale X";
		binder.memberPaths [15] = "SetLocalScaleX";

		binder.baseTargetLabels [16] = "Local Scale Y";
		binder.memberPaths [16] = "SetLocalScaleY";

		binder.baseTargetLabels [17] = "Local Scale Z";
		binder.memberPaths [17] = "SetLocalScaleZ";

		binder.baseTargetLabels [18] = "Front";
		binder.memberPaths [18] = "MoveFront";

		binder.baseTargetLabels [19] = "Back";
		binder.memberPaths [19] = "MoveBack";

		binder.baseTargetLabels [20] = "Left";
		binder.memberPaths [20] = "MoveLeft";

		binder.baseTargetLabels [21] = "Right";
		binder.memberPaths [21] =  "MoveRight";

		binder.baseTargetLabels [22] = "Top";
		binder.memberPaths [22] =  "MoveTop";

		binder.baseTargetLabels [23] = "Bottom";
		binder.memberPaths [23] = "MoveBottom";

		binder.baseTargetLabels [24] = "Look At";
		binder.memberPaths [24] = "LookAt";

	}

	public void UpdateTransform()
	{
		int i;
		if (t==null){t = transform;}
		Vector3 finalVector = Vector3.zero;
		bool updateSet = false;
		float weight = 0;
		Bounds bBounds = t.LocalBounds(false);
		float sideLength;

		if (sclX) {
			updateSet = true;
			weight = 0; 
			for (i = scaleX.Count - 1; i >= 0; i--) {
				weight += scaleX [i].y;
			}
			for (i = localScaleX.Count - 1; i >= 0; i--) {
				weight += localScaleX [i].y;
			}
			weight = 1 / weight;

			for (i = scaleX.Count - 1; i >= 0; i--) {
				sideLength = t.lossyScale.x * scaleX [i].x;
				if (sideLength == 0f || bBounds.size.x == 0f) {
					sideLength = t.localScale.x * (scaleX [i].y * weight);
				} else {
					sideLength = (sideLength / bBounds.size.x) * (scaleX [i].y * weight);
				}
				if (scaleX [i].z > 0) {
					finalVector.x += sideLength;
				} else {
					finalVector.x *= sideLength;
				}
			}
			for (i = localScaleX.Count - 1; i >= 0; i--) {
				if (localScaleX [i].z > 0) {
					finalVector.x += localScaleX [i].x * (localScaleX [i].y * weight);
				} else {
					finalVector.x *= localScaleX [i].x * (localScaleX [i].y * weight);
				}
			}
			localScaleX.Clear ();
			scaleX.Clear ();
			sclX = false;
		} else {
			finalVector.x = t.localScale.x;
		}
		if (sclY) {
			updateSet = true;
			weight = 0; 
			for (i = scaleY.Count - 1; i >= 0; i--) {
				weight += scaleY [i].y;
			}
			for (i = localScaleY.Count - 1; i >= 0; i--) {
				weight += localScaleY [i].y;
			}
			weight = 1 / weight;
			for (i = scaleY.Count - 1; i >= 0; i--) {
				sideLength = t.lossyScale.y* scaleY [i].x;
				if(sideLength == 0f || bBounds.size.y == 0f){sideLength = t.localScale.y * (scaleY [i].y * weight);}
				else{sideLength = (sideLength/ bBounds.size.y )* (scaleY [i].y * weight);}
				if (scaleX [i].z > 0) {
					finalVector.y += sideLength;
				} else {
					finalVector.y *= sideLength;
				}
			}
			for (i = localScaleY.Count - 1; i >= 0; i--) {
				if (localScaleY [i].z > 0) {
					finalVector.y += localScaleY [i].x * (localScaleY [i].y * weight);
				} else {
					finalVector.y *= localScaleY [i].x * (localScaleY [i].y * weight);
				}
			}
			localScaleY.Clear ();
			scaleY.Clear ();
			sclY = false;
		}else {
			finalVector.y = t.localScale.y;
		}
		if (sclZ) {
			updateSet = true;
			weight = 0; 
			for (i = scaleZ.Count - 1; i >= 0; i--) {
				weight += scaleZ [i].y;
			}
			for (i = localScaleZ.Count - 1; i >= 0; i--) {
				weight += localScaleZ [i].y;
			}
			weight = 1 / weight;
			for (i = scaleZ.Count - 1; i >= 0; i--) {
				sideLength = t.lossyScale.z* scaleZ [i].x;
				if(sideLength == 0f || bBounds.size.z == 0f){sideLength = t.localScale.z * (scaleZ [i].y * weight);}
				else{sideLength = (sideLength/ bBounds.size.z )* (scaleZ [i].y * weight);}
				if (scaleX [i].z > 0) {
					finalVector.z += sideLength;
				} else {
					finalVector.z *= sideLength;
				}
			}
			for (i = localScaleZ.Count - 1; i >= 0; i--) {
				if (localScaleZ [i].z > 0) {
					finalVector.z += localScaleZ [i].x * (localScaleZ [i].y * weight);
				} else {
					finalVector.z *= localScaleZ [i].x * (localScaleZ [i].y * weight);
				}
			}
			localScaleZ.Clear ();
			scaleZ.Clear ();
			sclZ = false;
		}else {
			finalVector.z = t.localScale.z;
		}
		if (updateSet) {
			t.localScale = finalVector;
			finalVector = Vector3.zero;
			updateSet = false;
		}


		if (rotX) {
			updateSet = true;
			weight = 0; 
			for (i = rotationX.Count - 1; i >= 0; i--) {
				weight += rotationX [i].y;
			}
			for (i = localRotationX.Count - 1; i >= 0; i--) {
				weight += localRotationX [i].y;
			}
			weight = 1 / weight;
			for (i = rotationX.Count - 1; i >= 0; i--) {
				if (rotationX [i].z > 0) {
					finalVector.x += rotationX [i].x * (rotationX [i].y * weight);
				} else {
					finalVector.x *= rotationX [i].x * (rotationX [i].y * weight);
				}
			}
			for (i = localRotationX.Count - 1; i >= 0; i--) {
				if (localRotationX [i].z > 0) {
					finalVector += t.ConvertEulerLocalToGlobal (new Vector3(localRotationX [i].x, 0, 0)) * (localRotationX [i].y * weight);
				} else {
					finalVector.Scale( t.ConvertEulerLocalToGlobal (new Vector3(localRotationX [i].x, 0, 0)) * (localRotationX [i].y * weight));
				}
			}
			localRotationX.Clear ();
			rotationX.Clear ();
			rotX = false;
		}
		if (rotY) {
			updateSet = true;
			weight = 0; 
			for (i = rotationY.Count - 1; i >= 0; i--) {
				weight += rotationY [i].y;
			}
			for (i = localRotationY.Count - 1; i >= 0; i--) {
				weight += localRotationY [i].y;
			}
			weight = 1 / weight;
			for (i = rotationY.Count - 1; i >= 0; i--) {
				if (rotationY [i].z > 0) {
					finalVector.y += rotationY [i].x * (rotationY [i].y * weight);
				} else {
					finalVector.y *= rotationY [i].x * (rotationY [i].y * weight);
				}
			}
			for (i = localRotationY.Count - 1; i >= 0; i--) {
				if (localRotationY [i].z > 0) {
					finalVector += t.ConvertEulerLocalToGlobal (new Vector3(0, localRotationY [i].x, 0)) * (localRotationY [i].y * weight);
				} else {
					finalVector.Scale( t.ConvertEulerLocalToGlobal (new Vector3(0, localRotationY [i].x, 0)) * (localRotationY [i].y * weight));
				}
			}
			localRotationY.Clear ();
			rotationY.Clear ();
			rotY = false;
		}
		if (rotZ) {
			updateSet = true;
			weight = 0; 
			for (i = rotationZ.Count - 1; i >= 0; i--) {
				weight += rotationZ [i].y;
			}
			for (i = localRotationZ.Count - 1; i >= 0; i--) {
				weight += localRotationZ [i].y;
			}
			weight = 1 / weight;
			for (i = rotationZ.Count - 1; i >= 0; i--) {
				if (rotationZ [i].z > 0) {
					finalVector.z += rotationZ [i].x * (rotationZ [i].y * weight);
				} else {
					finalVector.z *= rotationZ [i].x * (rotationZ [i].y * weight);
				}
			}
			for (i = localRotationZ.Count - 1; i >= 0; i--) {
				if (localRotationZ [i].z > 0) {
					finalVector += t.ConvertEulerLocalToGlobal (new Vector3(0, 0, localRotationZ [i].x)) * (localRotationZ [i].y * weight);
				} else {
					finalVector.Scale( t.ConvertEulerLocalToGlobal (new Vector3(0, 0, localRotationZ [i].x)) * (localRotationZ [i].y * weight));
				}
			}
			localRotationZ.Clear ();
			rotationZ.Clear ();
			rotZ = false;
		}
		if (updateSet) {
			t.eulerAngles = finalVector;
			finalVector = Vector3.zero;
			updateSet = false;
		}

		if (posX) {
			weight = 0;
			updateSet = true;
			for (i = positionX.Count - 1; i >= 0; i--) {
				weight += positionX [i].y;
			}
			for (i = localPositionX.Count - 1; i >= 0; i--) {
				weight += localPositionX [i].y;
			}
			weight = 1 / weight;
			for (i = positionX.Count - 1; i >= 0; i--) {
				if (positionX [i].z > 0) {
					finalVector.x += positionX [i].x * (positionX [i].y * weight);
				} else {
					finalVector.x *= positionX [i].x * (positionX [i].y * weight);
				}
			}
			for (i = localPositionX.Count - 1; i >= 0; i--) {
				if (localPositionX [i].z > 0) {
					finalVector += t.TransformPoint (localPositionX [i].x, 0, 0) * (localPositionX [i].y * weight);
				} else {
					finalVector.Scale(t.TransformPoint (localPositionX [i].x, 0, 0) * (localPositionX [i].y * weight));
				}
			}
			localPositionX.Clear ();
			positionX.Clear ();
			posX = false;
		}
		if (posY) {
			updateSet = true;
			weight = 0; 
			for (i = positionY.Count - 1; i >= 0; i--) {
				weight += positionY [i].y;
			}
			for (i = localPositionY.Count - 1; i >= 0; i--) {
				weight += localPositionY [i].y;
			}
			weight = 1 / weight;
			for (i = positionY.Count - 1; i >= 0; i--) {
				if (positionY [i].z > 0) {
					finalVector.y += positionY [i].x * (positionY [i].y * weight);
				} else {
					finalVector.y *= positionY [i].x * (positionY [i].y * weight);
				}
			}
			for (i = localPositionY.Count - 1; i >= 0; i--) {
				if (localPositionY [i].z > 0) {
					finalVector += t.TransformPoint (0, localPositionY [i].x, 0) * (localPositionY [i].y * weight);
				} else {
					finalVector.Scale( t.TransformPoint (0, localPositionY [i].x, 0) * (localPositionY [i].y * weight));
				}
			}
			localPositionY.Clear ();
			positionY.Clear ();
			posY = false;
		}
		if (posZ) {
			updateSet = true;
			weight = 0; 
			for (i = positionZ.Count - 1; i >= 0; i--) {
				weight += positionZ [i].y;
			}
			for (i = localPositionZ.Count - 1; i >= 0; i--) {
				weight += localPositionZ [i].y;
			}
			weight = 1 / weight;
			for (i = positionZ.Count - 1; i >= 0; i--) {
				if (positionZ [i].z > 0) {
					finalVector.z += positionZ [i].x * (positionZ [i].y * weight);
				} else {
					finalVector.z *= positionZ [i].x * (positionZ [i].y * weight);
				}
			}
			for (i = localPositionZ.Count - 1; i >= 0; i--) {
				if (localPositionZ [i].z > 0) {
					finalVector += t.TransformPoint (0,0, localPositionZ [i].x) * (localPositionZ [i].y * weight);
				} else {
					finalVector.Scale( t.TransformPoint (0,0, localPositionZ [i].z) * (localPositionZ [i].y * weight));
				}
			}
			localPositionZ.Clear ();
			positionZ.Clear ();
			posZ = false;
		}
		if (updateSet) {
			t.position = finalVector;

		}
	}
		

	public void PositionX(float value)
	{
		positionX.Add (new Vector3 (value, defaultWeight, defaultType));
		posX = true;
	}
	public void SetPositionX(Vector3 value)
	{
		positionX.Add (value);
		posX = true;
	}

	public void PositionY(float value)
	{
		positionY.Add (new Vector3 (value, defaultWeight, defaultType));
		posY = true;
	}
	public void SetPositionY(Vector3 value)
	{
		positionY.Add (value);
		posY = true;
	}

	public void PositionZ(float value)
	{
		positionZ.Add (new Vector3 (value, defaultWeight, defaultType));
		posZ = true;
	}
	public void SetPositionZ(Vector3 value)
	{
		positionZ.Add (value);
		posZ = true;
	}


	public void LocalPositionX(float value)
	{
		localPositionX.Add (new Vector3 (value, defaultWeight, defaultType));
		posX = true;
	}
	public void SetLocalPositionX(Vector3 value)
	{
		localPositionX.Add (value);
		posX = true;
	}

	public void LocalPositionY(float value)
	{
		localPositionY.Add (new Vector3 (value, defaultWeight, defaultType));
		posY = true;
	}
	public void SetLocalPositionY(Vector3 value)
	{
		localPositionY.Add (value);
		posY = true;
	}

	public void LocalPositionZ(float value)
	{
		localPositionZ.Add (new Vector3 (value, defaultWeight, defaultType));
		posZ = true;
	}
	public void SetLocalPositionZ(Vector3 value)
	{
		localPositionZ.Add (value);
		posZ = true;
	}



	///////////////////////////////////////////////////////////////////////////////	



	public void RotationX(float value)
	{
		rotationX.Add (new Vector3 (value, defaultWeight, defaultType));
		rotX = true;
	}
	public void SetRotationX(Vector3 value)
	{
		rotationX.Add (value);
		rotX = true;
	}

	public void RotationY(float value)
	{
		rotationY.Add (new Vector3 (value, defaultWeight, defaultType));
		rotY = true;
	}
	public void SetRotationY(Vector3 value)
	{
		rotationY.Add (value);
		rotY = true;
	}

	public void RotationZ(float value)
	{
		rotationZ.Add (new Vector3 (value, defaultWeight, defaultType));
		rotZ = true;
	}
	public void SetRotationZ(Vector3 value)
	{
		rotationZ.Add (value);
		rotZ = true;
	}


	public void LocalRotationX(float value)
	{
		localRotationX.Add (new Vector3 (value, defaultWeight, defaultType));
		rotX = true;
	}
	public void SetLocalRotationX(Vector3 value)
	{
		localRotationX.Add (value);
		rotX = true;
	}

	public void LocalRotationY(float value)
	{
		localRotationY.Add (new Vector3 (value, defaultWeight, defaultType));
		rotY = true;
	}
	public void SetLocalRotationY(Vector3 value)
	{
		localRotationY.Add (value);
		rotY = true;
	}

	public void LocalRotationZ(float value)
	{
		localRotationZ.Add (new Vector3 (value, defaultWeight, defaultType));
		rotZ = true;
	}
	public void SetLocalRotationZ(Vector3 value)
	{
		localRotationZ.Add (value);
		rotZ = true;
	}



	///////////////////////////////////////////////////////////////////////////////	



	public void ScaleX(float value)
	{
		scaleX.Add (new Vector3 (value, defaultWeight, defaultType));
		sclX = true;
	}
	public void SetScaleX(Vector3 value)
	{
		scaleX.Add (value);
		sclX = true;
	}

	public void ScaleY(float value)
	{
		scaleY.Add (new Vector3 (value, defaultWeight, defaultType));
		sclY = true;
	}
	public void SetScaleY(Vector3 value)
	{
		scaleY.Add (value);
		sclY = true;
	}

	public void ScaleZ(float value)
	{
		scaleZ.Add (new Vector3 (value, defaultWeight, defaultType));
		sclZ = true;
	}
	public void SetScaleZ(Vector3 value)
	{
		scaleZ.Add (value);
		sclZ = true;
	}


	public void LocalScaleX(float value)
	{
		localScaleX.Add (new Vector3 (value, defaultWeight, defaultType));
		sclX = true;
	}
	public void SetLocalScaleX(Vector3 value)
	{
		localScaleX.Add (value);
		sclX = true;
	}

	public void LocalScaleY(float value)
	{
		localScaleY.Add (new Vector3 (value, defaultWeight, defaultType));
		sclY = true;
	}
	public void SetLocalScaleY(Vector3 value)
	{
		localScaleY.Add (value);
		sclY = true;
	}

	public void LocalScaleZ(float value)
	{
		localScaleZ.Add (new Vector3 (value, defaultWeight, defaultType));
		sclZ = true;
	}
	public void SetLocalScaleZ(Vector3 value)
	{
		localScaleZ.Add (value);
		sclZ = true;
	}


	//////////////////////////////////////////////////////////////////////


	void SetSide(ExtensionMethods.BoundSides side, Vector3 value)
	{
		anchors = new Vector3[3];
		localBounds = t.LocalBounds (includeChildrenInLocalBounds);
		Bounds  newBounds = ExtensionMethods.SetSide(localBounds, side, value.x);
		Vector3 localScale = t.localScale;
		localScale.x = newBounds.size.x * localScale.x / localBounds.size.x;
		localScale.y = newBounds.size.y * localScale.y / localBounds.size.y;
		localScale.z = newBounds.size.z * localScale.z / localBounds.size.z;
		Vector3 originalScale = t.localScale;
		//Debug.Log ("Scale Before: " + originalScale.ToString () + "  Scale After: " + localScale.ToString ());
		t.localScale = localScale;
		//Debug.Log ("After rescaling, our new bounds are " + newLocalBounds.ToString ());
		localBounds = t.LocalBounds(includeChildrenInLocalBounds);
		Vector3  relativePosition = t.TransformDirection (localBounds.center), relativeNew =  t.TransformDirection(newBounds.center),
		newPos = t.position + relativeNew - relativePosition;

		//anchors [2] = relativePosition;
		//anchors [3] = relativeNew;
		//anchors [4] = newPos;
		Debug.Log (newPos);
		t.localScale = originalScale;

		switch (side) {
		case ExtensionMethods.BoundSides.Front:
		case ExtensionMethods.BoundSides.Back:
			localScaleZ.Add (new Vector3 (localScale.z, value.y, value.z));
			//localPositionZ.Add (new Vector3 (newPos.z, value.y, value.z));
			sclZ = true;
			break;
		case ExtensionMethods.BoundSides.Left:
		case ExtensionMethods.BoundSides.Right:
			localScaleX.Add (new Vector3 (localScale.x, value.y, value.z));
			//localPositionX.Add (new Vector3 (newPos.x, value.y, value.z));
			sclX = true;
			break;
		case ExtensionMethods.BoundSides.Top:
		case ExtensionMethods.BoundSides.Bottom:
			localScaleY.Add (new Vector3 (localScale.y, value.y, value.z));
			//localPositionY.Add (new Vector3 (newPos.y, value.y, value.z));
			sclY = true;
			break;
		}
		if(newPos.x != t.position.x){positionX.Add(new Vector3 (newPos.x, value.y, value.z)); posX = true;}
		if(newPos.y != t.position.y){positionY.Add(new Vector3 (newPos.y, value.y, value.z)); posY = true;}
		if(newPos.z != t.position.z){positionZ.Add(new Vector3 (newPos.z, value.y, value.z)); posZ = true;}
	}
	void MoveSide(ExtensionMethods.BoundSides side, Vector3 value)
	{
		if (Mathf.Abs(value.x) <= 0.01f){return;}
	
		localBounds = t.LocalBounds (includeChildrenInLocalBounds);
		Bounds  newBounds = ExtensionMethods.MoveSide(localBounds, side, value.x);
	
		Vector3 localScale = t.localScale;
		localScale.x = newBounds.size.x * localScale.x / localBounds.size.x;
		localScale.y = newBounds.size.y * localScale.y / localBounds.size.y;
		localScale.z = newBounds.size.z * localScale.z / localBounds.size.z;
		Vector3 originalScale = t.localScale;
		t.localScale = localScale;
	
		localBounds = t.LocalBounds(includeChildrenInLocalBounds);
		Vector3  relativePosition = t.TransformDirection (localBounds.center), 
		relativeNew =  t.TransformDirection(newBounds.center),
		newPos = t.position + relativeNew - relativePosition;

		t.localScale = originalScale;

		switch (side) {
		case ExtensionMethods.BoundSides.Front:
		case ExtensionMethods.BoundSides.Back:
			localScaleZ.Add (new Vector3 (localScale.z, value.y, value.z));
			sclZ = true;
			break;
		case ExtensionMethods.BoundSides.Left:
		case ExtensionMethods.BoundSides.Right:
			localScaleX.Add (new Vector3 (localScale.x, value.y, value.z));
			sclX = true;
			break;
		case ExtensionMethods.BoundSides.Top:
		case ExtensionMethods.BoundSides.Bottom:
			localScaleY.Add (new Vector3 (localScale.y, value.y, value.z));
			sclY = true;
			break;
		}
		if(newPos.x != t.position.x){positionX.Add(new Vector3 (newPos.x, value.y, value.z)); posX = true;}
		if(newPos.y != t.position.y){positionY.Add(new Vector3 (newPos.y, value.y, value.z)); posY = true;}
		if(newPos.z != t.position.z){positionZ.Add(new Vector3 (newPos.z, value.y, value.z)); posZ = true;}
	}

	public void Set_Front(float value)
	{
		SetSide (ExtensionMethods.BoundSides.Front, new Vector3(value, defaultWeight, defaultType));

	}
	public void Move_Front(float value)
	{
		MoveSide (ExtensionMethods.BoundSides.Front, new Vector3(value, defaultWeight, defaultType));

	}
	public void SetFront(Vector3 value){
		SetSide (ExtensionMethods.BoundSides.Front, value);
	}
	public void MoveFront(Vector3 value){
		MoveSide (ExtensionMethods.BoundSides.Front, value);
	}
	public Vector3 GetFront()
	{
		localBounds = t.LocalBounds (includeChildrenInLocalBounds);
		return t.position + t.TransformDirection(new Vector3(localBounds.center.x, localBounds.center.y, localBounds.max.z));
	}
	public float DistanceFromFront(Vector3 point)
	{
		Vector3 side = GetFront();
		float distance = Vector3.Distance(point, side);
		distance *= Vector3.Dot ((point-side).normalized, (side - GetCenter()).normalized);
		return  distance;
	}


	public void Set_Back(float value)
	{
		SetSide (ExtensionMethods.BoundSides.Back, new Vector3(value, defaultWeight, defaultType));

	}
	public void SetBack(Vector3 value){
		SetSide (ExtensionMethods.BoundSides.Back, value);
	}
	public void Move_Back(float value)
	{
		MoveSide (ExtensionMethods.BoundSides.Back, new Vector3(value, defaultWeight, defaultType));

	}
	public void MoveBack(Vector3 value){
		MoveSide (ExtensionMethods.BoundSides.Back, value);
	}
	public Vector3 GetBack()
	{
		localBounds = t.LocalBounds (includeChildrenInLocalBounds);
		return t.position + t.TransformDirection(new Vector3(localBounds.center.x, localBounds.center.y, localBounds.min.z));
	}
	public float DistanceFromBack(Vector3 point)
	{
		Vector3 side = GetBack();
		float distance = Vector3.Distance(point, side);
		distance *= Vector3.Dot ((point-side).normalized, (side - GetCenter()).normalized);
		return  distance;
	}


	public void Set_Left(float value)
	{
		SetSide (ExtensionMethods.BoundSides.Left, new Vector3(value, defaultWeight, defaultType));

	}
	public void SetLeft(Vector3 value){
		SetSide (ExtensionMethods.BoundSides.Left, value);
	}
	public void Move_Left(float value)
	{
		MoveSide (ExtensionMethods.BoundSides.Left, new Vector3(value, defaultWeight, defaultType));

	}
	public void MoveLeft(Vector3 value){
		MoveSide (ExtensionMethods.BoundSides.Left, value);
	}
	public Vector3 GetLeft()
	{
		localBounds = t.LocalBounds (includeChildrenInLocalBounds);
		return t.position + t.TransformDirection(new Vector3(localBounds.min.x, localBounds.center.y, localBounds.center.z));
	}
	public float DistanceFromLeft(Vector3 point)
	{
		Vector3 side = GetLeft();
		float distance = Vector3.Distance(point, side);
		distance *= Vector3.Dot ((point-side).normalized, (side - GetCenter()).normalized);
		return  distance;
	}


	public void Set_Right(float value)
	{
		SetSide (ExtensionMethods.BoundSides.Right, new Vector3(value, defaultWeight, defaultType));

	}
	public void SetRight(Vector3 value){
		
		SetSide (ExtensionMethods.BoundSides.Right, value);
	}
	public void Move_Right(float value)
	{
		MoveSide (ExtensionMethods.BoundSides.Right, new Vector3(value, defaultWeight, defaultType));

	}
	public void MoveRight(Vector3 value){

		MoveSide (ExtensionMethods.BoundSides.Right, value);
	}
	public Vector3 GetRight()
	{
		localBounds = t.LocalBounds (includeChildrenInLocalBounds);
		//Debug.Log ("Right Side: " + (t.position + t.TransformDirection(new Vector3(localBounds.max.x, localBounds.center.y, localBounds.center.z))).ToString());
		return t.position + t.TransformDirection(new Vector3(localBounds.max.x, localBounds.center.y, localBounds.center.z));
	}
	public float DistanceFromRight(Vector3 point)
	{
		Vector3 side = GetRight();
		float distance = Vector3.Distance(point, side);
		distance *= Vector3.Dot ((point-side).normalized, (side - GetCenter()).normalized);
		return  distance;
	}


	public void Set_Top(float value)
	{
		SetSide (ExtensionMethods.BoundSides.Top, new Vector3(value, defaultWeight, defaultType));

	}
	public void SetTop(Vector3 value){
		SetSide (ExtensionMethods.BoundSides.Top, value);
	}
	public void Move_Top(float value)
	{
		MoveSide (ExtensionMethods.BoundSides.Top, new Vector3(value, defaultWeight, defaultType));

	}
	public void MoveTop(Vector3 value){
		MoveSide (ExtensionMethods.BoundSides.Top, value);
	}
	public Vector3 GetTop()
	{
		localBounds = t.LocalBounds (includeChildrenInLocalBounds);
		return t.position + t.TransformDirection(new Vector3(localBounds.center.x, localBounds.max.y, localBounds.center.z));
	}
	public float DistanceFromTop(Vector3 point)
	{
		Vector3 side = GetTop();
		float distance = Vector3.Distance(point, side);
		distance *= Vector3.Dot ((point-side).normalized, (side - GetCenter()).normalized);
		return  distance;
	}


	public void Set_Bottom(float value)
	{
		SetSide (ExtensionMethods.BoundSides.Bottom, new Vector3(value, defaultWeight, defaultType));

	}
	public void SetBottom(Vector3 value){
		SetSide (ExtensionMethods.BoundSides.Bottom, value);
	}
	public void Move_Bottom(float value)
	{
		MoveSide (ExtensionMethods.BoundSides.Bottom, new Vector3(value, defaultWeight, defaultType));

	}
	public void MoveBottom(Vector3 value){
		MoveSide (ExtensionMethods.BoundSides.Bottom, value);
	}
	public Vector3 GetBottom()
	{
		localBounds = t.LocalBounds (includeChildrenInLocalBounds);
		return t.position + t.TransformDirection(new Vector3(localBounds.center.x, localBounds.min.y, localBounds.center.z));
	}
	public float DistanceFromBottom(Vector3 point)
	{
		Vector3 side = GetBottom();
		float distance = Vector3.Distance(point, side);
		distance *= Vector3.Dot ((point-side).normalized, (side - GetCenter()).normalized);
		return  distance;
	}


	public Vector3 GetCenter()
	{
		localBounds = t.LocalBounds (includeChildrenInLocalBounds);
		//Debug.Log ("Center: " + localBounds.center.ToString () + " | As Direction : " + t.TransformDirection (localBounds.center).ToString ());
		//Debug.Log("Get Center : " +(t.position + t.TransformDirection(localBounds.center)).ToString());
		return t.position + t.TransformDirection(localBounds.center);
	}

	public float DistanceFromCenter(Vector3 point)
	{
		Vector3 side = GetCenter();
		float distance = Vector3.Distance(point, side);
		return  distance;
	}

	//////////////////////////////////////////////////////////////////////

	public void LookAt(int value)
	{
		if (value >= 0 && value < binder.baseTargets.Length) {
			if (binder.baseTargets [value] is Component) {
				Transform lookTarget = ((Component)binder.baseTargets [value]).transform;
				LookAt (lookTarget, Vector3.zero, defaultWeight, defaultType);
			}
		}
	}
	void LookAt(Transform lookTarget, Vector3 lookingAngle, float weight, float type)
	{
		Vector3 lookRot, originalRot = t.eulerAngles;

		t.LookAt (lookTarget);
		lookRot = t.localEulerAngles;
		t.eulerAngles = originalRot;
		lookRot += lookingAngle;
		localRotationX.Add (new Vector3(lookRot.x, weight, type));
		localRotationY.Add (new Vector3(lookRot.y, weight, type));
		localRotationZ.Add (new Vector3(lookRot.z, weight, type));
		rotX = rotY = rotZ = true;
	}



	//////////////////////////////////////////////////////////////////////

	public Vector3 InverseTransformPointObjective(Vector3 point)
	{
		localBounds = t.LocalBounds (includeChildrenInLocalBounds);
		bounds = t.GlobalBounds (includeChildrenInGlobalBounds);

		Vector3 direction = (point - GetCenter());
		//Debug.Log (" Center("+GetCenter().ToString()  + ") + local space direction(" + t.InverseTransformDirection(direction).ToString() + " = " + (GetCenter() + t.InverseTransformDirection (direction)).ToString ());
		return t.InverseTransformDirection(direction) ;
	}

	public Vector3 GetAsWeightedVector(float val)
	{
		return new Vector3 (val, defaultWeight, defaultType);
	}
}


[System.Serializable]
public class LayoutSet
{
	public string name;
	public int[] bindings;
}
