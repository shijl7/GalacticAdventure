using UnityEngine;

public class Pole : MonoBehaviour
{
	public new CapsuleCollider collider { get; protected set; }

	public Vector3 center => transform.position;

	protected void Awake()
	{
		tag = GameTags.Pole;
		collider = GetComponent<CapsuleCollider>();
	}

	public virtual Vector3 GetDirectionToPole(Transform other) => GetDirectionToPole(other, out _);

	public virtual Vector3 GetDirectionToPole(Transform other, out float distance)
	{
		var target = new Vector3(center.x, other.position.y, center.z) - other.position;
		distance = target.magnitude;
		return target / distance;
	}

	public Vector3 ClampPointToPoleHeight(Vector3 point, float offset)
	{
		var minHeight = collider.bounds.min.y + offset;
		var maxHeight = collider.bounds.max.y - offset;
		var clampHeight = Mathf.Clamp(point.y, minHeight, maxHeight);
  		return new Vector3(point.x, clampHeight, point.z);
	}

	public Vector2 GetHeightRange(float offset)
	{
		var minHeight = collider.bounds.min.y + offset;
		var maxHeight = collider.bounds.max.y - offset;
		return new Vector2(minHeight, maxHeight);
	}

}