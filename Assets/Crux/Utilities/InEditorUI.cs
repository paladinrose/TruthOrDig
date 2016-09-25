using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class InEditorUI
{
	InEditorSelectable[] selectables = new InEditorSelectable[0];
	int length;
	public int mouseOver;

	public Camera drawCamera = null;

	public Vector2 mousePosition, mouseDelta;
	public float mouseClickTimer = 0.5f;
	public int mouseClickID;
	public bool mouseClick;
	#if UNITY_EDITOR
	public void DrawUI(Rect area)
	{
		/* Features to really round this thing out and get it completely squared away:
		 * All of the IPointer Handlers. Don't descriminate based on Type. Find out what OnPointerEvents they have events for and grab those events.
		 * 
		*/

		Handles.DrawCamera (area, drawCamera, DrawCameraMode.Normal);
		/*
		for(int i = length-1; i >=0; i--){
			Rect checkRect = new Rect(0,0,rects[i].rect.width, rects[i].rect.height);
			Vector2 rC = rects[i].localPosition
			rC.y *=-1;
			//rC = rects[i].TransformPoint(rC);
			rC.x += area.width*0.5f;
			rC.y += area.height*0.5f;

			checkRect.center = rC;
			GUI.Box (checkRect, rC.ToString());
		}
		*/
		Event e = Event.current;
		Vector2 evPos = e.mousePosition;
		evPos.y = area.height-evPos.y;


		//GUI.Box (new Rect(area.width-(area.width*0.4f), 0, area.width*0.4f, 20), area.ToString());
		//GUI.Box (new Rect(area.width-(area.width*0.2f), 20, area.width*0.2f, 20), evPos.ToString());
		if(evPos != mousePosition){
			mouseDelta = evPos - mousePosition;
			if(mouseOver >=0){
				if(!CheckPoint(evPos, mouseOver)){
					Animate(mouseOver, 1);
					mouseOver = -1;
				} 
			}
			
			if(mouseOver < 0){
				for(int i = length-1; i >=0; i--){
					if(CheckPoint(evPos, i) && selectables[i].source.interactable){
						//Debug.Log ("Over " + selectables[i].gameObject.name);
						mouseOver = i;
						Animate(i, 0);
					}
				}
			}
			mousePosition = evPos;
		}
		if (e.isMouse && mouseOver >= 0) {
			switch (e.type) {
			case EventType.MouseDown:
				Animate (mouseOver, 2);
				EditorCoroutine.StartEditorCoroutine (MouseClick ());
			//Debug.Log ("Mouse Down " + mouseOver);
				mouseClickID = mouseOver;
				selectables[mouseOver].MouseDown();
				break;
			
			case EventType.MouseUp:
				Animate (mouseClickID, 3);
				selectables[mouseClickID].MouseUp();
				if (mouseClick && mouseOver == mouseClickID) {selectables[mouseOver].MouseClick();}
				else if(mouseOver != mouseClickID){selectables[mouseOver].MouseDrop();}
				break;

			case EventType.MouseDrag:
				selectables[mouseClickID].MouseDrag(mouseDelta);
				break;
			}
		}
		if (e.isKey) {
			for(int k = length; k >=0; k--)
			{
				selectables[k].HandleKeyEvent(e);
			}
		}
	}
	
	public IEnumerator MouseClick()
	{
		double tss = EditorApplication.timeSinceStartup, delta;
		mouseClick = true;
		float mouseTime = 0;
		while(mouseTime <= mouseClickTimer)
		{
			delta = EditorApplication.timeSinceStartup - tss;
			mouseTime += (float)delta;
			yield return null;
		}
		mouseClick = false;
	}

	public bool CheckPoint(Vector2 point, int r)
	{
		//Rect checkRect = new Rect(0,0,rects[r].rect.width, rects[r].rect.height);
		//checkRect.center = rects[r].localPosition;
		point.y -= selectables[r].rect.rect.height;
		return RectTransformUtility.RectangleContainsScreenPoint(selectables[r].rect, point, drawCamera);
	}
	
	public void SetSelectables(Selectable[] s)
	{
		mouseOver = -1;
		length = s.Length;
		selectables = new InEditorSelectable[length];
		for(int i = length-1; i >=0; i--){
			selectables[i] = new InEditorSelectable(s[i], this);
		}
	}
	public void AddSelectable(Selectable s){
		Debug.Log ("Adding " + s.ToString ());
		List<InEditorSelectable> newSelectables = new List<InEditorSelectable> (selectables);
		InEditorSelectable ss = new InEditorSelectable (s, this);
		newSelectables.Add (ss);
		selectables = newSelectables.ToArray ();
	}
	public void Animate(int i, int transition)
	{
		if(selectables[i].currentAnimation != null){
			if(!selectables[i].currentAnimation.complete){selectables[i].currentAnimation.Stop ();}
		}

		switch(selectables[i].source.transition){
		case Selectable.Transition.ColorTint:
			selectables[i].currentAnimation = EditorCoroutine.StartEditorCoroutine (Tint(i, transition));
			break;
		case Selectable.Transition.SpriteSwap:
			selectables[i].currentAnimation = EditorCoroutine.StartEditorCoroutine (SpriteSwap(i, transition));
			break;
		case Selectable.Transition.Animation:
			selectables[i].currentAnimation = EditorCoroutine.StartEditorCoroutine (selectables[i].animator.Animate(Transition(transition, i)));
			break;
		}
	}

	IEnumerator Tint(int i, int transition)
	{
		float p = 0, currentTime = 0, time = selectables[i].source.colors.fadeDuration, tScale = 1/time;
		double deltaTime, tss;
		tss = EditorApplication.timeSinceStartup;
		switch(transition)
		{
			//Entering
		case 0:
			while(currentTime <=time){
				deltaTime = EditorApplication.timeSinceStartup - tss;
				currentTime += (float)deltaTime;
				p = currentTime*tScale;
				selectables[i].source.targetGraphic.color = Color.Lerp(selectables[i].source.colors.normalColor, selectables[i].source.colors.highlightedColor, p);
				yield return null;
			}
			break;
			
			//Exiting
		case 1:
			while(currentTime <=time){
				deltaTime = EditorApplication.timeSinceStartup - tss;
				currentTime += (float)deltaTime;
				p = currentTime*tScale;
				selectables[i].source.targetGraphic.color = Color.Lerp(selectables[i].source.colors.highlightedColor, selectables[i].source.colors.normalColor, p);
				yield return null;
			}
			break;
			
			//Pressing
		case 2:
			while(currentTime <=time){
				deltaTime = EditorApplication.timeSinceStartup - tss;
				currentTime += (float)deltaTime;
				p = currentTime*tScale;
				selectables[i].source.targetGraphic.color = Color.Lerp(selectables[i].source.colors.highlightedColor, selectables[i].source.colors.pressedColor, p);
				yield return null;
			}
			break;
			
			//Releasing
		case 3:
			while(currentTime <=time){
				deltaTime = EditorApplication.timeSinceStartup - tss;
				currentTime += (float)deltaTime;
				p = currentTime*tScale;
				selectables[i].source.targetGraphic.color = Color.Lerp(selectables[i].source.colors.pressedColor, selectables[i].source.colors.highlightedColor, p);
				yield return null;
			}
			break;
		}
	}

	IEnumerator SpriteSwap(int i, int transition)
	{
		yield return null;
		if(selectables[i].baseImage != null){
			switch(transition){
				//Entering
			case 0:
				selectables[i].baseImage.overrideSprite = selectables[i].source.spriteState.highlightedSprite;
				break;
				
				//Exiting
			case 1:
				selectables[i].baseImage.overrideSprite = null;
				break;
				
				//Pressing
			case 2:
				selectables[i].baseImage.overrideSprite = selectables[i].source.spriteState.pressedSprite;
				break;
				
				//Releasing
			case 3:
				selectables[i].baseImage.overrideSprite = null;
				break;

				//Disabled
			case 4:
				selectables[i].baseImage.overrideSprite = selectables[i].source.spriteState.disabledSprite;
				break;
			}
		}
	}
	public string Transition(int transition, int i)
	{
		string ret;
		switch(transition)
		{
			//Entering
		case 0:
			ret = selectables[i].source.animationTriggers.highlightedTrigger;
			
			break;
			
			//Exiting
		case 1:
			ret = selectables[i].source.animationTriggers.normalTrigger;
			break;
			
			//Pressing
		case 2:
			ret = selectables[i].source.animationTriggers.pressedTrigger;
			break;
			
			//Releasing
		case 3:
			ret = selectables[i].source.animationTriggers.normalTrigger;
			break;
			
			//Disabled
		default:
			ret = selectables[i].source.animationTriggers.disabledTrigger;
			break;
		}
		return ret;
	}
	#endif
}



