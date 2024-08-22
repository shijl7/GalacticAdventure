﻿using System.Collections;
using UnityEngine;

public class GridPlatform : MonoBehaviour
{
	public Transform platform;
	public float rotationDuration = 0.5f;

	protected bool m_clockwise = true;

	protected virtual void Start()
	{
		FindObjectOfType<Player>().playerEvents.OnJump.AddListener(Move);
	}

	public virtual void Move()
	{
		StopAllCoroutines();
		StartCoroutine(MoveRoutine());
	}

	protected virtual IEnumerator MoveRoutine()
	{
		var elapsedTime = 0f;
		var from = platform.localRotation;
		var to = Quaternion.Euler(0, 0, m_clockwise ? 180 : 0);
		m_clockwise = !m_clockwise;
		while (elapsedTime < rotationDuration)
		{
			elapsedTime += Time.deltaTime;
			platform.localRotation = Quaternion.Lerp(from, to, elapsedTime / rotationDuration);
			yield return null;
		}
		platform.localRotation = to;
	}

}