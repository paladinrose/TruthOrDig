using UnityEngine;
using System.Collections;

[RequireComponent( typeof( CharacterController2D ) )]
public class PlayerController : MonoBehaviour {
	// References
	public CharacterController2D controller;
	private Animator animator;
	public PlayerInput input = new PlayerInput();

	// Gameplay vars
	public int playerNum = 1;

	[Header( "Physics" )]
	public float gravity = 9.8f;

	[Header( "Movement" )]
	public float moveSpeed = 5.0f;
	public float runSpeed = 10.0f;
	public float groundAccel = 10.0f;
	public float airAccel = 10.0f;

	[Header( "Jumping" )]
	public float jumpSpeed = 5.0f;
	public float maxJumpTime = 0.5f;
	public float jumpCheckDist = 0.2f;
	public LayerMask groundLayerMask = 1;

	[Header( "Pushing" )]
	public float pushSpeed = 2.0f;
	public float pushDelay = 0.5f;

	// Player state
	private Vector2 velocity = Vector2.zero;
	public bool grounded = false;
	public bool jumping = false;
	public bool falling = false;
	
	private bool justJumped = false;

	private bool flipped;

	// Timers and misc
	private float groundDist;
	private float jumpTimer;
	private float pushTimer;
	private PushableBlock pushBlock;


	void Awake() {
		controller = GetComponent<CharacterController2D>();
		animator = GetComponentInChildren<Animator>();
	}

	void Start() {
		input.playerIndex = playerNum;
	}

	void Update() {
		input.ParseInput();
	}

	void FixedUpdate() {
		if( !input.updated )
			input.ParseInput();

		ResetFlags();
		ProbeWorld();
		UpdateState();
		Move();
		Animate();

		input.ClearInput();
	}

	void ResetFlags() {
		justJumped = false;
	}

	void ProbeWorld() {
		Vector2 pos = transform.position;
		Vector2 feetPos = pos + controller.center - new Vector2( 0.0f, controller.size.y * 0.5f );

		RaycastHit2D hitInfo;
		hitInfo = Physics2D.BoxCast( pos + controller.center, controller.size, 0.0f, -Vector2.up, 1.0f, groundLayerMask ); 
		if( hitInfo.collider )
			groundDist = feetPos.y - hitInfo.point.y;
		else
			groundDist = float.PositiveInfinity;
	}

	void UpdateState() {
		grounded = controller.isGrounded;

		if( !grounded && !jumping )
			falling = true;

		if( ( jumping || falling ) && grounded ) {
			jumping = false;
			falling = false;
		}

		if( ( grounded || groundDist <= jumpCheckDist ) && input.justJumped ) {
			justJumped = true;
			grounded = false;
			jumping = true;
			falling = false;
			jumpTimer = 0.0f;
		}

		if( jumping && !justJumped ) {
			if( input.jumping )
				jumpTimer += Time.deltaTime;

			if( jumpTimer > maxJumpTime || !input.jumping || controller.IsTouching( CollisionFlags2D.Top ) ) {
				jumping = false;
				falling = true;
			}
		}
	}

	void Move() {
		// If we're grounded, remove gravity accel
		if( grounded )
			velocity.y = Mathf.Max( velocity.y, 0.0f );

		// Handle jumping
		if( jumping )
			velocity.y = jumpSpeed;

		// Find the desired horizontal velocity and try to move towards it
		float desiredHorizVel = input.directionalInput.x * moveSpeed;
		velocity.x = Mathf.MoveTowards( velocity.x, desiredHorizVel, ( grounded ? groundAccel : airAccel ) * Time.deltaTime );

		// Apply gravity
		if( !jumping )
			velocity.y -= gravity * Time.deltaTime;

		// Handle block pushing
		if (pushBlock) {
			pushTimer += Time.deltaTime;

			if (pushTimer >= pushDelay) {
				velocity.x = Mathf.Clamp (velocity.x, -pushSpeed, pushSpeed);
				pushBlock.Push (velocity.x * Time.deltaTime);
			}
		} else {
			pushTimer = 0.0f;
			pushBlock = null;
		}
		// Move the character
		controller.Move( velocity * Time.deltaTime );
	}

	public void OnControllerColliderHit2D( ControllerColliderHit2D hit ) {
		PushableBlock pushableBlock = hit.gameObject.GetComponent<PushableBlock>();
		if( pushableBlock && hit.normal.y < 1.0f )
			pushBlock = pushableBlock;

		if( Vector2.Dot( velocity, hit.normal ) < 0.0f )
			velocity -= ( Vector2.Dot ( velocity, hit.normal ) * hit.normal );
	}

	void Animate() {
		if( velocity.x < 0 )
			flipped = true;
		if( velocity.x > 0 )
			flipped = false;

		animator.SetFloat( "SpeedX", Mathf.Abs( velocity.x ) );
		animator.SetFloat( "SpeedY", Mathf.Abs( velocity.y ) );
		animator.SetBool( "Mirrored", flipped );
		animator.SetBool( "Falling", falling );
		animator.SetBool( "Jumping", jumping );
		
		transform.localScale = flipped ? new Vector3( -1, 1, 1 ) : Vector3.one;
	}
}
