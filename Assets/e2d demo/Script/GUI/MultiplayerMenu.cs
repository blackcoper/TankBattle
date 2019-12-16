using UnityEngine;
using System.Collections;

public class MultiplayerMenu : MonoBehaviour {
	public int mode = 0;
	private Rect myWindowRect;
	string TitleText = "2D Tank Game";
	string subtitleText = "Multiplayer";
	private GUIStyle titleStyle;	
	private GUIStyle subtitleStyle;
			
	void Start(){
		titleStyle = new GUIStyle();
	    titleStyle.fontSize = 44;
	 	titleStyle.normal.textColor = Color.red;
		subtitleStyle = new GUIStyle();
	    subtitleStyle.fontSize = 18;
	 	subtitleStyle.normal.textColor = Color.red;
	}
	
	void OnGUI(){
		GUI.BeginGroup(new Rect(Screen.width / 2 - 150, Screen.height / 2 - 200, 300, 400));
		
		GUI.Label(new Rect(0, 13, 100, 40), TitleText, titleStyle);
		GUI.Label(new Rect(105, 75, 100, 40), subtitleText, subtitleStyle);
		
		if(GUI.Button(new Rect(55, 125, 180, 40), "Local Area Network")){
			this.enabled =false;
			GameManager.game_stat = 31;
		}			
		if(GUI.Button(new Rect(55, 175, 180, 40), "Online Network")){
			this.enabled =false;
			GameManager.game_stat = 32;
		}
		if(GUI.Button(new Rect(55, 275, 180, 40), "Back to Main Menu")) {
			this.enabled = false;
			GameManager.game_stat = 1;
		}
	    GUI.EndGroup(); 
	}
	
}
