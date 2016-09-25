using UnityEngine;
using System.Collections;

[RequireComponent( typeof( CharacterController2D ))]
[RequireComponent( typeof( BoxCollider2D ))]
public class PushableBlock : MonoBehaviour {
	private CharacterController2D controller;

	public float gravity = 9.8f;

	private Vector2 velocity;

	void Awake() {
		controller = GetComponent<CharacterController2D>();
		velocity = Vector2.zero;
	}

	public void Push( float xDist ) {
		controller.Move( new Vector2( xDist, 0.0f ) );
	}

	void FixedUpdate() {
		if( controller.isGrounded )
			velocity.y = 0.0f;

		velocity.y -= gravity * Time.deltaTime;

		controller.Move( velocity * Time.deltaTime );
	}
}
