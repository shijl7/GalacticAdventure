﻿using UnityEngine;

public class Enemy : Entity<Enemy>
{
	public EnemyEvents enemyEvents;

	public Player player { get; protected set; }
	public Health health { get; protected set; }
	public EnemyStatsManager stats {  get; protected set; }
	public WaypointManager waypoints { get; protected set; }
	protected Collider[] m_sightOverlaps = new Collider[1024];
	protected Collider[] m_contactAttackOverlaps = new Collider[1024];

	protected override void Awake()
	{
		base.Awake();
		InitialHealth();
		InitialTag();
		InitialStatsManager();
		InitialWaypointManager();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		HandleSight();
		ContactAttack();

	}

	protected void InitialHealth() => health = GetComponent<Health>();
	protected void InitialTag() => tag = GameTags.Enemy;
	protected void InitialStatsManager() => stats = GetComponent<EnemyStatsManager>();
	protected void InitialWaypointManager() => waypoints = GetComponent<WaypointManager>();

	public virtual void Accelerate(Vector3 direction, float acceleration, float topSpeed) =>
		Accelerate(direction,stats.current.turningDrag,acceleration,topSpeed);
	public virtual void Friction() => Decelerate(stats.current.friction);
	public virtual void Gravity() => Gravity(stats.current.gravity);
	public virtual void SnapToGround() => SnapToGround(stats.current.snapForce);
	public virtual void FaceDirectionSmooth(Vector3 direction) => FaceDirection(direction ,stats.current.rotationSpeed);

	public virtual void Decelerate() => Decelerate(stats.current.deceleration);

	protected virtual void HandleSight()
	{
		if (!player)
		{
			var overlaps = Physics.OverlapSphereNonAlloc(position, stats.current.spotRange, m_sightOverlaps);
			for(int i = 0; i < overlaps; i++)
			{
				if (m_sightOverlaps[i].CompareTag(GameTags.Player))
				{
					if (m_sightOverlaps[i].TryGetComponent<Player>(out var player))
					{
						this.player = player;
						enemyEvents.OnPlayerSpotted?.Invoke();
						return;
					}
				}
			}
		}
		else
		{
			var distance=Vector3.Distance(position, player.position);
			if((player.health.current==0) || (distance > stats.current.viewRange))
			{
				player = null;
				enemyEvents.OnPlayerScaped?.Invoke();
			}
		}
	}

	protected virtual void ContactAttack()
	{
		if(stats.current.canAttackOnContact)
		{
			var overlaps = OverlapEntity(m_contactAttackOverlaps, stats.current.contactOffset);
			for(int i = 0;i < overlaps; i++)
			{
				if (m_contactAttackOverlaps[i].CompareTag(GameTags.Player) 
					&& m_contactAttackOverlaps[i].TryGetComponent<Player>(out var player))
				{
					var stepping = controller.bounds.max + Vector3.down * stats.current.contactSteppingTolerance;
					if (!player.IsPointUnderStep(stepping))
					{
						if (stats.current.contactPushback)
						{
							lateralVelocity = -transform.forward * stats.current.contactPushBackForce;
						}
						player.ApplyDamage(stats.current.contactDamage, transform.position);
						enemyEvents.OnPlayerContact?.Invoke();
					}

				}
			}
		}
	}

	public override void ApplyDamage(int amount, Vector3 orign)
	{
		if(!health.isEmpty && !health.recovering)
		{
			health.Damage(amount);
			enemyEvents.OnDamage?.Invoke();
			if(health.isEmpty)
			{
				controller.enabled = false;
				enemyEvents.OnDie?.Invoke();
			}
		}
	}

}
