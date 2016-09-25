using UnityEngine;
using System.Collections;

public class Rotator : ContraptionBase {

	public Transform moveTarget;
	public float[] path = new float[0];
	public bool pathIsLocal;
	public float time = 2f;
	public float currentTime = 0f;
	float tScalar, pScalar;
	float percentComplete = 0f;
	public uint state = 2;

	void Start()
	{
		tScalar = 1f / time;
		pScalar = 1f / path.Length;
	}
	override public void Activate(int id)
	{
		state = (uint)id;
	}

	void Update()
	{
		if (path.Length < 2) {
			return;
		}
		
		float subPercent;
		int section;
		Vector3 workHorse;
		switch (state) {

		case 0:
			currentTime += Time.deltaTime;
			if(currentTime < time){
				percentComplete = currentTime *tScalar;
				if (path.Length > 2) {
					subPercent = percentComplete * path.Length;
					section = Mathf.FloorToInt (subPercent);
					subPercent -= section;

					//Debug.Log(percentComplete + " :    " + subPercent);
					if (section < path.Length - 1) {
						if (!pathIsLocal) {
							workHorse = moveTarget.eulerAngles;
							workHorse.z = Mathf.LerpAngle (path [section], path [section + 1], subPercent*pScalar);
							moveTarget.eulerAngles = workHorse;
						} else {
							workHorse = moveTarget.localEulerAngles;
							workHorse.z = Mathf.LerpAngle (path [section], path [section + 1], subPercent*pScalar);
							moveTarget.localEulerAngles = workHorse;
						}
					} else {
						if (!pathIsLocal) {
							workHorse = moveTarget.eulerAngles;
							workHorse.z = path [path.Length - 1];
							moveTarget.eulerAngles = workHorse;
						} else {
							workHorse = moveTarget.localEulerAngles;
							workHorse.z = path [path.Length - 1];
							moveTarget.localEulerAngles = workHorse;
						}
					}
				} else {
					if (!pathIsLocal) {
						workHorse = moveTarget.eulerAngles;
						workHorse.z = Mathf.LerpAngle (path [0], path [ 1], percentComplete);
						moveTarget.eulerAngles = workHorse;
					} else {
						workHorse = moveTarget.localEulerAngles;
						workHorse.z = Mathf.LerpAngle (path [0], path [1], percentComplete);
						moveTarget.localEulerAngles = workHorse;
					}
				}
			}else {
				currentTime = time;
				percentComplete = 1;
				if (!pathIsLocal) {
					workHorse = moveTarget.eulerAngles;
					workHorse.z = path [path.Length-1];
					moveTarget.eulerAngles = workHorse;
				} else {
					workHorse = moveTarget.localEulerAngles;
					workHorse.z = path [path.Length-1];
					moveTarget.localEulerAngles = workHorse;
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
					//Debug.Log(percentComplete + " :    " + subPercent);
					if (section < path.Length - 1) {
						if (!pathIsLocal) {
							workHorse = moveTarget.eulerAngles;
							workHorse.z = Mathf.LerpAngle (path [section], path [section + 1], subPercent*pScalar);
							moveTarget.eulerAngles = workHorse;
						} else {
							workHorse = moveTarget.localEulerAngles;
							workHorse.z = Mathf.LerpAngle (path [section], path [section + 1], subPercent*pScalar);
							moveTarget.localEulerAngles = workHorse;
						}
					} else {
						if (!pathIsLocal) {
							workHorse = moveTarget.eulerAngles;
							workHorse.z = path [path.Length - 1];
							moveTarget.eulerAngles = workHorse;
						} else {
							workHorse = moveTarget.localEulerAngles;
							workHorse.z = path [path.Length - 1];
							moveTarget.localEulerAngles = workHorse;
						}
					}
				} else {
					if (!pathIsLocal) {
						workHorse = moveTarget.eulerAngles;
						workHorse.z = Mathf.LerpAngle (path [0], path [1], percentComplete);
						moveTarget.eulerAngles = workHorse;
					} else {
						workHorse = moveTarget.localEulerAngles;
						workHorse.z = Mathf.LerpAngle (path [0], path [ 1], percentComplete);
						moveTarget.localEulerAngles = workHorse;
					}
				}

			} else {
				currentTime = 0;
				percentComplete = 0;
				if (!pathIsLocal) {
					workHorse = moveTarget.eulerAngles;
					workHorse.z = path [0];
					moveTarget.eulerAngles = workHorse;
				} else {
					workHorse = moveTarget.localEulerAngles;
					workHorse.z = path [0];
					moveTarget.localEulerAngles = workHorse;
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
