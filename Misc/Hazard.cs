using UnityEngine;

[RequireComponent (typeof(Collider))]
public class Hazard : MonoBehaviour,IEntityContact
{
	protected Collider m_collider;
	public bool isSolid;//是否是固体
	public bool damageOnlyFromAbove;
	public int damage = 1;

	protected void Awake()
	{
		tag = GameTags.Hazard;
		m_collider = GetComponent<Collider>();
		m_collider.isTrigger = !isSolid;
	}

	protected virtual void TryToApplyDamageTo(Player player)
	{
		if(!damageOnlyFromAbove || player.velocity.y <= 0 && player.IsPointUnderStep(m_collider.bounds.max))
		{
			player.ApplyDamage(damage, transform.position);
		}
	}

	public void OnEntityContact(Entity entity)
	{
		if(entity is Player player)
		{
			TryToApplyDamageTo(player);
		}
	}

	protected void OnTriggerStay(Collider other)
	{
		if(other.CompareTag(GameTags.Player))
		{
			if(other.TryGetComponent<Player>(out var player))
			{
				TryToApplyDamageTo(player);
			}
		}
	}

}