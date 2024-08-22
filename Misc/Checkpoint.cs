using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class Checkpoint : MonoBehaviour
{
	public UnityEvent onActivate;
	public Transform respawnPoint;
	public AudioClip respawnSound;

	protected AudioSource m_audio;
	protected Collider m_collider;
	public bool activated {  get; protected set; }

	protected void Awake()
	{
		if(!TryGetComponent(out m_audio))
		{
			m_audio = gameObject.AddComponent<AudioSource>();
		}
		m_collider = GetComponent<Collider>();
		m_collider.isTrigger = true;
	}

	protected virtual void OnTriggerEnter(Collider other)
	{
		if(!activated && other.CompareTag(GameTags.Player))
		{
			if(other.TryGetComponent<Player>(out var player))
			{
				Activate(player);
			}
		}
	}

	protected virtual void Activate(Player player)
	{
		if(!activated)
		{
			activated = true;
			m_audio.PlayOneShot(respawnSound);
			player.SetRespawn(respawnPoint.position,respawnPoint.rotation);
			onActivate?.Invoke();
		}
	}
}