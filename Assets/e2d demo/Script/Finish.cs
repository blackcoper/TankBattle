using UnityEngine;
using System.Collections;

public class Finish : MonoBehaviour {
	
	void OnTriggerEnter (Collider other) {
		if(other.tag == "Player"){
			LevelManager.game_stat = 4;
			this.enabled = false;
		}
	}
}
