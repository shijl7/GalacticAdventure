using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
	public UnityEvent onChange;

	protected int m_currentHealth;
	public int max;
	public int initial;
	public float coolDown = 1f;
	public int current
	{
		get { return m_currentHealth; }
		set
		{
			var last = m_currentHealth;
			if (last != value)
			{
				m_currentHealth = Mathf.Clamp(value, 0, max);
				onChange?.Invoke();
			}
		}

	}

	protected void Start()
	{
		current = initial;

	}

	public virtual void Reset()
	{
		current = initial;

	}

	public virtual bool isEmpty => current == 0;
	protected float m_lastDamageTime;
	public virtual bool recovering => Time.time < m_lastDamageTime + coolDown;
	public UnityEvent onDamage;

	public virtual void Damage(int amount)
	{
		if(!recovering)
		{
			current -= Mathf.Abs(amount);
			m_lastDamageTime = Time.time;
			onDamage?.Invoke();
		}
	}

	public virtual void Increase(int amount) => current += amount;

}
