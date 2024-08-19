using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Sound Volume")]
[RequireComponent(typeof(UISlider))]
public class UISoundVolume : MonoBehaviour
{
	private UISlider mSlider;

	private void Awake()
	{
		mSlider = GetComponent<UISlider>();
		mSlider.value = NGUITools.soundVolume;
		EventDelegate.Add(mSlider.onChange, OnChange);
	}

	private void OnChange()
	{
		NGUITools.soundVolume = UIProgressBar.current.value;
	}
}
