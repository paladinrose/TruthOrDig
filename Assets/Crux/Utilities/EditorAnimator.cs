using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class EditorAnimator {

	public Animator animator;
	//For now, let's assume we don't need an actual animation component to make this method work. Sure would save some pointless effort...
	//public Animation animation;
	#if UNITY_EDITOR
	public UnityEditor.Animations.AnimatorController controller;
	#endif

	public EditorAnimator(Animator a)
	{
		animator = a;
		/*
		animation = animator.gameObject.GetComponent<Animation>(); 
		if(!animation){animation = animator.gameObject.AddComponent<Animation>();}
		animation.hideFlags = HideFlags.HideInInspector;
		*/
		#if UNITY_EDITOR
		controller = animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
		#endif
	}
	#if UNITY_EDITOR
	public IEnumerator Animate(string clipName, bool deepScan = false)
	{
		UnityEditor.Animations.AnimatorState clipState = null;
		AnimationClip clip = null;

		for(int j = controller.layers.Length-1; j >=0; j--){
			
			UnityEditor.Animations.AnimatorStateMachine sm = controller.layers[j].stateMachine;
			
			for(int l = sm.anyStateTransitions.Length-1; l >=0; l--){
				for(int m = sm.anyStateTransitions[l].conditions.Length-1; m>=0; m--){
					if(sm.anyStateTransitions[l].conditions[m].parameter == clipName){
						clipState = sm.anyStateTransitions[l].destinationState;
						if(clipState.motion is AnimationClip){clip = clipState.motion as AnimationClip;}
						else {clipState = null;}
					}
				}
			}
		}

		if(clip != null){
			float  currentTime = 0, time, speed;
			double deltaTime, timeSinceStartup;
			timeSinceStartup = EditorApplication.timeSinceStartup;

			time = clip.length;
			speed = clipState.speed;
			if(clipState.speedParameterActive){
				for(int v = controller.parameters.Length-1; v >=0; v--){
					if(controller.parameters[v].name == clipState.speedParameter){
						speed*=controller.parameters[v].defaultFloat;
					}
				}
			}
			
			while(currentTime <=time){
				deltaTime = EditorApplication.timeSinceStartup - timeSinceStartup;
				currentTime += (float)deltaTime*speed;
				clip.SampleAnimation(animator.gameObject, currentTime);
				yield return null;
			}
		}
	}
	#endif
}
