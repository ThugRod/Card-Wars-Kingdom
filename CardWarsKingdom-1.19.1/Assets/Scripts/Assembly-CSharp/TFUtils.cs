#define ASSERTS_ON
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Ionic.Zlib;
using UnityEngine;

public class TFUtils
{
	private const bool CheckKeyAssert = false;

	public static DateTime EPOCH = new DateTime(1970, 1, 1).ToUniversalTime();

	public static string DeviceID;

	public static string DeviceName;

	public static string FacebookID;

	private static HashAlgorithm hash;

	private static DateTime lastServerTimeUpdate = new DateTime(0L);

	private static TimeSpan serverTimeDiff = new TimeSpan(0L);

	public static bool AmazonDevice { get; private set; }

	public static DateTime ServerTime
	{
		get
		{
			return DateTime.UtcNow + serverTimeDiff;
		}
	}

    public static bool TryParseDateTime(string dateString, out DateTime result)
    {
        result = DateTime.MinValue;
        if (string.IsNullOrEmpty(dateString))
            return false;

        string[] formats = { "dd/MM/yyyy", "MM/dd/yyyy", "yyyy-MM-dd", "yyyy/MM/dd" };
        return DateTime.TryParseExact(dateString, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
    }	

	public static void Init(string fbid)
	{
		DeviceID = Guid.NewGuid().ToString().Replace("-", string.Empty);
		DeviceName = SystemInfo.deviceName;
		FacebookID = (fbid != null) ? fbid : DeviceID;
        UnityEngine.Debug.Log("This device is:" + DeviceID + " / Player ID is:" + FacebookID);
		AmazonDevice = false;
	}

	public static void UpdateServerTime(DateTime serverTime)
	{
		lastServerTimeUpdate = serverTime;
		TimeSpan timeSpan = serverTime.Subtract(DateTime.UtcNow);
		double num = Math.Abs((timeSpan - serverTimeDiff).TotalSeconds);
		if (num > 10.0)
		{
			serverTimeDiff = timeSpan;
            UnityEngine.Debug.Log("Server time difference = " + timeSpan.TotalSeconds);
		}
	}

	public static TimeSpan GetServerTimeDiff()
	{
		return serverTimeDiff;
	}

	public static bool IsServerTimeValid()
	{
		return true;
	}

	public static int EpochTime()
	{
		return EpochTime(DateTime.UtcNow);
	}

	public static int EpochTime(DateTime dt)
	{
		return (int)(dt - EPOCH).TotalSeconds;
	}

	public static DateTime EpochToDateTime(int seconds)
	{
		return DateTime.SpecifyKind(EPOCH.AddSeconds(seconds), DateTimeKind.Utc);
	}

	public static string DurationToString(int duration)
	{
		if (duration < 60)
		{
			return string.Format("{0}s", duration);
		}
		int num = duration % 60;
		duration -= num;
		int num2 = duration / 60;
		if (num2 < 60)
		{
			if (num == 0)
			{
				return string.Format("{0}m", num2);
			}
			return string.Format("{0}m {1}s", num2, num);
		}
		int num3 = num2 / 60;
		num2 %= 60;
		if (num3 < 24)
		{
			if (num2 == 0)
			{
				return string.Format("{0}h", num3);
			}
			return string.Format("{0}h {1}m", num3, num2);
		}
		int num4 = num3 / 24;
		num3 %= 24;
		if (num3 == 0)
		{
			return string.Format("{0}d", num4);
		}
		return string.Format("{0}d {1}h", num4, num3);
	}

	public static Dictionary<KeyType, ValueType> CloneDictionary<KeyType, ValueType>(Dictionary<KeyType, ValueType> source)
	{
		Dictionary<KeyType, ValueType> dictionary = new Dictionary<KeyType, ValueType>();
		foreach (KeyType key in source.Keys)
		{
			dictionary[key] = source[key];
		}
		return dictionary;
	}

	public static void CloneDictionaryInPlace<KeyType, ValueType>(Dictionary<KeyType, ValueType> source, Dictionary<KeyType, ValueType> dest)
	{
		dest.Clear();
		foreach (KeyValuePair<KeyType, ValueType> item in source)
		{
			dest.Add(item.Key, item.Value);
		}
	}

	public static Dictionary<KeyType, ValueType> ConcatenateDictionaryInPlace<KeyType, ValueType>(Dictionary<KeyType, ValueType> dest, Dictionary<KeyType, ValueType> source)
	{
		foreach (KeyType key in source.Keys)
		{
			if (dest.ContainsKey(key))
			{
				throw new ArgumentException("Destination dictionary already contains key " + key.ToString());
			}
			dest[key] = source[key];
		}
		return dest;
	}

	public static List<To> CloneAndCastList<From, To>(List<From> list) where From : To
	{
		List<To> list2 = new List<To>(list.Count);
		foreach (From item in list)
		{
			list2.Add((To)(object)item);
		}
		return list2;
	}

	private static T AssertCast<T>(Dictionary<string, object> dict, string key)
	{
		return (T)dict[key];
	}

	[Conditional("DEBUG")]
	public static void AssertKeyExists(Dictionary<string, object> dict, string key)
	{
	}

	public static bool? LoadNullableBool(Dictionary<string, object> d, string key)
	{
		object obj = d[key];
		if (obj == null)
		{
			return (bool?)obj;
		}
		return AssertCast<bool?>(d, key);
	}

	public static List<T> TryLoadList<T>(Dictionary<string, object> data, string key)
	{
		if (!data.ContainsKey(key))
		{
			return null;
		}
		return LoadList<T>(data, key);
	}

	public static List<T> LoadList<T>(Dictionary<string, object> data, string key)
	{
		if (data[key] is List<T>)
		{
			return (List<T>)data[key];
		}
		List<object> list = (List<object>)data[key];
		List<T> retval = new List<T>(data.Count);
		list.ForEach(delegate(object obj)
		{
			retval.Add((T)Convert.ChangeType(obj, typeof(T)));
		});
		return retval;
	}

	public static Dictionary<string, object> LoadDict(Dictionary<string, object> data, string key)
	{
		return (Dictionary<string, object>)data[key];
	}

	public static Dictionary<string, object> TryLoadDict(Dictionary<string, object> data, string key)
	{
		if (!data.ContainsKey(key))
		{
			return null;
		}
		return (Dictionary<string, object>)data[key];
	}

		public static string LoadString(Dictionary<string, object> data, string key)
	{
    	return LoadString(data, key, string.Empty);
	}

	public static string LoadString(Dictionary<string, object> data, string key, string defaultValue)
	{
    	if (TryLoadString(data, key, out string result))
    	{
        	return string.IsNullOrEmpty(result) ? defaultValue : result;
    	}
    	return defaultValue;
	}

	public static string TryLoadString(Dictionary<string, object> data, string key)
	{
    	TryLoadString(data, key, out string result);
    	return result;
	}

	public static bool TryLoadString(Dictionary<string, object> data, string key, out string result)
	{
    	result = null;

    	if (data == null || key == null)
    	{
        	return false;
    	}

    	if (data.TryGetValue(key, out object value))
    	{
        	result = value?.ToString();
        	return true;
    	}

    	return false;
	}

	public static string LoadLocalizedString(Dictionary<string, object> data, string key, string defaultValue)
	{
		return KFFLocalization.Get(LoadString(data, key, defaultValue));
	}

	public static string LoadLocalizedString(Dictionary<string, object> data, string key)
	{
		return KFFLocalization.Get(LoadString(data, key));
	}

	public static string TryLoadLocalizedString(Dictionary<string, object> data, string key)
	{
    	if (TryLoadString(data, key, out string result))
    	{
        	return KFFLocalization.Get(result);
    	}
    	return string.Empty; // or return null, depending on your preference
	}

	public static string LoadNullableString(Dictionary<string, object> data, string key)
	{
		if (data.ContainsKey(key))
		{
			return AssertCast<string>(data, key);
		}
		return null;
	}

	public static int? LoadNullableInt(Dictionary<string, object> d, string key)
	{
		object obj = d[key];
		if (obj == null)
		{
			return (int?)obj;
		}
		return AssertCast<int?>(d, key);
	}

	public static int? TryLoadNullableInt(Dictionary<string, object> d, string key)
	{
		//Discarded unreachable code: IL_0024, IL_0039
		try
		{
			return (int)Math.Floor(Convert.ToSingle(d[key], CultureInfo.InvariantCulture) + 0.5f);
		}
		catch
		{
			return null;
		}
	}

	public static uint? LoadNullableUInt(Dictionary<string, object> d, string key)
	{
		object obj = d[key];
		if (obj == null)
		{
			return (uint?)obj;
		}
		return LoadUint(d, key);
	}

	public static uint? TryLoadNullableUInt(Dictionary<string, object> d, string key)
	{
		if (d.ContainsKey(key))
		{
			return LoadNullableUInt(d, key);
		}
		return null;
	}

	public static object NullableToObject<T>(T? nullable) where T : struct
	{
		return (!nullable.HasValue) ? null : ((object)nullable.Value);
	}

	public static int? TryLoadInt(Dictionary<string, object> data, string key)
	{
		if (data.ContainsKey(key))
		{
			return LoadIntHelper(data, key);
		}
		return null;
	}

	public static bool LoadBoolAsInt(Dictionary<string, object> d, string key)
	{
		return (LoadInt(d, key) != 0) ? true : false;
	}

	public static bool LoadBoolAsInt(Dictionary<string, object> d, string key, bool defaultValue)
	{
		int defaultValue2 = defaultValue ? 1 : 0;
		return (LoadInt(d, key, defaultValue2) != 0) ? true : false;
	}

	public static bool LoadBool(Dictionary<string, object> d, string key, bool defaultValue)
	{
		//Discarded unreachable code: IL_0095
		bool result = defaultValue;
		if (d.ContainsKey(key))
		{
			object obj = d[key];
			if (obj is int)
			{
				result = ((int)obj != 0) ? true : false;
			}
			else if (obj is string)
			{
				try
				{
					return bool.Parse((string)obj);
				}
				catch (Exception)
				{
					if ((string)obj == "0")
					{
						return false;
					}
					if ((string)obj == "1")
					{
						return true;
					}
					return defaultValue;
				}
			}
		}
		return result;
	}

	public static bool? TryLoadNullableBool(Dictionary<string, object> d, string key)
	{
		//Discarded unreachable code: IL_001c, IL_0031
		try
		{
			return bool.Parse((string)d[key]);
		}
		catch
		{
			return null;
		}
	}

	public static int LoadInt(Dictionary<string, object> d, string key)
	{
		return LoadIntHelper(d, key);
	}

    public static int LoadInt(Dictionary<string, object> d, string key, int defaultValue)
    {
        if (d == null)
        {
            UnityEngine.Debug.LogWarning($"LoadInt: Dictionary is null when trying to load key '{key}'. Using default value {defaultValue}.");
            return defaultValue;
        }

        if (!d.TryGetValue(key, out object obj))
        {
            UnityEngine.Debug.LogWarning($"LoadInt: Key '{key}' not found in dictionary. Using default value {defaultValue}.");
            return defaultValue;
        }

        if (obj == null)
        {
            UnityEngine.Debug.LogWarning($"LoadInt: Value for key '{key}' is null. Using default value {defaultValue}.");
            return defaultValue;
        }

        if (obj is int intValue)
        {
            return intValue;
        }

        if (obj is long longValue)
        {
            return (int)longValue;
        }

        if (obj is float floatValue)
        {
            return (int)Math.Floor(floatValue + 0.5f);
        }

        if (obj is double doubleValue)
        {
            return (int)Math.Floor(doubleValue + 0.5);
        }

        if (obj is string stringValue)
        {
            if (string.IsNullOrEmpty(stringValue))
            {
                UnityEngine.Debug.LogWarning($"LoadInt: Empty string value for key '{key}'. Using default value {defaultValue}.");
                return defaultValue;
            }

            if (int.TryParse(stringValue, out int parsedValue))
            {
                return parsedValue;
            }
        }

        UnityEngine.Debug.LogWarning($"LoadInt: Failed to parse value for key '{key}'. Type: {obj.GetType().Name}. Using default value {defaultValue}.");
        return defaultValue;
    }

	private static int LoadIntHelper(Dictionary<string, object> d, string key)
	{
		return (int)Math.Floor(Convert.ToSingle(d[key], CultureInfo.InvariantCulture) + 0.5f);
	}

	public static uint LoadUint(Dictionary<string, object> data, string key)
	{
		return LoadUintHelper(data, key);
	}

	public static uint LoadUint(Dictionary<string, object> d, string key, uint defaultValue)
	{
		uint result = defaultValue;
		if (d.ContainsKey(key))
		{
			object value = d[key];
			result = Convert.ToUInt32(value, CultureInfo.InvariantCulture);
		}
		return result;
	}

	public static uint? TryLoadUint(Dictionary<string, object> data, string key)
	{
		if (!data.ContainsKey(key))
		{
			return null;
		}
		return LoadUintHelper(data, key);
	}

	private static uint LoadUintHelper(Dictionary<string, object> data, string key)
	{
		return Convert.ToUInt32(data[key], CultureInfo.InvariantCulture);
	}

	public static float? TryLoadNullableFloat(Dictionary<string, object> d, string key)
	{
		//Discarded unreachable code: IL_0017, IL_002c
		try
		{
			return Convert.ToSingle(d[key], CultureInfo.InvariantCulture);
		}
		catch
		{
			return null;
		}
	}

	    public static T LoadEnum<T>(Dictionary<string, object> d, string key, T defaultValue) where T : struct, Enum
    {
        if (d == null || !d.TryGetValue(key, out object value))
        {
            return defaultValue;
        }

        if (value == null)
        {
            return defaultValue;
        }

        string stringValue = value.ToString().Trim();
        if (string.IsNullOrEmpty(stringValue))
        {
            return defaultValue;
        }

        if (Enum.TryParse<T>(stringValue, true, out T result))
        {
            return result;
        }

        UnityEngine.Debug.LogWarning($"Failed to parse enum value '{stringValue}' for key '{key}'. Using default value {defaultValue}.");
        return defaultValue;
    }

	public static float? TryLoadFloat(Dictionary<string, object> data, string key)
	{
		if (data.ContainsKey(key))
		{
			return (float)AssertCast<double>(data, key);
		}
		return null;
	}

	public static float LoadFloat(Dictionary<string, object> d, string key)
	{
		return Convert.ToSingle(d[key], CultureInfo.InvariantCulture);
	}

	public static float LoadFloat(Dictionary<string, object> d, string key, float defaultValue)
	{
    	if (d == null || !d.TryGetValue(key, out object value))
    	{
        	return defaultValue;
    	}

    	if (value == null)
    	{
        	return defaultValue;
    	}

    	if (value is float floatValue)
    	{
        	return floatValue;
    	}

    	if (float.TryParse(value.ToString(), out float result))
    	{
        	return result;
    	}

    	UnityEngine.Debug.LogWarning($"TFUtils.LoadFloat: Failed to parse value '{value}' for key '{key}'. Using default value {defaultValue}.");
    	return defaultValue;
}


	public static void LoadVector3(out Vector3 v3, Dictionary<string, object> d, float defaultValue)
	{
		v3.x = (!d.ContainsKey("x")) ? defaultValue : LoadFloat(d, "x");
		v3.y = (!d.ContainsKey("y")) ? defaultValue : LoadFloat(d, "y");
		v3.z = (!d.ContainsKey("z")) ? defaultValue : LoadFloat(d, "z");
	}

	public static void SaveVector3(Vector3 v3, string name, Dictionary<string, object> d)
	{
		d[name] = new Dictionary<string, object>
		{
			{ "x", v3.x },
			{ "y", v3.y },
			{ "z", v3.z }
		};
	}

	public static void LoadVector2(out Vector2 v2, Dictionary<string, object> d, float defaultValue)
	{
		Assert(!d.ContainsKey("z"), "Don't call LoadVector2 on something that has a z value! (do you want to use LoadVector3?)");
		v2.x = (!d.ContainsKey("x")) ? defaultValue : LoadFloat(d, "x");
		v2.y = (!d.ContainsKey("y")) ? defaultValue : LoadFloat(d, "y");
	}

	public static void LoadVector3(out Vector3 v3, Dictionary<string, object> d)
	{
		LoadVector3(out v3, d, 0f);
	}

	public static void LoadVector2(out Vector2 v2, Dictionary<string, object> d)
	{
		LoadVector2(out v2, d, 0f);
	}

	public static Vector3 ExpandVector(Vector2 vector)
	{
		return ExpandVector(vector, 0f);
	}

	public static Vector3 ExpandVector(Vector2 vector, float z)
	{
		return new Vector3(vector.x, vector.y, z);
	}

	public static Vector2 TruncateVector(Vector3 vector)
	{
		return (Vector2)vector;
	}

	public static void TruncateFile(string filePath)
	{
		DeleteFile(filePath);
        using FileStream fileStream = File.Create(filePath);
        fileStream.Close();
    }

	public static void DeleteFile(string filePath)
	{
		if (File.Exists(filePath))
		{
			File.Delete(filePath);
		}
	}

	public static string GetPersistentAssetsPath()
	{
		return Path.Combine(Application.persistentDataPath, "Contents");
	}

	public static string GetStreamingAssetsPath()
	{
		return Application.streamingAssetsPath;
	}

	public static string GetStreamingAssetsSubfolder(string path)
	{
		return GetStreamingAssetsPath() + Path.DirectorySeparatorChar + path;
	}

	public static string GetStreamingAssetsFileInDirectory(string path, string filename)
	{
		return GetStreamingAssetsFile(path + Path.DirectorySeparatorChar + filename);
	}

	public static string GetStreamingAssetsFile(string fileName)
	{
		string text = GetPersistentAssetsPath() + Path.DirectorySeparatorChar + fileName;
		if (File.Exists(text))
		{
			return text;
		}
		return GetStreamingAssetsPath() + Path.DirectorySeparatorChar + fileName;
	}

	public static string GetJsonFileContent(string filename)
	{
		string streamingAssetsFile = GetStreamingAssetsFile(filename);
		if (streamingAssetsFile.Contains("://"))
		{
			return GetAndroidFileContents(streamingAssetsFile);
		}
		return File.ReadAllText(streamingAssetsFile);
	}

	public static string GetJsonLocalContent(string filename)
	{
		if (filename.Contains("://"))
		{
			return GetAndroidFileContents(filename);
		}
		return File.ReadAllText(filename);
	}

	private static string GetAndroidFileContents(string filePath)
	{
		WWW wWW = null;
		wWW = new WWW(filePath);
		while (!wWW.isDone)
		{
		}
		return wWW.text;
	}

	public static string[] GetFilesInPath(string path, string searchPattern)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		string streamingAssetsSubfolder = GetStreamingAssetsSubfolder(path);
		string streamingAssetsPath = GetStreamingAssetsPath();
		string[] files = Directory.GetFiles(streamingAssetsSubfolder, searchPattern, SearchOption.AllDirectories);
		foreach (string text in files)
		{
			string key = text.Substring(streamingAssetsPath.Length);
			dictionary[key] = text;
		}
		string persistentAssetsPath = GetPersistentAssetsPath();
		string path2 = GetPersistentAssetsPath() + Path.DirectorySeparatorChar + path;
		if (Directory.Exists(path2))
		{
			string[] files2 = Directory.GetFiles(path2, searchPattern, SearchOption.AllDirectories);
			foreach (string text2 in files2)
			{
				string key2 = text2.Substring(persistentAssetsPath.Length);
				dictionary[key2] = text2;
			}
		}
		string[] array = new string[dictionary.Count];
		dictionary.Values.CopyTo(array, 0);
		return array;
	}

