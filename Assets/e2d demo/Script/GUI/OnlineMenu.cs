using UnityEngine;
using System.Collections;

public class OnlineMenu : MonoBehaviour {
	class PlayerInfo {
		public string username;
		public NetworkPlayer player;
	}
	private bool launchingGame = false;
	private ArrayList playerList = new ArrayList();
	private int serverMaxPlayers = 4;
	private string serverTitle = "Loading..";
	private bool serverPasswordProtected = false;
	private string playerName = "";
	private bool hostGame = false;
	private bool joinGame = false;
	private bool showMenu = false;
	
	private Rect windowRect1;
	private Rect windowRect2;
	private Rect windowRect3;
	static bool playNowMode = false;
	static bool login = false;
	static float playNowModeStarted = 0.0f;
	static string myPlayerName = "";
	private int hostPlayers = 8;
	private int hostPort =  25000;
	private int connectPort;
	private string connectIP = "";
	private Menu_multiplayerCode multiplayerScript;
	private string currentMenu = "";
	
	public string gameName = "TankBattle";
	public int serverPort =  23466;
	public string myMasterServerIP = "192.168.0.65"; //ip = System.Net.Dns.GetHostEntry("www.gameserver.com");
	public int myFacilitatorPort = 50005;
	public int myMasterServerPort = 23466;
	public int myConnectionTesterPort = 10737;
	public HostData[] hostData;
	private ConnectionTesterStatus natCapable = ConnectionTesterStatus.Undetermined;
	public bool filterNATHosts = false;
	private bool probingPublicIP = false;
	public bool doneTestingNAT = false;
	private float timer = 0.0f;
	private bool hideTest = false;
	private string testMessage = "Undetermined NAT capabilities";
	private bool tryingToConnectPlayNow = false;
	public int tryingToConnectPlayNowNumber = 0;
	private int[] remotePort = new int[3];
	private string[] remoteIP = new string[3];
	public string connectionInfo = "";
	public bool lastMSConnectionAttemptForcedNat = false;
	private bool NAToptionWasSwitchedForTesting = false;
	private bool officialNATstatus = true;
	public string errorMessage = "";
	private float lastPlayNowConnectionTime;	
	public bool nowConnecting = false;
	public ArrayList sortedHostList;
	private LobbyChat chat;
	
	void Awake ()
	{	
		//Screen.lockCursor = false;
		//myPlayerName = PlayerPrefs.GetString("playerName");
		multiplayerScript =	GetComponent<Menu_multiplayerCode>();
		//hostPort = serverPort;
		connectPort = serverPort;
		connectIP = "127.0.0.1";
		windowRect1 = new Rect(Screen.width/2-310,Screen.height/2-90,380,280);
		windowRect2 = new Rect(Screen.width/2+85,Screen.height/2-90,220,100);
		windowRect3 = new Rect(Screen.width/2+85,Screen.height/2+55,220,100);
		playNowMode=false;
		login=false;
		
		sortedHostList = new ArrayList();
		
		// Start connection test
		natCapable = Network.TestConnection();
			
		if (Network.HavePublicAddress()){
			Debug.Log("This machine has a public IP address");
		}else{
			Debug.Log("This machine has a private IP address");
		}	
		
		//If you dont want to use the Unity masterserver..
		MasterServer.ipAddress = myMasterServerIP;
		MasterServer.port = myMasterServerPort;//Change this
		Network.natFacilitatorIP = myMasterServerIP;
		Network.natFacilitatorPort = myFacilitatorPort;//Change this		
		Network.connectionTesterIP = myMasterServerIP;
		Network.connectionTesterPort = myConnectionTesterPort;//Change this
		
		//autoconnect chat server	
		auto_connect_chat();
	}
	
	void OnEnable(){
		myPlayerName = PlayerPrefs.GetString("playerName", "");

		lastRegTime=Time.time-3600;
		launchingGame=false;
		if(chat==null)chat = transform.GetComponent<LobbyChat>();
		chat.enabled = true;
		networkView.enabled = true;
		
		Camera mainCamera =	GameObject.Find("MainCamera").GetComponent<Camera>();
		mainCamera.orthographicSize = 86.96f;
		mainCamera.transform.position = new Vector3(-26.46f, 59.3f, -17.9f);
	}
		
