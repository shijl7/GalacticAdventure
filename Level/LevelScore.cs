using UnityEngine;
using UnityEngine.Events;

public class LevelScore : Singleton<LevelScore>
{
	public UnityEvent<int> onCoinsSet;
	public UnityEvent<bool[]> onStarsSet;
	public UnityEvent onScoreLoaded;

	public bool stopTime { get; set; } = true;
	protected int m_coins;
	protected bool[] m_stars = new bool[GameLevel.StarsPerLevel];

	protected Game m_game;
	protected GameLevel m_level;
	public bool[] stars => (bool[])m_stars.Clone();

	public float time { get; protected set; }


	protected virtual void Start()
	{
		m_game = Game.instance;
		m_level = m_game?.GetCurrentLevel();

		if (m_level != null)
		{
			m_stars = (bool[])m_level.stars.Clone();
		}
		onScoreLoaded?.Invoke();
	}

	protected void Update()
	{
		if (!stopTime)
		{
			time += Time.deltaTime;
		}
	}

	public int coins
	{
		get { return m_coins; }
		set
		{
			m_coins = value;
			onCoinsSet?.Invoke(coins);
		}
	}

	public virtual void Consolidate()
	{
		if(m_level != null)
		{
			if(m_level.time == 0 || time < m_level.time)
			{
				m_level.time = time;
			}
			if(coins > m_level.coins)
			{
				m_level.coins = coins;
			}
			m_level.stars = (bool[])stars.Clone();
			m_game.RequestSaveing();
		}
	}


}