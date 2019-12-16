using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {
	// game stat 
	/* ==============================
	 * 1 = main menu 
	 * 2 = play 
	 * 3 = multiplayer
	 * 3 1 = local
	 * 3 2 = online
	 * 4 = about
	 * 5 = score 
	 * ==============================
	 */
	public static int game_stat = 1;
	public static bool is_pause = false;
	//public static bool is_play = false;
	//public static bool is_destroy = false;
	//private int lives = 1;
	//public static int Score = 0;
	private MainMenu mainMenu;
	private PlayMenu playMenu;
	private MultiplayerMenu multiplayerMenu;
	private LanMenu lanMenu;
	private OnlineMenu onlineMenu;
	private AboutMenu aboutMenu;
	private ScoreMenu scoreMenu;
	void Start () {
		mainMenu = gameObject.transform.FindChild("MainMenu").gameObject.GetComponent<MainMenu>();
		playMenu = gameObject.transform.FindChild("PlayMenu").gameObject.GetComponent<PlayMenu>();
		multiplayerMenu = gameObject.transform.FindChild("MultiplayerMenu").gameObject.GetComponent<MultiplayerMenu>();
		lanMenu = multiplayerMenu.transform.FindChild("LanMenu").gameObject.GetComponent<LanMenu>();
		onlineMenu = multiplayerMenu.transform.FindChild("OnlineMenu").gameObject.GetComponent<OnlineMenu>();
		aboutMenu = gameObject.transform.FindChild("AboutMenu").gameObject.GetComponent<AboutMenu>();
		scoreMenu = gameObject.transform.FindChild("ScoreMenu").gameObject.GetComponent<ScoreMenu>();
		mainMenu.enabled = false;
		playMenu.enabled = false;
		aboutMenu.enabled = false;	
		scoreMenu.enabled = false;	
		multiplayerMenu.enabled = false;
		lanMenu.enabled = false;
		onlineMenu.enabled = false;
	}

	void Update () {
		switch(game_stat){
			case 1 :
				if(!mainMenu.enabled)mainMenu.enabled = true;
				break;
			case 2 :
				if(!playMenu.enabled)playMenu.enabled = true;
				break;
			case 3 :
				if(!multiplayerMenu.enabled)multiplayerMenu.enabled = true;
				break;
			case 31 :
				if(!lanMenu.enabled)lanMenu.enabled = true;
				break;
			case 32 :
				if(!onlineMenu.enabled)onlineMenu.enabled = true;
				break;
			case 4 :
				if(!aboutMenu.enabled)aboutMenu.enabled = true;
				break;
			case 5 :
				if(!scoreMenu.enabled)scoreMenu.enabled = true;
				break;
		}
		if(Input.GetKeyDown("escape")){
			mainMenu.enabled = false;
			playMenu.enabled = false;
			aboutMenu.enabled = false;	
			scoreMenu.enabled = false;	
			multiplayerMenu.enabled = false;
			if(game_stat < 10){
				game_stat = 1;
			}
		}
	}
}
