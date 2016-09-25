
using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class TriggerSensor : BaseSensor
{
	List<Collider> collidersInTrigger = new List<Collider>();

	void OnTriggerEnter(Collider col)
	{
		if(CheckActive()){events[0].CallBack (col.gameObject.tag);}
	}
	void OnTriggerStay(Collider col)
	{
		if(CheckActive()){events[1].CallBack (col.gameObject.tag);}
	}
	void OnTriggerExit(Collider col)
	{
		if(CheckActive()){events[2].CallBack (col.gameObject.tag);}
	}

	public T[] GetAllWithin<T>(bool includeChildren = false){
		List<T> comps = new List<T> ();
		for (int i = 0; i < collidersInTrigger.Count; i++) {
			T[] c; 
			if (!includeChildren) {
				c = collidersInTrigger [i].GetComponents<T> ();
			} else {
				c = collidersInTrigger [i].GetComponentsInChildren<T> ();
			}
			comps.AddRange (c);
		}
		return comps.ToArray ();
	}

}

public class Trigger2DSensor : BaseSensor
{
	List<Collider2D> collider2DsInTrigger = new List<Collider2D>();

	void OnTriggerEnter2D(Collider2D col)
	{
		if(CheckActive()){events[0].CallBack (col.gameObject.tag);}
		collider2DsInTrigger.Add (col);
	}
	void OnTriggerStay2D(Collider2D col)
	{
		if(CheckActive()){events[1].CallBack (col.gameObject.tag);}
	}
	void OnTriggerExit2D(Collider2D col)
	{
		if(CheckActive()){events[2].CallBack (col.gameObject.tag);}
		collider2DsInTrigger.Remove (col);
	}

	public T[] GetAllWithin<T>(bool includeChildren = false){
		List<T> comps = new List<T> ();
		for (int i = 0; i < collider2DsInTrigger.Count; i++) {
			T[] c; 
			if (!includeChildren) {
				c = collider2DsInTrigger [i].GetComponents<T> ();
			} else {
				c = collider2DsInTrigger [i].GetComponentsInChildren<T> ();
			}
			comps.AddRange (c);
		}
		return comps.ToArray ();
	}
}

