using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class GameControllerScript : MonoBehaviour {
	public GameObject Target;
	public HydrasController HC;
	public Vector3[] targetPositions;
	public Text scoreText;
	public InputField nameInput;
	public Transform displayPanel;
	public Text playerName;
	public Text playerScore;
	public Canvas menuScreen;
	public Canvas escapeScreen;
	public Canvas crosshairScreen;
	public Canvas inputScreen;
	public Text nameText;
	public Button[] menuButtons;
	public bool calibrated;
	public GameObject tree;

	List<GameObject> trees;
	int playerIndex;
	string currPlayer;
	int shotsFired;
	bool simMode = false;
	bool calMode = false;
	bool demoMode = false;
	List<Player> realPlayers;
	Dictionary<string, int> players;
	GameObject targetInstance=null;
	int targetIndex;
	int score;
	// Use this for initialization
	void Start () {
		trees = new List<GameObject> ();
		escapeScreen.enabled = false;
		calibrated = false;
		realPlayers = new List<Player> ();
		menuScreen.enabled = true;
		crosshairScreen.enabled = false;
		inputScreen.enabled = false;
		menuButtons [0].interactable = false;
		menuButtons [1].interactable = false;
		menuButtons [2].interactable = true;
		//Target = Instantiate (Target, new Vector3 (0.0f, 0.0f, 5.0f), Quaternion.identity) as GameObject;
		targetIndex = 0;

	}
	void startGame(){
		foreach (GameObject tr in trees) {
			Destroy(tr);
		}
		foreach (Vector3 t in targetPositions) {
			GameObject treeOne = Instantiate(tree, t+new Vector3(0, -1, 0), Quaternion.identity) as GameObject;
			treeControler tc = treeOne.GetComponent(typeof(treeControler)) as treeControler;
			trees.Add(treeOne);
//			tcs.Add(tc);
			//tc.startMoving();

		}
	}
	void setNextPlayer(){
		currPlayer = realPlayers[playerIndex].name;
		scoreText.text = realPlayers [playerIndex].score.ToString();
		nameText.text = currPlayer;
		playerIndex++;
		if (playerIndex > realPlayers.Count - 1) {
			playerIndex = 0;
		}
	}
	public void addNewPlayer(){
		string name = nameInput.text.Trim ();
		if (name.Length > 3) {
			Player pl = new Player (nameInput.text);
			if (!realPlayers.Contains (pl)) {
				realPlayers.Add (pl);
				Debug.Log ("Added " + pl.name);
				updateScoreBoard ();
			}
		}
		/*if(!players.ContainsKey(nameInput.text)){
			players.Add (nameInput.text, 0);
			Debug.Log ("Added " + nameInput.text);
			updateScoreBoard ();
		}*/
		nameInput.text = "";
	}
	public void fireShot(){
		shotsFired++;
	}

	void updateScoreBoard(){
		int i = 0;
		for (int j=0; j<displayPanel.childCount; j++) {
			Destroy(displayPanel.GetChild (j).gameObject);
		}
		realPlayers.Sort ();
		foreach (Player pl in realPlayers) {
			Text scoreLine = Instantiate(playerName, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity) as Text;
			scoreLine.transform.SetParent(displayPanel, false);
			scoreLine.text = pl.name;
			Text score = Instantiate(playerScore, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity) as Text;
			score.transform.SetParent(displayPanel, false);
			score.text = pl.score.ToString();
			scoreLine.rectTransform.anchoredPosition = new Vector3(230, -30-i*75, 0);
			score.rectTransform.anchoredPosition = new Vector3(-130, -30-i*75, 0);
			i++;
		}/*
		var neki = from e in players orderby e.Value ascending select e;
		int i = 0;
		foreach (var item in neki) {
			Text scoreLine = Instantiate(playerName, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity) as Text;
			scoreLine.transform.SetParent(displayPanel, false);
			scoreLine.text = item.Key;
			Text score = Instantiate(playerScore, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity) as Text;
			score.transform.SetParent(displayPanel, false);
			score.text = item.Value.ToString();

			//scoreLine.transform.parent = displayPanel;
		//	Text txt = scoreLine.GetComponent<Text>();
			scoreLine.rectTransform.anchoredPosition = new Vector3(230, -30-i*75, 0);
			score.rectTransform.anchoredPosition = new Vector3(-130, -30-i*75, 0);
			i++;*/
			//scoreLine.rectTransform.position = new Vector3(227, 0, 0);
/*			Text tekstIgralca = new Text();
			tekstIgralca.transform.parent = displayPanel;
			tekstIgralca.rectTransform.localPosition = new Vector3();
			tekstIgralca.rectTransform.anchorMax = new Vector2(0.0f, 1.0f);
			tekstIgralca.rectTransform.anchorMin = new Vector2(0.0f, 1.0f);
			tekstIgralca.rectTransform.pivot = new Vector2(0.5f, 0.5f);*/

	}
	public void startDemo(){
		crosshairScreen.enabled = true;
		menuScreen.enabled = false;
		demoMode = true;
		startGame ();
	}
	public void startSimMenu(){
		inputScreen.enabled = true;
		menuScreen.enabled = false;
	}
	public void CalButton(){
		crosshairScreen.enabled = true;
		menuScreen.enabled = false;
		calMode = true;
	}
	public void startSim(){
		crosshairScreen.enabled = true;
		inputScreen.enabled = false;
		shotsFired = 0;
		simMode = true;
		playerIndex = 0;
		setNextPlayer ();
		startGame ();
	}
	public void exitButton(){
		backToMenu ();
		escapeScreen.enabled = false;
		simMode = false;
		calMode = false;
		demoMode = false;
	}
	public void ContinueButton(){
		crosshairScreen.enabled = true;
		escapeScreen.enabled = false;
	}
	void backToMenu(){
		inputScreen.enabled = false;
		crosshairScreen.enabled = false;
		menuScreen.enabled = true;
		menuButtons [0].interactable = true;
		menuButtons [1].interactable = true;
		menuButtons [2].interactable = !calibrated;
	}
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			escapeScreen.enabled = true;
			crosshairScreen.enabled = false;
		}
		if (calMode && calibrated) {
			backToMenu();
			calMode = false;
		}
		if (shotsFired > 4 && simMode) {
			shotsFired = 0;
			realPlayers[playerIndex].score+=int.Parse(scoreText.text);
			setNextPlayer();
		}
		if (Input.GetKeyDown (KeyCode.Space)) {
			NextTarget();
		//	Target = Instantiate (Target, new Vector3 (Random.Range(-2.0f, 2.0f), Random.Range(0.0f, 1.0f), Random.Range(5.0f, 15.0f)), Quaternion.identity) as GameObject;
		
		}

	}
	public void addToScore(int s){
		score = score + s;
		scoreText.text = score.ToString ();
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
