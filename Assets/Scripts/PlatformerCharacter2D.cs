using System;
using System.Collections;
using UnityEngine;


public class PlatformerCharacter2D : MonoBehaviour
{
	static int currentPlayerCount;
	public int playerID;
	private string pName;
	[Header("Movement")]
	public float maxSpeed= 10f;                    // The fastest the player can travel in the x axis.
	[Range(0, 1)] public float walkSpeed = 0.36f;  // Amount of maxSpeed applied to crouching movement. 1 = 100%
	[Range(0, 1)] public float crouchSpeed = 0.25f;  // Amount of maxSpeed applied to walking movement. 1 = 100%
	public float speedUpTime = 0.2f, currentSpeed, currentVel;
	public bool running, crouching = false;

	[Header("Jumping")]
	public int numberOfJumps = 1;
	public int currentNumJumps;
	public float maxJumpTime = 0.25f;
	float currentJumpTime;
	public float jumpForce = 200f;                  // Amount of force added when the player jumps.
	public float doubleTapTime = 0.2f;
    public bool airControl = true, jumping = false;                 // Whether or not a player can steer while jumping;
    public LayerMask layerMask;                  // A mask determining what is ground to the character

	[Header("Platforming")]
    public Vector3 groundCheck;    // A position marking where to check if the player is grounded.
    public float groundCheckRadius = 0.2f; // Radius of the overlap circle to determine if grounded
    public bool grounded;            // Whether or not the player is grounded.
    public Vector3 ceilingCheck;   // A position marking where to check for ceilings
    public float ceilingRadius = 0.01f; // Radius of the overlap circle to determine if the player can stand up
	public Vector3 pushCheck;    // A position marking where to check if the player is grounded.
	public float pushCheckRadius = 0.2f; // Radius of the overlap circle to determine if grounded
	public bool pushing;

	private Animator animator;            // Reference to the player's animator component.
    private Rigidbody2D rigidBod;
	private bool facingRight = true,  waitForDoubleTap = false, checkForDoubleTap = false;  // For determining which way the player is currently facing.
	private Coroutine Double;

    void Awake()
    {
        // Setting up references.
        animator = GetComponentInChildren<Animator>();
        rigidBod = GetComponent<Rigidbody2D>();
		currentPlayerCount++;
		playerID = currentPlayerCount;
		pName = "P" + playerID.ToString () + "_";
    }

	void Update(){
		if (currentNumJumps < numberOfJumps) {
			
			if (Input.GetButtonDown (pName + "Jump")) {

				jumping = true;
			}

		}

	}

    void FixedUpdate()
    {

		//Start off seeing if we're on the ground.
        grounded = false;

		bool isPushing = false;
		Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position + groundCheck, groundCheckRadius, layerMask);
        for (int i = 0; i < colliders.Length; i++){
			if (colliders [i].gameObject != gameObject) {
				grounded = true;
				currentNumJumps = 0;
				break;
			}
        }
		animator.SetBool("Ground", grounded);
		float m = 0;

		if (grounded || airControl) {
			m = Input.GetAxis (pName + "Horizontal");
			bool doFlip = false;
			if (grounded) {
				if (m == 0) {
					if (checkForDoubleTap) {
						Double = StartCoroutine (DoubleTap (doubleTapTime));
					}
					currentSpeed = 0;
					running = false;
				} else {
					currentSpeed *= m;
					if (m > 0) {
						if (facingRight && waitForDoubleTap) {
							//currentSpeed = m * maxSpeed;
							running = true;
							StopCoroutine (Double);
							waitForDoubleTap = false;
						} else if (!facingRight) {
							doFlip = true;
							//currentSpeed = m * maxSpeed * walkSpeed;
							checkForDoubleTap = true;
							running = false;
						} else if (!running && !waitForDoubleTap) {
							checkForDoubleTap = true;

						}
					} else if (m < 0) {
						if (!facingRight && waitForDoubleTap) {
							//currentSpeed = m * maxSpeed;
							running = true;
							StopCoroutine (Double);
							waitForDoubleTap = false;
						} else if (facingRight) {
							doFlip = true;
							//currentSpeed = m * maxSpeed * walkSpeed;
							checkForDoubleTap = true;
							running = false;
						} else if (!running && !waitForDoubleTap) {
							checkForDoubleTap = true;
						}
					}
				}

				//Now check to see if we're forced to crouch.
				crouching = Input.GetButton (pName + "Crouch");
				if (!crouching && animator.GetBool ("Crouch")) {
					// If the character has a ceiling preventing them from standing up, keep them crouching
					colliders = Physics2D.OverlapCircleAll (transform.position + ceilingCheck, ceilingRadius, layerMask);
					for (int i = 0; i < colliders.Length; i++) {
						if (colliders [i].gameObject != gameObject) {
							crouching = true;
							jumping = false;
							break;
						}
					}
				}

				if (!jumping && m != 0) {
					colliders = Physics2D.OverlapCircleAll (transform.TransformPoint (pushCheck), pushCheckRadius, layerMask);
					for (int i = 0; i < colliders.Length; i++) {
						if (colliders [i].gameObject != gameObject) {
							crouching = false;
							isPushing = true;
							running = false;
							break;
						}
					}

				}
			} else {
				if (m > 0 && !facingRight) {
					doFlip = true;
				} else if (m < 0 && facingRight) {
					doFlip = true;
				}
			}
			if (doFlip) {
				Flip ();
			}
			if (crouching) {
				animator.SetBool ("Crouch", crouching);
				currentSpeed = m * maxSpeed * crouchSpeed;
			} else if (!running) {
				currentSpeed = m * maxSpeed * walkSpeed;
			} else {
				currentSpeed = m * maxSpeed;
			}
			rigidBod.velocity = new Vector2 (Mathf.SmoothDamp (rigidBod.velocity.x, currentSpeed, ref currentVel, speedUpTime), rigidBod.velocity.y);
			
			if (jumping) {
				isPushing = pushing = false;
				jumping = false;
				//animator.SetBool("Ground", false);

				animator.SetTrigger ("Jump");

				currentNumJumps++;
				currentJumpTime = 0;
				rigidBod.velocity = new Vector2 (rigidBod.velocity.x, 0);
				rigidBod.AddForce (new Vector2 (0, jumpForce * 0.2f), ForceMode2D.Impulse);
			} else if (Input.GetButton (pName + "Jump") && currentJumpTime < maxJumpTime) {
				rigidBod.AddForce (new Vector2 (0, jumpForce));
				currentJumpTime += Time.deltaTime;
			}
			if (isPushing && !pushing) {
				pushing = true;
				animator.SetTrigger ("Push Start");
			} else if (!isPushing && pushing) {
				pushing = false;
				animator.SetTrigger ("Push Stop");
			}
			animator.SetFloat ("SpeedY", rigidBod.velocity.y);
			animator.SetFloat ("SpeedX", Mathf.Abs (rigidBod.velocity.x));
		}
    }

	IEnumerator DoubleTap(float t){
		checkForDoubleTap = false;
		waitForDoubleTap = true;
		yield return new WaitForSeconds (t);
		waitForDoubleTap = false;

	}

    void Flip()
    {
		waitForDoubleTap = false;
		checkForDoubleTap = true;
		// Switch the way the player is labelled as facing.
        facingRight = !facingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

	#if UNITY_EDITOR
	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere (transform.position + ceilingCheck, ceilingRadius);
		
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere (transform.position + groundCheck, groundCheckRadius);

		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere (transform.TransformPoint(pushCheck), pushCheckRadius);
	}
	#endif
}

