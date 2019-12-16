using UnityEngine;
using System.Collections;

public class LanMenu : MonoBehaviour {
		
	int serverPort = 25001;
	string gameName = "TankBatle-Lan";
	
	private bool launchingGame = false;
	private bool hostGame = false;
	private bool joinGame = false;
	private bool showMenu = false;
	
	string TitleText = "2D Tank Game";
	string subtitleText = "Local Area Network";
	private GUIStyle titleStyle;	
	private GUIStyle subtitleStyle;
	private ArrayList playerList = new ArrayList();
	class PlayerInfo {
		public string username;
		public NetworkPlayer player;
	}
	
	private int serverMaxPlayers = 4;
	private string serverTitle = "Loading..";
	private bool serverPasswordProtected = false;
	private string playerName = "";
	
	private string serverIP = "";
	private bool nowConnecting = false;
	
	private string error_message = "";
	private LobbyChat chat;
	void Start(){
		titleStyle = new GUIStyle();
	    titleStyle.fontSize = 44;
	 	titleStyle.normal.textColor = Color.red;
		subtitleStyle = new GUIStyle();
	    subtitleStyle.fontSize = 18;
	 	subtitleStyle.normal.textColor = Color.red;
		serverIP = Network.player.ipAddress;
		chat = transform.GetComponent<LobbyChat>();
	}
	
	void OnEnable(){
		playerName = PlayerPrefs.GetString("playerName");
		lastRegTime=Time.time-3600;
		launchingGame=false;
		if(chat==null)chat = transform.GetComponent<LobbyChat>();
		chat.enabled = true;
		networkView.enabled = true;
		
	}
		
	void OnDisable(){
		chat.enabled = false;
		networkView.enabled = false;
	}
	string TestConnection() {
		// Start/Poll the connection test, report the results in a label and 
		// react to the results accordingly
		string testMessage = "";
		string testStatus = "";
		bool doneTesting = false;
		ConnectionTesterStatus	connectionTestResult = Network.TestConnection();
		switch (Network.TestConnection()) {
			case ConnectionTesterStatus.Error: 
				testMessage = "Problem determining NAT capabilities";
				doneTesting = true;
				break;
			case ConnectionTesterStatus.Undetermined: 
				testMessage = "Undetermined NAT capabilities";
				doneTesting = false;
				break;
			case ConnectionTesterStatus.PublicIPIsConnectable:
				testMessage = "Directly connectable public IP address.";
				doneTesting = true;
				break;
			case ConnectionTesterStatus.PublicIPPortBlocked:
				testMessage = "Non-connectable public IP address (port " +
					serverPort +" blocked), running a server is impossible.";
				break;
			case ConnectionTesterStatus.PublicIPNoServerStarted:
				testMessage = "Public IP address but server not initialized, "+
					"it must be started to check server accessibility. Restart "+
					"connection test when ready.";
				break;
							
			case ConnectionTesterStatus.LimitedNATPunchthroughPortRestricted:
				testMessage = "Limited NAT punchthrough capabilities. Cannot "+
					"connect to all types of NAT servers. Running a server "+
					"is ill advised as not everyone can connect.";
				doneTesting = true;
				break;
			case ConnectionTesterStatus.LimitedNATPunchthroughSymmetric:
				testMessage = "Limited NAT punchthrough capabilities. Cannot "+
					"connect to all types of NAT servers. Running a server "+
					"is ill advised as not everyone can connect.";
				doneTesting = true;
				break;
			
			case ConnectionTesterStatus.NATpunchthroughAddressRestrictedCone:
			case ConnectionTesterStatus.NATpunchthroughFullCone:
				testMessage = "NAT punchthrough capable. Can connect to all "+
					"servers and receive connections from all clients. Enabling "+
					"NAT punchthrough functionality.";
				doneTesting = true;
				break;
			default: 
				testMessage = "Error in test routine, got " + connectionTestResult;
				break;
		}
		return testMessage+"\n"+testStatus;
	}
	
	IEnumerator leaveLobby(){
		if (Network.isServer || Network.isClient){
			if(Network.isServer){
				MasterServer.UnregisterHost();
			}
			Network.Disconnect();
			yield return new WaitForSeconds(0.3f);
		}
	}
	
