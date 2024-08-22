﻿using UnityEngine;

[RequireComponent (typeof(Enemy))]
public class EnemyAnimator : MonoBehaviour
{
	public Animator animator;
	protected Enemy m_enemy;

	[Header("Parameters Names")]
	public string stateName = "State";
	public string lastStateName = "Last State";
	public string lateralSpeedName = "Lateral Speed";
	public string verticalSpeedName = "Vertical Speed";
	public string healthName = "Health";
	public string isGroundedName = "Is Grounded";
	public string onStateChangedName = "On State Changed";

	protected int m_stateHash;
	protected int m_lastStateHash;
	protected int m_lateralSpeedHash;
	protected int m_verticalSpeedHash;
	protected int m_healthHash;
	protected int m_isGroundedHash;
	protected int m_onStateChangedHash;

	protected void Start()
	{
		InitialEnemy();
		InitialParameterHash();
		InitialAnimatorTriggers();
	}

	protected virtual void LateUpdate()
	{
		var lateralSpeed = m_enemy.lateralVelocity.magnitude;
		var verticalSpeed = m_enemy.verticalVelocity.y;
		var health = m_enemy.health;

		animator.SetInteger(m_stateHash, m_enemy.states.index);
		animator.SetInteger(m_lastStateHash, m_enemy.states.lastIndex);
		animator.SetFloat(m_lateralSpeedHash, lateralSpeed);
		animator.SetFloat(m_verticalSpeedHash, verticalSpeed);
		animator.SetInteger(m_healthHash, m_enemy.health.current);
		animator.SetBool(m_isGroundedHash, m_enemy.isGrounded);
	}

	protected virtual void InitialEnemy() => m_enemy = GetComponent<Enemy>();

	protected virtual void InitialParameterHash()
	{
		m_stateHash = Animator.StringToHash(stateName);
		m_lastStateHash = Animator.StringToHash(lastStateName);
		m_lateralSpeedHash = Animator.StringToHash(lateralSpeedName);
		m_verticalSpeedHash = Animator.StringToHash(verticalSpeedName);
		m_healthHash = Animator.StringToHash(healthName);
		m_isGroundedHash = Animator.StringToHash(isGroundedName);
		m_onStateChangedHash = Animator.StringToHash(onStateChangedName);
	}
	protected virtual void InitialAnimatorTriggers() =>
		m_enemy.states.events.onChange.AddListener(() => animator.SetTrigger(m_onStateChangedHash));

}