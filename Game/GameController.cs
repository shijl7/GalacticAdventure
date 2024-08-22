using UnityEngine;

public class GameController : MonoBehaviour
{
	protected Game m_game => Game.instance;
	protected GameLoader m_Loader => GameLoader.instance;
	public virtual void AddRetries(int amount) => m_game.retries += amount;
	public virtual void LoadScene(string scene) => m_Loader.Load(scene);
}