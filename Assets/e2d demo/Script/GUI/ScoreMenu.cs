using UnityEngine;
using System.Collections;

public class ScoreMenu : MonoBehaviour {
	private GUIStyle labelStyle;
	void Start () {
		labelStyle = new GUIStyle();
	    labelStyle.fontSize = 44;
	 	labelStyle.normal.textColor = Color.red;
	}
	void OnEnable(){
		Camera mainCamera =	GameObject.Find("MainCamera").GetComponent<Camera>();
		CameraFocus camFocus = mainCamera.GetComponent<CameraFocus>();
		if(GameObject.FindGameObjectWithTag("Joystick")!=null)GameObject.FindGameObjectWithTag("Joystick").SetActive(false);
		camFocus.enabled = false;
		mainCamera.orthographicSize = 86.96f;
		mainCamera.transform.position = new Vector3(-26.46f, 59.3f, -17.9f);
	}
	void OnGUI () {
		GUI.BeginGroup(new Rect(Screen.width / 2 - 150, Screen.height / 2 - 200, 300, 600));
		string TitleText = "Game Over" ;
	    GUI.Label(new Rect(8, 13, 100, 40), TitleText, labelStyle);
		GUI.Label(new Rect(8, 53, 100, 40), "Your Score : "+ LevelManager.Score);
	    if(GUI.Button(new Rect(55, 150, 180, 40), "Back")) {
			LevelManager.reset();
	    	Application.LoadLevel("menu");
			this.enabled = false;
	    }
	    GUI.EndGroup(); 
	}
}