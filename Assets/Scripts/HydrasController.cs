using UnityEngine;
using System.Collections;

public class HydrasController : MonoBehaviour {
	Hydra[] 	m_hands;
	
	Vector3	m_baseOffset;
	float 	m_sensitivity = 0.001f; // Sixense units are in mm
	bool 	m_bInitialized;
	LineRenderer lRenderer;
	Vector3 fireDir;
	GameObject rifleInstance;
	ArrayList gHoles;

	public GameObject BulletHole;
	public int mode = 1;
	public GameObject rifle;
	public float laserLength = 10.0f;
	
	// Use this for initialization
	void Start () 
	{
		gHoles = new ArrayList ();
		fireDir = Vector3.zero;
		m_hands = GetComponentsInChildren<Hydra>();
		foreach (Hydra hand in m_hands) {
			if (hand.m_hand == SixenseHands.RIGHT){
				GameObject GORifle = Instantiate(rifle, hand.transform.position, Quaternion.Euler(new Vector3(0.0f, 180.0f, 0.0f))) as GameObject;
				//GORifle.transform.parent = hand.gameObject.transform;
				rifleInstance = GORifle;
			}
		}
		//w_parts = GetComponentsInChildren<WController>();
	}
	// Use this for initialization
	// Update is called once per frame
	void Update () {
		{
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
	void UpdateHand( Hydra hand )
	{
		bool bControllerActive = IsControllerActive( hand.m_controller );
		
		if ( bControllerActive )
		{
			hand.transform.localPosition = ( hand.m_controller.Position - m_baseOffset ) * m_sensitivity;
			hand.transform.localRotation = hand.m_controller.Rotation * hand.InitialRotation;
			if( hand.m_hand == SixenseHands.RIGHT)
			{
				rifleInstance.transform.position=hand.transform.position;
				rifleInstance.transform.Translate(new Vector3(0.0f, -0.15f, 0.0f));
				if(hand.m_controller.GetButtonDown(SixenseButtons.TRIGGER)){
					Shoot(hand);
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
	public void removeHoles(){
		foreach (GameObject go in gHoles) {
			Destroy(go);
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
		lRenderer.SetPosition (0, position1);
		lRenderer.SetPosition (1, position2);
		fireDir = Vector3.Normalize (position2 - position1);
		UpdateRifle ();
		
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
