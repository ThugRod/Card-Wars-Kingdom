using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using MiniJSON;
using UnityEngine;

public abstract class DataManager<T> : IDataManager where T : ILoadableData
{
	private const string parttwo = "210429q";

	private const string partfour = "bmmfzDb";

	private bool isLoaded;

	private string partone = "_ijmMW";

	private string partthree = "m201510";

	protected Dictionary<string, T> Database = new Dictionary<string, T>();

	protected List<T> DatabaseArray = new List<T>();

	protected List<IDataManager> Dependencies = new List<IDataManager>();

	protected static object threadLock;

	private bool doneLoadingAndParsingJsonData;

	private bool donePostLoad;

	private Exception ex;

	public string FilePath { get; set; }

	public bool IsLoaded
	{
		get
		{
			return isLoaded;
		}
		set
		{
			isLoaded = value;
		}
	}

	private bool ExceptionThrown
	{
		get
		{
			return null != ex;
		}
	}

	private void PrintException(Exception ex)
	{
		if (ex == null)
		{
			return;
		}
		Singleton<SimplePopupController>.Instance.ShowMessage(string.Empty, "An error occured while loading data. View log for details.", Application.Quit);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(ex.ToString());
		for (Exception innerException = ex.InnerException; innerException != null; innerException = innerException.InnerException)
		{
			stringBuilder.AppendLine(string.Format("\tInnerException: {0}", innerException.ToString()));
		}
		foreach (object key in ex.Data.Keys)
		{
			stringBuilder.AppendLine("****** Extra Data ******");
			stringBuilder.AppendLine(string.Format("[{0}] = {1}", key, ex.Data[key]));
		}
		Debug.LogError(stringBuilder.ToString());
	}

	private void CheckAndThrowExeptions()
	{
    	if (ex != null)
    	{
        	StringBuilder errorMessage = new StringBuilder();
        	errorMessage.AppendLine("An error occurred while loading data:");
        	errorMessage.AppendLine(ex.Message);
        	errorMessage.AppendLine("Stack trace:");
        	errorMessage.AppendLine(ex.StackTrace);

        	if (ex.Data.Contains("Filename"))
        	{
            	errorMessage.AppendLine($"Filename: {ex.Data["Filename"]}");
        	}

        	if (ex.Data.Contains("JSON"))
        	{
            	errorMessage.AppendLine("JSON data (first 1000 characters):");
            	errorMessage.AppendLine(ex.Data["JSON"].ToString().Substring(0, Math.Min(1000, ex.Data["JSON"].ToString().Length)));
        	}

        	Debug.LogError(errorMessage.ToString());

        	throw new Exception("Data loading error. Check logs for details.", ex);
    	}
	}


	public IEnumerator Load()
	{
		IsLoaded = false;
		doneLoadingAndParsingJsonData = false;
		donePostLoad = false;
		ex = null;
		if (threadLock == null)
		{
			threadLock = new object();
		}
		IDataManager manager2 = default(IDataManager);
		foreach (IDataManager manager in Dependencies)
		{
			new Thread((ThreadStart)delegate
			{
				loadDependencyThread(manager2);
			}).Start();
			while (!manager.IsLoaded && !ExceptionThrown)
			{
				yield return null;
			}
			CheckAndThrowExeptions();
		}
    	WWW www = new WWW(FilePath);
    	float startTime = Time.time;
    	while (!www.isDone)
    	{
        	if (Time.time - startTime > 30f) // 30 seconds timeout
        	{
            	Debug.LogError($"Timeout while loading data from {FilePath}");
            	yield break;
        	}
        	yield return null;
    	}

    	if (!string.IsNullOrEmpty(www.error))
    	{
        	Debug.LogError($"Error loading data from {FilePath}: {www.error}");
        	yield break;
    	}

    	string jsonText = www.text;
    	if (string.IsNullOrEmpty(jsonText))
    	{
        	Debug.LogError($"Received empty data from {FilePath}");
        	yield break;
    	}

    	LoadAndParseJsonDataThread(FilePath, jsonText);
			while (!doneLoadingAndParsingJsonData && !ExceptionThrown)
			{
				yield return null;
			}
			CheckAndThrowExeptions();
			IsLoaded = true;
			new Thread((ThreadStart)delegate
			{
				PostLoadThread();
			}).Start();
			while (!donePostLoad && !ExceptionThrown)
			{
				yield return null;
			}
			CheckAndThrowExeptions();
	}

