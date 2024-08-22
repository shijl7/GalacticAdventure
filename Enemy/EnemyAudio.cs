using UnityEngine;

public class EnemyAudio : MonoBehaviour
{
	protected Enemy m_enemy;
	protected AudioSource m_audio;
	public AudioClip deathAudio;

	protected void Start()
	{
		InitialEnemy();
		InitialAudio();
		InitialCallBacks();
	}

	protected virtual void InitialEnemy() => m_enemy = GetComponent<Enemy>();
	protected virtual void InitialAudio()
	{
		if(!TryGetComponent(out m_audio))
		{
			m_audio = gameObject.AddComponent<AudioSource>();
		}
	}

	protected virtual void InitialCallBacks()
	{
		m_enemy.enemyEvents.OnDie.AddListener(() => m_audio.PlayOneShot(deathAudio));
	}

}