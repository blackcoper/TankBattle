using UnityEngine;
using System.Collections;

public class MultiplayerManager : MonoBehaviour {
	
	public static bool is_destroy = false;
	public static int lives = 3;
	public static int Score = 0;
	public GameObject frefabPlayer;
	private GameObject player;
	public static Transform spawnPos;
	public string gameName = "TankBattle";
	public Texture live_texture;
	void Start(){
		Time.timeScale = 3f;
		spawnPos = GameObject.FindGameObjectWithTag("Respawn").transform;
		int _client = System.Int32.Parse(Network.player.ToString());
		spawnPos.transform.position = spawnPos.transform.position+(new Vector3(_client*10,0,0));
		Network.isMessageQueueRunning = true;
		if(Network.isServer){
			MasterServer.RegisterHost(gameName, "myGameTypeName", "MyComment");
		}
		spawn_player();
	}
	
	void follow_ally(){
		GameObject[] ally_player = GameObject.FindGameObjectsWithTag("Player");
		if(ally_player.Length>0){
			Camera mainCamera =	GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
			CameraFocus camFocus = mainCamera.GetComponent<CameraFocus>();
			camFocus.FocusTarget = ally_player[0].transform;
			ally_player[0].AddComponent("AudioListener");
		}else{
			if(ally_player.Length>0)follow_ally();
			return;
		}
	}
	
	void spawn_player(){
		GameObject player = Network.Instantiate(frefabPlayer, spawnPos.transform.position, spawnPos.transform.rotation, 0) as GameObject;
		Camera mainCamera =	GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
		CameraFocus camFocus = mainCamera.GetComponent<CameraFocus>();
		camFocus.FocusTarget = player.transform.FindChild("Model").transform;
		camFocus.enabled = true;
		mainCamera.orthographicSize = 35f;
	}
	
	void OnGUI ()
	{
		if (Network.peerType == NetworkPeerType.Disconnected){
			Application.LoadLevel("menu");
		}else{
			//We've got a connection(s)!
			if (Network.peerType == NetworkPeerType.Connecting){
				GUILayout.Label("Connection status: Connecting");
			} else if (Network.peerType == NetworkPeerType.Client){
				GUILayout.Label("Connection status: Client!");
				GUILayout.Label("Ping to server: "+Network.GetAveragePing(Network.connections[0]));				
			} else if (Network.peerType == NetworkPeerType.Server){
				GUILayout.Label("Connection status: Server!");
				GUILayout.Label("Connections: "+Network.connections.Length);
				if(Network.connections.Length>=1){
					GUILayout.Label("Ping to first player: "+Network.GetAveragePing(  Network.connections[0] ) );
				}
			}
			if (GUILayout.Button ("Disconnect"))
			{
				Network.Disconnect(200);
			}
			if(is_destroy){
				if(lives>0){
					spawn_player();
					lives--;
					is_destroy = false;
				}else{
					follow_ally();
				}
			}
			for(int i = 0; i < lives; i++){
				GUI.DrawTexture(new Rect(40*i+13, 13, 40, 40), live_texture);
			}
		}
		
	}
	
	//CLient function
	void OnDisconnectedFromServer(NetworkDisconnection info) {
		Debug.Log("This CLIENT has disconnected from a server");
	}
		
	//Server functions called by Unity
	void OnPlayerConnected(NetworkPlayer player) {
		Debug.Log("Player connected from: " + player.ipAddress +":" + player.port);
	}
	
	void OnPlayerDisconnected(NetworkPlayer player) {
		Debug.Log("Player disconnected from: " + player.ipAddress+":" + player.port);
		Debug.Log("Server destroying player");
		Network.RemoveRPCs(player, 0);
		Network.DestroyPlayerObjects(player);
	}
	
	void Update () {
		
	}
}