public class InEditorSelectable
{
	public InEditorUI boss;
	public Selectable source;
	public RectTransform rect;
	public EditorAnimator animator;
	public EditorCoroutine currentAnimation;
	public Image baseImage;
	public Draggable dragger;
	public Droppable dropper;

	public UnityEventBase[] selectableEvents;
	public uint[] selectableEventContexts;

	public InEditorSelectable(Selectable s, InEditorUI b)
	{
		source = s;
		boss = b;
		System.Type sT = source.GetType();
		//Find Unity events and register them by, what, name?
		rect = s.transform as RectTransform;
		baseImage = s.image;
		dropper = s.GetComponentInChildren<Droppable> ();
		dragger = s.GetComponentInChildren<Draggable> ();
		if(s.transition == Selectable.Transition.Animation){animator = new EditorAnimator(s.animator);}

		List<UnityEventBase> events = new List<UnityEventBase> ();
		FieldInfo[] fields = sT.GetFields ();
		for (int i = 0; i < fields.Length; i++) {
			
			System.Object fieldObj = fields [i].GetValue (source);
			if (fieldObj is UnityEventBase ) {
				Debug.Log ("Field: " +fields [i].ToString ());
				events.Add ((UnityEventBase)fieldObj);
			}
		}
		PropertyInfo[] props = sT.GetProperties ();
		for (int i = 0; i < props.Length; i++) {
			if (!props [i].IsDefined (typeof(System.ObsoleteAttribute), true)) {
				System.Object propObj = props [i].GetValue (source, null);
				if (propObj is UnityEventBase) {
					Debug.Log ("Property: " + props [i].ToString ());
					events.Add ((UnityEventBase)propObj);
				}
			}
		}
		//Now, in theory, I have a list of events. I should be able to deferentiate them either by name or direct type.
		selectableEvents = events.ToArray();
		selectableEventContexts = new uint[selectableEvents.Length];
		for (int i = 0; i < selectableEvents.Length; i++) {
			System.Type eType = selectableEvents [i].GetType ();
			/*
			switch (eType.ToString ()) {
			
			case "UnityEngine.UI.Button":
				button = source as Button;
				selectableType = 1;
				break;
			case "UnityEngine.UI.Toggle":
				toggle = source as Toggle;
				selectableType = 2;
				break;
			case "UnityEngine.UI.Slider":
				slider = source as Slider;
				selectableType = 3;
				break;
			
			case "UnityEngine.UI.Scrollbar":
				scrollbar = source as Scrollbar;
				selectableType = 4;
				break;
			
			case "UnityEngine.UI.Dropdown":
				dropdown = source as Dropdown;
				selectableType = 5;
				break;
			
			case "UnityEngine.UI.InputField":
				inputfield = source as InputField;
				selectableType = 6;
				break;
			}*/
			Debug.Log (eType.ToString ());
		}
	}

	public void MouseDown()
	{
		if(dragger != null){
			dragger.BeginDrag();
		}

	}
	public void MouseDrag(Vector2 mouseDelta)
	{
		if(dragger != null){
			dragger.Drag();
		}

	}
	public void MouseUp()
	{
		if(dragger != null){
			dragger.EndDrag();
		}



	}

	public void MouseClick()
	{
		
	}
	public void MouseDrop()
	{
		if(dropper != null){
			dropper.Drop();
		}
	}
	public void HandleKeyEvent(Event e)
	{

	}
}

