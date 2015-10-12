using UnityEngine;
using System.Collections;

public class GameControllerScript : MonoBehaviour {
	public GameObject Target;
	public HydrasController HC;
	public Vector3[] targetPositions;

	GameObject targetInstance=null;
	int targetIndex;
	// Use this for initialization
	void Start () {
		//Target = Instantiate (Target, new Vector3 (0.0f, 0.0f, 5.0f), Quaternion.identity) as GameObject;
		targetIndex = 0;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Space)) {
			NextTarget();
		//	Target = Instantiate (Target, new Vector3 (Random.Range(-2.0f, 2.0f), Random.Range(0.0f, 1.0f), Random.Range(5.0f, 15.0f)), Quaternion.identity) as GameObject;
		
		}
	}
	void NextTarget(){
		if (targetInstance != null) {
			Destroy (targetInstance);
			HC.removeHoles();
		}
		Vector3 nextPos = targetPositions [targetIndex % targetPositions.Length];
		targetInstance = Instantiate (Target, nextPos, Quaternion.identity) as GameObject;
		targetIndex++;
	}
}
