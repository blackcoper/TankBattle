using UnityEngine;
using System.Collections;

public class Tutorial1b : MonoBehaviour {
	private bool read = false;
	private bool showGUI = false;
	private GUIStyle labelStyle;
	void Start () {
		labelStyle = new GUIStyle();
	    labelStyle.fontSize = 42;
	 	labelStyle.normal.textColor = Color.red;
	}
	void OnTriggerEnter (Collider other) {
		if(other.tag == "Player" && !read){
			LevelManager.is_pause = true;
			showGUI = true;
		}
	}
	void OnGUI()
	{ 
		if(showGUI){
			GUI.BeginGroup(new Rect(Screen.width / 2 - 150, Screen.height / 2 - 200, 300, 200));
			GUI.Box(new Rect(0,0,300,200),"");
		    GUI.Label(new Rect(8, 13, 300, 40), "Info", labelStyle);
			GUI.Label(new Rect(8, 65, 300, 40), "use arrow up or down change angle weapon.");
			GUI.Label(new Rect(8, 75, 300, 40), "click to shoot.");
			GUI.Label(new Rect(8, 85, 300, 40), "enjoy.");
		    if(GUI.Button(new Rect(55, 150, 180, 40), "Close")) {
		    	read = true;
				showGUI = false;
				LevelManager.is_pause = false;
		    }
		    GUI.EndGroup(); 
		}
	}
}
