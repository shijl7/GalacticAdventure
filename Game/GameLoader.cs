using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameLoader : Singleton<GameLoader>
{
	public UnityEvent onLoadStart;
	public UnityEvent onLoadFinish;
	public bool isLoading { get; protected set; }
	public UIAnimator loadingScreen;
	public float loadingProgress { get; protected set; }
	public string currentScene => SceneManager.GetActiveScene().name;

	[Header("Minimum Time")]
	public float startDelay = 1f;
	public float finishDelay = 1f;

	public virtual void Load(string scene)
	{
		StartCoroutine(LoadRoutine(scene));
	}

	protected virtual IEnumerator LoadRoutine(string scene)
	{
		onLoadStart?.Invoke();
		isLoading = true;
		loadingScreen.SetActive(true);
		loadingScreen.Show();

		yield return new WaitForSeconds(startDelay);
		//异步加载地图
		var operation = SceneManager.LoadSceneAsync(scene);
		
		while(!operation.isDone)
		{
			loadingProgress = operation.progress;
			yield return null;
		}
		loadingProgress = 1;
		yield return new WaitForSeconds(finishDelay);

		isLoading = false;
		loadingScreen.Hide();

		onLoadFinish?.Invoke();
	}

	public virtual void Reload()
	{
		StartCoroutine(LoadRoutine(currentScene));
	}

}
