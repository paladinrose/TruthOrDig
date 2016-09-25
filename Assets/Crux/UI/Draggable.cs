using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections;

public class Draggable : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler {
	
	public static Draggable currentDrag;

	public Transform dragTarget;
	Vector3 mouseOffset, startingPosition;
	public Graphic targetGraphic;
	public int dragType;
	public float defaultDepth;

	public UnityEvent onBeginDrag, onDrag, onEndDrag, onDrop;
	public Droppable droppedOn;

	#region IBeginDragHandler implementation

	public void OnBeginDrag (PointerEventData eventData)
	{
		BeginDrag();
	}

	#endregion

	public void BeginDrag()
	{
		if (targetGraphic) {
			targetGraphic.raycastTarget = false;
		}
		if(dragTarget){
			startingPosition = dragTarget.position;
			mouseOffset = dragTarget.position - UIMousePosition();
		}
		currentDrag = this;
		onBeginDrag.Invoke();
	}

	#region IDragHandler implementation

	public void OnDrag (PointerEventData eventData)
	{
		Drag();
	}

	#endregion

	public void Drag()
	{
		if(dragTarget){dragTarget.position = mouseOffset+ UIMousePosition();}
		onDrag.Invoke ();
	}

	#region IEndDragHandler implementation

	public void OnEndDrag (PointerEventData eventData)
	{
		EndDrag();
	}

	#endregion

	public void EndDrag()
	{
		if (targetGraphic) {
			targetGraphic.raycastTarget = true;
		}
		currentDrag = null;
		onEndDrag.Invoke ();
	}

	public void Dropped(Droppable d)
	{
		droppedOn = d;
		onDrop.Invoke ();
	}

	public void ResetPosition()
	{
		dragTarget.position = startingPosition;
	}

	Vector3 UIMousePosition() {
		Vector3 mP = Input.mousePosition; 

		if (targetGraphic) {
			mP.z = Vector3.Distance (dragTarget.position, targetGraphic.canvas.worldCamera.transform.position);
			mP = targetGraphic.canvas.worldCamera.ScreenToWorldPoint (mP);
		} else {
			mP.z = Vector3.Dot((dragTarget.position - Camera.main.transform.position), Camera.main.transform.forward);
			mP = Camera.main.ScreenToWorldPoint (mP);
		}


		//Debug.Log (Input.mousePosition + ", " +mP);
		return mP; 
	}
}
