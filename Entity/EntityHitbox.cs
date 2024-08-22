using System.Data;
using UnityEngine;

public class EntityHitbox : MonoBehaviour
{
	[Header("Attack Setting")]
	public int damage = 1;
	public bool breakObjects;

	[Header("Rebound Setting")]
	public bool rebound;
	public float reboundMinForce = 10f;
	public float reboundMaxForce = 25f;

	[Header("Push Back Setting")]
	public bool pushBack;
	public float pushBackMinMagnitude = 5f;
	public float pushBackMaxMagnitude = 10f;

	protected Entity m_entity;
	protected Collider m_collider;

	protected virtual void Start()
	{
		InitializeEntity();
		InitializeCollider();
	}

	protected virtual void InitializeEntity()
	{
        if (!m_entity)
        {
			m_entity = GetComponentInParent<Entity>();
		}
	}

	protected virtual void InitializeCollider()
	{
		m_collider = GetComponent<Collider>();
		m_collider.isTrigger = true;
	}

	protected void OnTriggerEnter(Collider other)
	{
		HandleCollision(other);
		HandleCustomCollision(other);
	}

	protected virtual void HandleCollision(Collider other)
	{
		if(other != m_entity.controller)
		{
			if(other.TryGetComponent(out Entity target))//如果碰到的是实体
			{
				HandleEntityAttak(target);
				HandleRebound();
				HandlePushBack();
			}
			else if (other.TryGetComponent(out Breakable breakable))//如果碰到的是可破坏物
            {
				HandleBreakableObject(breakable);
            }
        }
	}

	protected virtual void HandleEntityAttak(Entity other)
	{
		other.ApplyDamage(damage,transform.position);
	}


	protected virtual void HandleRebound()
	{
		if (rebound)
		{
			var force = -m_entity.velocity.y;
			force = Mathf.Clamp(force, reboundMinForce, reboundMaxForce);
			m_entity.verticalVelocity = Vector3.up * force;
		}
	}
	protected virtual void HandlePushBack()
	{
		if (pushBack)
		{
			var force = -m_entity.lateralVelocity.magnitude;
			force = Mathf.Clamp(force, pushBackMinMagnitude, pushBackMaxMagnitude);
			m_entity.lateralVelocity = -transform.forward * force;
		}
	}

	protected virtual void HandleBreakableObject(Breakable breakable)
	{
		if (breakObjects)
		{
			breakable.Break();
		}
	}

	protected virtual void HandleCustomCollision(Collider other)
	{

	}

}