	private void loadDependencyThread(IDataManager manager)
	{
		lock (threadLock)
		{
			try
			{
				if (manager != null) manager.Load();
			}
			catch (Exception ex)
			{
				if (manager != null)
				{
					ex.Data.Add("Manager", manager.ToString());
				}
				this.ex = ex;
			}
		}
	}

	private void LoadAndParseJsonDataThread(string appliedFilePath)
	{
		LoadAndParseJsonDataThread(appliedFilePath, null);
	}

private void LoadAndParseJsonDataThread(string appliedFilePath, string wwwText)
{
    lock (threadLock)
    {
        try
        {
            doneLoadingAndParsingJsonData = false;

            if (string.IsNullOrEmpty(wwwText))
            {
                throw new Exception("Received empty or null JSON data.");
            }

            object deserializedData;
            try
            {
                deserializedData = Json.Deserialize(wwwText);
            }
            catch (Exception jsonEx)
            {
                Debug.LogError($"Error deserializing JSON: {jsonEx.Message}");
                Debug.LogError($"JSON text (first 1000 characters): {wwwText.Substring(0, Math.Min(1000, wwwText.Length))}");
                throw;
            }

            List<object> jlist;
            if (deserializedData is List<object>)
            {
                jlist = (List<object>)deserializedData;
            }
            else if (deserializedData is Dictionary<string, object>)
            {
                jlist = new List<object> { deserializedData };
            }
            else
            {
                throw new Exception($"Unexpected deserialized data type: {deserializedData?.GetType().Name ?? "null"}");
            }

            if (jlist == null || jlist.Count == 0)
            {
                throw new Exception("Deserialized JSON resulted in a null or empty list.");
            }

            ParseRows(jlist);
            doneLoadingAndParsingJsonData = true;
        }
        catch (Exception ex)
        {
            ex.Data.Add("Filename", appliedFilePath);
            ex.Data.Add("JSON", wwwText);
            this.ex = ex;
        }
    }
}



	private void PostLoadThread()
	{
		lock (threadLock)
		{
			try
			{
				donePostLoad = false;
				PostLoad();
				donePostLoad = true;
			}
			catch (Exception ex)
			{
				Exception ex2 = (this.ex = ex);
			}
		}
	}

	protected virtual void ParseRows(List<object> jlist)
	{
    	if (jlist == null)
    	{
        	Debug.LogError("ParseRows received a null jlist.");
        	return;
    	}

    	for (int i = 0; i < jlist.Count; i++)
   		{
        	object item = jlist[i];
        	try
        	{
            	if (!(item is Dictionary<string, object> dict))
            	{
                	Debug.LogWarning($"Skipping item {i}: not a dictionary. Item type: {item?.GetType().Name ?? "null"}");
                	continue;
            	}

            	T val = (T)Activator.CreateInstance(typeof(T));
            	val.Populate(dict);

            	if (string.IsNullOrEmpty(val.ID))
            	{
                	Debug.LogWarning($"Skipping item {i}: ID is null or empty. Item data: {Json.Serialize(dict)}");
                	continue;
            	}

            	if (!Database.ContainsKey(val.ID))
            	{
                	Database.Add(val.ID, val);
            	}
            	else
            	{
                	Debug.LogWarning($"Duplicate ID found: {val.ID}. Skipping this item.");
            	}

            	DatabaseArray.Add(val);
        	}
        	catch (Exception ex)
        	{
            	Debug.LogError($"Error parsing row {i}: {ex.Message}");
            	Debug.LogError($"Problematic data: {Json.Serialize(item)}");
            	Debug.LogError($"Stack trace: {ex.StackTrace}");
            	// Store the exception for later handling
            	this.ex = ex;
        	}
    	}
	}



	protected virtual void PostLoad()
	{
	}

	public void AddDependency(IDataManager manager)
	{
		Dependencies ??= new List<IDataManager>();
		Dependencies.Add(manager);
	}

	public T GetData(string ID)
	{
		if (Database.ContainsKey(ID))
		{
			return Database[ID];
		}
		return default(T);
	}

	public T GetData(int index)
	{
		if (index < 0 || index >= DatabaseArray.Count)
		{
			return default(T);
		}
		return DatabaseArray[index];
	}

	public int GetIndex(T data)
	{
		return DatabaseArray.IndexOf(data);
	}

	public T Find(Predicate<T> match)
	{
		return DatabaseArray.Find(match);
	}

	public List<T> GetDatabase()
	{
		return DatabaseArray;
	}

	public virtual void Unload()
	{
		Database.Clear();
		DatabaseArray.Clear();
		IsLoaded = false;
		doneLoadingAndParsingJsonData = false;
		donePostLoad = false;
		ex = null;
	}
}
