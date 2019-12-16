var bullet : Transform ;
var spawnpoint : Transform ;
var cam : Transform;
var muzzle : GameObject ;
var shootSound : AudioClip ;
var aim : boolean ;

function Update () 
{

	if(Input.GetKeyDown("mouse 1")){ aim = !aim;}
	if(aim) {transform.localPosition = Vector3(-1.073074,0.1459433,0.6036019);}
	if(!aim){transform.localPosition = Vector3(0.4653091,-0.1754849,0.556822);}
	if(Input.GetKeyDown("mouse 0"))
	{
		shoot();
		Instantiate(bullet, spawnpoint.transform.position, cam.transform.rotation);
		audio.PlayOneShot(shootSound);
		
	}
	
	if(!Input.GetKeyDown("escape"))
	{
		Screen.lockCursor = true ;
	}
	
	
	
	if(Input.GetKeyDown("f"))
	{
		Time.timeScale = 1 ;
	}
	
	if(Input.GetKeyDown("g"))
	{
		Time.timeScale = 0.05 ;
	}
	
	}

function shoot ()
{
	muzzle.renderer.enabled = true ;
	var lol: float = Random.Range(1,10);
	transform.eulerAngles.x -= lol;
	yield WaitForSeconds(0.05) ;
	muzzle.renderer.enabled = false ;
	transform.eulerAngles.x += lol; 
}
function OnMouseDown () {
        // Lock the cursor
        Screen.lockCursor = true;}