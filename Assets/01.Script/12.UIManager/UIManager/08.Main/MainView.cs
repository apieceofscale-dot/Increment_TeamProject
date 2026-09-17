using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class MainView : MonoBehaviour, IUIViewInitialize
{
    [SerializeField] GameObject startHud;
    [SerializeField] Button startButton;

    [SerializeField] GameObject choiceHud;
    [field: SerializeField] public Transform SpawnPosition { get; private set; } 

    [SerializeField] Button[] buttons;

    public event Action<int> OnChoiceButtonClicked;



    private void Awake()
    {
        startHud.SetActive(true);
        choiceHud.SetActive(false);
        startButton.onClick.AddListener(SwitchChoiceScreen);
    }

    private void SwitchChoiceScreen()
    {
        startHud.SetActive(false);
        choiceHud.SetActive(true);
    }

    public void SetButton(int index, Sprite sprite, string text)
    {
        if (index < 0 || index >= buttons.Length) return;

        Button targetButton = buttons[index];

        targetButton.image.sprite = sprite;

        TextMeshProUGUI buttonText = targetButton.GetComponentInChildren<TextMeshProUGUI>(true);
        buttonText.text = text;
    }

    public void InitializeView()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;
            buttons[i].onClick.AddListener(() => OnChoiceButtonClicked?.Invoke(index));
        }
    }
    

}
