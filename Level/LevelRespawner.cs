using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LevelRespawner : Singleton<LevelRespawner>
{
	public UnityEvent onGameOver;
	public UnityEvent onRespawn;

	public List<PlayerCamera> m_cameras;
	protected Level m_level => Level.instance;
	protected LevelPauser m_pauser => LevelPauser.instance;
	protected Game m_game => Game.instance;
	protected Fader m_fader => Fader.instance;
	protected LevelScore m_score => LevelScore.instance;
	public float respawnFadeOutDelay = 1f;
	public float respawnFadeInDelay = 0.5f;
	public float gameOverFadeOutDelay = 5f;
	public float restartFadeOutDelay = 0.5f;

	protected void Start()
	{
		m_cameras = new List<PlayerCamera>(FindObjectsOfType<PlayerCamera>());
		m_level.player.playerEvents.OnDie.AddListener(() => Respawn(true));
	}

	public virtual void Respawn(bool consumeRetries)
	{
		StopAllCoroutines();
		StartCoroutine(Routine(consumeRetries));
	}

	public virtual void Restart()
	{
		StopAllCoroutines();
		StartCoroutine(RestartRoutine());
	}

	protected virtual IEnumerator Routine(bool consumeRetries)
	{
		m_pauser.Pause(false);
		m_pauser.canPause = false;
		m_level.player.inputs.enabled = false;

		if(consumeRetries && m_game.retries == 0)
		{
			StartCoroutine(GameOverRoutine());
			yield break;
		}
		yield return new WaitForSeconds(respawnFadeOutDelay);

		m_fader.FadeOut(() => StartCoroutine(RespawnRoutine(consumeRetries)));
	}

	protected virtual IEnumerator RestartRoutine()
	{
		m_pauser.Pause(false);
		m_pauser.canPause = false;
		m_level.player.inputs.enabled = false;
		yield return new WaitForSeconds(restartFadeOutDelay);
		GameLoader.instance.Reload();
	}

	protected virtual IEnumerator GameOverRoutine()
	{
		m_score.stopTime = true;
		yield return new WaitForSeconds(gameOverFadeOutDelay);
		GameLoader.instance.Reload();
		onGameOver?.Invoke();
	}

	protected virtual IEnumerator RespawnRoutine(bool consumeRetries)
	{
		if(consumeRetries)
		{
			m_game.retries--;
		}
		m_level.player.Respawn();
		m_score.coins = 0;
		ResetCameras();
		onRespawn?.Invoke();

		yield return new WaitForSeconds(respawnFadeInDelay);

		m_fader.FadeIn(() =>
		{
			m_pauser.canPause = true;
			m_level.player.inputs.enabled = true;
		});
	}

	public virtual void ResetCameras()
	{
        foreach (var camera in m_cameras)
        {
            camera.Reset();
        }
    }

}