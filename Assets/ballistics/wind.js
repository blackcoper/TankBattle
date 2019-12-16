var speed: float = 0.3;
var targetAngle: Vector3;
var t : float ;
var dWindx : float = 0.5;
var dWindy : float = 3;
var windTimeVar : float = 5;
var windForce : float ;
var dWindForce : float = 0.1 ;
var maxWindForce : float = 10 ;
static var f : Vector3 ;
static var s : float ;

function Start()
{
    targetAngle = Vector3(Random.Range(-20,20),Random.Range(-360,360),0);
    windForce = Random.Range(0,maxWindForce);
}

function Update()
{
	rand();
	var newRot = Quaternion.Euler(targetAngle); 
    transform.rotation = Quaternion.Slerp(transform.rotation, newRot, speed*Time.deltaTime);
    transform.localScale.z = Mathf.Lerp(transform.localScale.z, windForce, speed*Time.deltaTime);
    
    //Debug.Log(windForce);
    
    f = transform.forward ;
    s = transform.localScale.z ;
}

function rand ()
{
	t=Random.Range(windTimeVar,2*windTimeVar);
	
	targetAngle += Vector3(Random.Range(-dWindx,dWindx),Random.Range(-dWindy,dWindy),0);
	targetAngle.x=Mathf.Clamp(targetAngle.x,-20,20);
	
	windForce += Random.Range(-dWindForce,dWindForce);
	windForce = Mathf.Clamp(windForce,0,maxWindForce);
	
	yield WaitForSeconds(t);
	
}