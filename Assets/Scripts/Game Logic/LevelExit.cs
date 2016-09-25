using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelExit : MonoBehaviour {
	public string nextLevelName = "";

	private bool p1Exited;
	private bool p2Exited;

	void Start() {
		p1Exited = false;
		p2Exited = false;
	}

	void OnTriggerEnter2D( Collider2D other ) {
		PlayerController player = other.GetComponent<PlayerController>();
		if( player ) {
			if( player.playerNum == 1 )
				p1Exited = true;
			if( player.playerNum == 2 )
				p2Exited = true;

			if( p1Exited && p2Exited )
				StartLevelTransition();
		}
	}

	public void StartLevelTransition() {
		SceneManager.LoadScene (nextLevelName);
	}
}
