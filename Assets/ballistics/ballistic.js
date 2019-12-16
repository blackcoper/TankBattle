var gravity = 9.8 ;
var m : float = 0.008 ;
var vitesse = 350 ;
var time : float ;
var lifeTime : float ;
var coefVent : float = 0.3 ;

var previousPos : Vector3 ;
var currentPos : Vector3 ;
var layerMask = ~(1 << 8);

var dur : float = .5f ;
var hit : RaycastHit ;

var angleContact : float ;
var reflectDirection : Vector3 ;
var randAngle : float = 3 ;
var randSpeed : float = 100 ;
var ricochetAmmount : float = 0.4;

var spawnpoint : GameObject ;
var v : Vector3; ;
var kv : float = 0.05;

var t2 : float ;
var dist : float ;
var gunPos : Vector3 ;

var ec : float ;
var kec : float = 0.001 ;

function Start()
{
	time = Time.time;
	spawnpoint = gameObject;
	gunPos = transform.position ;
	v=vitesse*transform.forward ;
}

function FixedUpdate()
{
	previousPos = transform.position ;
}

function Update () 
{
	lifeTime = Time.time-time;
	
	if(lifeTime>2.5)
	{
		Destroy(gameObject);
	}
	
	transform.position += transform.right * Vector3.Magnitude(v)*Time.deltaTime ; // travel speed
	transform.position -= transform.right * kv*Vector3.Magnitude(v)*Time.deltaTime ; //air friction (just remove speed proportionally to the actual speed of the bullet)  
	
	t2 = Mathf.Pow((Time.time-time),2) ;
	transform.position.y +=  -(gravity/2)*t2 ; // add gravity : y(t) = 1/2*g*t²
	
	transform.position += wind.f * wind.s * t2 * coefVent; // wind
	v=(transform.position-previousPos)/(Time.deltaTime);
	ec = 0.5*m*Mathf.Pow(Vector3.Magnitude(v),2); // kinetic energy = 1/2*m*v²
	//Debug.Log(0.5*m*Mathf.Pow(Vector3.Magnitude(v),2));
	if(previousPos==Vector3.zero)
	{
		previousPos=spawnpoint.transform.position; //avoid getting linecast from origin 
	}
	currentPos = transform.position ;
	if((Time.time-time) > 0.001) //avoid getting a line from origin to bullet when shooting (bullet instantiated at (0,0,0)) !
	{
		Debug.DrawLine(previousPos, currentPos, Color.red, dur);
	}
	
	if (Physics.Linecast(previousPos, currentPos, hit, layerMask) && (Time.time-time) > 0.001) 
	{
		if (hit.rigidbody)
		{
			hit.rigidbody.AddForceAtPosition(kec*ec * transform.right, hit.point);
		}
		
		Debug.DrawRay(hit.point,hit.normal*1,Color.green, dur);
		angleContact = Vector3.Angle(transform.right,hit.normal);
		
		if(angleContact <= 120.0 && Random.Range(-1,ricochetAmmount)>=0) // ricochet 
		{
			reflectDirection = Vector3.Reflect(transform.right,hit.normal);
			reflectDirection += Vector3(Random.Range(0,randAngle),Random.Range(-randAngle,randAngle),Random.Range(-randAngle,randAngle));
			transform.right = reflectDirection;
			transform.position = hit.point ;
			vitesse = angleContact + Random.Range(-randSpeed,randSpeed) ;
		}
		else Destroy(gameObject);
		
	}
	
}
