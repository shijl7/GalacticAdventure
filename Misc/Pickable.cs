using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class Pickable : MonoBehaviour
{
	public UnityEvent onPicked;
	public UnityEvent onRespawn;
	public UnityEvent onReleased;

	[Header("General Setting")]
	public Vector3 offset;
	public float releaseOffset = 0.5f;


	[Header("Attack Setting")]
	public bool attackEnemies = true;
	public int damage = 1;
	public float minDamageSpeed = 5f;

	[Header("Respawn Settings")]
	public bool autoRespawn;
	public bool respawnOnHitHazards;
	public float respawnHeightLimit = -100;

	protected Collider m_collider;
	protected Rigidbody m_rigidbody;
	protected Vector3 m_initialPosition;
	protected Quaternion m_initialRotation;
	protected Transform m_initialParent;

	protected RigidbodyInterpolation m_interpolation;

	public bool beingHold { get; protected set; }

	protected void Start()
	{
		m_collider = GetComponent<Collider>();
		m_rigidbody = GetComponent<Rigidbody>();
		m_initialPosition = transform.localPosition;
		m_initialRotation = transform.localRotation;
		m_initialParent = transform.parent;
	}

	public void OnEntityContact(Entity entity)
	{
		if(attackEnemies && entity is Enemy && m_rigidbody.velocity.magnitude > minDamageSpeed)
		{
			entity.ApplyDamage(damage, transform.position);
		}
	}

	public virtual void PickUp(Transform slot)
	{
		if(!beingHold)
		{
			beingHold = true;
			transform.parent = slot;
			transform.localPosition = Vector3.zero + offset;
			transform.localRotation = Quaternion.identity;
			m_rigidbody.isKinematic = true;
			m_collider.isTrigger = true;
			m_interpolation = m_rigidbody.interpolation;
			m_rigidbody.interpolation = RigidbodyInterpolation.None;
			onPicked?.Invoke();
		}
	}

	public virtual void Release(Vector3 direction, float force)
	{
		if(beingHold)
		{
			transform.parent = null;
			transform.position += direction * releaseOffset;
			m_collider.isTrigger = m_rigidbody.isKinematic = beingHold = false;
			m_rigidbody.interpolation = m_interpolation;
			m_rigidbody.velocity = direction * force;
			onReleased?.Invoke();
		}
	}

	protected void OnTriggerEnter(Collider other)
	{
		EvaluateHazardRespawn(other);
	}
	protected void OnCollisionEnter(Collision collision)
	{
		EvaluateHazardRespawn(collision.collider);
	}

	protected virtual void EvaluateHazardRespawn(Collider other)
	{
		if(autoRespawn && respawnOnHitHazards && other.CompareTag(GameTags.Hazard))
		{
			Respawn();
		}
	}

	private void Respawn()
	{
		m_rigidbody.velocity = Vector3.zero;
		transform.parent = m_initialParent;
		transform.SetPositionAndRotation(m_initialPosition, m_initialRotation);
		m_rigidbody.isKinematic = m_collider.isTrigger = beingHold = false;
	}
}