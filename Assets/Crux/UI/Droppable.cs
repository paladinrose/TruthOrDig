using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

using System.Collections;

public class Droppable : MonoBehaviour, IDropHandler {
	public int[] acceptedDragTypes;
	public UnityEvent[] dropEvents;

	public Transform dropTarget;
	public Vector3 dropPosition;

	#region IDropHandler implementation
	public void OnDrop (PointerEventData eventData)
	{
		Drop();
	}
	#endregion

	public void Drop()
	{
		Draggable d = Draggable.currentDrag;
		if(ReceiveDrop(d.dragType)){
			if(d.dragTarget){
				if(dropTarget){
					d.dragTarget.parent = dropTarget;
					d.dragTarget.localPosition = dropPosition;
				} else {
					d.dragTarget.position = dropPosition;
				}
			}
			dropEvents[d.dragType].Invoke ();
			d.Dropped(this);
		}
	}

	bool ReceiveDrop(int id)
	{
		for(int i = acceptedDragTypes.Length-1; i >=0; i--){
			if(acceptedDragTypes[i] == id){return true;}
		}
		return false;
	}
}
