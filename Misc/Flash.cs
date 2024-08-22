using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof(Image))]
public class Flash : Singleton<Flash>
{
	public float duration = 0.1f;
	public float fadeDuration = 0.5f;

	protected Image image;

	protected virtual void Start()
	{
		image = GetComponent<Image>();
	}

	public void Trigger() => Trigger(duration,fadeDuration);

	public void Trigger(float duration,float fadeDuration)
	{
		StopAllCoroutines();
		StartCoroutine(Routine(duration, fadeDuration));
	}

	protected virtual IEnumerator Routine(float duration, float fadeDuration)
	{
		float elapsedTime = 0f;
		var color = image.color;
		color.a = 1f;
		image.color = color;

		yield return new WaitForSeconds(duration);

		while (elapsedTime < fadeDuration)
		{
			color.a = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
			elapsedTime += Time.deltaTime;
			image.color = color;
			yield return null;
		}
		color.a = 0f;
		image.color = color;
	}
}