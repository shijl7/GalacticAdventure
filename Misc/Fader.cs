using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Fader : Singleton<Fader>
{
	public float speed = 1f;
	protected Image m_image;

	protected override void Awake()
	{
		base.Awake();

		m_image = GetComponent<Image>();
	}

	public void FadeOut(Action onFinished)
	{
		StopAllCoroutines();
		StartCoroutine(FadeOutRountine(onFinished));
	}

	protected virtual IEnumerator FadeOutRountine(Action onFinished)
	{
		while(m_image.color.a < 1)
		{
			var color = m_image.color;
			color.a += speed * Time.deltaTime;
			m_image.color = color;
			yield return null;
		}
		onFinished?.Invoke();
	}

	public void FadeIn(Action onFinished)
	{
		StopAllCoroutines();
		StartCoroutine(FadeInRountine(onFinished));
	}

	protected virtual IEnumerator FadeInRountine(Action onFinished)
	{
		while (m_image.color.a > 0)
		{
			var color = m_image.color;
			color.a -= speed * Time.deltaTime;
			m_image.color = color;
			yield return null;
		}
		onFinished?.Invoke();
	}

}