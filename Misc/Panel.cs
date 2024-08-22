﻿using UnityEngine;
using UnityEngine.Events;

public class Panel : MonoBehaviour,IEntityContact
{
	public UnityEvent onActivate;
	public UnityEvent onDeactivate;

	public bool autioToggle;
	protected Collider m_collider;
	protected Collider m_entityActivator;
	protected Collider m_otherActivator;
	protected AudioSource m_audio;
	public bool requirePlayer;
	public bool requireStomp;

	public AudioClip activateClip;
	public AudioClip deactivateClip;

	protected void Start()
	{
		gameObject.tag = GameTags.Panel;
		m_collider = GetComponent<Collider>();
		m_audio = GetComponent<AudioSource>();
	}

	protected void Update()
	{
		if (m_entityActivator || m_otherActivator)
		{
			var center = m_collider.bounds.center;
			var contactOffset = Physics.defaultContactOffset + 0.1f;
			var size = m_collider.bounds.size + Vector3.up * contactOffset;
			var bounds = new Bounds(center, size);

			var intersectsEntity = m_entityActivator && bounds.Intersects(m_entityActivator.bounds);
			var intersectsOther = m_otherActivator && bounds.Intersects(m_otherActivator.bounds);
			if (intersectsEntity || intersectsOther)
			{
				Activate();
			}
            else
            {
				m_entityActivator = intersectsEntity ? m_entityActivator : null;
				m_otherActivator = intersectsOther ? m_otherActivator : null;
				if(autioToggle)
				{
					Deactivate();
				}
            }

        }
	}

	public bool activated {  get; protected set; }

	public virtual void Activate()
	{
		if(!activated)
		{
			if (activateClip)
			{
				m_audio.PlayOneShot(activateClip);
			}
			activated = true;
			onActivate?.Invoke();
		}
	}

	public virtual void Deactivate()
	{
		if (activated)
		{
			if (deactivateClip)
			{
				m_audio.PlayOneShot(deactivateClip);
			}
			activated = false;
			onDeactivate?.Invoke();
		}
	}

	public void OnEntityContact(Entity entity)
	{
		if(entity.velocity.y <= 0 && entity.IsPointUnderStep(m_collider.bounds.max))
		{
			if((!requirePlayer || entity is Player) && 
				(!requireStomp || (entity as Player).states.IsCurrentOfType(typeof(StompPlayerState))))
			{
				m_entityActivator = entity.controller;
			}
		}
	}

	protected void OnCollisionStay(Collision collision)
	{
		if(!(requirePlayer || requireStomp) && !collision.collider.CompareTag(GameTags.Player))
		{
			m_otherActivator = collision.collider;
		}
	}

}