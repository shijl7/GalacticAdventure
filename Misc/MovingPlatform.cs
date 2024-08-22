using UnityEngine;

[RequireComponent (typeof(WaypointManager))]
[RequireComponent(typeof(Collider))]
public class MovingPlatform : MonoBehaviour
{
	public float speed = 3f;
	public WaypointManager pointManager;

	protected virtual void Awake()
	{
		tag = GameTags.Platform;
		pointManager = GetComponent<WaypointManager>();
	}

	protected virtual void Update()
	{
		var position = transform.position;
		var target = pointManager.current.position;
		position = Vector3.MoveTowards(position, target, speed * Time.deltaTime);
		transform.position = position;

		if(Vector3.Distance(position,target) == 0)
		{
			pointManager.Next();
		}
	}

}
