using UnityEngine;
using UnityEngine.Splines;

public abstract class Entity : MonoBehaviour
{
	public EntityEvents entityEvents;
	public bool isGrounded { get; protected set; } = true;
	public bool onRails { get; set; }
	public Vector3 velocity { get; set; }
	public float turningDragMultiplier { get; set; } = 1f;
	public float topSpeedMultiplier { get; set; } = 1f;
	public float accelerationMultiplier { get; set; } = 1f;
	public float decelerationMultiplier { get; set; } = 1f;
	public float gravityMultiplier { get; set; } = 1f;

	public CharacterController controller { get; protected set; }
	public float lastGroundTime {  get; protected set; }
	protected Collider[] m_contactBuffer = new Collider[10];
	protected CapsuleCollider m_collider;
	protected Rigidbody m_rigidbody;
	protected readonly float m_groundOffset = 0.1f;
	protected readonly float m_slopingGroundAngle = 20f;
	public float originalHeight { get; protected set; }
	public Vector3 unsizePosition => position - transform.up * height * 0.5f + transform.up * originalHeight * 0.5f;
	public RaycastHit groundHit;

	public float groundAngle { get; protected set; }
	public Vector3 groundNormal { get; protected set; }
	public Vector3 localSlopeDirection { get; protected set; }
	public float positionDleta { get; protected set; }
	public Vector3 lastPosition { get; protected set; }

	public Vector3 lateralVelocity
	{
		get { return new Vector3(velocity.x, 0, velocity.z); }
		set { velocity = new Vector3(value.x, velocity.y, value.z); }
	}

	public SplineContainer rails { get; protected set; }

	public Vector3 verticalVelocity
	{
		get { return new Vector3(0, velocity.y, 0); }
		set { velocity = new Vector3(velocity.x, value.y, velocity.z); }
	}
	public float height => controller.height;
	public float radius => controller.radius;
	public Vector3 center => controller.center;
	public Vector3 position => transform.position + center;

