using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent (typeof(BoxCollider))]
public class ItemBox : MonoBehaviour,IEntityContact
{
	protected BoxCollider m_collider;
	protected Vector3 m_initialScale;
	public Collectable[] collectables;
	protected bool m_enabled = true;

	public MeshRenderer itemBoxRender;
	public Material emptyItemBoxMaterial;

	[Space(15)]
	public UnityEvent onCollect;
	public UnityEvent onDisable;

	protected int m_index;
	protected void Start()
	{
		m_collider = GetComponent<BoxCollider> ();
		m_initialScale = transform.localScale;
		InitializeCollectables ();
	}

	protected virtual void InitializeCollectables()
	{
		foreach (var collectable in collectables)
		{
			if(!collectable.hidden)
			{
				collectable.gameObject.SetActive(false);
			}
			else
			{
				collectable.collectOnContact = false;
			}
		}
	}

	public void OnEntityContact(Entity entity)
	{
		//entity是否是Player类的实例，如果是将其转换为 Player 类型的变量 player
		if (entity is Player player)
		{
            if (entity.velocity.y > 0 && entity.position.y < m_collider.bounds.min.y)
            {
				Collect(player);
            }
        }
	}

	public virtual void Collect(Player player)
	{
		if(m_enabled)
		{
			if(m_index < collectables.Length)
			{
				if (collectables[m_index].hidden)
				{
					collectables[m_index].Collect(player);
				}
                else
                {
					collectables[m_index].gameObject.SetActive(true);
                }
				m_index = Mathf.Clamp(m_index + 1, 0, collectables.Length);
				onCollect?.Invoke();
            }
			if(m_index == collectables.Length)
			{
				Disable();
			}
		}
	}

	public void Disable()
	{
		if (m_enabled)
		{
			m_enabled = false;
			itemBoxRender.sharedMaterial = emptyItemBoxMaterial;
			onDisable?.Invoke();
		}
	}
}