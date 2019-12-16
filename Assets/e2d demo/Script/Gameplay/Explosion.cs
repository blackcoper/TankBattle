using UnityEngine;
using System.Collections;

public class Explosion : MonoBehaviour{
	public Transform explosion;
	private Vector3 point;
	private Quaternion explosionRotation;
	public float power = 10f;
	public float radius = 3f;
	private bool is_hit = true;
	private Collider[] objectsInRange;
	void Start(){
		audio.PlayDelayed(1f);
	}
	void Update()
	{
		RaycastHit hit = new RaycastHit();
		/*Vector3 fwd = transform.TransformDirection(Vector3.right);
		if(Physics.Raycast(this.transform.position,fwd,out hit,1f)){
			point = hit.point;
			explosionRotation = Quaternion.FromToRotation(Vector3.right,hit.normal);
			Explode();
		}*/
		objectsInRange = Physics.OverlapSphere(transform.position, 1f);
	    is_hit = true;
		foreach(Collider child in objectsInRange){
			if(child.gameObject.tag == "Tutorial")is_hit = false;
		}
		if(objectsInRange.Length>1 && is_hit){
			point = transform.position;
			explosionRotation = Quaternion.FromToRotation(Vector3.right,hit.normal);
			Explode();
		}
	}
	void Explode()
	{
		Instantiate(explosion, point, explosionRotation);
		Vector3 explosionPos = transform.position;
		Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);
		
		foreach (Collider hit in colliders) {
			if(hit.gameObject.tag == "Player"){
				cameraShake cam =  (cameraShake) GameObject.Find("MainCamera").GetComponent("cameraShake");  
				cam.Shake();
			}
			if(hit.gameObject.tag == "Enemy" || hit.gameObject.tag == "Player"){
				UnitStatus targetStatus = hit.gameObject.transform.parent.GetComponent<UnitStatus>();
				if(hit.gameObject.networkView!= null){
					if(hit.gameObject.networkView.isMine)hit.gameObject.networkView.RPC("receiveDamage", RPCMode.All, power, hit.gameObject.networkView.viewID);
				}else{
					hit.gameObject.GetComponent<CarControl>().receiveDamage(power);
				}
				//targetStatus.receiveDemage(power);
			}
			if (!hit)
				continue;
			
			if (hit.rigidbody)
				hit.rigidbody.AddExplosionForce(power, explosionPos, radius, 3.0f);
		}
		Destroy(gameObject,0.01f);
	}
	/* void boomTerrain(){
		for (int i = 0; i < Terrain.TerrainCurve.Count; i++)
		{
			Vector2 point = Terrain.TerrainCurve[i].position;
			float influence = GetBrushInfluence(point);
			switch (Inspector.Tool)
			{
				case e2dTerrainTool.ADJUST_HEIGHT:
					Vector2 delta = e2dConstants.BRUSH_HEIGHT_RATIO * influence * BrushOpacity * e2dUtils.Vector2dFromAngle(-BrushAngle + 90);
					Terrain.TerrainCurve[i].position = point + delta;
					change = true;
					break;
				case e2dTerrainTool.CURVE_TEXTURE:
					if (influence > 0 && Terrain.TerrainCurve[i].texture != Inspector.SelectedTexture)
					{
						Terrain.TerrainCurve[i].texture = Inspector.SelectedTexture;
						change = true;
					}
					break;
				case e2dTerrainTool.GRASS:
					float d = influence * BrushOpacity * e2dConstants.BRUSH_GRASS_RATIO;
					if (ControlPressed()) d = -d;
					Terrain.TerrainCurve[i].grassRatio = Mathf.Clamp01(Terrain.TerrainCurve[i].grassRatio + d);
					if (influence > 0) change = true;
					break;
			}
		}
	}*/
}

