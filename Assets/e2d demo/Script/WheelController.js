// WheelController.js
//private var GC : GameController;
//private var EC : EnvironmentController;
/*private var VC : VehicleController;
private var doScript=false;

var drive=0.0;
var steer=0.0;

var skidTexture : Texture2D;

@HideInInspector
var col : WheelCollider;
private var bounds : Bounds;
private var radius=0.0;
private var rotation=0.0;
private var FB=0.0;
private var lastSkid=-1;
private var skidObject : Transform;
private var hasSetup=false;

function Start(){
	if(hasSetup) return;
	var obj : GameObject;
	//obj=GameObject.Find("GameController");
	//if(!obj) return;
	//GC=obj.GetComponent("GameController");
	//if(!GC) return;
	obj=GameObject.Find("EnvironmentController");
	if(!obj) return;
	EC=obj.GetComponent("EnvironmentController");
	if(!EC) return;
	obj=transform.parent.gameObject;
	if(!obj) return;
	VC=obj.GetComponent("VehicleController");
	if(!VC) return;
	VC.Start();
	if(!VC.hasSetup) return;
	
	if(!gameObject.GetComponent(MeshFilter)) return;
	
	bounds=gameObject.GetComponent(MeshFilter).mesh.bounds;
	radius=bounds.extents.y;
	
	if(collider){Destroy(collider);}
	var count=0;
	for(var objs : Transform in transform){count++;}
	if(count>0){
		transform.renderer.enabled=false;
	}
	
	obj=GameObject(gameObject.name + "_ColliderObj");
	obj.transform.parent=transform.parent;
	obj.transform.localPosition=transform.localPosition + Vector3(0,VC.suspensionOffset,0);
	
	//var RL=obj.localPosition.x;
	FB=obj.transform.localPosition.z - VC.centerOfMass.z;
	
	col=obj.AddComponent(WheelCollider);
	col.radius=radius;
	col.mass=bounds.size.y * bounds.size.x * 20;
	col.suspensionDistance=radius * VC.relativeSuspensionAmount;
	var suspensionBias=FB>0 ? VC.mass * VC.suspensionBias : VC.mass * (1-VC.suspensionBias);
	
	col.suspensionSpring.spring=(VC.mass  + suspensionBias) * VC.suspensionHardness;
	col.suspensionSpring.damper=2;
	col.suspensionSpring.targetPosition=radius * VC.relativeSuspensionAmount;
	
	col.forwardFriction.stiffness=0.02;
	col.sidewaysFriction.stiffness=0.03;
	
	skidObject=EC.createSkidObject(transform.parent.name + "_"+ this.name, bounds.size.x, skidTexture);
	
	hasSetup=true;
	doScript=true;
}

function Update(){
	if(!doScript) return;
	
	var sidewaysFriction=VC.sideSlipCurve.Evaluate(VC.relativeVelocity.magnitude / 100);
	var forwardFriction=0.02;
	
	var hit : WheelHit;
	var friction = 1.0;
	if(col.GetGroundHit(hit)){
		if(hit.collider.material)friction = hit.collider.material.staticFriction;
	}
	
	col.sidewaysFriction.stiffness=sidewaysFriction * friction;
	col.forwardFriction.stiffness=forwardFriction * friction;
	
	if(VC.brake>0.0){
		var b=FB>0 ? VC.brake * VC.brakeBias : VC.brake * (1.25-VC.brakeBias);
		
		col.brakeTorque=b;
		col.motorTorque=0.0;
	} else {
		if(VC.power!=0.0){
			col.brakeTorque=0.0;
			col.motorTorque=VC.power * drive;
		}else{
			col.brakeTorque=VC.enginePower * drive;
			col.motorTorque=0.0;
		}
	}
	
	col.steerAngle=VC.steer * steer;
}

var lastSmoke=0.0;
var lastSqueel=0.0;

function LateUpdate(){
	if(!doScript) return;
	var hit : WheelHit;
	transform.position=col.transform.position;
	if(VC.brake==0.0){
		rotation = (rotation + col.rpm * col.radius * 6.28 * Time.deltaTime) %360.0;
	}
	transform.localRotation = Quaternion.Euler( rotation, col.steerAngle, 0.0 );
	if(col.GetGroundHit(hit)){
		var relativeVelocity=transform.parent.rigidbody.GetRelativePointVelocity(col.transform.localPosition);
		var rv=col.transform.InverseTransformPoint(col.transform.position + relativeVelocity);
		relativeVelocity.Normalize();
		var mps=(col.rpm / 60) * col.radius * 2 * 3.14159265;
		var sideSlip=Mathf.Abs(rv.x/10) * (Mathf.Abs(relativeVelocity.z) > 5.0 ? 1.0 : 0.0);
		var driveSlip = Mathf.Abs(rv.z) < 20 ? Mathf.Abs(rv.z-mps) * drive: 0.0;
		var brakeSlip=0.0;
		if(VC.brake>0.0)brakeSlip = Mathf.Abs(rv.z) > 1.0 ? 1.0 : 0.0;
		var vishit : RaycastHit;
		if(Physics.Linecast(transform.position, col.transform.TransformPoint(-Vector3.up * radius * 2), vishit)){
			if(vishit.collider.gameObject.tag!=""){
				if(Time.time > lastSmoke + 0.08){
					var direction=col.transform.TransformDirection(-Vector3.forward * driveSlip / 5.0);
					if(VC.brake>0.0) direction=Vector3.zero;
					if(EC.doSmoke(vishit.point, driveSlip, sideSlip, brakeSlip, direction,  vishit.collider.gameObject.tag))
						lastSmoke=Time.time;
				}
				if(Time.time > lastSqueel + 0.2){
					if(EC.doSkidSound(vishit.point, driveSlip, sideSlip, brakeSlip, vishit.collider.gameObject.tag))
						lastSqueel=Time.time;
				}
				lastSkid=EC.AddSkidMark(skidObject, driveSlip, sideSlip, brakeSlip, vishit.collider.gameObject.tag, vishit.point, vishit.normal, lastSkid);
			}
		} else {
			lastSkid=-1;
		}
		
		if(hit.collider.gameObject.tag=="mud"){
			col.radius=radius/2;
		} else {
			col.radius=radius;
		}
		
		transform.position+=col.transform.up * (col.transform.InverseTransformPoint(hit.point).y + col.radius);
	} else {
		transform.position-=col.transform.up * col.suspensionSpring.targetPosition;
	}
}



function FindChild(name : String, inObject : GameObject){
	var objs : Component[] = inObject.GetComponentsInChildren(Transform);
	for(var obj : Component in objs){
		if(obj.name==name) return obj.gameObject;
	}
	return null;
}*/



