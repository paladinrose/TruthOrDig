using System;
using System.Collections;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
public class EditorCoroutine
{
	public static EditorCoroutine StartEditorCoroutine( IEnumerator _routine )
	{
		EditorCoroutine coroutine = new EditorCoroutine(_routine);
		coroutine.start();
		return coroutine;
	}


	readonly IEnumerator routine;
	public bool complete;
	public EditorCoroutine( IEnumerator _routine )
	{
		routine = _routine;
		//start ();
	}
	
	void start()
	{
		#if UNITY_EDITOR
		EditorApplication.update += update;
		#endif
		complete = false;
	}
	public void Stop()
	{
		#if UNITY_EDITOR
		EditorApplication.update -= update;
		#endif
		complete = true;
	}
	
	void update()
	{
		if (!routine.MoveNext())
		{
			Stop();
		}
	}
}

public class EditorWaitForSeconds : IEnumerator
{
	float time, startTime, currentTime;
	public EditorWaitForSeconds(float t)
	{
		startTime = Time.realtimeSinceStartup;
		time = startTime + t;

	}
	public bool MoveNext() {
		currentTime = Time.realtimeSinceStartup;
		if (currentTime >= time) {
			return true;
		}
		return false;
	}

	public void Reset ()
	{
		time = currentTime + (time - startTime);
		startTime = currentTime;
	}

	public object Current {
		get {
			return currentTime;
		}
	}
}


