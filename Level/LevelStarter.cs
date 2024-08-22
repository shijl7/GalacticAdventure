using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class LevelStarter : Singleton<LevelStarter>
{
	public UnityEvent OnStart;

	public Level m_level => Level.instance;
	public LevelScore m_score => LevelScore.instance;
	public LevelPauser m_pauser => LevelPauser.instance;
	public float enablePlayerDelay = 1f;
	protected void Start()
	{
		StartCoroutine(Routinue());
	}

	protected virtual IEnumerator Routinue()
	{
		//先锁定光标，等一切就绪在显示光标
		Game.LockCursor();
		m_level.player.controller.enabled = false;
		m_level.player.inputs.enabled = false;
		yield return new WaitForSeconds(enablePlayerDelay);
		m_score.stopTime = false;
		m_level.player.controller.enabled = true;
		m_level.player.inputs.enabled = true;
		m_pauser.canPause = true;//可以暂停
		OnStart?.Invoke();
	}

}