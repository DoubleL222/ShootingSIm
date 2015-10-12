 using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;


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
	Vector3 topLeft, topRight, bottomLeft, bottomRight, topCenter, bottomCenter, centerLeft, centerCenter, centerRight;
	Vector3 debugTopLeft, debugTopRight, debugBottomLeft, debugBottomRight;
	float xMin, xMax, yMin, yMax, sWidth, sHeight;
	float realScreenWidth, realScreenHeight;
	float crosshairSize;
	int ControlPoints = 9;
	List<Vector2> prevCrosshairPos;
	float SMAy, SMAx;
	float WMATOTALx;
	float WMATOTALy;
	kalmanState KSx;
	kalmanState[] KSRight;
	bool recording = false;
	StreamWriter twFiltered;
	StreamWriter tw;
	bool firstIter = true;
	float finishRec;
	
	public bool centroid = true;
	public Vector3 rightHandOffset;
	public Vector3 leftHandOffset;
	public bool useLine;
	public Image crosshair;
	public Vector3 HidraScreenCenterDiff;
	public bool useRifle;
	public GameObject BulletHole;
	public int mode = 1;
	public GameObject rifle;
	public float laserLength = 10.0f;
	public GameObject intersectMarker;

	public float recordTime=3;
	public float estimatedError;
	public float processNoise;
	public float sensorNoise;
	public float kalmanGain;


	public bool SimpleMovingAverage;
	public bool WeightedMovingAverage;
	public int nData = 50;

	Plane screenPlane;

	public class kalmanState {
		public float q;
		public float r;
		public float x;
		public float p;
		public float k;

		public kalmanState(float q, float r, float x, float p, float k){
			this.q = q;
			this.r = r;
			this.x = x;
			this.p = p;
			this.k = k;
		}
	}
	void kalman_update(kalmanState state, float measurement){
		state.p = state.p + state.q;

		state.k = state.p / (state.p + state.r);
		state.x = state.x + state.k * (measurement - state.x);
		state.p = (1 - state.k) * state.p;
	}

	// Use this for initialization
	public Vector3 getFireDir(){
		return fireDir;
	}
	public Vector3 getRaySource(){
		return raySource;
	}
	void Start () 
	{
	//	KSx = new kalmanState (0.0625f, 32.0f, 0.0f, 1.3833094f, 0.043228418f);
		KSRight = new kalmanState[3];
		prevCrosshairPos = new List<Vector2> ();
		crosshairSize = crosshair.rectTransform.rect.width;
		Debug.Log (crosshairSize);
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
		crosshair.rectTransform.localPosition = new Vector3 (-realScreenWidth/2 + crosshairSize/2 , realScreenHeight/2 - crosshairSize/2);
		//w_parts = GetComponentsInChildren<WController>();
	}
	// Use this for initialization
	// Update is called once per frame
	void StartRecording(){
		int fileCount = Directory.GetFiles ("Assets/Temporary/").Length;
		//Debug.Log ("file count" + fileCount);
		int testNum = 0;
		if(fileCount>0){
			testNum = fileCount / 2 + 1;
		}
		string path1 = "Assets/Temporary/test"+testNum+".txt";
		string path2 = "Assets/Temporary/test"+testNum+"Filtered.txt";
		//File.Create(path2);
		tw = new StreamWriter (File.Create (path1));
		twFiltered = new StreamWriter(File.Create (path2));
		recording = true;
	}
	void StopRecording(){
		tw.Close();
		twFiltered.Close();
		recording = false;
	}
	void DrawDebugScreen(){
		Debug.DrawLine(debugTopLeft, debugTopRight, Color.black);
		Debug.DrawLine(debugTopRight, debugBottomRight);
		Debug.DrawLine(debugBottomRight, debugBottomLeft);
		Debug.DrawLine(debugBottomLeft, debugTopLeft);
	}
	void Update () {
		{	/*
			if (Input.GetKeyDown(KeyCode.R)){
				finishRec = Time.time + recordTime;
				recording = true;
				StartRecording();
				Debug.Log("started recording");
			}*/
			if(recording){
				if(Time.time>=finishRec){
					StopRecording();
					Debug.Log("stoped recording");
					recording = false;
				}
			}

			if (Input.GetKeyDown(KeyCode.E)){
				Debug.Log("clicked E");
				cameraShoot ();
			}
			if (lukaCalibrated){
				DrawDebugScreen();
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
			if(useLine){
				if (lRenderer == null) 
				{
					lRenderer = GetComponentInChildren (typeof(LineRenderer)) as LineRenderer;
				} 
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
	float Remap(float value, float from1, float to1, float from2, float to2){
		return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
	}
	void updateCrosshairPos(){
		Vector3 intersection = calculateIntersection ();
		if (intersection.x <= xMax && intersection.x >= xMin && intersection.y <= yMax && intersection.y >= yMin) {
			if (crosshair.enabled == false)
				crosshair.enabled = true;
			//Debug.Log ("pointingAtScreen");
			float relativeX, relativeY;
			relativeX = Remap (intersection.x, xMin, xMax, -0.5f, 0.5f);
			relativeY = Remap (intersection.y, yMin, yMax, -0.5f, 0.5f);
			//Debug.Log ("realtive X = " + relativeX + " realtive Y = " + relativeY);
			float nextX = relativeX * realScreenWidth;
			float nextY = relativeY * realScreenHeight;
			Vector2 nextPos = new Vector2(nextX, nextY);
			prevCrosshairPos.Add(nextPos);
			if(prevCrosshairPos.Count >= nData && SimpleMovingAverage){
				nextPos = SMA(nextPos);
			} else if(prevCrosshairPos.Count >= nData && WeightedMovingAverage){
				nextPos = WMA(nextPos);
			}
			crosshair.rectTransform.localPosition = new Vector3 (nextPos.x, nextPos.y);
		} else
			crosshair.enabled = false;
	}
	Vector2 WMA(Vector2 pos){
		Debug.Log ("USING WEIGHTED MOVING AVERAGE");
		int len = prevCrosshairPos.Count;
		float bottom = (nData * (nData + 1)) / 2;
		if (prevCrosshairPos.Count == nData) {
			SMAx = 0;
			SMAy = 0;
			WMATOTALx =0;
			WMATOTALy =0;
			for (int i=0; i<nData; i++) {
				WMATOTALx+=prevCrosshairPos [len - 1 - i].x;
				WMATOTALy+=prevCrosshairPos [len - 1 - i].y;
				SMAx += prevCrosshairPos [len - 1 - i].x * (nData - i);
				SMAy += prevCrosshairPos [len - 1 - i].y * (nData - i);
			}

			SMAx = SMAx / bottom;
			SMAy = SMAy / bottom;
		} else {
			float newTOTALx = WMATOTALx +pos.x - prevCrosshairPos[len-1-nData+1].x;
			float newSMAx = SMAx + nData*pos.x - WMATOTALx;
			SMAx = newSMAx /bottom;
			WMATOTALx = newTOTALx;
			float newTOTALy = WMATOTALy +pos.y - prevCrosshairPos[len-1-nData+1].y;
			float newSMAy = SMAy + nData*pos.y - WMATOTALy;
			SMAy = newSMAy /bottom;
			WMATOTALy = newTOTALy;
		}
		return new Vector2(SMAx, SMAy);
	}
	

	Vector2 SMA(Vector2 pos){
		Debug.Log ("USING SIMPLE MOVING AVERAGE");
		if (prevCrosshairPos.Count == nData) {
			SMAx = 0;
			SMAy = 0;
			foreach (Vector2 prevPos in prevCrosshairPos) {
				SMAx += prevPos.x;
				SMAy += prevPos.y;
			}
			SMAx = SMAx / prevCrosshairPos.Count;
			SMAy = SMAy / prevCrosshairPos.Count;
		} else {
			float newSMAx = SMAx + pos.x/nData - prevCrosshairPos[prevCrosshairPos.Count - nData].x/nData;
			float newSMAy = SMAy + pos.y/nData - prevCrosshairPos[prevCrosshairPos.Count - nData].y/nData;
			SMAx = newSMAx;
			SMAy = newSMAy;

		}
		return new Vector2(SMAx, SMAy);
	}

	void recordPoints(){
		Debug.Log ("recording point " + numClicked);
		switch(numClicked){
		case 0:
			topLeft = calculateIntersection();
			crosshair.rectTransform.localPosition = new Vector3 (realScreenWidth/2 - crosshairSize/2 , realScreenHeight/2 - crosshairSize/2);
			break;
		case 1:
			topRight = calculateIntersection();
			crosshair.rectTransform.localPosition = new Vector3 (realScreenWidth/2 - crosshairSize/2 , -realScreenHeight/2 + crosshairSize/2);
			break;
		case 2: 
			bottomRight = calculateIntersection();
			crosshair.rectTransform.localPosition = new Vector3 (- realScreenWidth/2 + crosshairSize/2 , -realScreenHeight/2 + crosshairSize/2);
			break;
		case 3: 
			bottomLeft = calculateIntersection();
			if(ControlPoints==4){
				xMin = (bottomLeft.x + topLeft.x)/2.0f;
				xMax = (bottomRight.x +topRight.x)/2.0f;
				yMin = (bottomRight.y +bottomLeft.y)/2.0f;
				yMax = (topRight.y + topLeft.y)/2.0f;
				sWidth = xMax-xMin;
				sHeight = yMax-yMin;
				lukaCalibrated = true;
				Debug.Log("CASE 3!!");
			}
			else
			{
				crosshair.rectTransform.localPosition = new Vector3 (0 , realScreenHeight/2 - crosshairSize/2);
			}
			break;

		case 4: 
			topCenter = calculateIntersection();
			crosshair.rectTransform.localPosition = new Vector3 (0 , -realScreenHeight/2 + crosshairSize/2);
			break;

		case 5: 
			bottomCenter = calculateIntersection();
			crosshair.rectTransform.localPosition = new Vector3 (- realScreenWidth/2 + crosshairSize/2 , 0);
			break;

		case 6: 
			centerLeft = calculateIntersection();
			crosshair.rectTransform.localPosition = new Vector3 (realScreenWidth/2 - crosshairSize/2 , 0);
			break;

		case 7: 
			centerRight = calculateIntersection();
			crosshair.rectTransform.localPosition = new Vector3 (0 , 0);
			break;

		case 8: 
			centerCenter = calculateIntersection();
			if(ControlPoints==9){
				xMin = (bottomLeft.x + topLeft.x +centerLeft.x)/3.0f;
				xMax = (bottomRight.x +topRight.x + centerRight.x)/3.0f;
				yMin = (bottomRight.y +bottomLeft.y +bottomCenter.y)/3.0f;
				yMax = (topRight.y + topLeft.y+topCenter.y)/3.0f;
				sWidth = xMax-xMin;
				sHeight = yMax-yMin;

				if(centroid){
					xMin = centerCenter.x-sWidth/2;
					xMax = centerCenter.x+sWidth/2;
					yMin = centerCenter.y-sHeight/2;
					yMax = centerCenter.y+sHeight/2;
				}

				debugTopLeft = new Vector3(xMin, yMax, 0.0f);
				debugTopRight =	new Vector3(xMax,yMax, 0.0f);
				debugBottomRight = new Vector3(xMax,yMin, 0.0f);
				debugBottomLeft = new Vector3(xMin,yMin, 0.0f);




				lukaCalibrated = true;
				Debug.Log("CASE 8!!");
			}
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

			Vector3 rawPos = ( hand.m_controller.Position - m_baseOffset ) * m_sensitivity;
			Quaternion rawRot = hand.m_controller.Rotation * hand.InitialRotation;

			/*if(firstIter){
				KSx = new kalmanState(processNoise,sensorNoise,rawPos.x,estimatedError,kalmanGain);
				firstIter = false;
			}*/

			hand.transform.localPosition = rawPos;
			hand.transform.localRotation = rawRot;
			if( hand.m_hand == SixenseHands.RIGHT)
			{
				if(firstIter){
					for(int i =0; i<KSRight.Length; i++){
						switch(i){
						case 0:
							KSRight[i] = new kalmanState(processNoise,sensorNoise,rawPos.x,estimatedError,kalmanGain);
							break;

						case 1:
							KSRight[i] = new kalmanState(processNoise,sensorNoise,rawPos.y,estimatedError,kalmanGain);
							break;

						case 2:
							KSRight[i] = new kalmanState(processNoise,sensorNoise,rawPos.z,estimatedError,kalmanGain);
							break;

						default:
							break;
						}
					}
					firstIter = false;
				}
				else{
					for(int i =0; i<KSRight.Length; i++){
						switch(i){
						case 0:
							kalman_update(KSRight[i],rawPos.x);
							break;
							
						case 1:
							kalman_update(KSRight[i],rawPos.y);
							break;
							
						case 2:
							kalman_update(KSRight[i],rawPos.z);
							break;
							
						default:
							break;
						}	
					}
				}
				
				//KALMAN FILTER
				//kalman_update(KSx, rawPos.x);
				
				

				if(recording){
					tw.WriteLine(rawPos.x+" "+rawPos.y+" "+rawPos.z);
					twFiltered.WriteLine(KSRight[0].x+" "+KSRight[1].x+" "+ KSRight[2].x);
				}
				hand.transform.position = hand.transform.position + rightHandOffset;
				if(useRifle){
					rifleInstance.transform.position=hand.transform.position;
					rifleInstance.transform.Translate(new Vector3(0.0f, -0.15f, 0.0f));
				}
				if(hand.m_controller.GetButtonDown(SixenseButtons.TRIGGER)){
					if(!lukaCalibrated){
						if(numClicked<ControlPoints){
							recordPoints();
							numClicked++;
						}
					}else{
						//Shoot(hand);
						cameraShoot ();
					}
				}
			}
			else if (hand.m_hand == SixenseHands.LEFT) {
				hand.transform.position = hand.transform.position + leftHandOffset;
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
		if (useLine) {
			lRenderer.SetPosition (0, position1 + hydrasOffset);
			lRenderer.SetPosition (1, position2 + hydrasOffset);
		}
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