	[Conditional("DEBUG")]
	public static void DebugDict(Dictionary<string, object> d)
	{
	}

	public static string DebugDictToString(Dictionary<string, object> d)
	{
		return "[Dictionary Debug View]\n" + PrintDict(d, string.Empty);
	}

	public static string DebugListToString(List<object> l)
	{
		return "[List Debug View]\n" + PrintList(l, string.Empty);
	}

	public static string DebugListToString(List<Vector3> list)
	{
		return DebugListToString(list.ConvertAll((Converter<Vector3, object>)((Vector3 v) => "\t(" + v.x + ",\t" + v.y + ",\t" + v.z + ")")));
	}

	public static string DebugListToString(List<Vector2> list)
	{
		return DebugListToString(list.ConvertAll((Vector2 v) => ExpandVector(v)));
	}

	public static string getJsonTextFromWWW(string filePath)
	{
		WWW wWW = null;
		wWW = new WWW(filePath);
		while (!wWW.isDone)
		{
		}
		return wWW.text;
	}

	private static string PrintDict(Dictionary<string, object> d, string lead)
	{
		if (d == null)
		{
			return "null";
		}
		string text = "{\n";
		foreach (string key in d.Keys)
		{
			if (d[key] != null)
			{
				string text2 = text;
				text = text2 + lead + key + ":" + PrintGenericValue(d[key], lead + " ") + ",\n";
			}
			else
			{
				string text2 = text;
				text = string.Concat(text2, lead, key, ":", d[key], ",\n");
			}
		}
		return text + lead + "}";
	}

