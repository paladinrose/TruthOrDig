using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

public class InEditorAnimator : MonoBehaviour {
	#if UNITY_EDITOR
	public EditorAnimator editorAnimator;
	public Animator animator;
	public EditorCoroutine currentAnimation;

	public bool animatorIsSetup = false;
	public void SetupInEditorAnimator()
	{
		if(!animator){
			animator = gameObject.GetComponent<Animator>();
		}

		if(animator){
			animatorIsSetup = true;
			if(!animator){
				animator = gameObject.GetComponent<Animator>();
			}
			editorAnimator = new EditorAnimator(animator);
		}
	}
	public void PlayClip(string clipName)
	{
		if(currentAnimation != null){
			if(!currentAnimation.complete){currentAnimation.Stop();}
		}

		currentAnimation = EditorCoroutine.StartEditorCoroutine(editorAnimator.Animate(clipName));
	}
	#endif
}
