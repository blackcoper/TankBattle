using UnityEngine;
using System.Collections;

public class MultiplayerCarControl : MonoBehaviour 
{
	public NetworkPlayer netPlayer;
	public GameObject RightWheel;
	public GameObject LeftWheel;
	public GameObject MidRightWheel;
	public GameObject MidLeftWheel;
	public Transform weapon;
	public Transform fire_pos;
	
	public ParticleEmitter smokeEmiter;
	public GameObject bullet;
	public int speed = 10;
	public float updateRate = 5f;
	public float shootAngle = 30;
	public float force = 30; 
	public float tarvel = 4000;
	public string face = "left";
	
	private float velocity;	
	//private float x = 0.0f;
	public AudioClip shootAudio;
	
	private Joystick upTouchPad;
	private Joystick downTouchPad;
	private Joystick leftTouchPad;
	private Joystick rightTouchPad;
	private Joystick fireTouchPad;
	private Joystick jumpTouchPad;
	
	private float nextFire = 0.0f;
	private float nextUpdate = 0.0f;
	private GameObject player;
	private UnitStatus unit_status;
	private Collider[] objectsInRange;
	void Start(){
		player = transform.parent.gameObject;
		unit_status = player.GetComponent<UnitStatus>();
		rigidbody.centerOfMass = new Vector3(1,0,0);
		if(!unit_status.is_enemy){
			int _client = System.Int32.Parse(Network.player.ToString());
			player.name = "player "+_client;
			gameObject.tag = "Player";
			rigidbody.velocity = new Vector3(0,0,1);
			string playerName = (PlayerPrefs.GetString("playerName","") != "")? PlayerPrefs.GetString("playerName","") : "Player "+_client;
			networkView.RPC("set_player_name",RPCMode.All, playerName, networkView.viewID);
			
			if(GameObject.Find("FireButton")!=null)fireTouchPad = GameObject.Find("FireButton").GetComponent<Joystick>();
			if(GameObject.Find("JumpButton")!=null)jumpTouchPad = GameObject.Find("JumpButton").GetComponent<Joystick>();
			if(GameObject.Find("LeftButton")!=null)leftTouchPad = GameObject.Find("LeftButton").GetComponent<Joystick>();
			if(GameObject.Find("RightButton")!=null)rightTouchPad = GameObject.Find("RightButton").GetComponent<Joystick>();
			if(GameObject.Find("UpButton")!=null)upTouchPad = GameObject.Find("UpButton").GetComponent<Joystick>();
			if(GameObject.Find("DownButton")!=null)downTouchPad = GameObject.Find("DownButton").GetComponent<Joystick>();
			#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
				if(GameObject.Find("Touchpad").gameObject!=null)GameObject.Find("Touchpad").gameObject.SetActive(false);
			#endif
			
			if(networkView.isMine)gameObject.AddComponent("AudioListener");
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
	void FixedUpdate ()
	{
		if(unit_status.die || !networkView.isMine)return;
		if(!unit_status.is_enemy){
			float h = 0;
			float v = 0;
			#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
				h = Input.GetAxis("Horizontal");
				v = Input.GetAxis("Vertical");
			#else
				if(upTouchPad.isFingerDown)v = 1;
				if(downTouchPad.isFingerDown)v = -1;
				if(rightTouchPad.isFingerDown)h = 1;
				if(leftTouchPad.isFingerDown)h = -1;
			#endif
			if(h!=0){
				if(h<0){
					velocity = h * tarvel;
					if(face.Equals("left")){
						//rigidbody.velocity=Vector3.zero;
						face = "right";
						networkView.RPC("update_transform",RPCMode.Others, rigidbody.velocity, transform.position, transform.rotation, networkView.viewID);
						networkView.RPC("change_direction",RPCMode.All, face, networkView.viewID);
						//transform.eulerAngles = new Vector3(0, 180, -transform.eulerAngles.z);
					}
				}else{
					velocity = -h * tarvel;
					if(face.Equals("right")){
						//rigidbody.velocity=Vector3.zero;
						face = "left";
						networkView.RPC("update_transform",RPCMode.Others, rigidbody.velocity, transform.position, transform.rotation, networkView.viewID);
						networkView.RPC("change_direction",RPCMode.All, face, networkView.viewID);
						//transform.eulerAngles = new Vector3(0, 0, -transform.eulerAngles.z);
					}
				}
				networkView.RPC("add_velocity",RPCMode.AllBuffered,h, velocity, networkView.viewID);
				smokeEmiter.emit = true;
			}else{
				smokeEmiter.emit = false;
			}
			if(rigidbody.velocity!=Vector3.zero && Time.time > nextUpdate){
				nextUpdate = Time.time + updateRate;
				networkView.RPC("update_transform",RPCMode.Others, rigidbody.velocity, transform.position, transform.rotation, networkView.viewID);
			}
			if(v!= 0){
				float angle = weapon.rotation.z;
				if (weapon.rotation.z < -360)
					angle += 360;
				if (weapon.rotation.z > 360)
					angle -= 360;
				Debug.Log(angle);
				networkView.RPC("change_angle",RPCMode.All, angle+v, false, networkView.viewID);
			}
			
			#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
			// Using Mouse
	      	Vector3 mouse_pos = Input.mousePosition;
		    mouse_pos.z = 5.23f; 
		    Vector3 object_pos = Camera.main.WorldToScreenPoint(weapon.position);
		    mouse_pos.x = mouse_pos.x - object_pos.x;
		    mouse_pos.y = mouse_pos.y - object_pos.y;
		    float _angle = Mathf.Atan2(mouse_pos.y, mouse_pos.x) * Mathf.Rad2Deg;
			if(_angle != weapon.rotation.z){
		    	weapon.rotation = Quaternion.Euler(0, 0, _angle);
				networkView.RPC("change_angle",RPCMode.All, _angle, true, networkView.viewID);
			}
			//shoot
			if (Input.GetButtonUp("Fire1") && Time.time > nextFire)
			{
				nextFire = Time.time + unit_status.fireRate;
				networkView.RPC("update_transform",RPCMode.Others, rigidbody.velocity, transform.position, transform.rotation, networkView.viewID);
				networkView.RPC("player_shoot", RPCMode.All, networkView.viewID);
			}
			
			// flip the car
			if ( Input.GetButton("Jump"))
		 	{
				float deltaAngle = Mathf.DeltaAngle(transform.eulerAngles.z, 0);
				if (Mathf.Abs(deltaAngle) > 10)
				{
					networkView.RPC("update_transform",RPCMode.Others, rigidbody.velocity, transform.position, transform.rotation, networkView.viewID);
					networkView.RPC("player_jump", RPCMode.All, deltaAngle, networkView.viewID);
				}
		 	}	
			#else
			//shoot
			if (fireTouchPad.isFingerDown && Time.time > nextFire)
			{
				nextFire = Time.time + unit_status.fireRate;
				networkView.RPC("update_transform",RPCMode.Others, rigidbody.velocity, transform.position, transform.rotation, networkView.viewID);
				networkView.RPC("player_shoot", RPCMode.All, networkView.viewID);
			}
			
			// flip the car
			if (jumpTouchPad.isFingerDown)
		 	{
				float deltaAngle = Mathf.DeltaAngle(transform.eulerAngles.z, 0);
				if (Mathf.Abs(deltaAngle) > 10)
				{
					networkView.RPC("update_transform",RPCMode.Others, rigidbody.velocity, transform.position, transform.rotation, networkView.viewID);
					networkView.RPC("player_jump", RPCMode.All, deltaAngle, networkView.viewID);
				}
		 	}
			#endif
		}else{
			if(Time.time > nextFire){
				nextFire = Time.time + unit_status.fireRate;
				objectsInRange = Physics.OverlapSphere(transform.position, 80f);
				foreach (Collider col in objectsInRange)
			    {
					if(col.gameObject.tag == "Player"){
						UnitStatus targetStat = col.gameObject.transform.parent.GetComponent<UnitStatus>();
						if(!targetStat.die){
							if(transform.position.x > col.gameObject.transform.position.x){
								if(face.Equals("left")){
									face = "right";
									networkView.RPC("update_transform",RPCMode.Others, rigidbody.velocity, transform.position, transform.rotation, networkView.viewID);
									networkView.RPC("change_direction",RPCMode.All, face, networkView.viewID);
									//transform.eulerAngles = new Vector3(0, 180, -transform.eulerAngles.z);
								}
							}else{
								if(face.Equals("right")){
									face = "left";
									networkView.RPC("update_transform",RPCMode.Others, rigidbody.velocity, transform.position, transform.rotation, networkView.viewID);
									networkView.RPC("change_direction",RPCMode.All, face, networkView.viewID);
									//transform.eulerAngles = new Vector3(0, 0, -transform.eulerAngles.z);
								}
							}
							//GameObject ball = (GameObject)Network.Instantiate(bullet, fire_pos.transform.position, Quaternion.identity);
							Vector3 balistic = BallisticVel(col.gameObject.transform, shootAngle);
							if(!float.IsNaN(balistic.x) && !float.IsNaN(balistic.y) && !float.IsNaN(balistic.z))
							{ 
								networkView.RPC("update_transform",RPCMode.Others, rigidbody.velocity, transform.position, transform.rotation, networkView.viewID);
								networkView.RPC("enemy_shoot", RPCMode.All, balistic, networkView.viewID);								
							}
							break;
						}
					}
			    }
				/*GameObject bulletObj = (GameObject)Instantiate(bullet,this.transform.position, this.transform.rotation);
				bulletObj.rigidbody.velocity = transform.TransformDirection(Vector3.right * speed);
				Destroy(bulletObj, 15f);*/
			}
			if(rigidbody.velocity!=Vector3.zero && Time.time > nextUpdate){//delay each reposition
				nextUpdate = Time.time + updateRate;
				networkView.RPC("update_transform",RPCMode.Others, rigidbody.velocity, transform.position, transform.rotation, networkView.viewID);
			}
		}	
	}
	[RPC]
	void change_angle(float _angle,bool mouse, NetworkViewID idv){
		if(networkView.viewID == idv){
			if(mouse){
				weapon.rotation = Quaternion.Euler(0, 0, _angle);
			}else{
				weapon.Rotate(0,0, _angle);
			}
		}
	}
	
	[RPC]
	public void receiveDamage(float percent, NetworkViewID idv){
		if(networkView.viewID == idv){	
			unit_status.Health -= percent;
			unit_status.currentHP.localScale = new Vector3(unit_status.maxHP.localScale.x * unit_status.Health/100,1,0.04f);
			if(unit_status.Health <= 0){
				unit_status.currentHP.localScale = new Vector3(0,1,0.04f);
				if(!unit_status.die){
					audio.Play();
					foreach(Transform child in transform){
						if(child.gameObject.tag == "Breakable"){
						 	HingeJoint hingeJointTire = child.GetComponent<HingeJoint>();
							hingeJointTire.breakForce = 0.01f;
						}
					}
					Vector3 explosionPos = transform.position;
					explosionPos.y -= 1; 
					rigidbody.AddExplosionForce(250000f, explosionPos, 10f);
					unit_status.Health = 0;
					unit_status.die = true;
					StartCoroutine(unit_status.WaitSecond());
				}
			}
		}
	}
		
	[RPC]
	void set_player_name(string _name, NetworkViewID idv){
		if(networkView.viewID == idv)unit_status.setPlayerName(_name);
	}
	
	[RPC]
	void player_jump(float deltaAngle, NetworkViewID idv){
		if(networkView.viewID == idv){
			rigidbody.AddTorque(0, 0, deltaAngle * deltaAngle * deltaAngle, ForceMode.VelocityChange);
		}
	}
	
	[RPC]
	void change_direction( string direction, NetworkViewID idv){
		if(networkView.viewID == idv){
			if(direction.Equals("right")){
				transform.eulerAngles = new Vector3(0, 180, -transform.eulerAngles.z);
			}
			if(direction.Equals("left")){
				transform.eulerAngles = new Vector3(0, 0, -transform.eulerAngles.z);
			}
		}
	}
	
	[RPC]
	void add_velocity(float _h,float _velocity, NetworkViewID idv){
		if(networkView.viewID == idv){
			JointMotor motor = RightWheel.hingeJoint.motor;
			motor.force = (_h!=0?1:0) * force;
			motor.targetVelocity = _velocity;
			if(isGrounded(LeftWheel)){
				LeftWheel.hingeJoint.useMotor = _h != 0;
				LeftWheel.hingeJoint.motor = motor;
			}
			if(isGrounded(RightWheel)){
				RightWheel.hingeJoint.useMotor = _h != 0;
				RightWheel.hingeJoint.motor = motor;
			}
			if(isGrounded(MidLeftWheel)){
				MidLeftWheel.hingeJoint.useMotor = _h != 0;
				MidLeftWheel.hingeJoint.motor = motor;
			}
			if(isGrounded(MidRightWheel)){
				MidRightWheel.hingeJoint.useMotor = _h != 0;
				MidRightWheel.hingeJoint.motor = motor;
			}
		}
	}
	
	[RPC]
	void update_transform(Vector3 _vel,Vector3 position, Quaternion rotation, NetworkViewID idv){
		if(networkView.viewID == idv){
			rigidbody.velocity = _vel;
			transform.position = position;
			transform.rotation =  rotation;
		}
	}
	
	[RPC]
	void player_shoot(NetworkViewID idv){
		if(networkView.viewID == idv){
			AudioSource.PlayClipAtPoint(shootAudio, fire_pos.transform.position,1);
			GameObject bulletObj = (GameObject)Instantiate(bullet, fire_pos.transform.position, fire_pos.transform.rotation);
			bulletObj.GetComponent<Explosion>().power = unit_status.fireDamage;
			bulletObj.rigidbody.velocity = fire_pos.transform.TransformDirection(Vector3.right * speed);
			Destroy(bulletObj, 15f);
		}
	}
	
	[RPC]
	void enemy_shoot(Vector3 _vel, NetworkViewID idv){
		if(networkView.viewID == idv){
			AudioSource.PlayClipAtPoint(shootAudio, fire_pos.transform.position,1);
			GameObject ball = (GameObject)Instantiate(bullet, fire_pos.transform.position, Quaternion.identity);
			ball.GetComponent<Explosion>().power = unit_status.fireDamage;
			ball.rigidbody.velocity = _vel;
			Destroy(ball, 15f);
		}
	}
	//check if hit on player body
	bool isGrounded(GameObject obj) {
    	return Physics.Raycast(obj.transform.position, -Vector3.up, obj.GetComponent<SphereCollider>().bounds.extents.y + 1f);
    }
	float ClampAngle (float angle, float min, float max ) {
		if (angle < -360)
			angle += 360;
		if (angle > 360)
			angle -= 360;
		return Mathf.Clamp (angle, min, max);
	}
} 