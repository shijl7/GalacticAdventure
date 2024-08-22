using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider),typeof(AudioSource))]
public class Breakable : MonoBehaviour
{
	public UnityEvent onBreak;

	public GameObject display;
	public AudioClip clip;

	protected Collider m_collider;
	protected Rigidbody m_rigidbody;
	protected AudioSource m_audio;

	public bool broken { get; protected set; }

	protected void Start()
	{
		m_audio = GetComponent<AudioSource>();
		m_collider = GetComponent<Collider>();
		TryGetComponent<Rigidbody>(out m_rigidbody);
	}

	public virtual void Break()
	{
		if(!broken)
		{
			if (m_rigidbody)
			{
				m_rigidbody.isKinematic = true;
			}
			broken = true;
			display.SetActive(false);
			m_collider.enabled = false;
			m_audio.PlayOneShot(clip);
			onBreak?.Invoke();
		}
	}

}