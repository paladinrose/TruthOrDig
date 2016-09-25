using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
public class TruthOrDigManager : MonoBehaviour {

	public float fadeInTime;
	public CanvasGroup startScreen;
	public Button startButton;

	// Use this for initialization
	void Start () {
		StartCoroutine (FadeIn ());
	}
	IEnumerator FadeIn(){
		startButton.interactable = false;

		float time = 0, fadePer = 1 / fadeInTime;
		while (time < fadeInTime) {
			startScreen.alpha = time * fadePer;
			yield return null;
			time += Time.deltaTime;

		}
		startScreen.alpha = 1;
		startButton.interactable = true;
	}
	// Update is called once per frame
	void Update () {
	
	}

	public void LoadLevelOne()
	{
		SceneManager.LoadScene ("Protolevel");
	}
}
