using UnityEngine;

public class LevelController : MonoBehaviour
{
	protected LevelScore m_score => LevelScore.instance;
	protected LevelFinisher m_finisher => LevelFinisher.instance;
	protected LevelPauser m_pauser => LevelPauser.instance;
	protected LevelRespawner m_respawner => LevelRespawner.instance;
	public void AddCoins(int amount) => m_score.coins += amount;

	public void Exit() => m_finisher.Exit();

	public void Finish() => m_finisher.Finish();

	public virtual void Pause(bool value) => m_pauser.Pause(value);

	public virtual void Restart() => m_respawner.Restart();

	public virtual void Respawn(bool consumeRetries) => m_respawner.Respawn(consumeRetries);

}