	private static string PrintList(List<object> l, string lead)
	{
		if (l == null)
		{
			return "null";
		}
		string text = "[\n";
		for (int i = 0; i < l.Count; i++)
		{
			string text2 = text;
			text = text2 + lead + i + ":" + PrintGenericValue(l[i], lead + " ") + ",\n";
		}
		return text + lead + "]";
	}

	private static string PrintGenericValue(object v, string lead)
	{
		if (v is Dictionary<string, object>)
		{
			return PrintDict(v as Dictionary<string, object>, lead + " ");
		}
		if (v is List<object>)
		{
			return PrintList(v as List<object>, lead + " ");
		}
		if (v == null)
		{
			return "null\n";
		}
		return v.ToString();
	}

	[Conditional("DEBUG")]
	public static void LogFormat(string format, params object[] args)
	{
	}

	[Conditional("DEBUG")]
	public static void UnexpectedEntry()
	{
		throw new Exception("Unexpected path of code execution! You should not be here!");
	}

	[Conditional("ASSERTS_ON")]
	public static void Assert(bool condition, string message)
	{
		if (!condition)
		{
			throw new Exception(message);
		}
	}

	[Conditional("ASSERTS_ON")]
	public static void Assert(bool condition)
	{
		if (!condition)
		{
			throw new Exception(condition.ToString());
		}
	}

