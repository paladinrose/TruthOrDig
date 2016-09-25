using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class BaseSensor : MonoBehaviour {


	public static bool[] globalLayerStates = new bool[32];
	public bool active = true, useLayerSorting;

	//Implement Tag sorting.
	public virtual void SetActive(bool a)
	{
		active = a;
	}

	public void ActivateLayer(int l)
	{
		globalLayerStates [l] = true;
	}
	public void DeactivateLayer(int l)
	{
		globalLayerStates [l] = false;
	}
	public void ToggleLayer(int l)
	{
		if (globalLayerStates [l]) {
			globalLayerStates [l] = false;
		} else {
			globalLayerStates [l] = true;
		}
	}

	public SensorEvent[] events;
	public bool CheckActive()
	{
		if (active){ 
			if(!useLayerSorting){return true;} 
			else if(globalLayerStates [gameObject.layer]) {return true;}
		}
		return false;
	}
}

[System.Serializable]
public class SensorEvent {
	public UnityEvent callBack;
	public string[] tags = new string[0];

	public bool CheckTag(string s)
	{
		if (tags.Length == 0) {
			return true;
		}else {
			for (int i = tags.Length - 1; i >= 0; i--) {
				if (tags [i] == s) {
					return true;
				}
			}
		}	   
		return false;
	}

	public void CallBack(string s)
	{
		if(CheckTag(s)){callBack.Invoke ();}
	}
}