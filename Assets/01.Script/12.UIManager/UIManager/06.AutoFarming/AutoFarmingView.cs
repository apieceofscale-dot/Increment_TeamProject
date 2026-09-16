using UnityEngine;
using System;
using UnityEngine.UI;


public class AutoFarmingView : MonoBehaviour, IUIViewInitialize
{
    [SerializeField] private Toggle autoFarmingToggle;
    [SerializeField] private GameObject effect;

    public event Action<bool> OnAutoFarmingChanged;


    public void InitializeView()
    {
        autoFarmingToggle.onValueChanged.AddListener(HandleToggleChanged);
    }
    private void HandleToggleChanged(bool isOn)
    {
        SetEffect(isOn);

        OnAutoFarmingChanged?.Invoke(isOn);
    }
    /*
    public void SetAutoFarming(bool isOn)
    {
        autoFarmingToggle.SetIsOnWithoutNotify(isOn);//중요. 토글 상태와 화면은 on이지만, onValueChanged는 발생시키지 않음.
                                                     //model에서 상태를 바꿀 때, view에서 .isOn을 true로 해버리는 상황을 봉쇄한다.

        SetEffect(isOn);
    }
    */
    private void SetEffect(bool isOn)
    {
        effect.SetActive(isOn);
    }

}
