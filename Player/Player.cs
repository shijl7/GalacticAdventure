using PLAYERTWO.PlatformerProject;
using UnityEngine;

public class Player : Entity<Player>
{
	public PlayerEvents playerEvents;
	public PlayerInputManager inputs { get; protected set; }
	public PlayerStatsManager stats { get; protected set; }//玩家的数值统计信息管理者，不是动画状态机，是某些数值统计
	public Pole pole { get; protected set; }
	public int jumpCounter {  get; protected set; }
	public int airDashCounter { get; protected set; }
	public float lastDashTime { get; protected set; }
	public Vector3 lastWallNormal { get; protected set; }
	public bool holding { get; protected set; }
	public Health health { get; protected set; }
	public Pickable pickable { get; protected set; }
	public bool onWater { get; protected set; }
	public Collider waterCollider { get; protected set; }
	public int airSpinCounter { get; protected set; }
	public Transform skin;

	protected Vector3 m_respawnPosition;
	protected Quaternion m_respawnRotation;

	protected Vector3 m_skinInitialPosition;
	protected Quaternion m_skinInitialRotation;

	public Transform pickableSlot;
	public virtual bool isAlive => !health.isEmpty;

	public virtual void FaceDirectionSmooth(Vector3 direction) => FaceDirection(direction, stats.current.rotationSpeed);
	public virtual void Decelerate() => Decelerate(stats.current.deceleration);

	public virtual bool canStandUp => !SphereCast(Vector3.up, originalHeight);

	protected const float k_waterExitOffset = 0.25f;
	protected virtual void InitializeInputs() => inputs = GetComponent<PlayerInputManager>();
	protected virtual void InitializeStats() => stats = GetComponent<PlayerStatsManager>();
	protected virtual void InitializeTags() => tag = GameTags.Player;
	protected virtual void InitializeHealth() => health = GetComponent<Health>();
	protected virtual void InitializeRespawn()
	{
		m_respawnPosition = transform.position;
		m_respawnRotation = transform.rotation;
	}

	protected override void Awake()
	{
		base.Awake();
		InitializeInputs();
		InitializeStats();
		InitializeTags();
		InitializeHealth();
		InitializeRespawn();

		entityEvents.OnGroundEnter.AddListener(() =>
		{
			ResetJumps();
			ResetAirSpins();
			ResetAirDash();
		});
		entityEvents.OnRailsEnter.AddListener(() =>
		{
			ResetJumps();
			ResetAirSpins();
			ResetAirDash();
			StartGrind();
		});
	}

	protected void OnTriggerStay(Collider other)
	{
		if(other.CompareTag(GameTags.VolumeWater))
		{
			if(!onWater && other.bounds.Contains(unsizePosition))
			{
				EnterWater(other);
			}
			else if (onWater)
			{
				var exitPoint = position + Vector3.down * k_waterExitOffset;
				if(!other.bounds.Contains(exitPoint))
				{
					ExitWater();
				}
			}
		}
	}

	public virtual void Accelerate(Vector3 direction)
	{
		var turningDrag = isGrounded && inputs.GetRun()
			? stats.current.runningTurningDrag
			: stats.current.turningDrag;
		var acceleration = isGrounded && inputs.GetRun()
			? stats.current.runningAccelerate
			: stats.current.acceleration;
		var topSpeed = inputs.GetRun()
			? stats.current.runningTopSpeed
			: stats.current.topSpeed;
		var finalAcceleration = isGrounded ? acceleration : stats.current.airAcceleration;

		Accelerate(direction, turningDrag, finalAcceleration, topSpeed);

		if(inputs.GetRunUp())
		{
			lateralVelocity = Vector3.ClampMagnitude(lateralVelocity, topSpeed);
		}
	}

	public virtual void Friction()
	{
		Decelerate(stats.current.friction);
	}