	void OnGUI () {
		//for debug NAT 
		GUI.Label(new Rect(0, 0, Screen.width, 200), TestConnection());		
		if(launchingGame){		
			launchingGameGUI();
		} else if(hostGame){		
			if(!Network.isServer)hostSettings();else showLobby();
			
		} else if(joinGame){		
			if(!Network.isClient)GUILayout.Window (0, new Rect(Screen.width/2-190,Screen.height/2-90,380,280), directConnectGUIWindow, "enter ip host game");else showLobby();
		}else {
			GUI.BeginGroup(new Rect(Screen.width / 2 - 150, Screen.height / 2 - 200, 300, 400));
			GUI.Label(new Rect(0, 13, 100, 40), TitleText, titleStyle);
			GUI.Label(new Rect(70, 75, 100, 40), subtitleText, subtitleStyle);
			if(GUI.Button(new Rect(55, 125, 180, 40), "Create")){
				hostGame = true;
			}
			if(GUI.Button(new Rect(55, 175, 180, 40), "Join")){
				joinGame = true;
			}
			if(GUI.Button(new Rect(55, 275, 180, 40), "Cancel") || Input.GetKeyDown("escape")){
				this.enabled = false;
				GameManager.game_stat = 3;
			}
		    GUI.EndGroup();
		}
		
		//for leaving lobby
		/*if(GUI.Button(new Rect(40,10,180, 40), "Back to main menu")){
			leaveLobby();
		}*/
	}
	
	private string hostSetting_title = "No server title";
	private int hostSetting_players = 4;
	private string hostSetting_password = "";
	
	void hostSettings(){
		
		GUI.BeginGroup (new Rect (Screen.width/2-175, Screen.height/2-75-50, 350, 170));
		GUI.Box (new Rect (0,0,350,170), "Create Host Game");
		
		GUI.Label (new Rect (10,20,150,20), "Server title");
		hostSetting_title = GUI.TextField (new Rect (175,20,160,20), hostSetting_title);
		
		GUI.Label (new Rect (10,40,150,20), "Max. players (2-32)");
		hostSetting_players = int.Parse(GUI.TextField (new Rect (175,40,160,20), hostSetting_players+""));
		
		GUI.Label (new Rect (10,60,150,50), "Password\n");
		hostSetting_password = (GUI.TextField (new Rect (175,60,160,20), hostSetting_password));
		
		if(GUI.Button (new Rect (10,115,120, 40), "cancel") || Input.GetKeyDown("escape"))hostGame = false;
		
		if(GUI.Button (new Rect (150,115,180, 40), "Go to lobby")){
			StartHost(hostSetting_password, int.Parse(hostSetting_players.ToString()), hostSetting_title);
		}
		GUI.EndGroup();
	}
	
