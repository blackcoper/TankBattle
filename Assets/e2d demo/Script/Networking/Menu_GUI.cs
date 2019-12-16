using UnityEngine;
using System.Collections;

public class Menu_GUI : MonoBehaviour {
	private Rect windowRect1;
	private Rect windowRect2;
	private Rect windowRect3;
	
	static bool playNowMode = false;
	static bool advancedMode = false;
	static float playNowModeStarted = 0.0f;
	
	static string myPlayerName = "MyPlayerName";
	
	//GUI vars
	private int hostPlayers = 8;
	private int hostPort;
	private int connectPort;
	private string connectIP = "";
	
	private Menu_multiplayerCode multiplayerScript;
	private string currentMenu = "";
	
	
	void Awake ()
	{	
		//Screen.lockCursor = false;
		//myPlayerName = PlayerPrefs.GetString("playerName");
		multiplayerScript =	GetComponent<Menu_multiplayerCode>();
		hostPort = multiplayerScript.serverPort;
		connectPort = multiplayerScript.serverPort;
		connectIP = "127.0.0.1";
		windowRect1 = new Rect(Screen.width/2-310,Screen.height/2-90,380,280);
		windowRect2 = new Rect(Screen.width/2+85,Screen.height/2-90,220,100);
		windowRect3 = new Rect(Screen.width/2+85,Screen.height/2+55,220,100);
		
		playNowMode=false;
		advancedMode=false;
	}
	
