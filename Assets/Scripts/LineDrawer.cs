using UnityEngine;
using System.Collections;

public class LineDrawer : MonoBehaviour {
	public SixenseHands	m_hand;
	public SixenseInput.Controller m_controller = null;
	public float length = 10.0f;
	public GameObject bullet;
	public float bForce = 20.0f;
	public GameObject BulletHole;

	public LineRenderer lRenderer = null;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (m_controller == null) {
			m_controller = SixenseInput.GetController (m_hand);
		} else {
			if(m_controller.GetButtonDown(SixenseButtons.TRIGGER)){
				shoot();
			}
			if (lRenderer == null) {
				lRenderer = GetComponentInChildren (typeof(LineRenderer)) as LineRenderer;
			} else {
				Vector3 endPoint = (this.transform.position + (m_controller.Rotation * (Vector3.forward*length)));
				lRenderer.SetPosition(0,this.transform.position);
				lRenderer.SetPosition(1, endPoint);
			}

		}
	}
	protected void shoot(){
		Vector3 fwd = transform.TransformDirection (Vector3.forward);
		RaycastHit hit;
		if (Physics.Raycast (transform.position, fwd, out hit)) {
			Instantiate (BulletHole, hit.point, Quaternion.identity);
		}
		//GameObject bul1 = Instantiate (bullet, transform.position , transform.rotation) as GameObject;
		//bul1.transform.Rotate (new Vector3 (90, 0 ,0));
		//Rigidbody rb = bul1.GetComponent(typeof(Rigidbody)) as Rigidbody;
		//rb.AddForce (m_controller.Rotation*Vector3.forward * bForce);
	}
}
