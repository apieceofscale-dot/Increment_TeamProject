using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSettingView : MonoBehaviour, IUIViewInitialize
{
    [SerializeField] Button characterBtn; 
    [SerializeField] TextMeshProUGUI characterText;
    [SerializeField] GameObject characterPopUp;

    [SerializeField] Button equipmentBtn;
    [SerializeField] TextMeshProUGUI equipmentText;
    [SerializeField] GameObject equipmentPopUp;

    [SerializeField] Button skillBtn;
    [SerializeField] TextMeshProUGUI skillText;
    [SerializeField] GameObject skillPopUp;

    [SerializeField] Button enchantBtn;
    [SerializeField] TextMeshProUGUI enchantText;
    [SerializeField] GameObject enchantPopUp;

    [SerializeField] GameObject dim;
    [SerializeField] Button dimBtn;


    public void InitializeView()
    {
        dimBtn.onClick.AddListener(SetAllPopUpFalse);
        characterBtn.onClick.AddListener(OpenCharacterPopup);
        equipmentBtn.onClick.AddListener(OpenEquipmentPopup);
        skillBtn.onClick.AddListener(OpenSkillPopup);
        enchantBtn.onClick.AddListener(OpenEnchantPopup);

        SetAllPopUpFalse();

    }

    public void SetAllPopUpFalse()//PlayerHud Start에서 선언.
    {
        characterPopUp.SetActive(false);
        equipmentPopUp.SetActive(false);
        skillPopUp.SetActive(false);
        enchantPopUp.SetActive(false);
        dim.SetActive(false);
    }

    private void OpenPopup(GameObject target)
    {
        characterPopUp.SetActive(target == characterPopUp);
        equipmentPopUp.SetActive(target == equipmentPopUp);
        skillPopUp.SetActive(target == skillPopUp);
        enchantPopUp.SetActive(target == enchantPopUp);

        dim.SetActive(true);
    }



    // [변경] View는 00.Script에 있으므로 팝업 활성화만으로 OnEnable이 호출되지 않는다.
    // private void OpenCharacterPopup() => OpenPopup(characterPopUp);
    private void OpenCharacterPopup()
    {
        OpenPopup(characterPopUp);
        CharacterPopupView popupView = GetComponent<CharacterPopupView>();
        if (popupView != null)
            popupView.InitializePopup();
    }
    private void OpenEquipmentPopup() => OpenPopup(equipmentPopUp);
    private void OpenSkillPopup() => OpenPopup(skillPopUp);
    private void OpenEnchantPopup() => OpenPopup(enchantPopUp);









}
