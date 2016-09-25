using UnityEngine;
using System.Collections;

public class ParallaxCameraController : MonoBehaviour {
	public Camera cam; // New keyword to shut up Unity warning
	public Camera nearParallax, farParallax;

	public PlatformerCharacter2D player1;
	public PlatformerCharacter2D player2;

	public Rect playerScreenBounds = new Rect( 0.0f, 0.0f, 0.5f, 0.5f );
	public float minZoom = 5.0f;
	public Vector2 positionTolerance = new Vector2 (0.2f, 0.2f), moveVel = Vector2.zero;
	public float zoomLength = 1.0f, moveLength = 0.1f;
	private float zoomVel;

	private float zPos;


	void Awake() {
		cam = GetComponent<Camera>();
		zPos = cam.transform.position.z;

		zoomVel = 0.0f;
	}

	void Update() {
		Vector2 center;
		float zoom;

		if( player1 && player2 ) {
			Bounds viewBounds = new Bounds();
			viewBounds.center = player1.transform.position;
			viewBounds.Encapsulate( player2.transform.position );
			viewBounds.size = new Vector3( viewBounds.size.x / playerScreenBounds.size.x, viewBounds.size.y / playerScreenBounds.size.y );

			center = viewBounds.center;
			zoom = Mathf.Max( viewBounds.size.x * 0.5f / cam.aspect, viewBounds.size.y * 0.5f, minZoom );
		}
		else if( player1 ) {
			center = player1.transform.position;
			zoom = minZoom;
		}
		else if( player2 ) {
			center = player2.transform.position;
			zoom = minZoom;
		}
		else {
			center = transform.position;
			zoom = cam.orthographicSize;
		}


		Vector3 newPos = new Vector3( center.x, center.y, zPos ) - cam.transform.position;
		if (Mathf.Abs (newPos.x) > positionTolerance.x || Mathf.Abs (newPos.y) > positionTolerance.y) {
			newPos = new Vector3 (center.x, center.y, zPos); 
			newPos.x = Mathf.SmoothDamp (cam.transform.position.x, newPos.x, ref moveVel.x, moveLength);
			newPos.y = Mathf.SmoothDamp (cam.transform.position.y, newPos.y, ref moveVel.y, moveLength);
			cam.transform.position = newPos;
		} else {
			cam.transform.position = new Vector3 (center.x, center.y, zPos);
		}
		float a = cam.orthographicSize = Mathf.SmoothDamp( cam.orthographicSize, zoom, ref zoomVel, zoomLength );

		float b = Mathf.Abs(cam.transform.position.z);

		//change clipping planes based on main camera z-position
		farParallax.nearClipPlane = b;
		farParallax.farClipPlane = cam.farClipPlane;
		nearParallax.farClipPlane = b;
		nearParallax.nearClipPlane = cam.nearClipPlane;

		//update field fo view for parallax cameras
		float fieldOfView = Mathf.Atan(a / b)  * Mathf.Rad2Deg * 2f;
		nearParallax.fieldOfView = farParallax.fieldOfView = fieldOfView;
	}






	void OnDrawGizmosSelected() {
		Camera cam = GetComponent<Camera>();
		if( !cam )
			return;

		Vector2 viewSize = new Vector3( cam.orthographicSize * cam.aspect * 2.0f, cam.orthographicSize * 2.0f );
		Vector2 boundsCenter = new Vector2( transform.position.x, transform.position.y ) + Vector2.Scale( playerScreenBounds.position, viewSize );

		Gizmos.color = new Color( 0.5f, 0.5f, 1.0f, 0.5f );
		Gizmos.DrawWireCube( boundsCenter, Vector2.Scale( playerScreenBounds.size, viewSize ) );
	}
}
