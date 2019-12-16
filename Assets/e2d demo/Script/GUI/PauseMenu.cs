using UnityEngine;
using System.Collections;

public class PauseMenu : MonoBehaviour {
	private GUIStyle labelStyle;
	void Start () {
		labelStyle = new GUIStyle();
	    labelStyle.fontSize = 44;
	 	labelStyle.normal.textColor = Color.red;
	}
	void OnEnable(){
		LevelManager.is_pause = true;
		/*Camera mainCamera =	GameObject.Find("MainCamera").GetComponent<Camera>();
		mainCamera.orthographicSize = 86.96f;
		mainCamera.transform.position = new Vector3(-26.46f, 59.3f, -17.9f);*/
	}
	void Update(){
		if(Input.GetKeyDown("escape")){
			LevelManager.game_stat = 2;
			LevelManager.is_pause = false;
			this.enabled = false;
		}
		
	}
	void OnGUI () {
		GUI.BeginGroup(new Rect(Screen.width / 2 - 150, Screen.height / 2 - 200, 300, 600));
		string TitleText = "PAUSE";
	    GUI.Label(new Rect(8, 13, 100, 40), TitleText, labelStyle);
	    if(GUI.Button(new Rect(55, 100, 180, 40), "Resume game")) {
			LevelManager.game_stat = 2;
			LevelManager.is_pause = false;
	    	this.enabled = false;
	    }
		if(GUI.Button(new Rect(55, 150, 180, 40), "Restart")) {
			LevelManager.game_stat = 2;
			LevelManager.is_pause = false;
			LevelManager.reset();
			Application.LoadLevel(1);
			this.enabled = false;
	    }
	    if(GUI.Button(new Rect(55, 200, 180, 40), "Main Menu")) {
	    	Application.LoadLevel(0);
			LevelManager.game_stat = 2;
			LevelManager.is_pause = false;
			this.enabled = false;
	    }
	    GUI.EndGroup(); 
	}
}