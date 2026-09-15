using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSettingView : MonoBehaviour
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


    [SerializeField] Button weaponBtn;
    [SerializeField] TextMeshProUGUI weaponText;
    [SerializeField] GameObject weaponPopUp;



    public void InitializePopUp()//PlayerHud Start에서 선언.
    {
        characterPopUp.SetActive(false);
        equipmentPopUp.SetActive(false);
        skillPopUp.SetActive(false);
        weaponPopUp.SetActive(false);
    }

    





}
