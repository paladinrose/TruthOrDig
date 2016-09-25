using UnityEngine;
using System.Collections;

public class Spring : ContraptionBase {

	public SpringJoint2D joint;
	public Transform springGraphic;
	public float tensileStrength;

	Vector2 origSize;
	void Start()
	{
		origSize.x =springGraphic.localScale.x;
		origSize.y =springGraphic.localScale.y;
	}
	void Update()
	{
		//We start matching our scale and rotation, so that the spring appears to be authentically sproinging.
		//1. Get the distance between the two points
		//2. Set the springGraphic.localScale.y to that length
		if (springGraphic) {
			Vector3 vec = springGraphic.localScale;
			vec.y = Vector2.Distance (joint.transform.position, joint.connectedBody.transform.position);
			vec.x = origSize.x * (origSize.y / vec.y);
			springGraphic.localScale = vec;
			//3. Position directly between the two
			vec = joint.transform.position + joint.connectedBody.transform.position;
			vec *= 0.5f;
			springGraphic.position = vec;
			//4. Find degrees of rotation from anchor to connectedAnchor
			//5. Set springGraphic.eulerAngles.z to that angle.
			vec.x = vec.y = 0;
			vec = (Vector3)joint.connectedBody.position - joint.transform.position;
			vec.z = Mathf.Atan2 (vec.y, vec.x) * Mathf.Rad2Deg - 90;
			vec.x = vec.y = 0;
			springGraphic.eulerAngles = vec;
		}

		//Now, we check to see if we've exceeded our tensileStrength, at which point we should break.
		if (tensileStrength > 0) {
			Vector2 force = joint.GetReactionForce (Time.deltaTime);
			float tForce = Mathf.Abs(force.x)+Mathf.Abs(force.y);
			if(tForce > tensileStrength)
			{
				//SNAP!!!
			}
		}
	}
}
