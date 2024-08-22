using UnityEngine;

[RequireComponent (typeof(Collider),typeof(AudioSource))]
public class Portal : MonoBehaviour
{
	public bool useFlash = true;
	public Portal exit;
	public float exitOffset = 1f;
	public AudioClip teleportClip;

	protected Collider m_collider;
	protected AudioSource m_audio;
	protected PlayerCamera m_playerCamera;

	public Vector3 position => transform.position;
	public Vector3 forward => transform.forward;

	protected virtual void Start()
	{
		m_collider = GetComponent<Collider>();
		m_collider.isTrigger = true;
		m_audio = GetComponent<AudioSource>();
		m_playerCamera = FindObjectOfType<PlayerCamera>();
	}

	protected void OnTriggerEnter(Collider other)
	{
		if(exit && other.TryGetComponent(out Player player))
		{
			var yOffset = player.transform.position.y - position.y;
			player.transform.position = exit.position + Vector3.up * yOffset;
			player.FaceDirection(exit.forward);
			m_playerCamera.Reset();

			var inputDirection = player.inputs.GetMovementCamerDirection();
			if(Vector3.Dot(exit.forward, inputDirection) < 0f)
			{
				player.FaceDirection(-exit.forward);
			}
			player.transform.position += player.transform.forward * exit.exitOffset;
			player.lateralVelocity = player.transform.forward * player.lateralVelocity.magnitude;

			if(useFlash)
			{
				Flash.instance?.Trigger();
			}
			m_audio.PlayOneShot(teleportClip);
		}
	}

}