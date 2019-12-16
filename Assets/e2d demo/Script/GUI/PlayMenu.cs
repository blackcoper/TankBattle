using UnityEngine;
using System.Collections;

public class PlayMenu : MonoBehaviour {
	private GUIStyle labelStyle;
	public Texture live;
	public GameObject frefabPlayer;
	private GameObject player;
	void Start () {
		if(GameObject.FindGameObjectWithTag("Player")==null){
			LevelManager.is_play = true;
			spawnPlayer();
		}
		labelStyle = new GUIStyle();
	    labelStyle.fontSize = 44;
	 	labelStyle.normal.textColor = Color.red;
	}

	public void spawnPlayer()
	{
		Transform spawnPos = GameObject.FindGameObjectWithTag("Respawn").transform;
		player = (GameObject)Instantiate(frefabPlayer, spawnPos.transform.position, Quaternion.identity);
		player.name = "Player";
		player.transform.FindChild("Model").tag = "Player";
		player.transform.FindChild("Model").rigidbody.velocity = new Vector3(0,0,1);
		Camera mainCamera =	GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
		CameraFocus camFocus = mainCamera.GetComponent<CameraFocus>();
		camFocus.FocusTarget = player.transform.FindChild("Model").transform;
		camFocus.enabled = true;
		//GameObject.Find("MoveJoystick").SetActive(true);
		mainCamera.orthographicSize = 35f;
	}
	void OnGUI () {
		for(int i = 0; i < LevelManager.lives; i++){
			GUI.DrawTexture(new Rect(40*i+13, 13, 40, 40), live);
		}
		GUI.BeginGroup(new Rect(Screen.width - 55, 5, 50, 50));
	    if(GUI.Button(new Rect(0, 0, 50, 40), "Menu")) {
	    	this.enabled = false;
			LevelManager.game_stat = 1;
	    }
	    GUI.EndGroup(); 
	}
}