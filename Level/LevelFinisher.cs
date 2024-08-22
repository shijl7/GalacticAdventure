using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class LevelFinisher : Singleton<LevelFinisher>
{
	public UnityEvent onExit;
	public UnityEvent onFinish;

	public float loadingDelay = 1f;
	public bool unlockNextLevel;
	public string nextScene;
	public string exitScene;
	protected LevelPauser m_pauser => LevelPauser.instance;
	protected Level m_level => Level.instance;
	protected GameLoader m_loader => GameLoader.instance;
	protected LevelScore m_score => LevelScore.instance;
	protected Game m_game => Game.instance;

	public virtual void Exit()
	{
		StopAllCoroutines();
		StartCoroutine(ExitRoutine());
	}

	protected virtual IEnumerator ExitRoutine()
	{
		m_pauser.Pause(false);
		m_pauser.canPause = false;
		m_level.player.inputs.enabled = false;
		yield return new WaitForSeconds(loadingDelay);
		Game.LockCursor(false);
		m_loader.Load(exitScene);
		onExit?.Invoke();
	}

	public virtual void Finish()
	{
		StopAllCoroutines();
		StartCoroutine(FinishRoutine());
	}

	protected virtual IEnumerator FinishRoutine()
	{
		m_pauser.Pause(false);
		m_pauser.canPause = false;
		m_score.stopTime = true;
		m_level.player.inputs.enabled = false;
		yield return new WaitForSeconds(loadingDelay);

		if(unlockNextLevel)
		{
			m_game.UnlockNextLevel();//解锁下一关
		}

		Game.LockCursor(false);
		m_score.Consolidate();
		m_loader.Load(nextScene);
		onFinish?.Invoke();
	}

}