using UnityEngine;

[RequireComponent (typeof(Collider))]
public class Spring : MonoBehaviour,IEntityContact
{
	public float force = 25f;
	public AudioClip clip;

	protected Collider m_collider;
	protected AudioSource m_audio;

	protected void Start()
	{
		tag = GameTags.Spring;
		m_collider = GetComponent<Collider>();
		if(!TryGetComponent(out m_audio))
		{
			m_audio=gameObject.AddComponent<AudioSource>();
		}
	}

	public void OnEntityContact(Entity entity)
	{
        if (entity.IsPointUnderStep(m_collider.bounds.max) && entity is Player player && player.isAlive)
        {
			ApplyForce(player);
			player.SetJump(1);
			player.ResetAirDash();
			player.ResetAirSpins();
			player.states.Change<FallPlayerState>();
        }
    }

	public void ApplyForce(Player player)
	{
		if(player.verticalVelocity.y <= 0)
		{
			player.verticalVelocity = Vector3.up * force;
			m_audio.PlayOneShot(clip);
		}
	}
}