using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public  class Game : Singleton<Game>
{
	public UnityEvent<int> onRetriesSet;
	public UnityEvent onSavingRequested;

	public int initialRetries = 3;
	public List<GameLevel> levels;

	protected int m_retries;
	public int retries
	{
		get { return m_retries; }

		set
		{
			m_retries = value;
			onRetriesSet?.Invoke(m_retries);
		}
	}
	protected int m_dataIndex;
	protected DateTime m_createdAt;
	protected DateTime m_updatedAt;

	protected override void Awake()
	{
		base.Awake();
		retries = initialRetries;
		DontDestroyOnLoad(gameObject); 
	}

	public virtual void LoadState(int index, GameData data)
	{
		m_dataIndex = index;
		m_retries = data.retries;
		m_createdAt = DateTime.Parse(data.createdAt);
		m_updatedAt = DateTime.Parse(data.updatedAt);
		for(int i = 0; i < data.levels.Length; i++)
		{
			levels[i].LoadState(data.levels[i]);
		}
	}

	public static void LockCursor(bool value = true)
	{
#if UNITY_STANDALONE || UNITY_WEBGL
		Cursor.visible = value;
		Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
#endif

	}

	public virtual GameLevel GetCurrentLevel()
	{
		var scene = GameLoader.instance.currentScene;
		return levels.Find((level) => level.scene == scene);
	}

	public virtual int GetCurrentLevelIndex()
	{
		var scene = GameLoader.instance.currentScene;
		return levels.FindIndex((level) => level.scene == scene);
	}

	public virtual void UnlockNextLevel()
	{
		var index = GetCurrentLevelIndex() + 1;
		if(index >= 0 && index < levels.Count)
		{
			levels[index].locked = false;
		}
	}

	public virtual LevelData[] LevelData()
	{
		return levels.Select(level => level.ToData()).ToArray();
	}

	public virtual GameData ToData()
	{
		return new GameData
		{
			retries = m_retries,
			levels = LevelData(),
			createdAt = m_createdAt.ToString(),
			updatedAt = DateTime.UtcNow.ToString(),
		};
	}

	public virtual void RequestSaveing()
	{
		GameSaver.instance.Save(ToData(),m_dataIndex);
		onSavingRequested?.Invoke();
	}

}