	void OnGUI ()
	{		
		//If we've connected;  load the game when it's ready to load
		if(Network.isClient || Network.isServer){
			
			//Since we're connected, load the game
			GUI.Box(new Rect(Screen.width/4+0,Screen.height/2-30,450,50), "");
			if(Application.CanStreamedLevelBeLoaded ((Application.loadedLevel+1))){
				GUI.Label(new Rect(Screen.width/4+10,Screen.height/2-25,285,150), "Starting the game!");
				Application.LoadLevel((Application.loadedLevel+1));
			}else{
				GUI.Label(new Rect(Screen.width/4+10,Screen.height/2-25,285,150), "Connecting");
				GUI.Label(new Rect(Screen.width/4+10,Screen.height/2-25,285,150), "Loading the game: "+Mathf.Floor(Application.GetStreamProgressForLevel((Application.loadedLevel+1))*100)+" %");
			}
			//LevelManager.game_stat = 2;
			/*GetComponent<CoopMenu>().enabled = true;
			this.enabled = false;
			return;*/
		}
		
		
		//Dirty error message popup
		if(multiplayerScript.errorMessage!="" && multiplayerScript.errorMessage!=""){	
			GUI.Box(new Rect(Screen.width/2-100,Screen.height/2-60,200,60), "Error");
			GUI.Label(new Rect(Screen.width/2-90,Screen.height/2-50,180,50), multiplayerScript.errorMessage);
			if(GUI.Button(new Rect(Screen.width/2+40,Screen.height/2-30,50,20), "Close")){
				multiplayerScript.errorMessage="";
			}
		}	
		
		if(playNowMode){
			playNowFunction();
		}else if(advancedMode){
			if(multiplayerScript.errorMessage=="" || multiplayerScript.errorMessage==""){ //Hide windows on error
				windowRect1 = GUILayout.Window (0, windowRect1, listGUI, "Join a game via the list");	
				windowRect2 = GUILayout.Window (1, windowRect2, directConnectGUIWindow, "Directly join a game via an IP");	
				windowRect3 = GUILayout.Window (2, windowRect3, hostGUI, "Host a game");
				if(GUI.Button(new Rect(455,90,140,30), "Back to main menu")){
					currentMenu="";
					advancedMode=false;
				}
			}	
			
		}else{		
			GUI.Box(new Rect(90, 180, 260, 105), "Playername");
			GUI.Label(new Rect(100, 195, 250, 100), "Please enter your name:");
			
			myPlayerName = GUI.TextField(new Rect(178, 215, 147, 27), myPlayerName);	
			if(GUI.changed){
				//Save the name changes
				PlayerPrefs.SetString("playerName", myPlayerName);
			}
			
			if(myPlayerName==""){
				GUI.Label(new Rect(100, 240, 260, 100), "After entering your name you can start playing!");
				return;
			}
			
			GUI.Label(new Rect(100, 240, 260, 100), "Click on quickplay to start playing right away!");
					
			if(GUI.Button(new Rect(400,150,150,30), "Quickplay") ){
				currentMenu="playnow";
				playNowMode=true;
				playNowModeStarted=Time.time;		
			}
			if(GUI.Button(new Rect(400,245,150,30), "Advanced") ){
				currentMenu="advanced";
				advancedMode=true;
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
	
			if(multiplayerScript.tryingToConnectPlayNowNumber>=10){
				//If players get fed up waiting they can choose to start a host right away
				if(GUI.Button(new Rect(400,185,75,20), "Just host")){
					multiplayerScript.StartHost(hostPlayers, multiplayerScript.serverPort);
				}
			}
			
			string connectStatus = multiplayerScript.PlayNow(playNowModeStarted);
			
			if(connectStatus=="failed"){
				//Couldn't find a proper host; host ourselves
				Debug.Log("PlayNow: No games hosted, so hosting one ourselves");	
				multiplayerScript.StartHost(7, multiplayerScript.serverPort);				
			}else{
				//Still trying to connect to the first hit
				GUI.Label(new Rect(Screen.width/4+10,Screen.height/2-45,385,50), connectStatus);
			}
	}


	void hostGUI(int id){
	
		GUILayout.BeginVertical();
		GUILayout.Space(10);
		GUILayout.EndVertical();
		
		GUILayout.BeginHorizontal();
		GUILayout.Space(10);
			GUILayout.Label("Use port: ");
			hostPort = int.Parse(GUILayout.TextField(hostPort.ToString(), GUILayout.MaxWidth(75)));
		GUILayout.Space(10);
		GUILayout.EndHorizontal();	
		
		GUILayout.BeginHorizontal();
		GUILayout.Space(10);
			GUILayout.Label("Players: ");
			hostPlayers = int.Parse(GUILayout.TextField(hostPlayers.ToString(), GUILayout.MaxWidth(75)));
		GUILayout.Space(10);
		GUILayout.EndHorizontal();
		
		
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
			// Start a new server
			if (GUILayout.Button ("Start hosting a server")){
				multiplayerScript.StartHost(hostPlayers, hostPort);
			}			
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
	}

	void directConnectGUIWindow(int id){
	
		GUILayout.BeginVertical();
		GUILayout.Space(5);
		GUILayout.EndVertical();
		GUILayout.Label(multiplayerScript.connectionInfo);
			
		if(multiplayerScript.nowConnecting){
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Trying to connect to "+connectIP+":"+connectPort);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			
		} else {		
	
			GUILayout.BeginHorizontal();
			GUILayout.Space(10);
				connectIP = GUILayout.TextField(connectIP, GUILayout.MinWidth(70));
				connectPort = int.Parse(GUILayout.TextField(connectPort+""));
			GUILayout.Space(10);
			GUILayout.EndHorizontal();
		
			GUILayout.BeginHorizontal();
			GUILayout.Space(10);
			GUILayout.FlexibleSpace();
				
			if (GUILayout.Button ("Connect"))
			{
				multiplayerScript.Connect(connectIP, connectPort);
			}	
			
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		
		}
		
	}

	private Vector2 scrollPosition;

	void listGUI (int id) {
	
		GUILayout.BeginVertical();
		GUILayout.Space(6);
		GUILayout.EndVertical();
	
		GUILayout.BeginHorizontal();
		GUILayout.Space(200);

		// Refresh hosts
		if (GUILayout.Button ("Refresh available Servers"))
		{
			StartCoroutine(multiplayerScript.FetchHostList(true));
		}
		StartCoroutine(multiplayerScript.FetchHostList(false));
			
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		//scrollPosition = GUI.BeginScrollView (Rect (0,60,385,200),	scrollPosition, Rect (0, 100, 350, 3000));
		scrollPosition = GUILayout.BeginScrollView (scrollPosition);

		int aHost = 0;
		
		if(multiplayerScript.sortedHostList!=null && multiplayerScript.sortedHostList.Count>=1){
			foreach(int myElement in multiplayerScript.sortedHostList)
			{
				var element=multiplayerScript.hostData[myElement];
				GUILayout.BeginHorizontal();

				// Do not display NAT enabled games if we cannot do NAT punchthrough
				if ( !(multiplayerScript.filterNATHosts && element.useNat) )
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
					if(!multiplayerScript.nowConnecting){
					if (GUILayout.Button("Connect"))
						{
							multiplayerScript.Connect(element.ip[0], element.port);
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
}