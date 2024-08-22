using UnityEngine;
using UnityEngine.Events;

public class LevelPauser : Singleton<LevelPauser>
{
	public bool canPause {  get; set; }
	public bool paused {  get; protected set; }
	public UIAnimator pauseScreen;
	public UnityEvent OnPause;
	public UnityEvent OnUnpause;


	public virtual void Pause(bool value)
	{
		if(paused != value)
		{
			if (!paused)
			{
				Game.LockCursor(false);//显示光标
				paused = true;
				Time.timeScale = 0f;
				pauseScreen.SetActive(true);
				pauseScreen?.Show();
				OnPause?.Invoke();
			}
			else
			{
				Game.LockCursor();//不显示光标
				paused = false;
				Time.timeScale = 1f;
				pauseScreen.SetActive(false);
				pauseScreen?.Hide();
				OnUnpause?.Invoke();
			}
		}
	}

}
