using UnityEngine;
using System.Collections;

public class Gameplay : MonoBehaviour {

	public string gameName = "TankBattle";
	
	void Awake(){
		//RE-enable the network messages now we've loaded the right level
		Network.isMessageQueueRunning = true;
		if(Network.isServer){
			Debug.Log("Server registered the game at the masterserver.");
			MasterServer.RegisterHost(gameName, "myGameTypeName", "MyComment");
		}
	}
		
	void OnGUI ()
	{
		if (Network.peerType == NetworkPeerType.Disconnected){
		//We are currently disconnected: Not a client or host
			GUILayout.Label("Connection status: We've (been) disconnected");
			if(GUILayout.Button("Back to main menu")){
				Application.LoadLevel(0);
			}
			
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
	
	}
}
