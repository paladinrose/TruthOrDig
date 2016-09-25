using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections;

public class MouseSensor : BaseSensor, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler {

	
	public static MouseSensor lastClicked, over, held;

	public int clickCount = 0;
	public int resetClicksAt = 0;
	public bool useClickCycleEvents;
	#region IPointerClickHandler implementation

	public void OnPointerClick (PointerEventData eventData)
	{
		//Debug.Log (gameObject.name + gameObject.layer.ToString () + " : " + globalLayerStates [gameObject.layer]);
		if (CheckActive()) {

			lastClicked = this;
			events [0].callBack.Invoke ();
			if(useClickCycleEvents){
				events[5+clickCount].callBack.Invoke();
			}
			clickCount++;
			if(resetClicksAt >0 && clickCount >=resetClicksAt){clickCount = 0;}
		}
	}

	#endregion

	#region IPointerEnterHandler implementation
	public void OnPointerEnter (PointerEventData eventData)
	{
		if (CheckActive()) {
			over = this;
			events [1].callBack.Invoke ();
		}
	}
	#endregion

	#region IPointerExitHandler implementation

	public void OnPointerExit (PointerEventData eventData)
	{
		if (CheckActive()) {
			events [2].callBack.Invoke ();
			if (over == this) {
				over = null;
			}
		}
	}

	#endregion

	#region IPointerDownHandler implementation
	
	public void OnPointerDown (PointerEventData eventData)
	{
		if (CheckActive()) {
			held = this;
			events [3].callBack.Invoke ();
		}
	}
	
	#endregion
	
	#region IPointerUpHandler implementation
	
	public void OnPointerUp (PointerEventData eventData)
	{

		if (CheckActive()) {
			events [4].callBack.Invoke ();
			if (held == this) {
				held = null;
			}
		}
	}
	
	#endregion

	public void SpecialLayerDeactivate(int i)
	{
		
		if (gameObject.layer == i && active && useLayerSorting ) {
			
			if (over == this) {
				events [2].callBack.Invoke ();
				over = null;
			}
		}
	}

	public static void DeactivateMouseLayer(int[] i)
	{
		MouseSensor[] ms = FindObjectsOfType<MouseSensor> ();
		for (int j = ms.Length - 1; j >= 0; j--) {
			for (int k = i.Length - 1; k >= 0; k--) {
				globalLayerStates [i[k]] = false;
				ms [j].SpecialLayerDeactivate (i[k]);
			}
		}
	}
}