	void OnDisable(){
		chat.enabled = false;
		networkView.enabled = false;
	}
	void OnGUI ()
	{				
		if(errorMessage!="" && errorMessage!=""){	
			GUI.Box(new Rect(Screen.width/2-100,Screen.height/2-60,200,60), "Error");
			GUI.Label(new Rect(Screen.width/2-90,Screen.height/2-50,180,50), errorMessage);
			if(GUI.Button(new Rect(Screen.width/2+40,Screen.height/2-30,50,20), "Close") || Input.GetKeyDown("escape")){
				errorMessage="";
			}
		}	
		if(launchingGame){		
			launchingGameGUI();
		} else if(Network.isClient || Network.isServer){
			showLobby();
		}else if(playNowMode){
			playNowFunction();
		}else if(login){
			if(errorMessage==null || errorMessage==""){ //Hide windows on error
				if(hostGame){
					windowRect1 = GUILayout.Window (0, windowRect1,	hostGUI, "Setting Host Game");
				}else{
					windowRect1 = GUILayout.Window (0, windowRect1,	listGUI, "Join a game");
				}
				if(GUI.Button(new Rect(455,90,140,30), "Log out") || Input.GetKeyDown("escape")){
					currentMenu="";
					login=false;
				}
				if(GUI.Button(new Rect(400,150,150,30), "Quickplay") ){
					currentMenu="playnow";
					playNowMode=true;
					playNowModeStarted=Time.time;		
				}
			}			
		}else{		
			GUI.Box(new Rect(90, 180, 260, 105), "Playername");
			GUI.Label(new Rect(100, 195, 250, 100), "Please enter your name:");
			
			myPlayerName = GUI.TextField(new Rect(178, 215, 147, 27), myPlayerName);	
						
			if(myPlayerName==""){
				GUI.Label(new Rect(100, 240, 260, 100), "After entering your name you can start playing!");
				return;
			}
			
			GUI.Label(new Rect(100, 240, 260, 100), "Click on quickplay to start playing right away!");
			
			if(GUI.Button(new Rect(400,180,180,40), "Login") ){
				PlayerPrefs.SetString("playerName", myPlayerName);
				StartCoroutine(seekHost());
				login=true;
			}
			if(GUI.Button(new Rect(400,245,180,40), "Cancel") || Input.GetKeyDown("escape")){
				this.enabled = false;
				GameManager.game_stat = 3;
			}
				
		}
	}
		