	public virtual bool SphereCast(Vector3 direction, float distance,
		int layer = Physics.DefaultRaycastLayers,
		QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
	{
		return SphereCast(direction, distance, out _, layer, queryTriggerInteraction);
	}

	public virtual bool SphereCast(Vector3 direction,float distance,out RaycastHit hit,
		int layer=Physics.DefaultRaycastLayers,
		QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
	{
		var castDistance = Mathf.Abs(distance - radius);
		return Physics.SphereCast(position, radius, direction, out hit, castDistance, layer, queryTriggerInteraction);
	}

	public Vector3 stepPosition => position - transform.up * (height * 0.5f - controller.stepOffset);
	public virtual bool IsPointUnderStep(Vector3 point) => stepPosition.y > point.y;

	public virtual bool OnSlopingGround()
	{
		if (isGrounded && groundAngle > m_slopingGroundAngle)
		{
			if (Physics.Raycast(transform.position, -transform.up, out var hit, height * 2f,
				Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
				return Vector3.Angle(hit.normal, Vector3.up) > m_slopingGroundAngle;
			else
				return true;
		}

		return false;
	}

	public virtual int OverlapEntity(Collider[] result,float skinOffset = 0)
	{
		var contactOffset = skinOffset + controller.skinWidth + Physics.defaultContactOffset;
		var overlapsRadius = radius + contactOffset;
		var offset = (height + contactOffset) * 0.5f - overlapsRadius;
		var top = position + Vector3.up * offset;
		var bottom = position + Vector3.down * offset;
		return Physics.OverlapCapsuleNonAlloc(top, bottom, overlapsRadius, result);
	}

	public virtual void ApplyDamage(int amount, Vector3 origin) { }

	public virtual void ResizeCollider(float height)
	{
		var delta = height - this.height;
		controller.height = height;
		controller.center += Vector3.up * delta * 0.5f;
	}

	public virtual bool CapsuleCast(Vector3 direction, float distance, int layer = Physics.DefaultRaycastLayers,
		QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
	{
		return CapsuleCast(direction, distance, out _, layer, queryTriggerInteraction);
	}

	public virtual bool CapsuleCast(Vector3 direction, float distance,out RaycastHit hit, int layer = Physics.DefaultRaycastLayers,
		QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
	{
		var origin = position - direction * radius + center;
		var offset = transform.up * (height * 0.5f - radius);
		var top = origin + offset;
		var bottom = origin - offset;
		return Physics.CapsuleCast(top, bottom, radius, direction, out hit, distance + radius, layer,
			queryTriggerInteraction);
	}

}

public abstract class Entity<T> : Entity where T : Entity<T>
{

	public EntityStateManager<T> states { get; protected set; }
	protected virtual void HandleState() => states.Step();

	protected virtual void InitializeStateManager() => states = GetComponent<EntityStateManager<T>>();
	

	protected virtual void InitializeController()
	{
		controller = GetComponent<CharacterController>();
		if(!controller)
		{
			controller = gameObject.AddComponent<CharacterController>();
		}
		controller.skinWidth = 0.005f;
		controller.minMoveDistance = 0;
		originalHeight = controller.height;
	}

	protected virtual void InitializeCollider()
	{
		m_collider = gameObject.AddComponent<CapsuleCollider>();
		m_collider.height = controller.height;
		m_collider.radius = controller.radius;
		m_collider.center = controller.center;
		m_collider.isTrigger = true;
		m_collider.enabled = false;
	}

	protected virtual void InitializeRigidbody()
	{
		m_rigidbody = gameObject.AddComponent<Rigidbody>();
		m_rigidbody.isKinematic = true;
	}

	protected virtual void Awake()
	{
		InitializeController();
		InitializeStateManager();
	}

	protected virtual void Update()
	{
		if(controller.enabled || m_collider != null)
		{
			HandleState();
			HandleController();
			HandleSpinline();
			HandleGround();
			HandleContacts();
			OnUpdate();
		}
	}

	protected virtual void LateUpdate()
	{
		if(controller.enabled)
		{
			HandlePosition();
		}
	}

	protected virtual void OnUpdate()
	{
		
	}

	protected virtual void HandlePosition()
	{
		positionDleta = (position - lastPosition).magnitude;
		lastPosition = position;
	}

	protected virtual void HandleSpinline()
	{
		var distance = (height * 0.5f) + height * 0.5f;
		if(SphereCast(-transform.up,distance,out var hit) && hit.collider.CompareTag(GameTags.InteractiveRail))
		{
			if (!onRails && verticalVelocity.y <= 0)
			{
				EnterRail(hit.collider.GetComponent<SplineContainer>());
			}
		}
		else
		{
			ExitRail();
		}
	}

	protected virtual void HandleGround()
	{
		var distance = (height * 0.5f) + m_groundOffset;
		if(SphereCast(Vector3.down,distance,out var hit) && verticalVelocity.y <= 0)
		{
			if(!isGrounded)
			{
				if(EvaluateLanding(hit))
				{
					EnterGround(hit);
				}
				else
				{
					HnadleHightLedge(hit);
				}
			}
			else if (IsPointUnderStep(hit.point))
			{
				UpdateGround(hit);
				if (Vector3.Angle(hit.normal, Vector3.up) >= controller.slopeLimit)
				{

				}
			}
			else
			{
				HnadleHightLedge(hit);
			}
		}
		else //说明此时不在地面
		{
			ExitGround();
		}
	}

	protected virtual void HandleContacts()
	{
		//检测和几个碰撞体发生了碰撞,将碰撞体放入m_contactBuffer数组
		var overlaps = OverlapEntity(m_contactBuffer);
        for (int i = 0; i < overlaps; i++)
        {
			//如果碰撞体没有开启Trigger（关闭Overlap重叠检测，开启碰撞检测），和并且碰撞体的位置不同
			if (!m_contactBuffer[i].isTrigger && m_contactBuffer[i].transform != transform)
            {
				//通知状态管理者与这个碰撞体发生碰撞
				OnContact(m_contactBuffer[i]);
				//通知这个碰撞体发生碰撞了
				var listeners = m_contactBuffer[i].GetComponents<IEntityContact>();
				foreach (var contact in listeners)
				{
					contact.OnEntityContact((T)this);
				}

				//如果碰撞体在上方，则速度改变
				if (m_contactBuffer[i].bounds.min.y > controller.bounds.max.y)
				{
					verticalVelocity = Vector3.Min(verticalVelocity, Vector3.zero);
				}

            }
        }
    }

	protected virtual void HandleController()
	{
		if(controller.enabled)
		{
			controller.Move(velocity * Time.deltaTime);
			return;
		}
		transform.position += velocity * Time.deltaTime;
	}

	protected virtual void HnadleHightLedge(RaycastHit hit)
	{

	}

	protected virtual void EnterGround(RaycastHit hit)
	{
		if(!isGrounded)
		{
			groundHit = hit;
			isGrounded = true;
			entityEvents.OnGroundEnter?.Invoke();
		}
	}

	protected virtual void ExitGround()
	{
		if(isGrounded)
		{
			isGrounded = false;
			transform.parent = null;
			lastGroundTime = Time.time;
			verticalVelocity = Vector3.Max(verticalVelocity, Vector3.zero);
			entityEvents.OnGroundExit?.Invoke();
		}
	}

	protected virtual void UpdateGround(RaycastHit hit)
	{
		if (isGrounded)
		{
			groundHit = hit;
			groundNormal = groundHit.normal;
			groundAngle = Vector3.Angle(Vector3.up, groundHit.normal);
			localSlopeDirection = new Vector3(groundNormal.x, 0, groundNormal.z).normalized;
			transform.parent = hit.collider.CompareTag(GameTags.Platform) ? hit.transform : null;
		}
	}

	public virtual void EnterRail(SplineContainer rails)
	{
		if(!onRails)
		{
			onRails = true;
			this.rails = rails;
			entityEvents.OnRailsEnter?.Invoke();
		}
	}

	public virtual void ExitRail()
	{
		if(onRails)
		{
			onRails = false;
			entityEvents.OnRailsExit?.Invoke();
		}
	}

	protected virtual bool EvaluateLanding(RaycastHit hit)
	{
		return IsPointUnderStep(hit.point) && Vector3.Angle(hit.normal, Vector3.up) < controller.slopeLimit;
	}

	protected virtual void OnContact(Collider other)
	{
		if (other)
		{
			//通知状态管理者发生了碰撞
			states.OnContact(other);
		}
	}

	public virtual void Accelerate(Vector3 direction, float turningDrag, float acceleration, float topSpeed)
	{
		if (direction.sqrMagnitude > 0)
		{
			var speed = Vector3.Dot(direction, lateralVelocity);
			var velocity = direction * speed;
			var turningVelocity = lateralVelocity - velocity;
			var turningDelta = turningDrag * turningDragMultiplier * Time.deltaTime;
			var targetTopSpeed = topSpeed * topSpeedMultiplier;

			if (lateralVelocity.magnitude < targetTopSpeed || speed < 0)
			{
				speed += acceleration * accelerationMultiplier * Time.deltaTime;
				speed = Mathf.Clamp(speed, -targetTopSpeed, targetTopSpeed);
			}

			velocity = direction * speed;
			turningVelocity = Vector3.MoveTowards(turningVelocity, Vector3.zero, turningDelta);
			lateralVelocity = velocity + turningVelocity;
		}
	}

	public virtual void FaceDirection(Vector3 direction)
	{
		if (direction.sqrMagnitude > 0)
		{
			var rotation = Quaternion.LookRotation(direction,Vector3.up);
			transform.rotation = rotation;
		}
	}

	public virtual void FaceDirection(Vector3 direction,float degreesPerSpeed)
	{
		if(direction != Vector3.zero)
		{
			var rotation = transform.rotation;
			var rotationDelta = degreesPerSpeed * Time.deltaTime;
			var target = Quaternion.LookRotation(direction,Vector3.up);
			transform.rotation = Quaternion.RotateTowards(rotation, target, rotationDelta);
		}
	}

	//减速
	public virtual void Decelerate(float deceleration)
	{
		var delta = deceleration * decelerationMultiplier * Time.deltaTime;
		lateralVelocity = Vector3.MoveTowards(lateralVelocity, Vector3.zero, delta);
	}

	//重力
	public virtual void Gravity(float gravity)
	{
		if(!isGrounded)
		{
			verticalVelocity += Vector3.down * gravity * gravityMultiplier * Time.deltaTime;
		}
	}

	public virtual void SlopeFactor(float upwardForce, float downwardForce)
	{
		if (!isGrounded || !OnSlopingGround()) return;

		var factor = Vector3.Dot(Vector3.up, groundNormal);
		var downwards = Vector3.Dot(localSlopeDirection, lateralVelocity) > 0;
		var multiplier = downwards ? downwardForce : upwardForce;
		var delta = factor * multiplier * Time.deltaTime;
		lateralVelocity += localSlopeDirection * delta;
	}

	public virtual void SnapToGround(float force)
	{
		if(isGrounded && (verticalVelocity.y <= 0))
		{
			verticalVelocity = Vector3.down * force;
		}
	}

	public virtual bool FitsIntoPosition(Vector3 position)
	{
		var bounds = controller.bounds;
		var radius = controller.radius - controller.skinWidth;
		var offset = height * 0.5f - radius;
		var top = position + Vector3.up * offset;
		var bottom = position - Vector3.up * offset;

		return !Physics.CheckCapsule(top, bottom, radius,
			Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
	}

	public virtual void UseCustomCollision(bool value)
	{
		controller.enabled = !value;
		if(value)
		{
			InitializeCollider();
			InitializeRigidbody();
		}
		else
		{
			Destroy(m_collider);
			Destroy(m_rigidbody);
		}
	}

}