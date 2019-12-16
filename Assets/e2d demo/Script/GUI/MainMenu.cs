using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour {
	private GUIStyle labelStyle;
	void Start () {
		labelStyle = new GUIStyle();
	    labelStyle.fontSize = 44;
	 	labelStyle.normal.textColor = Color.red;
	}
	void OnEnable(){
		Camera mainCamera =	GameObject.Find("MainCamera").GetComponent<Camera>();
		mainCamera.orthographicSize = 86.96f;
		mainCamera.transform.position = new Vector3(-26.46f, 59.3f, -17.9f);
	}
	void OnGUI () {
		GUI.BeginGroup(new Rect(Screen.width / 2 - 150, Screen.height / 2 - 200, 300, 400));
		string TitleText = "2D Tank Game";
	    GUI.Label(new Rect(0, 13, 100, 40), TitleText, labelStyle);
		string StartText = "Play Demo" ;
	    if(GUI.Button(new Rect(55, 125, 180, 40), StartText)) {
			Application.LoadLevel("level_demo");
	    	this.enabled = false;
	    }
		if(GUI.Button(new Rect(55, 175, 180, 40), "Multiplayer")) {
	    	this.enabled = false;
			GameManager.game_stat = 3;
	    }
		if(GUI.Button(new Rect(55, 225, 180, 40), "About")) {
	    	this.enabled = false;
			GameManager.game_stat = 4;
	    }
	    if(GUI.Button(new Rect(55, 275, 180, 40), "Quit")) {
	    	Application.Quit();
	    }
	    GUI.EndGroup(); 
	}
}