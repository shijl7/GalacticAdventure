using System.Collections;
using UnityEngine;

[RequireComponent (typeof(Collider))]
public class Collectable : MonoBehaviour
{
	[Header("General Settings")]
	public GameObject display;
	public bool collectOnContact = true;
	public AudioClip clip;
	public int times = 1;
	public ParticleSystem particle;
	public float ghostingDuration = 0.5f;

	[Header("Visibility Settings")]
	public bool hidden;
	public float quickShowHeight = 2f;
	public float quickShowDuration = 0.25f;
	public float hideDuration = 0.5f;


	[Header("Life Time")]
	public bool hasLifeTime;
	public float lifeTimeDuration = 5f;

	[Header("Physics Settings")]
	public bool usePhysics;
	public Vector3 initialVelocity = new Vector3(0, 12, 0);
	public float gravity = 15f;
	public bool randomizeInitialDirection = true;
	public float collisionRadius = 0.5f;
	public float bounciness = 0.98f;
	public float maxBounceYVelocity = 10f;
	public AudioClip collisionClip;
	public float minForceToStopPhysics = 3f;

	[Space(15)]
	public PlayerEvent OnCollect;
	protected AudioSource m_audio;
	protected Collider m_collider;
	protected Vector3 m_velocity;
	protected bool m_vanished;
	protected bool m_ghosting = true;
	protected float m_elapsedGhostingTime;
	protected float m_elapsedLifeTime;


	protected const int k_verticalMinRotation = 0;
	protected const int k_verticalMaxRotation = 30;
	protected const int k_horizontalMinRotation = 0;
	protected const int k_horizontalMaxRotation = 360;


	protected void Awake()
	{
		//初始化音效
		InitializeAudio();
		InitializeCollider();
		InitializeTransform();
		InitializeDisplay();
		InitializeVelocity();

	}

	protected virtual void Update()
	{
		if(!m_vanished)
		{
			HandleGhosting();
			HandleLifeTime();
			if(usePhysics)
			{
				HnadleMovement();
				HandleSweep();
			}
		}
	}

	protected virtual void OnTriggerStay(Collider other)
	{
		if(collectOnContact && other.CompareTag(GameTags.Player))
		{
			if(other.TryGetComponent<Player>(out var player))
			{
				Collect(player);
			}
		}
	}

	protected void OnDrawGizmos()
	{
		if (usePhysics)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(transform.position, collisionRadius);
		}
	}

	protected virtual void InitializeAudio()
	{
		if(!TryGetComponent(out m_audio))
		{
			m_audio = gameObject.AddComponent<AudioSource>();
		}
	}

	protected virtual void InitializeCollider()
	{
		m_collider = GetComponent<Collider>();
		m_collider.isTrigger = true;
	}

	protected virtual void InitializeTransform()
	{
		transform.parent = null;
		transform.rotation = Quaternion.identity;
	}

	protected virtual void InitializeDisplay()
	{
		display.SetActive(!hidden);
	}

	protected virtual void InitializeVelocity()
	{
		var direction = initialVelocity.normalized;
		var force = initialVelocity.magnitude;
		if (randomizeInitialDirection)
		{
			var randomZ = UnityEngine.Random.Range(k_verticalMinRotation,k_verticalMaxRotation);
			var randomY = UnityEngine.Random.Range(k_horizontalMinRotation, k_horizontalMaxRotation);

			direction = Quaternion.Euler(0, 0, randomZ) * direction;
			direction = Quaternion.Euler(0, randomY, 0) * direction;
		}
		m_velocity = direction * force;
	}

	public virtual void Vanish()
	{
		if(!m_vanished)
		{
			m_vanished = true;
			m_elapsedLifeTime = 0;
			display.SetActive(false);
			m_collider.enabled = false;
		}
	}

	protected virtual void HandleGhosting()
	{
		if(m_ghosting)
		{
			m_elapsedGhostingTime += Time.deltaTime;
            if (m_elapsedGhostingTime >= ghostingDuration)
            {
				m_elapsedGhostingTime = 0;
				m_ghosting = false;
			}
        }
	}

	protected virtual void HandleLifeTime()
	{
		if(hasLifeTime)
		{
			m_elapsedLifeTime += Time.deltaTime;
			if(m_elapsedLifeTime >= lifeTimeDuration)
			{
				Vanish();//消失
			}
		}
	}

	protected virtual void HnadleMovement()
	{
		m_velocity.y -= gravity * Time.deltaTime;
	}

	protected virtual void HandleSweep()
	{
		var direction = m_velocity.normalized;
		var magnitude = m_velocity.magnitude;
		var distance = magnitude * Time.deltaTime;

		if(Physics.SphereCast(transform.position,collisionRadius,direction,
			out var hit,distance,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore))
		{
			if(!hit.collider.CompareTag(GameTags.Player))
			{
				//计算反弹方向
				var bounceDirection = Vector3.Reflect(direction, hit.normal);
				m_velocity = bounceDirection * magnitude * bounciness;
				m_velocity.y = Mathf.Min(m_velocity.y, maxBounceYVelocity);
				m_audio.Stop();
				m_audio.PlayOneShot(collisionClip);

				if(m_velocity.y <= minForceToStopPhysics)
				{
					usePhysics = false;
				}

			}
		}

		transform.position += m_velocity * Time.deltaTime;
	}

	public virtual void Collect(Player player)
	{
		if(!m_vanished && !m_ghosting)
		{
            if (!hidden)
            {
				Vanish();
                if (particle != null)
                {
                    particle.Play();
                }
            }
			else
			{
				StartCoroutine(QuickShowRoutine());
			}

			StartCoroutine(CollectRoutine(player));
        }
	}

	protected virtual IEnumerator CollectRoutine(Player player)
	{
        for (int i = 0; i < times; i++)
        {
			m_audio.Stop();
			m_audio.PlayOneShot(clip);
			OnCollect.Invoke(player);
			yield return new WaitForSeconds(0.1f);
        }
    }

	//展示常规
	protected virtual IEnumerator QuickShowRoutine()
	{
		var elapsedTime = 0f;
		var initialPosition = transform.position;
		var targetPosition = initialPosition + Vector3.up * quickShowHeight;
		display.SetActive(true);
		m_collider.enabled = false;

		while(elapsedTime < quickShowDuration)
		{
			var t = elapsedTime / quickShowDuration;
			transform.position = Vector3.Lerp(initialPosition, targetPosition, t);
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		transform.position = targetPosition;
		yield return new WaitForSeconds(hideDuration);
		transform.position = initialPosition;
		Vanish();
	}

}