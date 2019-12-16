using UnityEngine;
using System.Collections;

public class AboutMenu : MonoBehaviour {
	private GUIStyle titleStyle;
	private GUIStyle subtitleStyle;
	private GUIStyle contentStyle;
	string subtitleText = "About";
	void Start () {
		titleStyle = new GUIStyle();
	 	titleStyle.normal.textColor = Color.red;
		titleStyle.fontSize = 42;
		subtitleStyle = new GUIStyle();
	    subtitleStyle.normal.textColor = Color.red;
		subtitleStyle.fontSize = 18;
	 	contentStyle = new GUIStyle();
	    contentStyle.normal.textColor = Color.red;
		contentStyle.fontSize = 16;
	}
	
	void OnGUI () {
		
		GUI.BeginGroup(new Rect(Screen.width / 2 - 150, Screen.height / 2 - 200, 300, 400));
		string TitleText = "2D Tank Game";
	    GUI.Label(new Rect(0, 13, 100, 40), TitleText, titleStyle);
		GUI.Label(new Rect(105, 75, 100, 40), subtitleText, subtitleStyle);
		GUI.Label(new Rect(55, 125, 100, 40), "2d game combat, side scroll.\n multiplayer v.1", contentStyle);
		if(GUI.Button(new Rect(55, 275, 180, 40), "Back")) {
	    	this.enabled = false;
			GameManager.game_stat = 1;
	    }
	    GUI.EndGroup(); 
	}
}