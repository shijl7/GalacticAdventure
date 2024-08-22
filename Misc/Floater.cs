using UnityEngine;

public class Floater : MonoBehaviour
{
	public float speed = 2f;
	public float amplitude = 0.5f;

	protected void LateUpdate()
	{
		var wave = Mathf.Sin(Time.time * speed) * amplitude;
		transform.position += Vector3.up * wave * Time.deltaTime;
	}


}