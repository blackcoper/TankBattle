using UnityEngine;
using System.Collections;

public class LevelManager : MonoBehaviour {
	// game stat 
	/* ==============================
	 * 1 = pause menu 
	 * 2 = play 
	 * 3 = restart
	 * 4 = score 
	 * ==============================
	 */
	public static int game_stat = 2;
	public static bool is_pause = false;
	public static bool is_play = false;
	public static bool is_destroy = false;
	public static int lives = 3;
	public static int Score = 0;
	private PauseMenu pauseMenu;
	private PlayMenu playMenu;
	private AboutMenu aboutMenu;
	private ScoreMenu scoreMenu;
	void Start () {
		pauseMenu = gameObject.transform.FindChild("PauseMenu").gameObject.GetComponent<PauseMenu>();
		playMenu = gameObject.transform.FindChild("PlayMenu").gameObject.GetComponent<PlayMenu>();
		aboutMenu = gameObject.transform.FindChild("AboutMenu").gameObject.GetComponent<AboutMenu>();
		scoreMenu = gameObject.transform.FindChild("ScoreMenu").gameObject.GetComponent<ScoreMenu>();
		pauseMenu.enabled = false;
		playMenu.enabled = false;
		aboutMenu.enabled = false;		
		scoreMenu.enabled = false;
	}
	public static void reset(){
		game_stat = 2;
		is_pause = false;
		is_play = false;
		is_destroy = false;
		lives = 3;
		Score = 0;
	}
	void Update () {
		switch(game_stat){
			case 1 :
				if(!pauseMenu.enabled)pauseMenu.enabled = true;
				break;
			case 2 :
				if(!playMenu.enabled)playMenu.enabled = true;
				break;
			case 3 :
				if(!aboutMenu.enabled)aboutMenu.enabled = true;
				break;
			case 4 :
				if(!scoreMenu.enabled)scoreMenu.enabled = true;
				break;
		}
		if(is_pause){
			Time.timeScale = 0;
		}else{
			Time.timeScale = 3;
		}
		if(is_destroy){
			if(lives>0){
				playMenu.spawnPlayer();
				lives--;
				is_destroy = false;
			}else{
				game_stat = 4;
			}
		}
		if(Input.GetKeyDown("escape")){
			game_stat = 1;
		}
	}
}
