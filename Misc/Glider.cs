using System.Collections;
using UnityEngine;

public class Glider : MonoBehaviour
{
	public Player player;
	public TrailRenderer[] trails;
	public float scaleDuration = 0.7f;

	[Header("Audio Clip")]
	public AudioClip openAudio;
	public AudioClip closeAudio;

	protected AudioSource m_audio;

	protected virtual void Start()
	{
		InitialPlayer();
		InitialCallback();
		InitialAudio();
		InitializeGlider();
	}

	protected virtual void InitialPlayer()
	{
		if (!player)
			player = GetComponentInParent<Player>();
	}

	protected virtual void InitialAudio()
	{
		if (!TryGetComponent(out m_audio))
			m_audio = gameObject.AddComponent<AudioSource>();
	}

	protected virtual void InitialCallback()
	{
		player.playerEvents.OnGlidingStart.AddListener(ShowGlider);
		player.playerEvents.OnGlidingStop.AddListener(HideGlider);
	}

	protected virtual void InitializeGlider()
	{
		SetTrailsEmitting(false);
		transform.localScale = Vector3.zero;
	}

	protected virtual void ShowGlider()
	{
		StopAllCoroutines();
		StartCoroutine(ScaleGliderRoutine(Vector3.zero, Vector3.one));
		SetTrailsEmitting(true);
		m_audio.PlayOneShot(openAudio);
	}

	protected virtual void HideGlider()
	{
		StopAllCoroutines();
		StartCoroutine(ScaleGliderRoutine(Vector3.one, Vector3.zero));
		SetTrailsEmitting(false);
		m_audio.PlayOneShot(closeAudio);
	}

	protected virtual IEnumerator ScaleGliderRoutine(Vector3 from, Vector3 to)
	{
		var elaspedTime = 0f;
		transform.localScale = from;
		while(elaspedTime < scaleDuration)
		{
			var scale = Vector3.Lerp(from, to, elaspedTime / scaleDuration);
			transform.localScale = scale;
			elaspedTime += Time.deltaTime;
			yield return null;
		}
		transform.localScale = to;
	}

	protected virtual void SetTrailsEmitting(bool value)
	{
		if(trails == null) return;
        foreach (var trail in trails)
        {
			trail.emitting = value;
        }
    }
}