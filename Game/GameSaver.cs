using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Windows;
using File = System.IO.File;

public class GameSaver : Singleton<GameSaver>
{
	//用于保存数据的可用插槽的数量
	protected static readonly int TotalSlots = 5;
	public enum Mode
	{
		Binary,JSON,PlayerPrefas
	}
	public Mode mode = Mode.Binary;//默认是二进制
	public string binaryFileExtension = "data";
	public string fileName = "save";
	public virtual GameData[] LoadList()
	{
		var list = new GameData[TotalSlots];
        for (int i = 0; i < list.Length; i++)
        {
			var data = Load(i);
			if(data != null)
			{
				list[i] = data;
			}
        }
		return list;
    }

	public virtual GameData Load(int index)
	{
		switch (mode)
		{
			case Mode.Binary:
				return LoadBinary(index);
			case Mode.JSON:
				return LoadJSON(index);
			case Mode.PlayerPrefas:
				return LoadPlayerPrefas(index);
			default: return null;
		}
	}

	public virtual void Save(GameData data, int index)
	{
		switch (mode)
		{
			case Mode.Binary:
				SaveBinary(data, index);
				break;
			case Mode.JSON:
				SaveJSON(data, index);
				break;
			case Mode.PlayerPrefas:
				SavePlayerPrefas(data, index);
				break;
			default:
				break;
		}
	}

	//保存
	protected virtual void SaveBinary(GameData data,int index)
	{
		var path = GetFilePath(index);
		var formatter = new BinaryFormatter();
		var stream = new FileStream(path, FileMode.Create);
		formatter.Serialize(stream, data);
		stream.Close();
	}

	//加载
	protected virtual GameData LoadBinary(int index)
	{
		var path = GetFilePath(index);
		//如果路径存在
		if (File.Exists(path))
		{
			var formatter = new BinaryFormatter();
			var stream = new FileStream(path, FileMode.Open);//打开指定路径的文件来获取数据流
			var data = formatter.Deserialize(stream);//反序列化数据
			stream.Close();//关闭数据流
			return data as GameData;//将 data 强制转换为 GameData 类型
		}
		return null;
	}

	protected virtual void SaveJSON(GameData data, int index)
	{
		var json = data.ToJson();
		var path = GetFilePath(index);
		File.WriteAllText(path, json);
	}

	protected virtual GameData LoadJSON(int index)
	{
		var path = GetFilePath(index);
		//如果路径存在
		if (File.Exists(path))
		{
			var json = File.ReadAllText(path);
			
			return GameData.FormJson(json);
		}
		return null;
	}

	protected virtual void SavePlayerPrefas(GameData data, int index)
	{
		var json = data.ToJson();
		var key = index.ToString();
		PlayerPrefs.SetString(key, json);
	}

	protected virtual GameData LoadPlayerPrefas(int index)
	{
		var key = index.ToString();
		if (PlayerPrefs.HasKey(key))
		{
			var json = PlayerPrefs.GetString(key);
			return GameData.FormJson(json);
		}
		return null;
	}


	protected virtual string GetFilePath(int index)
	{
		var extension = mode == Mode.JSON ? "JSON" : binaryFileExtension;
		return Application.persistentDataPath + $"/{fileName}_{index}.{extension}";
	}
}