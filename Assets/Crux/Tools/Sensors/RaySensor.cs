using UnityEngine;
using System.Collections;

public class RaySensor : BaseSensor {

	public bool fireOnStart;
	public float rayLength, fireRate;
	public Vector3 origin, direction;
	public Space originSpace, directionSpace;

	public SensorEvent onFire;
	public SensorEvent[] onHit = new SensorEvent[0];
	public LayerHit[] onLayerHit = new LayerHit[0];

	//For each object in onHit, we can hit one object, passing through that one to subsequent hits until we've reached the limit of our list.
	//For each layer, we'll have a corresponding onLayerHit. If two numbers in layers match, the first event applies to the first hit on that layer, 
	//and the second to the next hit with that layer, etc...

	private bool firing;

	//These two are always the same length, but a lot of the items in lastHitSensors will be null.
	//We call OnRayStart, OnRayStay, and OnRayStop on any sensors its got, as appropriate, but only up to maxSensorHits.
	public GameObject[] lastHitObjects = new GameObject[0];
	public RaySensor[] lastHitSensors = new RaySensor[0];
	public int maxSensorHits;
	void Start()
	{
		if (fireOnStart) {
			StartCoroutine (CastRay ());
		}
			
	}
	public void FireRay()
	{
		StartCoroutine (CastRay ());
	}

	public void StopRay()
	{
		firing = false;
	}
	public IEnumerator CastRay()
	{
		firing = true;
		float refireTime = 0;
		while (firing) {
			
				//Fire Ray
			onFire.callBack.Invoke();
			Vector3 rO;
			rO = new Vector3 ();
			switch (originSpace) {
			case Space.Self:
				rO = transform.TransformPoint (origin);
				break;
			case Space.World:
				rO = origin;
				break;
			}

			Vector3 rD;
			rD = new Vector3 ();
			switch (directionSpace) {
			case Space.Self:
				rD = transform.TransformDirection(direction);
				break;
			case Space.World:
				rD = direction;
				break;
			}

			int i, j;

			Ray ray;
			ray = new Ray (rO, rD);

			RaycastHit[] hits = Physics.RaycastAll (ray, rayLength);
			bool[] inList = new bool[hits.Length];
			for (i = lastHitObjects.Length-1; i >=0; i--) {
				bool notInList = true;
				for (j = hits.Length - 1; j >= 0; j--) {

					if (hits [j].collider.gameObject == lastHitObjects [i]) {
						notInList = false;
						inList [j] = true; 
						if (lastHitSensors [i] != null) {
							lastHitSensors [i].OnRayStay (this);
						}
					}
				}
				if (notInList && lastHitSensors [i] != null) {
					lastHitSensors [i].OnRayExit (this);
				}
			}

			lastHitObjects = new GameObject[hits.Length];
			lastHitSensors = new RaySensor[hits.Length];
			int hC = 0;
			for (i = 0; i < hits.Length; i++) {
				lastHitObjects [i] = hits [i].collider.gameObject;
				lastHitSensors [i] = hits [i].collider.gameObject.GetComponent<RaySensor> ();
				if (hC < onHit.Length) {
				onHit[hC].CallBack (lastHitObjects[i].tag);
					hC++;
				}
				if (!inList[i] && lastHitSensors [i] != null) {
					lastHitSensors [i].OnRayEnter (this);
				}
				for (j = 0; j < onLayerHit.Length; j++) {
					if (lastHitObjects [i].layer ==onLayerHit[j].layer){
						onLayerHit [j].Hit (lastHitObjects[i].tag);
					}
				}
			}
			for (j = 0; j < onLayerHit.Length; j++) {
				onLayerHit [j].counter = 0;
			}
			yield return new WaitForSeconds(refireTime);
		}
	}


	public void OnRayEnter(RaySensor hitSource)
	{
		if (CheckActive ()) {
			events [0].CallBack (hitSource.gameObject.tag);
		}
	}

	public void OnRayStay(RaySensor hitSource)
	{
		if (CheckActive ()) {
			events [1].CallBack (hitSource.gameObject.tag);
		}
	}

	public void OnRayExit(RaySensor hitSource)
	{
		if (CheckActive ()) {
			events [2].CallBack (hitSource.gameObject.tag);
		}
	}

}

[System.Serializable]
public class LayerHit
{
	public int layer, counter;
	public SensorEvent[] layerHits;

	public void Hit(string tag)
	{
		if (counter < layerHits.Length) {
			layerHits [counter].CallBack(tag);
			counter++;
		}
	}
}