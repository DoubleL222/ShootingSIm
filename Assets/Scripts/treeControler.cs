using UnityEngine;
using System.Collections;

public class treeControler : MonoBehaviour {

	// Use this for initialization
	public GameObject target;
	public bool destroyed = false;

	Vector3 originalPos;
	Vector3 destPos;
	Vector3 endPos;
	Vector3 startPos;
	bool moving;
	GameObject targetInstance;
	float moveTime = 3;
	float moveDistance = 2;
	float t = 0;

	void Start(){
		originalPos = this.transform.position + new Vector3 (0.0f, 0.5f, 2.0f);
		float dif = originalPos.x >= 0 ? -1 : 1;
		destPos = originalPos + new Vector3 (dif * moveDistance, 0.0f, 0.0f);
		startMoving ();
	}

	public void startMoving(){
		targetInstance = Instantiate(target, originalPos, Quaternion.identity) as GameObject;
		moving = true;
		startPos = originalPos;
		endPos = destPos;
		destroyed = false;
	}
	void Update(){
		if (moving) {
			if (targetInstance != null) {
				t += Time.deltaTime / moveTime;
				targetInstance.transform.position = Vector3.Lerp (targetInstance.transform.position, endPos, t);
				if (targetInstance.transform.position == endPos) {
					Vector3 temp = endPos;
					endPos = startPos;
					startPos = temp;
					t = 0;
				} 
			} else if (targetInstance == null) {
				destroyed = true;
			}
		}
	}
}