	void directConnectGUIWindow(int id){
		GUILayout.BeginVertical();
		GUILayout.Space(5);
		GUILayout.EndVertical();
		if(nowConnecting){
			GUILayout.BeginHorizontal();
			GUILayout.Space(10);
			GUILayout.Label("Trying to connect to "+serverIP+":"+serverPort);
			GUILayout.Space(10);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Space(10);
			GUILayout.FlexibleSpace();
			if(GUI.Button (new Rect (10,115,120, 40), "cancel")){
				Network.Disconnect();
				nowConnecting = false;
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		} else {		
			GUILayout.BeginVertical();
			GUILayout.Space(10);
				serverIP = GUILayout.TextField(serverIP, GUILayout.Height(40));
				hostSetting_password = GUILayout.TextField(hostSetting_password, GUILayout.Height(40));
				//connectPort = int.Parse(GUILayout.TextField(connectPort+""));
				if(error_message!="")GUI.Label(new Rect(10,80,340, 100),error_message);
			GUILayout.Space(10);
			GUILayout.EndVertical();
			GUILayout.BeginHorizontal();
			GUILayout.Space(10);
			GUILayout.FlexibleSpace();
			if(GUI.Button (new Rect (10,115,120, 40), "cancel") || Input.GetKeyDown("escape")){
				error_message="";
				joinGame = false;
			}
			if(GUI.Button (new Rect (150,115,180, 40), "Connect")){
				error_message="";
				Network.Connect(serverIP, serverPort, hostSetting_password);
				nowConnecting =true;
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}
	}
	
	void StartHost(string password, int players, string serverName){
		if(players<1){
			players=1;
		}
		if(players>=32){
			players=32;
		}
		if(password!=null && password!=""){
			serverPasswordProtected  = true;
			Network.incomingPassword = password;
		}else{
			serverPasswordProtected  = false;
			Network.incomingPassword = "";
		}
		
		serverTitle = serverName;
	
		//Network.InitializeSecurity();
		Network.InitializeServer((players-1), serverPort);
	}
	
	
	void showLobby(){
		if(Network.isClient)nowConnecting = false;
		string players = "";
		int currentPlayerCount = 0;
		foreach(PlayerInfo playerInstance in playerList) {
			players=playerInstance.username+"\n"+players;
			currentPlayerCount++;
		}
		GUI.BeginGroup (new Rect (Screen.width/2-200, Screen.height/2-200, 400, 180));
		GUI.Box (new Rect (0,0,400,200), "Game lobby");
		var pProtected="no";
		if(serverPasswordProtected){
			pProtected="yes";
		}
		if(Network.isServer){
			GUI.Label(new Rect (10,20,100,100), "IP HOST");
			GUI.Label (new Rect (150,20,100,100), Network.player.ipAddress);
		}
		GUI.Label (new Rect (10,20,150,20), "Password protected");
		GUI.Label (new Rect (150,20,100,100), pProtected);
		
		GUI.Label (new Rect (10,40,150,20), "Server title");
		GUI.Label (new Rect (150,40,100,100), serverTitle);
		
		GUI.Label (new Rect (10,60,150,20), "Players");
		GUI.Label (new Rect (150,60,100,100), currentPlayerCount+"/"+serverMaxPlayers);
		
		GUI.Label (new Rect (10,80,150,20), "Current players");
		GUI.Label (new Rect (150,80,100,100), players);
		if(Network.isServer){
			if(GUI.Button (new Rect (150,115,180, 40), "Start the game")){
				HostLaunchGame();
			}
		}else{
			GUI.Label (new Rect (25,140,200,40), "Waiting for the server to start the game..");
		}
		if(GUI.Button (new Rect (10,115,120, 40), "cancel") || Input.GetKeyDown("escape"))StartCoroutine(leaveLobby());
		
		GUI.EndGroup();
	}
	
	void OnFailedToConnect(NetworkConnectionError error) {
		if(enabled){
	        nowConnecting = false;
			error_message = "Could not connect to server: " + error;
		}
    }
	void OnConnectedToServer(){
		if(enabled){
			playerList  = new ArrayList();
			playerName = PlayerPrefs.GetString("playerName","");
			networkView.RPC("addPlayer",RPCMode.AllBuffered, Network.player, playerName);
		}
	}
	
	
	void OnServerInitialized(){
		if(enabled){
			playerList  = new ArrayList();
			networkView.RPC("addPlayer",RPCMode.AllBuffered, Network.player, playerName);
			
			bool pProtected = false;
			if(Network.incomingPassword!=null && Network.incomingPassword!=""){
				pProtected=true;
			}
			int maxPlayers = Network.maxConnections+1;
			networkView.RPC("setServerSettings",RPCMode.AllBuffered, pProtected, maxPlayers, hostSetting_title);
		}
	}
	
	
	float lastRegTime = -60;
	void Update(){
		if(Network.isServer && lastRegTime<Time.time-60){
			lastRegTime=Time.time;
			MasterServer.RegisterHost(gameName,hostSetting_title, "No description");
		}
	}
		
	void OnPlayerDisconnected(NetworkPlayer player) {
		if(enabled){
			networkView.RPC("playerLeft", RPCMode.All, player);
			LobbyChat chat= GetComponent<LobbyChat>();
			chat.addGameChatMessage("A player left the lobby");
		}
	}
	
	void HostLaunchGame(){
		if(!Network.isServer){
			return;
		}
		Network.maxConnections = -1;
		MasterServer.UnregisterHost();
		networkView.RPC("launchGame",RPCMode.All);
	}
	
	void launchingGameGUI(){
		GUI.Box(new Rect(Screen.width/4+180,Screen.height/2-30,280,50), "");
		if(Application.CanStreamedLevelBeLoaded("level_multiplayer")){
			GUI.Label(new Rect(Screen.width/4+200,Screen.height/2-25,285,150), "Loaded, starting the game!");
			Application.LoadLevel("level_multiplayer");
		}else{
			GUI.Label(new Rect(Screen.width/4+200,Screen.height/2-25,285,150), "Starting..Loading the game: "+Mathf.Floor(Application.GetStreamProgressForLevel("level_multiplayer")*100)+" %");
		}
	}
	
	[RPC]
	void setServerSettings(bool password, int maxPlayers, string newSrverTitle){
		serverMaxPlayers = maxPlayers;
		serverTitle  = newSrverTitle;
		serverPasswordProtected  = password;
	}
	
	[RPC]
	void addPlayer(NetworkPlayer player, string username){
		Debug.Log("got addplayer "+username);
		PlayerInfo playerInstance = new PlayerInfo();
		playerInstance.player = player;
		playerInstance.username = username;		
		playerList.Add(playerInstance);
	}
	
	[RPC]
	void playerLeft(NetworkPlayer player){
		PlayerInfo deletePlayer = null;
		foreach (PlayerInfo playerInstance in playerList) {
			if (player == playerInstance.player) {			
				deletePlayer = playerInstance;
			}
		}
		playerList.Remove(deletePlayer);
		Network.RemoveRPCs(player);
		Network.DestroyPlayerObjects(player);
	}
		
	[RPC]
	void launchGame(){
		Network.isMessageQueueRunning=false;
		launchingGame=true;
	}
	
}
