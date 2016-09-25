using UnityEngine;
using System.Collections;

public class PressurePlate : ContraptionBase {

	public float weightLimit = 2f;
	public float currentWeight = 0f;

	public ContraptionBase[] onDown = new ContraptionBase[0], onUp = new ContraptionBase[0];

	void OnTriggerEnter2D(Collider2D c)
	{
		float cw = currentWeight + c.attachedRigidbody.mass * -Physics.gravity.y;
		if (currentWeight < weightLimit && cw >= weightLimit) {
			for(int i = onDown.Length-1; i >=0; i--){
				onDown[i].enabled = true;
				onDown[i].Activate(0);
			}
		} 
		currentWeight = cw;
	}
	void OnTriggerExit2D(Collider2D c)
	{
		float cw = currentWeight - c.attachedRigidbody.mass * -Physics.gravity.y;
		if (currentWeight >= weightLimit && cw < weightLimit) {
			for(int i = onUp.Length-1; i >=0; i--){
				onUp[i].enabled = true;
				onUp[i].Activate(1);
			}
		} 
		currentWeight = cw;
	}
}
