using UnityEngine;
using System.Collections;

public class MouseFollowController : MonoBehaviour {
	public CharacterController2D controller;

	void Update() {
		if( Input.GetKey(KeyCode.LeftShift ) ) {
			Vector2 movement = Vector2.zero;
			if( Input.GetKey( KeyCode.W ) ) movement += Vector2.up;
			if( Input.GetKey( KeyCode.S ) ) movement += Vector2.down;
			if( Input.GetKey( KeyCode.A ) ) movement += Vector2.left;
			if( Input.GetKey( KeyCode.D ) ) movement += Vector2.right;

			controller.Move( movement * Time.deltaTime );
		}
		else {
			Vector3 targetPos = Camera.main.ScreenToWorldPoint( Input.mousePosition );
			targetPos.z = 0.0f;

			controller.Move( ( targetPos- transform.position ) * Time.deltaTime );
		}
	}
}