	void playNowFunction(){
			if(GUI.Button(new Rect(490,185,75,20), "Cancel")){
				Network.Disconnect();
				currentMenu="";
				playNowMode=false;
			}
			
			GUI.Box(new Rect(Screen.width/4+0,Screen.height/2-50,420,50), "");
	
			if(tryingToConnectPlayNowNumber>=10){
				
				serverPasswordProtected  = false;
				Network.incomingPassword = "";
				
				//If players get fed up waiting they can choose to start a host right away
				if(GUI.Button(new Rect(400,185,75,20), "Just host")){
					StartHost("", hostPlayers, "random host title here...");
				}
			}
			
			string connectStatus = PlayNow(playNowModeStarted);
			
			if(connectStatus=="failed"){
				//Couldn't find a proper host; host ourselves
				Debug.Log("PlayNow: No games hosted, so hosting one ourselves");	
				StartHost("", 7, "random host title here...");				
			}else{
				//Still trying to connect to the first hit
				GUI.Label(new Rect(Screen.width/4+10,Screen.height/2-45,385,50), connectStatus);
			}
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
	IEnumerator leaveLobby(){
		if (Network.isServer || Network.isClient){
			if(Network.isServer){
				MasterServer.UnregisterHost();
			}
			Network.Disconnect();
			yield return new WaitForSeconds(0.3f);
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
		if(Application.CanStreamedLevelBeLoaded ("level_multiplayer")){
			GUI.Label(new Rect(Screen.width/4+200,Screen.height/2-25,285,150), "Loaded, starting the game!");
			Application.LoadLevel( "level_multiplayer" );
		}else{
			GUI.Label(new Rect(Screen.width/4+200,Screen.height/2-25,285,150), "Starting..Loading the game: "+Mathf.Floor(Application.GetStreamProgressForLevel("level_multiplayer")*100)+" %");
		}
	}
	
	private string hostSetting_title = "No server title";
	private int hostSetting_players = 4;
	private string hostSetting_password = "";
	
	void hostGUI(int id){
		GUILayout.BeginVertical();
		GUILayout.Space(10);
		GUILayout.EndVertical();
		GUILayout.BeginHorizontal();
		GUILayout.Space(10);
			GUILayout.Label("Server title");
			hostSetting_title = GUILayout.TextField(hostSetting_title, GUILayout.MaxWidth(75));
		GUILayout.Space(10);
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Space(10);
			GUILayout.Label("Max. players (2-32)");
			hostSetting_players = int.Parse(GUILayout.TextField(hostSetting_players.ToString(), GUILayout.MaxWidth(75)));
		GUILayout.Space(10);
		GUILayout.EndHorizontal();
		
		
		GUILayout.BeginHorizontal();
		GUILayout.Space(10);
			GUILayout.Label("Password");
			hostSetting_password = GUILayout.TextField(hostSetting_password, GUILayout.MaxWidth(75));
		GUILayout.Space(10);
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
			// Start a new server
			if(GUILayout.Button ("cancel") || Input.GetKeyDown("escape"))hostGame = false;
			if (GUILayout.Button ("Go to lobby")){
				lastRegTime=Time.time-3600;
				StartHost(hostSetting_password, int.Parse(hostSetting_players.ToString()), hostSetting_title);
			}			
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
	}
	
	private Vector2 scrollPosition;

	void listGUI (int id) {
	
		GUILayout.BeginVertical();
		GUILayout.Space(6);
		GUILayout.EndVertical();
	
		GUILayout.BeginHorizontal();
		GUILayout.Space(200);

		// Refresh hosts
		if (GUILayout.Button ("Create Host"))hostGame = true;
		if (GUILayout.Button ("Refresh available Servers"))
		{
			StartCoroutine(FetchHostList(true));
		}
		StartCoroutine(FetchHostList(false));
			
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		//scrollPosition = GUI.BeginScrollView (Rect (0,60,385,200),	scrollPosition, Rect (0, 100, 350, 3000));
		scrollPosition = GUILayout.BeginScrollView (scrollPosition);

		int aHost = 0;
		
		if(sortedHostList!=null && sortedHostList.Count>=1){
			foreach(int myElement in sortedHostList)
			{
				var element=hostData[myElement];
				GUILayout.BeginHorizontal();

				// Do not display NAT enabled games if we cannot do NAT punchthrough
				if ( !(filterNATHosts && element.useNat) )
				{				
					aHost=1;
					var name = element.gameName + " ";
					GUILayout.Label(name);	
					GUILayout.FlexibleSpace();
					GUILayout.Label(element.connectedPlayers + "/" + element.playerLimit);
					
					if(element.useNat){
						GUILayout.Label(".");
					}
					GUILayout.FlexibleSpace();
					GUILayout.Label("[" + element.ip[0] + ":" + element.port + "]");	
					GUILayout.FlexibleSpace();
					if(!nowConnecting){
					if (GUILayout.Button("Connect"))
						{
							Connect(element.ip[0], element.port);
						}
					}else{
						GUILayout.Button("Connecting");
					}
					GUILayout.Space(15);
				}
				GUILayout.EndHorizontal();	
			}
		}		
		GUILayout.EndScrollView ();
		if(aHost==0){
			GUILayout.Label("No games hosted at the moment..");
		}
	}
	
	void auto_connect_chat()
	{
		//Network.Connect(myMasterServerIP, myMasterServerPort);
		/*int i =0;
		foreach(int myElement in sortedHostList)
		{
			var element=hostData[myElement];
			if ( !(filterNATHosts && element.useNat) && !element.passwordProtected  )
			{
				if(element.connectedPlayers<element.playerLimit)
				{					
					if(tryingToConnectPlayNow){
						string natText=" with option 2/2";
						if((lastPlayNowConnectionTime+CONNECT_TIMEOUT<=Time.time) || (lastPlayNowConnectionTime+CONNECT_NAT_TIMEOUT<=Time.time)){
							Debug.Log("Interrupted by timer, NAT:");
							FailedConnRetry(NetworkConnectionError.ConnectionFailed);							
						}
						return "Trying to connect to host "+(tryingToConnectPlayNowNumber+1)+"/"+sortedHostList.Count+" "+natText;	
					}		
					if(!tryingToConnectPlayNow && tryingToConnectPlayNowNumber<=i){
						Debug.Log("Trying to connect to game NR "+i+" & "+tryingToConnectPlayNowNumber);
						tryingToConnectPlayNow=true;
						tryingToConnectPlayNowNumber=i;
						lastMSConnectionAttemptForcedNat=element.useNat;
						int connectPort = element.port;
						print("Connecting directly to host");
						Debug.Log("connecting to "+element.gameName+" "+element.ip+":"+connectPort);
						Network.Connect(element.ip, connectPort);	
						lastPlayNowConnectionTime=Time.time;
					}
					i++;		
				}
			}
		}
		if(Time.time<timeStarted+7){
			FetchHostList(true);	
			return "Waiting for masterserver..."+Mathf.Ceil((timeStarted+7)-Time.time);	
		}
		if(!tryingToConnectPlayNow){
			return "failed";
		}
		return "done";*/
	}
	
	void Start(){//must be in start because of coroutine
		StartCoroutine(seekHost());
	}
	
	IEnumerator seekHost(){
		yield return new WaitForSeconds(0.5f);
		int tries =0;
		while(tries<=10){		
			if(hostData!=null && hostData.Length>0){
				//Waiting for hostData
				for(int i =0 ;i < hostData.Length;i++){
					Debug.Log(hostData[i].gameName+","+hostData[i].gameType+","+hostData[i].comment);
				}
			}else{
				FetchHostList(true);
			}
			yield return new WaitForSeconds(0.5f);
			tries++;
		}
	}
	
	void OnFailedToConnectToMasterServer(NetworkConnectionError info)
	{
		Debug.Log("FailedToConnectToMasterServer info:"+info);
	}
	
	void OnFailedToConnect(NetworkConnectionError error) {
		if(enabled){
			Debug.Log("FailedToConnect info:"+error);
			FailedConnRetry(error);
	        nowConnecting = false;
			errorMessage = "Could not connect to server: " + error;
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
	
	public void Connect(string ip, int port){
		// Enable NAT functionality based on what the hosts if configured to do	
		//lastMSConnectionAttemptForcedNat = usenat;		
		Debug.Log("Connecting to "+ip+":"+port+" NAT:");
		Network.Connect(ip, port);		
		nowConnecting=true;	
	}
	
	//This second definition of Connect can handle the ip string[] passed by the masterserver
	void Connect(string[] ip, int port){
		// Enable NAT functionality based on what the hosts if configured to do
		//lastMSConnectionAttemptForcedNat = usenat;
		Debug.Log("Connecting to "+ip[0]+":"+port+" NAT:");
		Network.Connect(ip, port);		
		nowConnecting=true;
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
		Network.SetLevelPrefix(2);
		//Network.InitializeSecurity();
		Network.InitializeServer((players-1), hostPort);
	}
	
	void FailedConnRetry(NetworkConnectionError info){
		bool mayRetry=true;
		if(info == NetworkConnectionError.InvalidPassword){
			mayRetry=false;
		}		
		nowConnecting=false;
		//Quickplay
		if(tryingToConnectPlayNow){
			//Try again without NAT if we used NAT
			if(mayRetry  && lastMSConnectionAttemptForcedNat){
				Debug.Log("Failed connect 1A: retry without NAT");
			
				remotePort[0]=serverPort;//Fall back to default server port
				Connect(remoteIP, remotePort[0]);
				lastPlayNowConnectionTime=Time.time;
			}else{
				//We didn't use NAT and failed
				Debug.Log("Failed connect 1B: Don't retry");
				//Connect to next playnow/quickplay host
				tryingToConnectPlayNowNumber++;
				tryingToConnectPlayNow=false;
			}
		}else{
			//Direct connect or via host list manually
			connectionInfo="Failed to connect!";
			
			if(mayRetry  && lastMSConnectionAttemptForcedNat){
				//Since the last connect forced NAT usage,
				// let's try again without NAT.		
				
				Network.Connect(remoteIP, remotePort[0]);
				nowConnecting=true;
				lastPlayNowConnectionTime=Time.time;
				
			}else{
				Debug.Log("Failed 2b");
			
				if(info == NetworkConnectionError.InvalidPassword){
					errorMessage="Failed to connect: Wrong password supplied";
				}else if(info == NetworkConnectionError.TooManyConnectedPlayers){
					errorMessage="Failed to connect: Server is full";
				}else{
					errorMessage="Failed to connect";
				}
				Debug.Log (errorMessage);
				//reset to default port
				remotePort[0]=serverPort;				
			}
		}	
	}
	public float CONNECT_TIMEOUT  = 0.75f;
	public float CONNECT_NAT_TIMEOUT = 5.00f;
	//Our quickplay function: Go trough the gameslist and try to connect to all games
	public string PlayNow(float timeStarted){
		int i =0;
		foreach(int myElement in sortedHostList)
		{
			var element=hostData[myElement];
			// Do not try NAT enabled games if we cannot do NAT punchthrough
			// Do not try connecting to password protected games
			if ( !(filterNATHosts && element.useNat) && !element.passwordProtected  )
			{
				//aHost=1; ??
				if(element.connectedPlayers<element.playerLimit)
				{					
					if(tryingToConnectPlayNow){
						string natText=" with option 2/2";
						if((lastPlayNowConnectionTime+CONNECT_TIMEOUT<=Time.time) || (lastPlayNowConnectionTime+CONNECT_NAT_TIMEOUT<=Time.time)){
							Debug.Log("Interrupted by timer, NAT:");
							FailedConnRetry(NetworkConnectionError.ConnectionFailed);							
						}
						return "Trying to connect to host "+(tryingToConnectPlayNowNumber+1)+"/"+sortedHostList.Count+" "+natText;	
					}		
					if(!tryingToConnectPlayNow && tryingToConnectPlayNowNumber<=i){
						Debug.Log("Trying to connect to game NR "+i+" & "+tryingToConnectPlayNowNumber);
						tryingToConnectPlayNow=true;
						tryingToConnectPlayNowNumber=i;
						
						// Enable NAT functionality based on what the hosts if configured to do
						lastMSConnectionAttemptForcedNat=element.useNat;
						
						int connectPort = element.port;
						
							//connectPort=serverPort; //bad idea!
							print("Connecting directly to host");
						
						Debug.Log("connecting to "+element.gameName+" "+element.ip+":"+connectPort);
						Network.Connect(element.ip, connectPort);	
						lastPlayNowConnectionTime=Time.time;
					}
					i++;		
				}
			}
			
		}
		
		//If we reach this point then either we've parsed the whole list OR the list is still empty
		//Dont give up yet: Give MS 7 seconds to feed the list
		if(Time.time<timeStarted+7){
			FetchHostList(true);	
			return "Waiting for masterserver..."+Mathf.Ceil((timeStarted+7)-Time.time);	
		}
		
		if(!tryingToConnectPlayNow){
			return "failed";
		}
		return "done";
	}
	float lastRegTime = -60;
	void Update() {
		// If network test is undetermined, keep running
		if (!doneTestingNAT) {
			TestConnection();
		}
		if(Network.isServer && lastRegTime<Time.time-60){
			lastRegTime=Time.time;
			MasterServer.RegisterHost(gameName,hostSetting_title, "No description");
		}

	}
	
	void TestConnection() {
		testMessage = "";
		// Start/Poll the connection test, report the results in a label and react to the results accordingly
		natCapable = Network.TestConnection();
		switch (natCapable) {
			case ConnectionTesterStatus.Error: 
				testMessage = "Problem determining NAT capabilities";
				doneTestingNAT = true;
				break;
			case ConnectionTesterStatus.Undetermined: 
				testMessage = "Undetermined NAT capabilities";
				doneTestingNAT = false;
				break;
			/*case ConnectionTesterStatus.PrivateIPNoNATPunchthrough: //No longer returned, use newer connection tester enums instead.'
				testMessage = "Cannot do NAT punchthrough, filtering NAT enabled hosts for client connections, local LAN games only.";
				filterNATHosts = true;
				doneTestingNAT = true;
				break;
			case ConnectionTesterStatus.PrivateIPHasNATPunchThrough: //No longer returned, use newer connection tester enums instead.'
				if (probingPublicIP)
					testMessage = "Non-connectable public IP address (port "+ serverPort +" blocked), NAT punchthrough can circumvent the firewall.";
				else
					testMessage = "NAT punchthrough capable. Enabling NAT punchthrough functionality.";
				doneTestingNAT = true;
				break;*/
			case ConnectionTesterStatus.PublicIPIsConnectable:
				testMessage = "Directly connectable public IP address.";
				
				doneTestingNAT = true;
				break;
				
			// This case is a bit special as we now need to check if we can 
			// cicrumvent the blocking by using NAT punchthrough
			case ConnectionTesterStatus.PublicIPPortBlocked:
				testMessage = "Non-connectble public IP address (port " + serverPort +" blocked), running a server is impossible.";
				
				// If no NAT punchthrough test has been performed on this public IP, force a test
				if (!probingPublicIP)
				{
					Debug.Log("Testing if firewall can be circumnvented");
					natCapable = Network.TestConnectionNAT();
					probingPublicIP = true;
					timer = Time.time + 10;
				}
				// NAT punchthrough test was performed but we still get blocked
				else if (Time.time > timer)
				{
					probingPublicIP = false; 		// reset
					
					doneTestingNAT = true;
				}
				break;
			case ConnectionTesterStatus.PublicIPNoServerStarted:
				testMessage = "Public IP address but server not initialized, it must be started to check server accessibility. Restart connection test when ready.";
				break;
			default: 
				testMessage = "Error in test routine, got " + natCapable;
			break;
		}
		officialNATstatus=true;
		if(doneTestingNAT){
			Debug.Log("TestConn:"+testMessage);
			Debug.Log("TestConn:"+natCapable + " " + probingPublicIP + " " + doneTestingNAT);
		}
	}
	
	private float lastHostListRequest = 0;
	public IEnumerator FetchHostList(bool manual){
		int timeout = 120;
		if(manual){
			timeout=5;
		}
		if(lastHostListRequest==0 || Time.realtimeSinceStartup > lastHostListRequest + timeout){
			lastHostListRequest = Time.realtimeSinceStartup;
			MasterServer.RequestHostList(gameName);			
			yield return new WaitForSeconds(1);//We gotta wait :/			
			hostData = MasterServer.PollHostList();
			yield return new WaitForSeconds(1);//We gotta wait :/	
			CreateSortedArray();
			Debug.Log("Requested new host list, got: "+hostData.Length);
		}	
	}
	
	void CreateSortedArray(){
		sortedHostList = new ArrayList();
		int i=0;
		HostData[] data = hostData;
		foreach(HostData element in data)
		{
			AddToArray(i);
			i++;		
		}			
	}
	
	void AddToArray(int nr){
		sortedHostList.Add (nr);	
		SortLastItem();
	}
	
	void SortLastItem(){
		if(sortedHostList.Count<=1){
			return;
		}
		for(int i=sortedHostList.Count-1;i>0;i--){
		int value1 = hostData[(int)sortedHostList[i-1]].connectedPlayers;
		int value2 = hostData[(int)sortedHostList[i]].connectedPlayers;
			if(value1<value2){
				SwapArrayItem((i-1), i);
			}else{
				//Sorted!
				return;
			}
		}
	}
	
	void SwapArrayItem(int nr1, int nr2){
		var tmp=sortedHostList[nr1];
		sortedHostList[nr1]=sortedHostList[nr2];
		sortedHostList[nr2]=tmp;
	}
	
	[RPC]
	void launchGame(){
		Network.isMessageQueueRunning=false;
		launchingGame=true;
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
}
