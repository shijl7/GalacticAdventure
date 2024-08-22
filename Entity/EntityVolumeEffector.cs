using UnityEngine;

[RequireComponent (typeof(Collider))]
public class EntityVolumeEffector : MonoBehaviour
{
	public float velocityConversion = 1f;
	public float accelerationMultiplier = 1f;
	public float topSpeedMultiplier = 1f;
	public float decelerationMultiplier = 1f;
	public float turningDragMultiplier = 1f;
	public float gravityMultiplier = 1f;

	protected Collider m_collider;

	protected virtual void Start()
	{
		InitialCollider();
	}
	protected virtual void InitialCollider()
	{
		m_collider = GetComponent<Collider>();
		m_collider.isTrigger = true;
	}

	protected void OnTriggerEnter(Collider other)
	{
		if(other.TryGetComponent(out Entity entity))
		{
			entity.velocity *= velocityConversion;
			entity.accelerationMultiplier = accelerationMultiplier;
			entity.topSpeedMultiplier = topSpeedMultiplier;
			entity.decelerationMultiplier = decelerationMultiplier;
			entity.turningDragMultiplier = turningDragMultiplier;
			entity.gravityMultiplier = gravityMultiplier;
		}
	}


	protected void OnTriggerExit(Collider other)
	{
		if (other.TryGetComponent(out Entity entity))
		{
			entity.accelerationMultiplier = 1f;
			entity.topSpeedMultiplier = 1f;
			entity.decelerationMultiplier = 1f;
			entity.turningDragMultiplier = 1f;
			entity.gravityMultiplier = 1f;
		}
	}

}