	public virtual void Gravity()
	{
		if(!isGrounded && verticalVelocity.y > -stats.current.gravityTopSpeed)
		{
			var speed = verticalVelocity.y;
			var force = verticalVelocity.y > 0 ? stats.current.gravity : stats.current.fallGravity;
			speed -= force * gravityMultiplier * Time.deltaTime;
			speed = Mathf.Max(speed,-stats.current.gravityTopSpeed);
			verticalVelocity = new Vector3(0,speed,0);
		}
	}

	public virtual void PickAndThrow()
	{
		if(stats.current.canPickUp && inputs.GetPickAndDropDown())
		{
			if (!holding)
			{
				if(CapsuleCast(transform.forward,stats.current.pickDistance,out var hit))
				{
					if(hit.transform.TryGetComponent(out Pickable pickable))
					{
						PickUp(pickable);
					}
				}
			}
			else
			{
				Throw();
			}
		}
	}

	public virtual void PickUp(Pickable pickable)
	{
		if(!holding && (isGrounded || stats.current.canPickUpOnAir))
		{
			holding = true;
			this.pickable = pickable;
			pickable.PickUp(pickableSlot);
			pickable.onRespawn.AddListener(RemovePickable);
			playerEvents.OnPickUp?.Invoke();
		}
	}

	public virtual void RemovePickable()
	{
		if(holding)
		{
			pickable = null;
			holding = false;
		}
	}

	public virtual void AirDive()
	{
		if (stats.current.canAirDive && !isGrounded && !holding && inputs.GetAirDiveDown())
		{
			states.Change<AirDivePlayerState>();
			playerEvents.OnAirDive?.Invoke();
		}
	}

	public virtual void Dash()
	{
		var canAirDash = stats.current.canAirDash && !isGrounded && airDashCounter < stats.current.allowedAirDashes;
		var canGroundDash = stats.current.canGroundDash && isGrounded &&
			Time.time > lastDashTime + stats.current.dashDuration;
        if (inputs.GetDashDown() && (canAirDash || canGroundDash))
        {
            if(!isGrounded) airDashCounter++;
			lastDashTime = Time.time;
			states.Change<DashPlayerState>();
        }
    }

	public virtual void Glide()
	{
		if(!isGrounded && stats.current.canGlide && inputs.GetGlide() && verticalVelocity.y <= 0)
		{
			states.Change<GlidingPlayerState>();
		}
	}

	public virtual void SnapToGround() => SnapToGround(stats.current.snapForce);

	public virtual void Fall()
	{
		if(!isGrounded)
		{
			states.Change<FallPlayerState>();
		}
	}

	protected override bool EvaluateLanding(RaycastHit hit)
	{
		return base.EvaluateLanding(hit) && !hit.collider.CompareTag(GameTags.Spring);
	}

	public virtual void Jump()
	{
		var canMultiJump = (jumpCounter > 0) && (jumpCounter < stats.current.multiJumps);
		var canCoyoteJump = (jumpCounter == 0) && (Time.time < lastGroundTime + stats.current.coyoteJumpThreshold);
		var holdJump = !holding;
		if((isGrounded || canMultiJump || canCoyoteJump) && holdJump)
		{
			if (inputs.GetJumpDown())
			{
				Jump(stats.current.maxJumpHeight);
			}
		}

		if(inputs.GetJumpUp() && (jumpCounter > 0) && verticalVelocity.y > stats.current.minJumpHeight)
		{
			verticalVelocity = Vector3.up * stats.current.minJumpHeight;
		}

	}

	public virtual void Jump(float height)
	{
		jumpCounter++;
		verticalVelocity = Vector3.up * height;
		states.Change<FallPlayerState>();
		playerEvents.OnJump?.Invoke();
	}

	public virtual void DirectionalJump(Vector3 direction, float height, float distance)
	{
		jumpCounter++;
		verticalVelocity = Vector3.up * height;
		lateralVelocity = direction * distance;
		playerEvents.OnJump?.Invoke();
	}

	public virtual void SetJump(int amount) => jumpCounter = amount;

