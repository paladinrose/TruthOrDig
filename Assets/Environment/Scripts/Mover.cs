using UnityEngine;
using System.Collections;

public class Mover : ContraptionBase {

	public Transform moveTarget;
	public Vector2[] path = new Vector2[0];
	public int[] sectionTypes;
	public bool pathIsLocal;
	public float time = 2f;
	public float currentTime = 0f;
	float tScalar;
	float percentComplete = 0f;
	public uint state = 2;

	void Start()
	{
		tScalar = 1f / time;
	}
	override public void Activate(int id)
	{
		state = (uint)id;
	}

	void Update()
	{
		float subPercent;
		int section; 
		switch (state) {
		
		case 0:
			currentTime += Time.deltaTime;
			if (currentTime < time) {
				percentComplete = currentTime * tScalar;
				if (path.Length > 2) {
					subPercent = percentComplete * path.Length;
					section = Mathf.FloorToInt (subPercent);
					subPercent -= section;
					//Debug.Log(percentComplete + " :    " + subPercent);
					if (section < path.Length - 1) {
						if (!pathIsLocal) {
							moveTarget.position = Vector2.Lerp (path [section], path [section + 1], subPercent);
						} else {
							moveTarget.localPosition = Vector2.Lerp (path [section], path [section + 1], subPercent);
						}
					} else {
						if (!pathIsLocal) {
							moveTarget.position = path [path.Length - 1];
						} else {
							moveTarget.localPosition = path [path.Length - 1];
						}
					}
				} else {
					if (!pathIsLocal) {
						moveTarget.position = Vector2.Lerp (path [0], path [1], percentComplete);
					} else {
						moveTarget.localPosition = Vector2.Lerp (path [0], path [ 1], percentComplete);
					}
				}
			
			}else {
				currentTime = time;
				percentComplete = 1;

				if (!pathIsLocal) {
					moveTarget.position = path [path.Length - 1];
				} else {
					moveTarget.localPosition = path [path.Length - 1];
				}
				state = 2;
			}

			break;
		case 1:
			currentTime -= Time.deltaTime;
			if(currentTime > 0f){
				percentComplete = currentTime *tScalar;
				if (path.Length > 2) {
					subPercent = percentComplete * path.Length;
					section = Mathf.FloorToInt (subPercent);
					subPercent -= section;

					if (section < path.Length - 1) {
						if (!pathIsLocal) {
							moveTarget.position = Vector2.Lerp (path [section], path [section + 1], subPercent);
						} else {
							moveTarget.localPosition = Vector2.Lerp (path [section], path [section + 1], subPercent);
						}
					} else {

						if (!pathIsLocal) {
							moveTarget.position = path [path.Length - 1];
						} else {
							moveTarget.localPosition = path [path.Length - 1];
						}
					}
				} else {
					if (!pathIsLocal) {
						moveTarget.position = Vector2.Lerp (path [0], path [1], percentComplete);
					} else {
						moveTarget.localPosition = Vector2.Lerp (path [0], path [ 1], percentComplete);
					}
				}
			} else {
				currentTime = 0;
				percentComplete = 0;
				if (!pathIsLocal) {
					moveTarget.position = path [0];
				} else {
					moveTarget.localPosition = path [0];
				}
				state = 2;
			}
			break;
		case 2:
			this.enabled = false;
			break;
		}
	}
}
