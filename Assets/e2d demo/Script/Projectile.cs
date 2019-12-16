using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour 
{
    // start and end positions (used for caclulating the curve)

    public Vector3 start, end;
    // the distance where t is increased
    public float ReachedDistance = 0.1f;
    // the speed of the projectile
    public float Speed = 1f;
    // the highest point of the curve
    [HideInInspector]
    public float Height = 5f;
    // the resolution of the curve the lower value the smoother the curve will be
    public float Resolution = 0.05f;
    // used internally to calculate velocity
    Vector3 targetPos;
    float speedMultiplier;
    float t;
    void Start()
    {
        transform.position = start;
        targetPos = start;
    }

    void OnCollisionEnter(Collision col)
    {
        // if it hits something it will stop following the trail
        enabled = false;
    }

    void OnDrawGizmos()
    {
        if (!enabled)
            return;
        float res = 0.05f;
        float height = Height;
        Gizmos.color = Color.blue;
        for (float f = 0; f < 1; f += res)
        {
            Gizmos.DrawLine(GetPointOnTrail(start, end, height, f), GetPointOnTrail(start, end, height, f + res));
        }
    }

    public static Vector3 GetPointOnTrail(Vector3 start, Vector3 end, float height, float t)
    {
        // calculates the position on the parabolic cuvre (t must be between 0 and 1)
        return Vector3.Lerp(start, end, t) + Vector3.up * (-1f * Mathf.Pow(t * 2f - 1f, 2) + 1f) * height;
    }

	void FixedUpdate() 
    {
        if (Vector3.Distance(transform.position, targetPos) <= ReachedDistance)
        {
            t += Resolution;
            t = Mathf.Clamp01(t);
            speedMultiplier = 0.5f + Mathf.Abs(0.5f - t);
            targetPos = GetPointOnTrail(start, end, Height, t);
        }
        rigidbody.velocity = (targetPos - transform.position).normalized * Speed * speedMultiplier;
	}
}
