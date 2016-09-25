using UnityEngine;
using System.Collections;

[System.Serializable]
public class PlayerInput {
	public bool allowInput = true;
	public int playerIndex = 1;
	public bool updated { get; private set; }

	// Constant inputs
	public Vector3 directionalInput = Vector2.zero;
	public bool jumping = false;

	// Instantaneous inputs
	public bool justJumped = false;


	public void ParseInput() {
		updated = true;

		if( !allowInput )
			return;

		directionalInput.x = Input.GetAxis( "P" + playerIndex + "_Horizontal" );
		directionalInput.y = Input.GetAxis( "P" + playerIndex + "_Vertical" );

		jumping = Input.GetButton( "P" + playerIndex + "_Jump" );

		justJumped |= Input.GetButtonDown( "P" + playerIndex + "_Jump" );
	}

	public void ClearInput() {
		updated = false;

		directionalInput = Vector2.zero;
		jumping = false;
		justJumped = false;
	}
}
