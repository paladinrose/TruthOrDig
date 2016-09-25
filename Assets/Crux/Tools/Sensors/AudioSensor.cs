using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class AudioSensor : BaseSensor
{
	public AudioSource[] sounds;
	bool soundsArePlaying;
	//For each sound, we keep four events in our events list:
	//0 = OnPlay
	//1 = OnPlaying
	//2 = OnLoop
	//3 = OnEnd

	public List<ListenerContainer> listeners = new List<ListenerContainer>();
	List<int> toRemove = new List<int> ();
	public bool listening;
	
	public AudioSensorEvent onStartHear, onHear, onStopHear; 

	public float recallOnHearTimer;

	void OnTriggerEnter(Collider col)
	{
		AudioSensor aS = col.GetComponent<AudioSensor> ();
		if (aS != null) {
			if (aS.listening) {
				bool isNew = true;
				for (int i = listeners.Count - 1; i >= 0; i--) {
					if (aS == listeners [i].listener) {
						isNew = false;
					}
				}
				if(isNew){
					ListenerContainer newListener = new ListenerContainer (aS, sounds.Length);
					listeners.Add (newListener);
					if (listeners.Count == 1) {
						StartCoroutine (UpdateListeners ());
					}
				}
			}
		}
	}

	IEnumerator UpdateListeners(){
		while(listeners.Count >0 || soundsArePlaying){
			bool rem = false;
			for(int j = listeners.Count -1; j >=0; j--){
				if (rem) {
					listeners.RemoveAt (j+1);
				}
				rem = false;
				soundsArePlaying = false;
				if (listeners [j].listener.listening) {
					for (int i = sounds.Length - 1; i >= 0; i--) {
						if (sounds [i].isPlaying == false && listeners [j].canHear [i] == false) {
							break;
						} else {
							if (sounds [i].isPlaying) {
								soundsArePlaying = true;
								float soundDist = Vector3.Distance (listeners [j].listener.transform.position, sounds [i].transform.position);
								if (soundDist <= sounds [i].maxDistance) {
									if (!listeners [j].canHear [i]) {
										listeners [j].currentUpdateTimer = 0;
										listeners [j].listener.onStartHear.Invoke (this, i);
										listeners [j].canHear [i] = true;
									} else {
										listeners [j].currentUpdateTimer += Time.deltaTime;
										if (listeners [j].currentUpdateTimer >= recallOnHearTimer) {
											listeners [j].currentUpdateTimer -= recallOnHearTimer;
											listeners [j].listener.onHear.Invoke (this, i);
										}
										listeners [j].listener.onHear.Invoke (this, i);
									}
								} else if (listeners [j].canHear [i]) {
									listeners [j].listener.onStopHear.Invoke (this, i);
									listeners [j].canHear [i] = false;
								}
							} else if (listeners [j].canHear [i]) {
								listeners [j].listener.onStopHear.Invoke (this, i);
								listeners [j].canHear [i] = false;
							}
						}
					}
				}
					
				for (int i = toRemove.Count - 1; i >= 0; i--) {
					if (toRemove [i] == j) {
						rem = true;
					}
				}
			}
			if (rem) {
				listeners.RemoveAt (0);
			}
			toRemove = new List<int> ();
			yield return null;
		}
	}

	void OnTriggerExit(Collider col)
	{
		AudioSensor aS = col.GetComponent<AudioSensor> ();
		if (aS != null) {
			int isNew = -1;
			for (int i = listeners.Count - 1; i >= 0; i--) {
				if (aS == listeners [i].listener) {
					isNew = i;
				}
			}
			if(isNew>=0){
				toRemove.Add (isNew);
			}
		}
	}

		
	public void PlaySound(int id)
	{
		if(CheckActive() && id >=0 && id < sounds.Length){
			events [id * 4].callBack.Invoke ();	
			sounds [id].Play ();
			if (!soundsArePlaying) {
				StartCoroutine (UpdateListeners ());
			}
		}
	}
	
	public void StopSound(int id)
	{
		if(id >=0 && id < sounds.Length ){
			if (sounds [id].isPlaying) {
				sounds [id].Stop ();	
				events [id * 4 + 3].callBack.Invoke ();	
			}
		}
	}
}

[System.Serializable]
public class ListenerContainer
{
	public AudioSensor listener;
	public bool[] canHear;
	public float currentUpdateTimer;

	public ListenerContainer(AudioSensor l, int stateLength){
		listener = l;
		canHear = new bool[stateLength];
	}
}

[System.Serializable]
public class AudioSensorEvent : UnityEvent<AudioSensor, int>{
	
}