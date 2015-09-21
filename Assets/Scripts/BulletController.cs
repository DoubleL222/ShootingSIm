using UnityEngine;
using System.Collections;

public class BulletController : MonoBehaviour {
	// Use this for initialization
	public GameObject BulletHole;
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}
	void OnCollisionEnter(Collision col){
		//Vector3 pis = col.contacts [0].point;
		//Instantiate (BulletHole, pis, Quaternion.FromToRotation (Vector3.forward, pis));
		Destroy (gameObject);

	}
	void OnCollisionStay(Collision col){
		//Vector3 pis = col.contacts [0].point;
		//Quaternion decentRot = Quaternion.FromToRotation (Vector3.forward, pis);
		//Instantiate (BulletHole, pis, BulletHole.transform.rotation );
		Destroy (gameObject);
		
	}
}
