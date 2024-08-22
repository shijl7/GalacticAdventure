using UnityEngine;

[RequireComponent (typeof(Collider))]
public class GravityField : MonoBehaviour
{
	public float force = 75f;
	protected Collider m_collider;

	protected void Start()
	{
		m_collider = GetComponent<Collider>();
		m_collider.isTrigger = true;
	}

	protected void OnTriggerStay(Collider other)
	{
		if(other.tag.Contains(GameTags.Player))
		{
			if(other.TryGetComponent(out Player player))
			{
				if(player.isGrounded)
				{
					player.verticalVelocity = Vector3.zero;
				}
				player.velocity += transform.up * force * Time.deltaTime;
			}
		}
	}

}