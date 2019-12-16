using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CoopMenu : MonoBehaviour {
	private GUIStyle labelStyle;
	public Texture live;
	public GameObject frefabPlayer;
	private GameObject player;
	private Hashtable players = new Hashtable();
	void Start () {
		//MasterServer.RegisterHost(gameName, "Tank Batle Host", "client host");
		/*if(GameObject.FindGameObjectWithTag("Player")==null){
			LevelManager.is_play = true;
			spawnPlayer();
		}*/
		labelStyle = new GUIStyle();
	    labelStyle.fontSize = 44;
	 	labelStyle.normal.textColor = Color.red;
	}
	//for chat messaging
	/*private bool isServer = false;
    private string username = "user";
    private Vector2 scrollPos = Vector2.zero;
    private bool isServerStarted = false;
    private bool isClientConnected = false;
	
	private Vector2 scrollPosition;
	private string inputField = "";*/
	private int playerCount = 0;
	void OnPlayerConnected(NetworkPlayer p) 
	{
		if(Network.isServer)
		{
			playerCount++;
			NetworkViewID newViewID = Network.AllocateViewID();
			networkView.RPC("JoinPlayer", RPCMode.AllBuffered, newViewID, Vector3.zero, p);
			Debug.Log("Player " + newViewID.ToString() + " connected from " + p.ipAddress + ":" + p.port);
			Debug.Log("There are now " + playerCount + " players.");
		}
    }
	void OnConnectedToServer()
	{
		networkView.RPC("SendAllPlayers", RPCMode.Server);
	}
	
	[RPC]
	void SendAllPlayers(NetworkMessageInfo info)
	{
		if(Network.isServer)
		{
			GameObject[] goPlayers = GameObject.FindGameObjectsWithTag("Player");
			foreach(GameObject gop in goPlayers)
			{
				NetworkPlayer gonp = gop.GetComponent<MultiplayerCarControl>().netPlayer;
				NetworkViewID gonvid = gop.GetComponent<NetworkView>().viewID;
				Debug.Log(gonp.ToString() +"!="+ info.sender.ToString());
				if(gonp.ToString() != info.sender.ToString())
				{
					networkView.RPC("JoinPlayer", info.sender, gonvid, gop.transform.position, gonp);
				}
	    	}
		}
	}
	void OnPlayerDisconnected(NetworkPlayer player) 
	{
		if(Network.isServer){
			playerCount--;
			Debug.Log("Player " + player.ToString() + " disconnected.");
			Debug.Log("There are now " + playerCount + " players.");
			networkView.RPC("DisconnectPlayer", RPCMode.All, player);
		}
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
		mainCamera.orthographicSize = 23.7f;
	}
	void OnGUI () {
		for(int i = 0; i < LevelManager.lives; i++){
			GUI.DrawTexture(new Rect(40*i+13, 13, 40, 40), live);
		}
		GUI.BeginGroup(new Rect(Screen.width - 205, 5, 200, 50));
	    if(GUI.Button(new Rect(0, 0, 80, 40), "Disconnect")) {
			Network.Disconnect();
            MasterServer.UnregisterHost();
	    	this.enabled = false;
			GetComponent<Menu_GUI>().enabled = true;
			//LevelManager.game_stat = 1;
	    }
	    GUI.EndGroup(); 
	}
	
	[RPC]
	void JoinPlayer(NetworkViewID newPlayerView, Vector3 pos, NetworkPlayer p)
	{
		//newPlayer.tag = "Player";
		Transform spawnPos = GameObject.FindGameObjectWithTag("Respawn").transform;
		GameObject newPlayer = (GameObject)Instantiate(frefabPlayer, spawnPos.transform.position, Quaternion.identity);
		//newPlayer.name = "Player";
		//newPlayer.transform.FindChild("Model").GetComponent<NetworkView>().viewID = newPlayerView;
		//newPlayer.transform.FindChild("Model").GetComponent<MultiplayerCarControl>().netPlayer = p;
		newPlayer.transform.FindChild("Model").tag = "Player";
		newPlayer.transform.FindChild("Model").rigidbody.velocity = new Vector3(0,0,1);
		if(newPlayerView.isMine){
			Camera mainCamera =	GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
			CameraFocus camFocus = mainCamera.GetComponent<CameraFocus>();
			camFocus.FocusTarget = newPlayer.transform.FindChild("Model").transform;
			camFocus.enabled = true;
			mainCamera.orthographicSize = 23.7f;
		}
		players.Add(p,newPlayer);
	}
	[RPC]
	void DisconnectPlayer(NetworkPlayer player)
	{
		if(Network.isClient) 
		{
			Debug.Log("Player Disconnected: " + player.ToString());
		}
		if(players.ContainsKey(player))
		{
			if((GameObject)players[player]) {
				Destroy((GameObject)players[player]);
			}
			players.Remove(player);
		}
	}
}