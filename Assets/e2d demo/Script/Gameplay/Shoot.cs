using UnityEngine;
using System.Collections;

public class Shoot : MonoBehaviour {
	
	public GameObject bullet;
	public Joystick jumpTouchPad; 
	public int speed = 10;
	public float fireRate = 5F; 
 	
	private float nextFire = 0.0F;
	private GameObject mesin;
	private Collider[] objectsInRange;
	
	public float shootAngle = 30;  // elevation angle
	void Update()
	{
		mesin = findParentWithTag("Player", this.gameObject);
		if(mesin!= null){
			if ((jumpTouchPad.isFingerDown || Input.GetButtonUp("Fire1"))&& Time.time > nextFire)
			{
				nextFire = Time.time + fireRate;
				GameObject bulletObj = (GameObject)Instantiate(bullet,this.transform.position, this.transform.rotation);
				bulletObj.rigidbody.velocity = transform.TransformDirection(Vector3.right * speed);
				Destroy(bulletObj, 15f);
			}
		}else{
			if(Time.time > nextFire){
				nextFire = Time.time + fireRate;
				GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");
				objectsInRange = Physics.OverlapSphere(enemy.transform.position, 100f);
			    bool fire = false;
				foreach (Collider col in objectsInRange)
			    {
					if(col.gameObject.tag == "Player")fire = true;
			    }
				if(fire){
					GameObject ball = (GameObject)Instantiate(bullet, transform.position, Quaternion.identity);
					ball.rigidbody.velocity = BallisticVel(GameObject.FindGameObjectWithTag("Player").transform, shootAngle);
					Destroy(ball, 15f);
					
					/*GameObject bulletObj = (GameObject)Instantiate(bullet,this.transform.position, this.transform.rotation);
					bulletObj.rigidbody.velocity = transform.TransformDirection(Vector3.right * speed);
					Destroy(bulletObj, 15f);*/
				}	
				
			}
		}
	}
	Vector3 BallisticVel(Transform target, float angle) {
		var dir = target.position - transform.position;  // get target direction
		var h = dir.y;  // get height difference
		dir.y = 0;  // retain only the horizontal direction
		var dist = dir.magnitude ;  // get horizontal distance
		var a = angle * Mathf.Deg2Rad;  // convert angle to radians
		dir.y = dist * Mathf.Tan(a);  // set dir to the elevation angle
		dist += h / Mathf.Tan(a);  // correct for small height differences
		// calculate the velocity magnitude
		var vel = Mathf.Sqrt(dist * Physics.gravity.magnitude / Mathf.Sin(2 * a));
		return vel * dir.normalized;
	}
	GameObject findParentWithTag(string tagToFind, GameObject startingObject) {
	    var parent = startingObject.transform.parent;
	    while (parent != null) { 
	       if (parent.tag == tagToFind) {
	         return parent.gameObject as GameObject;
	       }
	       parent = parent.transform.parent;
	    }
	    return null;
	}
}