	public virtual void ResetJumps() => jumpCounter = 0;

	public virtual void ResetAirSpins() => airSpinCounter = 0;
	public virtual void ResetAirDash() => airDashCounter = 0;

	public virtual void StartGrind() => states.Change<RailGrindPlayerState>();

	public virtual void Spin()
	{
		var canAirSpin = (isGrounded || stats.current.canAirSpin) && airSpinCounter < stats.current.allowAirSpin;
		if(stats.current.canSpin && canAirSpin && inputs.GetSpinDown())
		{
			if(!isGrounded)
			{
				airSpinCounter++;
			}
			states.Change<SpinPlayerState>();
			playerEvents.OnSpin?.Invoke();
		}
	}

	public virtual void WallDrag(Collider other)
	{
		if(stats.current.canWallDrag && velocity.y <= 0 && !holding && !other.TryGetComponent<Rigidbody>(out _))
		{
			if(CapsuleCast(transform.forward,0.25f,out var hit,stats.current.wallDragLayers) && 
				!DetectingLedge(0.25f,height,out _))
			{
				if (hit.collider.CompareTag(GameTags.Platform))
					transform.parent = hit.transform;
				lastWallNormal = hit.normal;
				states.Change<WallDragPlayerState>();
			}
		}
	}

	public virtual void PushRigidbody(Collider other)
	{
		if(!IsPointUnderStep(other.bounds.max) && other.TryGetComponent(out Rigidbody rigidbody))
		{
			var force = lateralVelocity * stats.current.pushForce;
			rigidbody.velocity = force / rigidbody.mass * Time.deltaTime;
		}
	}

	public virtual void StompAttack()
	{
		if (!isGrounded && !holding && stats.current.canStompAttack && inputs.GetStompDown())
		{
			states.Change<StompPlayerState>();
		}
	}

