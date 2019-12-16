using UnityEngine;
using System.Collections;

public class HUD : MonoBehaviour {
	public Transform target;
	public float Yoffset;
	void Update (){
		if(target==null)return;
	    Vector3 wantedPos = target.position;
		wantedPos.y += Yoffset; 
		transform.position = wantedPos;
	}
}
