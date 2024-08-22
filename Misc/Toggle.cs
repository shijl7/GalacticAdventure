using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Toggle : MonoBehaviour
{
	public UnityEvent onActivate;
	public UnityEvent onDeactivate;
	public float delay;
	public bool state = true;
	public Toggle[] multiTrigger;

	public virtual void Set(bool value)
	{
		StopAllCoroutines();
		StartCoroutine(SetRoutine(value));
	}

	protected virtual IEnumerator SetRoutine(bool value)
	{
		yield return new WaitForSeconds(delay);
		if(value)
		{
			if (!state)
			{
				state = true;
                foreach (var toggle in multiTrigger)
                {
                    toggle.Set(state);
                }
				onActivate?.Invoke();
            }
		}
		else if(state)
		{
			state = false;
			foreach (var toggle in multiTrigger)
			{
				toggle.Set(state);
			}
			onDeactivate?.Invoke();
		}
	}

}