	protected virtual bool DetectingLedge(float forwardDistance, float downwardDistance, out RaycastHit ledgeHit)
	{
		var contactOffset = Physics.defaultContactOffset + positionDleta;
		var ledgeMaxDistance = radius + forwardDistance;
		var ledgeHeightOffset = height * 0.5f + contactOffset;
		var upwardOffset = transform.up * ledgeHeightOffset;
		var forwardOffset = transform.forward * ledgeMaxDistance;
		if (Physics.Raycast(position + upwardOffset, transform.forward, ledgeMaxDistance,
			Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) ||
			Physics.Raycast(position + forwardOffset * 0.01f, transform.up, ledgeHeightOffset,
			Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
		{
			ledgeHit = new RaycastHit();
			return false;
		}

		var origin = position + upwardOffset + forwardOffset;
		var distance = downwardDistance + contactOffset;
		return Physics.Raycast(origin, Vector3.down, out ledgeHit, distance, stats.current.ledgeHangingLayers,
			QueryTriggerInteraction.Ignore);
	}

	public virtual void LedgeGrab()
	{
		if(stats.current.canLedgeHang && velocity.y < 0 && !holding && 
			states.ContainsStateOfType(typeof(LedgeHangingPlayerState)) && 
			DetectingLedge(stats.current.ledgeMaxForwardDistance,stats.current.ledgeMaxDownwardDistance,
			out var hit))
		{
			if(!(hit.collider is CapsuleCollider) && !(hit.collider is SphereCollider))
			{
				var ledgeDistance = radius + stats.current.ledgeMaxForwardDistance;
				var lateralOffset = transform.forward * ledgeDistance;
				var verticalOffset = Vector3.down * height * 0.5f - center;
				velocity = Vector3.zero;
				transform.parent = hit.collider.CompareTag(GameTags.Platform) ? hit.transform : null;
				transform.position = hit.point - lateralOffset + verticalOffset;
				states.Change<LedgeHangingPlayerState>();
				playerEvents.OnLedgeGrabbed?.Invoke();
			}
		}
	}

	public virtual void WaterAcceleration(Vector3 inputDirection)
	{
		Accelerate(inputDirection, stats.current.waterTurningDrag, stats.current.swimAcceleration, stats.current.swimTopSpeed);
	}

	public virtual void WaterFaceDirection(Vector3 direction)
	{
		FaceDirection(direction, stats.current.waterRotationSpeed);
	}

	public virtual void EnterWater(Collider waterCollider)
	{
		if(!onWater && !health.isEmpty)
		{
			Throw();
			onWater = true;
			this.waterCollider = waterCollider;
			states.Change<SwimPlayerState>();
		}
	}

	public virtual void ExitWater()
	{
		if(onWater)
		{
			onWater = false;

		}
	}

	public virtual void GrabPole(Collider other)
	{
		if(stats.current.canPoleClimb && !holding && velocity.y <= 0
			&& other.TryGetComponent(out Pole pole))
		{
			this.pole = pole;
			states.Change<PoleClimbingPlayerState>();
		}
	}

	public virtual void Backflip(float force)
	{
		if(stats.current.canBackflip && !holding)
		{
			verticalVelocity = Vector3.up * stats.current.backflipJumpHeight;
			lateralVelocity = -transform.forward * force;
			states.Change<BackflipPlayerState>();
			playerEvents.OnBackflip.Invoke();
		}
	}

	public virtual void BackflipAcceleration()
	{
		var direction = inputs.GetMovementCamerDirection();
		Accelerate(direction, stats.current.backflipTurningDrag, stats.current.backflipAirAcceleration, stats.current.backflipTopSpeed);
	}

	public virtual void CrawlingAccelerate(Vector3 direction)
	{
		Accelerate(direction, stats.current.crawlingTurningSpeed, stats.current.crawlingAcceleration, stats.current.crawlingTopSpeed);
	}

	public virtual void RegularSlopeFactor()
	{
		if (stats.current.applySlopeFactor)
			SlopeFactor(stats.current.slopeUpwardForce, stats.current.slopeDownwardForce);
	}

	public virtual void Throw()
	{
		if(holding)
		{
			var force = lateralVelocity.magnitude * stats.current.throwVelocityMultiplier;
			pickable.Release(transform.forward, force);
			pickable = null;
			holding = false;
			playerEvents.OnThrow?.Invoke();
		}
	}

	public virtual void Respawn()
	{
		health.Reset();
		transform.SetPositionAndRotation(m_respawnPosition, m_respawnRotation);
		states.Change<IdlePlayerState>();
	}

	public virtual void SetRespawn(Vector3 respawnPosition, Quaternion respawnRotation)
	{
		m_respawnPosition = respawnPosition;
		m_respawnRotation = respawnRotation;
	}

	public virtual void AccelerateToInputDirection()
	{
		var inputDirection = inputs.GetMovementCamerDirection();
		Accelerate(inputDirection);
	}

	public virtual void SetSkinParent(Transform parent)
	{
		if(skin)
		{
			skin.parent = parent;
		}
	}

	public virtual void ResetSkinParent()
	{
		if (skin)
		{
			skin.parent = transform;
			skin.localPosition = Vector3.zero;
			skin.localRotation = Quaternion.identity;
		}
	}

	public override void ApplyDamage(int amount,Vector3 origin)
	{
		if(!health.isEmpty && !health.recovering)
		{
			health.Damage(amount);
			var damageDir = origin - transform.position;
			damageDir.y = 0;
			damageDir = damageDir.normalized;
			FaceDirection(damageDir);//朝向受到伤害的方向
			lateralVelocity = -transform.forward * stats.current.hurtBackwardsForce;

			if (!onWater)
			{
				verticalVelocity = Vector3.up * stats.current.hurtUpwardsForce;
				states.Change<HurtPlayerState>();
			}
			playerEvents.OnHurt?.Invoke();
			if(health.isEmpty)
			{
				Throw();
				playerEvents.OnDie?.Invoke();
			}
		}
	}


}
