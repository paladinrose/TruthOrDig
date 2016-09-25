using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;

public class OmniRoutine{
	//public static Needle omniThread;


	public List<IEnumerator> routines;
	public List<OmniRoutine> children;
	public bool isRunning;

	public OmniRoutine()
	{
		routines = new List<IEnumerator>();
		isRunning = false;
	}

	/*
	public static OmniRoutine StartOmniRoutine(this MonoBehaviour target, IEnumerator routine)
	{
		OmniRoutine omni = new OmniRoutine();

		omni.routines.Add(routine);
		if(!omni.isRunning){
			omni.isRunning = true;
			if(!Application.isEditor){
				target.StartCoroutine(omni.UpdateRoutine());
			} else {
				#if UNITY_EDITOR
				EditorApplication.update += omni.UpdateRoutine;
				#endif
			}

		}
		return omni;
	}
*/
	public IEnumerator UpdateRoutine()
	{
		while(isRunning)
		{
			Update();
			yield return null;
		}

	}
	public OmniRoutine StartOmniRoutine(IEnumerator route, MonoBehaviour target)
	{
		OmniRoutine omni = target.StartOmniRoutine (route);
		children.Add (omni);
		return omni;
	}
	public void Update()
	{
		isRunning = false;
		if(routines.Count >0){
			isRunning = true;
			for(int i = routines.Count-1; i >=0; i--){
				if(!routines[i].MoveNext()){
					routines.RemoveAt(i);
				}
			}
		} 
		if (children.Count > 0) {
			isRunning = true;
			for(int i = children.Count-1; i >=0; i--){
				if(!children[i].isRunning){
					children.RemoveAt(i);
				}
			}
		}

	}

	public void Stop()
	{
		int i;
		for(i = routines.Count-1; i >=0; i--){
			routines.RemoveAt(i);
		}
		for (i = children.Count - 1; i >= 0; i--) {
			children [i].Stop ();
		}
		if(Application.isEditor){
			#if UNITY_EDITOR
			EditorApplication.update -= Update;
			#endif

		}
	}
}
public class OmniWait: CustomYieldInstruction
{
	private float waitTime;
	public override bool keepWaiting{
		get { if(!Application.isEditor){
				return Time.realtimeSinceStartup < waitTime;
			}
			#if UNITY_EDITOR
			else {
			return (float)EditorApplication.timeSinceStartup < waitTime;
			}
			#endif 
		}
	}
	public OmniWait(float time){
		waitTime =0;
		if(!Application.isEditor){
			waitTime = Time.realtimeSinceStartup + time;
		}
		#if UNITY_EDITOR
		else {

			waitTime = (float)EditorApplication.timeSinceStartup + time;
		}
		#endif
	}
}

