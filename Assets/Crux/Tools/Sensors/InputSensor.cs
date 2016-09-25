using UnityEngine;
using System.Collections;

public class InputSensor : BaseSensor {

	public KeyCode[] inputs = new KeyCode[0]; 
	public uint[] inputStates = new uint[0];
	public float inputTimer = 0.1f;
	private bool checkInput = false;
	void Start()
	{
		if (active) {
			StartCoroutine (CheckInput ());
		} else {
			checkInput = false;
		}
	}
	override public void SetActive(bool a)
	{
		active = a;
		if (active) {
			StartCoroutine (CheckInput ());
		} else {
			checkInput = false;
		}
	}
	// Instead of Update, I should make this a more independent function and use it in my own loops.
	IEnumerator CheckInput () {
		checkInput = true;
		while(checkInput){
			if (CheckActive ()) {
				for (int i = inputStates.Length-1; i >=0; i--) {
					int j = i * 3;

					switch(inputStates[i])
					{
					//Not pressed, last we checked.
					case 0:
						//Pressed now, or within our last timer.
						if (Input.GetKeyDown (inputs [i]) || Input.GetKey (inputs [i])) {
							events [j].callBack.Invoke ();
							inputStates [i] = 1;
						}else if(Input.GetKeyUp(inputs[i])){
							//Pressed and released in less than our inputTimer
							events[j].callBack.Invoke();
							inputStates [i] = 2;
						}
						break;

					//Pressed and held, last we checked.
					case 1:
						if(Input.GetKey(inputs[i])){
							events[j+1].callBack.Invoke();
						}else{
							events[j+2].callBack.Invoke();
							inputStates[i] = 0;
						}
						break;

					//Pressed and released in less than one timer, forced wait.
					case 2:
						events [j + 2].callBack.Invoke ();
						inputStates [i] = 0;
						break;
					}
				}
			}
			yield return new WaitForSeconds (inputTimer);
		}
	}
}
