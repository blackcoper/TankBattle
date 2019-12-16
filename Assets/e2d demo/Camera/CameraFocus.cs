using UnityEngine;
using System.Collections;

public class CameraFocus : MonoBehaviour 
{
	public Transform FocusTarget;
	public float limitMinX = -178.25f;
	public float limitMinY;
	public float limitMaxX = 163.15f;
	public float limitMaxY;
	public GameObject parallax01;
	public GameObject parallax02;

	void Update () 
	{
		if(FocusTarget == null)return;
		Vector3 newPos = FocusTarget.position;
		if(newPos.x<=limitMinX){
			newPos.x = limitMinX;
		}else if(newPos.x>=limitMaxX){
			newPos.x = limitMaxX;
		}
		newPos.z = transform.position.z;
		newPos.y = newPos.y + 15;
		Vector3 oldPos = transform.position;
		float delta = 0.3f;
		transform.position = (1 - delta) * oldPos + delta * newPos;
		if(oldPos.x > newPos.x){
			parallax01.renderer.material.mainTextureOffset = new Vector2(Time.deltaTime * newPos.x - oldPos.x * 0.1f, 0);
 		    parallax02.renderer.material.mainTextureOffset = new Vector2(Time.deltaTime * newPos.x - oldPos.x * 0.5f, 0);
		}else if(oldPos.x < newPos.x){
			parallax01.renderer.material.mainTextureOffset = new Vector2(Time.deltaTime * newPos.x - oldPos.x * 0.1f, 0);
 		    parallax02.renderer.material.mainTextureOffset = new Vector2(Time.deltaTime * newPos.x - oldPos.x * 0.5f, 0); 
		}
	}
}
