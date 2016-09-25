using UnityEngine;
using System.Collections;

public class ControllerColliderHit2D {
	public Collider2D collider;
	public CharacterController2D controller;
	public GameObject gameObject;
	public Vector2 moveDirection;
	public float moveLength;
	public Vector2 normal;
	public Vector2 point;
	public Rigidbody2D rigidbody;
	public Transform transform;
}

[System.Flags]
public enum CollisionFlags2D { None = 0, Bottom = 1, Top = 2, Left = 4, Right = 8 }

public class CharacterController2D : MonoBehaviour {
	private const float kEpsilon = 0.001f;
	private const int kMoveIterations = 4;
	private const string kHitCallbackName = "OnControllerColliderHit2D";

	public float slopeLimit = 45.0f;
	public float skinWidth = 0.01f;
	public float minMoveDistance = 0.001f;
	public Vector2 size = Vector2.one;
	public Vector2 center = Vector2.zero;

	public CollisionFlags2D collisionFlags { get; private set; }
	public bool isGrounded { get { return ( collisionFlags & CollisionFlags2D.Bottom ) != 0; } }
	public bool IsTouching( CollisionFlags2D side ) { return ( collisionFlags & side ) != 0; }

	private int prevLayer;
	private LayerMask layerMask;
	private ControllerColliderHit2D hitData;


	void Awake() {
		prevLayer = gameObject.layer;
		UpdateLayerMask();

		hitData = new ControllerColliderHit2D();
	}

	public void Move( Vector2 motion ) {
		if( gameObject.layer != prevLayer )
			UpdateLayerMask();

		collisionFlags = CollisionFlags2D.None;

		float moveDist = motion.magnitude;
		Vector2 moveDir = motion / Mathf.Max( moveDist, kEpsilon );

		for( int i = 0; i < kMoveIterations; i++ ) {
			if( moveDist < minMoveDistance )
				break;

			Vector2 pos = transform.position + new Vector3( center.x, center.y );

			RaycastHit2D hitInfo = Physics2D.BoxCast( pos, size, 0.0f, moveDir, moveDist, layerMask );

			if( hitInfo.collider ) {
				bool ignoreCollision = false;

				// Ignore collisions with triggers
				ignoreCollision |= hitInfo.collider.isTrigger;

				// Pass through one-way platforms if the normal counts as a ground plane
				PlatformEffector2D platformEffector2D = hitInfo.collider.GetComponent<PlatformEffector2D>();
				//ignoreCollision |= ( hitInfo.collider.usedByEffector && platformEffector2D && platformEffector2D.useOneWay && hitInfo.normal.y < Mathf.Cos( platformEffector2D.sideAngleVariance ) );
				ignoreCollision |= ( hitInfo.collider.usedByEffector && platformEffector2D && platformEffector2D.useOneWay);
				// If we're ignoring the collision, move to the contact and then slightly into the collider to avoid contacting it next iteration
				if( ignoreCollision ) {
					moveDist -= hitInfo.distance;
					Vector2 move = moveDir * moveDist * hitInfo.fraction;
					move -= hitInfo.normal * kEpsilon;
					
					transform.position += new Vector3( move.x, move.y );
					i--;
				}
				// Otherwise, move to the collision, project motion onto the contact plane, and move slightly out of the collider
				else {
					Vector2 clippedMove = moveDir * moveDist * hitInfo.fraction;
					clippedMove += hitInfo.normal * kEpsilon;

					moveDir = ( moveDir - ( Vector2.Dot( moveDir, hitInfo.normal ) * hitInfo.normal ) ).normalized;
					moveDist -= hitInfo.distance;

					transform.position += new Vector3( clippedMove.x, clippedMove.y );

					collisionFlags |= GetCollisionSide( hitInfo );
					UpdateColliderHitData( ref hitData, hitInfo, moveDir );
					SendMessage( kHitCallbackName, hitData, SendMessageOptions.DontRequireReceiver );
				}
			}
			// If there was no collision, move to the unobstructed position
			else {
				transform.position += new Vector3( moveDir.x * moveDist, moveDir.y * moveDist );
				break;
			}
		}
	}

	void UpdateLayerMask() {
		layerMask.value = 0;
		for( int i = 0; i < 32; i++ ) {
			if( LayerMask.LayerToName( i ).Length > 0 && Physics2D.GetIgnoreLayerCollision( gameObject.layer, i ) == false )
				layerMask.value |= 1 << i;
		}
		prevLayer = gameObject.layer;
	}

	void UpdateColliderHitData( ref ControllerColliderHit2D colliderHit, RaycastHit2D hitInfo, Vector2 moveDir ) {
		colliderHit.collider = hitInfo.collider;
		colliderHit.controller = this;
		colliderHit.gameObject = hitInfo.transform.gameObject;
		colliderHit.moveDirection = moveDir;
		colliderHit.moveLength = hitInfo.distance;
		colliderHit.normal = hitInfo.normal;
		colliderHit.point = hitInfo.point;
		colliderHit.rigidbody = hitInfo.rigidbody;
		colliderHit.transform = hitInfo.transform;
	}

	CollisionFlags2D GetCollisionSide( RaycastHit2D hitInfo ) {
		float horizAngle = Mathf.Cos( slopeLimit );
		if( hitInfo.normal.y >= horizAngle )		return CollisionFlags2D.Bottom;
		else if( hitInfo.normal.y <= -horizAngle )	return CollisionFlags2D.Top;
		else if( hitInfo.normal.x >= 0 )			return CollisionFlags2D.Right;
		else										return CollisionFlags2D.Left;
	}

	void OnDrawGizmosSelected() {
		Gizmos.color = new Color( 0.5f, 1.0f, 0.5f, 0.5f );
		Gizmos.DrawWireCube( transform.position + new Vector3( center.x, center.y ), size );
	}
}
