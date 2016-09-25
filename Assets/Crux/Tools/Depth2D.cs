using UnityEngine;
using System.Collections;

public class Depth2D : MonoBehaviour {

	/*
	public SpriteRenderer[] sprites;
	public int[] depthOffsets;
	public bool mobile;
	public float scanTime = 0.5f, depthScalar = 1;
	void Start()
	{
		sprites = gameObject.GetComponentsInChildren<SpriteRenderer>(true);
		depthOffsets = new int[sprites.Length];
		depthOffsets[0] = sprites[0].sortingOrder;
		sprites[0].sortingOrder = Mathf.RoundToInt (transform.position.y * depthScalar);
		for(int i = sprites.Length-1; i >0; i--){
			depthOffsets[i] = sprites[i].sortingOrder - depthOffsets[0];
			sprites[i].sortingOrder = depthOffsets[i] + sprites[0].sortingOrder;
		}

		if(mobile){StartCoroutine(ScanDepth());}
	}

	IEnumerator ScanDepth()
	{
		mobile = true;
		while(mobile){
			yield return new WaitForSeconds(scanTime);
			sprites[0].sortingOrder = Mathf.RoundToInt (transform.position.y * depthScalar);
			for(int i = sprites.Length-1; i > 0; i--)
			{
				sprites[i].sortingOrder = depthOffsets[i] + sprites[0].sortingOrder;
			}
		}
	}
	*/

	public SpriteRenderer targetSprite;
	public int defaultDepth;
	public string[] targetTags;
	public float scanTime = 0.5f, yOffset = 0;

	public int colliderPoolSize = 10;
	public Collider2D[] targetPool;
	int[] depths;
	int[] isFull;
	bool scanY;

	void Start()
	{
		targetPool = new Collider2D[colliderPoolSize];
		depths = new int[colliderPoolSize];
		isFull = new int[colliderPoolSize];
		if(targetSprite){defaultDepth = targetSprite.sortingOrder;}
	}

	void OnTriggerEnter2D(Collider2D col)
	{
		if (CheckTags(col.gameObject.tag)) {
			int openSlot = FindOpenSlot();
			//Debug.Log ("Putting " + col.gameObject.name + " in slot " + openSlot);
			if(openSlot >=0){
				Depth2D dCheck = col.gameObject.GetComponentInChildren<Depth2D>();
				if(dCheck!= null){
					isFull[openSlot] = 2;
					targetPool[openSlot] = col;
					SetTargetDepth();
					StartCoroutine(ScanY());
				} else{
					SpriteRenderer[] targetSwap = col.gameObject.GetComponentsInChildren<SpriteRenderer>();
					//Debug.Log (col.gameObject.name + " has " + targetSwap.Length + " renderers.");
					int l = targetSwap.Length;
					if(l >0){
						
						targetPool[openSlot] = col;
						depths[openSlot] = targetSwap[l-1].sortingOrder;
						for(int i = l-2; i >=0; i--)
						{
							if(targetSwap[i].sortingOrder < depths[openSlot]){depths[openSlot] = targetSwap[i].sortingOrder;}
						}
						isFull[openSlot] = 1;
						SetTargetDepth();
					}
				}
			}
		}
	}
	
	void OnTriggerExit2D(Collider2D col)
	{
		for(int i = colliderPoolSize-1; i >=0; i--){
			if(col == targetPool[i]){
				targetPool[i] = null;
				isFull[i] = 0;
				SetTargetDepth();
			}
		}
	}

	void SetTargetDepth()
	{
		bool foundDepth = false;
		for(int i = targetPool.Length-1; i >=0; i--){
			switch(isFull[i]){
			case 1:
				foundDepth = true;
				if(depths[i] < targetSprite.sortingOrder){targetSprite.sortingOrder = depths[i]-1;}
				break;

			case 2:
				foundDepth = true;
				Depth2D d = targetPool[i].gameObject.GetComponentInChildren<Depth2D>();
				if(d.targetSprite.transform.position.y < targetSprite.transform.position.y){

					targetSprite.sortingOrder = d.targetSprite.sortingOrder-1;
				}
				break;
			}
		}
		if(!foundDepth){targetSprite.sortingOrder = defaultDepth;}
	}
	bool CheckTags(string s)
	{
		for (int i = targetTags.Length-1; i >=0; i--) {
			if(targetTags[i] == s){return true;}
		}
		return false;
	}
	
	int FindOpenSlot()
	{
		for(int i = colliderPoolSize-1; i >=0; i--){
			if(isFull[i]==0)return i;
		}
		return -1;
	}

	IEnumerator ScanY()
	{
		scanY=true;
		bool foundDepth2D;
		while(scanY){
			foundDepth2D = false;
			for(int i = targetPool.Length-1; i >=0; i--){
				if(isFull[i] == 2){
					foundDepth2D = true;
					Depth2D d = targetPool[i].gameObject.GetComponentInChildren<Depth2D>();
					if(d.targetSprite.transform.position.y < targetSprite.transform.position.y){

						targetSprite.sortingOrder = d.targetSprite.sortingOrder-1;
					}
				}
			}
			if(!foundDepth2D){scanY = false;}
			yield return new WaitForSeconds(scanTime);
		}
	}

}
