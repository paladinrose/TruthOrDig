using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class CollisionSensor : BaseSensor {

	void OnCollisionEnter(Collision col)
	{
		if(CheckActive()){events[0].CallBack (col.gameObject.tag);}
	}
	void OnCollisionStay(Collision col)
	{
		if(CheckActive()){events[1].CallBack (col.gameObject.tag);}
	}
	void OnCollisionExit(Collision col)
	{
		if(CheckActive()){events[2].CallBack (col.gameObject.tag);}
	}
}

public class Collision2DSensor : BaseSensor {

	void OnCollisionEnter2D(Collision2D col)
	{
		if(CheckActive()){events[0].CallBack (col.gameObject.tag);}
	}
	void OnCollisionStay2D(Collision2D col)
	{
		if(CheckActive()){events[1].CallBack (col.gameObject.tag);}
	}
	void OnCollisionExit2D(Collision2D col)
	{
		if(CheckActive()){events[2].CallBack (col.gameObject.tag);}
	}
}