using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
	Quaternion initialRot;
	// Use this for initialization
	void Start () {
		initialRot = transform.rotation;
	}
	
	// Update is called once per frame
	void Update () {
		transform.rotation = initialRot;
		transform.position.Set(transform.position.x, transform.position.y ,-0.624f);

	}
}
