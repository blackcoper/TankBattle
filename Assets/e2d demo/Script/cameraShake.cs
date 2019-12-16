using UnityEngine;
using System.Collections;

public class cameraShake : MonoBehaviour {
	//public Vector3 originPosition;  
	public Quaternion originRotation;  
	  
	public float shake_decay = 0.002f;  
	public float shake_intensity;  
	public float coef_shake_intensity = 0.3f;
	
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(shake_intensity > 0){  
	        //transform.position = originPosition + Random.insideUnitSphere * shake_intensity;  
	        transform.rotation = new Quaternion(  
	            0,//originRotation.x + Random.Range(-shake_intensity,shake_intensity)*.2f,  
	            0,//originRotation.y + Random.Range(-shake_intensity,shake_intensity)*.2f,  
	            originRotation.z + Random.Range(-shake_intensity,shake_intensity)*.2f,  
	            originRotation.w + Random.Range(-shake_intensity,shake_intensity)*.2f
			);  
	        shake_intensity -= shake_decay;  
	    }else{
			transform.rotation = new Quaternion(0,0,0,0);
		}
	}
	public void Shake(){  
	    //originPosition = transform.position;  
	    originRotation = transform.rotation;  
	    shake_intensity = coef_shake_intensity;  
	}
}