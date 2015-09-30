using UnityEngine;
using System.Collections;

public class SimSetupScript : MonoBehaviour {
	public float screenSizeX;
	public float screenSizeY;

	public Vector3 HidraScreenCenterDiff;
	public HydrasController HC;


	public GameObject HydrasCont;
	public GameObject ScreenPrefab;
	public GameObject intersectMarker;
	Plane screenPlane;
	// Use this for initialization
	public void recordPoints(){

	}
	void Start () {
		GameObject scr = Instantiate (ScreenPrefab, Vector3.zero , Quaternion.identity) as GameObject;
		scr.transform.localScale = new Vector3 (screenSizeX, screenSizeY, scr.transform.localScale.z);
		HydrasCont.transform.Translate (HidraScreenCenterDiff);
		screenPlane = new Plane (Vector3.right, Vector3.up, new Vector3(1,1,0));
		//Debug.Log ("Normal of screenPlane" + screenPlane.normal);

	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.KeypadEnter)) {
			calculateIntersection ();
		}
	}

	void calculateIntersection(){
		Vector3 planeNormal = screenPlane.normal;
		Vector3 pointOnPlane = new Vector3 (1, 1, 0);
		Vector3 fireDir = HC.getFireDir();
		Vector3 raySource = HC.getRaySource ();
		//Debug.Log ("plane normal:" + planeNormal + " fireDir: " + fireDir + " raySource: " + raySource);
		float top = Vector3.Dot (planeNormal, (raySource - pointOnPlane));
		float bottom = Vector3.Dot (planeNormal, fireDir);
		float t = -(top/bottom);
		Vector3 PIS = raySource + t * fireDir;
		Instantiate (intersectMarker, PIS, Quaternion.identity);
		Debug.Log ("Intersect point :" + PIS);

	}
}
