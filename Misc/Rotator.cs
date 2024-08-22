﻿using UnityEngine;

public class Rotator : MonoBehaviour
{
	public Space space;
	public Vector3 eulars = new Vector3(0, 180, 0);

	protected void LateUpdate()
	{
		transform.Rotate(eulars * Time.deltaTime, space);
	}
}