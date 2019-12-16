using UnityEngine;
using System.Collections;

public class Menu_multiplayerCode : MonoBehaviour {
	public string gameName = "TankBattle";
	public int serverPort =  23466;
	//ip = System.Net.Dns.GetHostEntry("www.gameserver.com");
	public string myMasterServerIP = "192.168.0.65";
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
	
	void Awake()
	{
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
		//Yikes
		Debug.Log("FailedToConnectToMasterServer info:"+info);
	}
	
	void OnFailedToConnect(NetworkConnectionError info)
	{
		Debug.Log("FailedToConnect info:"+info);
		FailedConnRetry(info);		
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
	
	
	public void StartHost(int players,int port){
		if(players<=1){
			players=1;
		}
		//Network.InitializeSecurity(); //
		//Debug.Log("start Host:"+players+","+port);
		Network.InitializeServer(players, port);
	}
	
	void OnConnectedToServer(){
		//Stop communication until in the game
		Network.isMessageQueueRunning = false;
		//Save these details so we can use it in the next scene
		PlayerPrefs.SetString("connectIP", Network.connections[0].ipAddress);
		PlayerPrefs.SetInt("connectPort", Network.connections[0].port);
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
	
	void Update() {
		// If network test is undetermined, keep running
		if (!doneTestingNAT) {
			TestConnection();
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
		//if(doneTestingNAT){
			Debug.Log("TestConn:"+testMessage);
			Debug.Log("TestConn:"+natCapable + " " + probingPublicIP + " " + doneTestingNAT);
		//}
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
}