	public static GameObject FindGameObjectInHierarchy(GameObject root, string name)
	{
		if (root.name.Equals(name))
		{
			return root;
		}
		GameObject gameObject = null;
		int childCount = root.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			gameObject = FindGameObjectInHierarchy(root.transform.GetChild(i).gameObject, name);
			if (gameObject != null)
			{
				break;
			}
		}
		return gameObject;
	}

	public static GameObject FindParentGameObjectInHierarchy(GameObject root, string name)
	{
		Transform transform = root.transform;
		while (transform.parent != null)
		{
			if (transform.gameObject.name.Equals(name))
			{
				return transform.gameObject;
			}
			transform = transform.parent;
		}
		return null;
	}

	public static void PlayMovie(string movie)
	{
#if UNITY_ANDROID || UNITY_IOS
		Handheld.PlayFullScreenMovie(movie, Color.black, FullScreenMovieControlMode.CancelOnInput);
#endif
	}

	public static byte[] Zip(string str)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(str);
		return Zip(bytes);
	}

	public static byte[] Zip(byte[] bytedata)
	{
		//Discarded unreachable code: IL_003e
		using (MemoryStream memoryStream = new MemoryStream())
		{
			using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
			{
				gZipStream.Write(bytedata, 0, bytedata.Length);
				gZipStream.Close();
			}
			return memoryStream.ToArray();
		}
	}

	public static byte[] UnzipToBytes(byte[] input)
	{
		MemoryStream stream = new MemoryStream(input);
		MemoryStream memoryStream = new MemoryStream();
		using (GZipStream gZipStream = new GZipStream(stream, CompressionMode.Decompress))
		{
			byte[] array = new byte[1024];
			int num = 0;
			while ((num = gZipStream.Read(array, 0, array.Length)) > 0)
			{
				memoryStream.Write(array, 0, num);
			}
		}
		return memoryStream.ToArray();
	}

	public static string Unzip(byte[] input)
	{
		return Encoding.UTF8.GetString(UnzipToBytes(input));
	}

	public static int BoolToInt(bool myBool)
	{
		if (myBool)
		{
			return 1;
		}
		return 0;
	}

	public static int KontagentCurrencyLevelIndex(int kRange)
	{
		if (kRange > 0 && kRange < 10)
		{
			return 1;
		}
		if (kRange > 10 && kRange < 100)
		{
			return 2;
		}
		if (kRange > 100 && kRange < 1000)
		{
			return 3;
		}
		if (kRange > 1000 && kRange < 10000)
		{
			return 4;
		}
		if (kRange > 10000 && kRange < 100000)
		{
			return 5;
		}
		if (kRange > 100000)
		{
			return 6;
		}
		return 0;
	}

	public static string GetiOSDeviceTypeString()
	{
		return "Unknown";
	}

	public static void WriteFile(string filename, string data)
	{
		File.WriteAllText(filename, data);
	}

	public static string ReadFile(string filename)
	{
		return File.ReadAllText(filename);
	}

	public static string ComputeDigest(string input)
	{
		if (input == null)
		{
			input = string.Empty;
		}
		if (hash == null)
		{
			hash = MD5.Create();
		}
		Stream inputStream = new MemoryStream(Encoding.UTF8.GetBytes(input));
		StringBuilder stringBuilder = new StringBuilder();
		byte[] array = hash.ComputeHash(inputStream);
		foreach (byte b in array)
		{
			stringBuilder.Append(b.ToString("X2"));
		}
		return stringBuilder.ToString();
	}

	public static bool IsAndroidDeviceRooted()
	{
		bool flag = false;
		string[] array = new string[8] { "/sbin/", "/system/bin/", "/system/xbin/", "/data/local/xbin/", "/data/local/bin/", "/system/sd/xbin/", "/system/bin/failsafe/", "/data/local/" };
		string[] array2 = array;
		foreach (string text in array2)
		{
			flag = File.Exists(text + "su");
			if (flag)
			{
				break;
			}
		}
		return flag;
	}

	public string max256(string prm_key, string prm_iv, string prm_text_to_encrypt)
	{
		RijndaelManaged rijndaelManaged = new RijndaelManaged();
		rijndaelManaged.Padding = PaddingMode.Zeros;
		rijndaelManaged.Mode = CipherMode.CBC;
		rijndaelManaged.KeySize = 256;
		rijndaelManaged.BlockSize = 256;
		byte[] array = new byte[0];
		byte[] array2 = new byte[0];
		array = Encoding.UTF8.GetBytes(prm_key);
		array2 = Encoding.UTF8.GetBytes(prm_iv);
		ICryptoTransform transform = rijndaelManaged.CreateEncryptor(array, array2);
		MemoryStream memoryStream = new MemoryStream();
		CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
		byte[] bytes = Encoding.UTF8.GetBytes(prm_text_to_encrypt);
		cryptoStream.Write(bytes, 0, bytes.Length);
		cryptoStream.FlushFinalBlock();
		byte[] inArray = memoryStream.ToArray();
		return Convert.ToBase64String(inArray);
	}

	public string min256(string prm_key, string prm_iv, string prm_text_to_decrypt)
	{
		RijndaelManaged rijndaelManaged = new RijndaelManaged();
		rijndaelManaged.Padding = PaddingMode.Zeros;
		rijndaelManaged.Mode = CipherMode.CBC;
		rijndaelManaged.KeySize = 256;
		rijndaelManaged.BlockSize = 256;
		byte[] array = new byte[0];
		byte[] array2 = new byte[0];
		array = Encoding.UTF8.GetBytes(prm_key);
		array2 = Encoding.UTF8.GetBytes(prm_iv);
		ICryptoTransform transform = rijndaelManaged.CreateDecryptor(array, array2);
		byte[] array3 = Convert.FromBase64String(prm_text_to_decrypt);
		byte[] array4 = new byte[array3.Length];
		MemoryStream stream = new MemoryStream(array3);
		CryptoStream cryptoStream = new CryptoStream(stream, transform, CryptoStreamMode.Read);
		cryptoStream.Read(array4, 0, array4.Length);
		return Encoding.UTF8.GetString(array4);
	}
}
