using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HydrasController : MonoBehaviour {
	Hydra[] 	m_hands;
	
	Vector3	m_baseOffset;
	float 	m_sensitivity = 0.001f; // Sixense units are in mm
	bool 	m_bInitialized;
	LineRenderer lRenderer;
	Vector3 fireDir;
	Vector3 raySource;
	GameObject rifleInstance;
	ArrayList gHoles;
	Vector3 hydrasOffset;
	bool lukaCalibrated = false;
	int numClicked=0;
	Vector3 topLeft, topRight, bottomLeft, bottomRight;
	float xMin, xMax, yMin, yMax, sWidth, sHeight;
	float realScreenWidth, realScreenHeight;

	public Image crosshair;
	public Vector3 HidraScreenCenterDiff;
	public bool useRifle;
	public GameObject BulletHole;
	public int mode = 1;
	public GameObject rifle;
	public float laserLength = 10.0f;
	public GameObject intersectMarker;


	Plane screenPlane;
	
	// Use this for initialization
	public Vector3 getFireDir(){
		return fireDir;
	}
	public Vector3 getRaySource(){
		return raySource;
	}
	void Start () 
	{
		realScreenHeight = Screen.height;
		realScreenWidth = Screen.width;
		Debug.Log (realScreenWidth + " " + realScreenHeight);
		hydrasOffset = transform.position; 
		transform.Translate (HidraScreenCenterDiff);
		gHoles = new ArrayList ();
		fireDir = Vector3.zero;
		raySource = Vector3.zero;
		m_hands = GetComponentsInChildren<Hydra>();
		screenPlane = new Plane (Vector3.right, Vector3.up, new Vector3(1,1,0));
		if (useRifle) {
			foreach (Hydra hand in m_hands) {
				if (hand.m_hand == SixenseHands.RIGHT) {
					GameObject GORifle = Instantiate (rifle, hand.transform.position, Quaternion.Euler (new Vector3 (0.0f, 180.0f, 0.0f))) as GameObject;
					//GORifle.transform.parent = hand.gameObject.transform;
					rifleInstance = GORifle;
				}
			}
		}
		//w_parts = GetComponentsInChildren<WController>();
	}
	float Remap(float value, float from1, float to1, float from2, float to2){
		return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
	}
	void updateCrosshairPos(){
		Vector3 intersection = calculateIntersection ();
		if (intersection.x <= xMax && intersection.x >= xMin && intersection.y <= yMax && intersection.y >= yMin) {
			Debug.Log("pointingAtScreen");
			float relativeX, relativeY;
			relativeX = Remap (intersection.x, xMin, xMax, -0.5f, 0.5f);
			relativeY = Remap (intersection.y, yMin, yMax, -0.5f, 0.5f);
			Debug.Log("realtive X = " +relativeX+" realtive Y = "+relativeY);
			crosshair.rectTransform.localPosition = new Vector3(relativeX * realScreenWidth, relativeY * realScreenHeight);
		}
	}
	// Use this for initialization
	// Update is called once per frame
	void Update () {
		{
			if (Input.GetKeyDown(KeyCode.E)){
				Debug.Log("clicked E");
				cameraShoot ();
			}
			if (lukaCalibrated){
				Debug.DrawLine(new Vector3(xMin, yMax, 0.0f), new Vector3(xMax,yMax, 0.0f), Color.black);
				Debug.DrawLine(new Vector3(xMax,yMax, 0.0f), new Vector3(xMax,yMin, 0.0f), Color.black);
				Debug.DrawLine(new Vector3(xMax,yMin, 0.0f), new Vector3(xMin,yMin, 0.0f), Color.black);
				Debug.DrawLine(new Vector3(xMin,yMin, 0.0f), new Vector3(xMin,yMax, 0.0f), Color.black);
				updateCrosshairPos();
			}
			if (Input.GetKeyDown (KeyCode.KeypadEnter)) {
		//		calculateIntersection ();
			}
			if(Input.GetKeyDown( KeyCode.Space)){

			}
			if(hydrasOffset != transform.position){
				hydrasOffset = transform.position;
			}
			bool bResetHandPosition = false;

			if (lRenderer == null) 
			{
				lRenderer = GetComponentInChildren (typeof(LineRenderer)) as LineRenderer;
			} 
			else 
			{
				if (m_bInitialized)
				{
					UpdateLaser(m_hands);
				}
				//Vector3 endPoint = (this.transform.position + (m_controller.Rotation * (Vector3.forward*length)));

			}

			foreach ( Hydra hand in m_hands )
			{
				if ( IsControllerActive( hand.m_controller ) && hand.m_controller.GetButtonDown( SixenseButtons.START ) )
				{
					bResetHandPosition = true;
				}
				
				if ( m_bInitialized )
				{
					UpdateHand( hand );				
				}
			}

			if ( bResetHandPosition )
			{
				m_bInitialized = true;
				
				m_baseOffset = Vector3.zero;
				
				// Get the base offset assuming forward facing down the z axis of the base
				foreach ( Hydra hand in m_hands )
				{
					m_baseOffset += hand.m_controller.Position;
				}
				
				m_baseOffset /= 2;
			}
		}
	}
	void recordPoints(){
		Debug.Log ("recording point " + numClicked);
		switch(numClicked){
		case 0:
			topLeft = calculateIntersection();
			break;
		case 1:
			topRight = calculateIntersection();
			break;
		case 2: 
			bottomRight = calculateIntersection();
			break;
		case 3: 
			bottomLeft = calculateIntersection();
			xMin = (bottomLeft.x + topLeft.x)/2.0f;
			xMax = (bottomRight.x +topRight.x)/2.0f;
			yMin = (bottomRight.y +bottomLeft.y)/2.0f;
			yMax = (topRight.y + topLeft.y)/2.0f;
			sWidth = xMax-xMin;
			sHeight = yMax-yMin;
			lukaCalibrated = true;
			Debug.Log("CASE 3!!");
			break;
		default:
			break;
		}
	}
	void UpdateHand( Hydra hand )
	{
		bool bControllerActive = IsControllerActive( hand.m_controller );
		
		if ( bControllerActive )
		{
			hand.transform.localPosition = ( hand.m_controller.Position - m_baseOffset ) * m_sensitivity;
			hand.transform.localRotation = hand.m_controller.Rotation * hand.InitialRotation;
			if( hand.m_hand == SixenseHands.RIGHT)
			{
				if(useRifle){
					rifleInstance.transform.position=hand.transform.position;
					rifleInstance.transform.Translate(new Vector3(0.0f, -0.15f, 0.0f));
				}
				if(hand.m_controller.GetButtonDown(SixenseButtons.TRIGGER)){
					if(!lukaCalibrated){
						if(numClicked<4){
							recordPoints();
							numClicked++;
						}
					}else{
						Shoot(hand);
					}
				}
			}
		}
		
		else
		{
			// use the inital position and orientation because the controller is not active
			hand.transform.localPosition = hand.InitialPosition;
			hand.transform.localRotation  = hand.InitialRotation;
		}
	}

	Vector3 calculateIntersection(){
		Vector3 planeNormal = screenPlane.normal;
		Vector3 pointOnPlane = new Vector3 (1, 1, 0);
		//Debug.Log ("plane normal:" + planeNormal + " fireDir: " + fireDir + " raySource: " + raySource);
		float top = Vector3.Dot (planeNormal, (raySource - pointOnPlane));
		float bottom = Vector3.Dot (planeNormal, fireDir);
		float t = -(top/bottom);
		Vector3 PIS = raySource + t * fireDir;
		if (!lukaCalibrated) {
			Instantiate (intersectMarker, PIS, Quaternion.identity);
		}
	//	Debug.Log ("Intersect point :" + PIS);
		return PIS;
		
	}

	public void removeHoles(){
		foreach (GameObject go in gHoles) {
			Destroy(go);
		}
	}
	void cameraShoot(){
		Vector3 crossHairPos = crosshair.rectTransform.localPosition;
		Vector3 rayVector = new Vector3 (crossHairPos.x + realScreenWidth / 2, crossHairPos.y + realScreenHeight / 2, 0);
		Ray cameraRay = Camera.main.ScreenPointToRay (rayVector);
		Debug.Log(rayVector);
		RaycastHit hit;
		if (Physics.Raycast(cameraRay, out hit, 10)) {
			Debug.Log ("hitSOmething");
			if(hit.collider.gameObject.tag == "Target"){
				GameObject gh = Instantiate (BulletHole, hit.point, Quaternion.identity) as GameObject;
				gHoles.Add(gh);
			}
		}
	}
	protected void Shoot(Hydra hand){
		RaycastHit hit;
		if (Physics.Raycast (hand.transform.position, fireDir, out hit)) {
			if(hit.collider.gameObject.tag == "Target"){
				GameObject gh = Instantiate (BulletHole, hit.point, Quaternion.identity) as GameObject;
				gHoles.Add(gh);
			}
		}
	}
	void UpdateLaser(Hydra[] hands)
	{
		Vector3 position1 = Vector3.zero;
		Vector3 position2 = Vector3.zero;
		if (mode == 1) {
			foreach (Hydra hand in hands) {
				if (hand.m_hand == SixenseHands.RIGHT) {
					position1 = (hand.m_controller.Position - m_baseOffset) * m_sensitivity;
				} else if (hand.m_hand == SixenseHands.LEFT) {
					position2 = (hand.m_controller.Position - m_baseOffset) * m_sensitivity;
				}
			}
			position2 = Vector3.Normalize((position2 - position1)) * laserLength;
		} else if (mode == 0) {
			foreach (Hydra hand in hands) {
				if (hand.m_hand == SixenseHands.RIGHT) {
					position1 = (hand.m_controller.Position - m_baseOffset) * m_sensitivity;
					position2 = (position1 + (hand.m_controller.Rotation * (Vector3.forward * laserLength)));
				}
			}
		} else if (mode == 2) {
			Vector3 posR = Vector3.zero;
			Vector3 posL = Vector3.zero;
			Quaternion rotR = Quaternion.identity;
			Quaternion rotL = Quaternion.identity;
			foreach (Hydra hand in hands) {
				if (hand.m_hand == SixenseHands.RIGHT) {
					posR = (hand.m_controller.Position - m_baseOffset) * m_sensitivity;
					rotR = hand.m_controller.Rotation;
				}
				else if (hand.m_hand == SixenseHands.LEFT) {
					posL = (hand.m_controller.Position - m_baseOffset) * m_sensitivity;
					rotL = hand.m_controller.Rotation;
				}
			}
			position1 = Vector3.Lerp(posR, posL, 0.5f);
			//position1 = 0.5f*posR+0.5f*posL;
			Quaternion dir = Quaternion.Lerp(rotR, rotL, 0.5f);
			position2 = (position1+(dir*(Vector3.forward*laserLength)));
		}
		raySource = position1 + hydrasOffset;
		lRenderer.SetPosition (0, position1+hydrasOffset);
		lRenderer.SetPosition (1, position2+hydrasOffset);
		fireDir = Vector3.Normalize (position2 - position1);
		if(useRifle)UpdateRifle ();
		
	}

	void UpdateRifle ()
	{

		rifleInstance.transform.rotation = Quaternion.FromToRotation (Vector3.forward, fireDir);
		rifleInstance.transform.Rotate(new Vector3(0.0f, 180.0f, 0.0f));
	}

	void OnGUI()
	{
		if ( !m_bInitialized )
		{
			GUI.Box( new Rect( Screen.width / 2 - 50, Screen.height - 40, 100, 30 ),  "Press Start" );
		}
	}

	bool IsControllerActive( SixenseInput.Controller controller )
	{
		return ( controller != null && controller.Enabled && !controller.Docked );
	}
}
