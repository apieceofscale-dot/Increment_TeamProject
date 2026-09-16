using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//ONPointuphandler인가 넣을거?
public class SkillView : MonoBehaviour, IUIViewInitialize
{      
    [SerializeField] Button[] button;
    Dictionary<Button, TextMeshProUGUI> text = new Dictionary<Button, TextMeshProUGUI>();

    public event Action<int> OnSkillClicked; //버튼이 눌렸다는 것을 presenter가 알게 하는 역할.

   
    public void InitializeView()
    {
        for (int i = 0; i < button.Length; i++)
        {
            int slotIndex = i;
            button[i].onClick.AddListener(() => OnSkillClicked?.Invoke(slotIndex));
        }
    }


    public void SetSkill(int slotIndex, Sprite sprite, string skillName)
    {
        Button targetbtn = button[slotIndex];
        targetbtn.image.sprite = sprite;

        if(!text.TryGetValue(targetbtn, out TextMeshProUGUI skillText))
        {
            skillText = targetbtn.GetComponentInChildren<TextMeshProUGUI>(true);
            text.TryAdd(targetbtn, skillText);
        }

        skillText.text = skillName;
    }

    public void SetCooltime(int slotIndex, float timeLeft, float totalTime)
    {
        Button targetbtn = button[slotIndex];
        text[targetbtn].text = Mathf.CeilToInt(timeLeft).ToString();
        targetbtn.image.fillAmount = timeLeft / totalTime;
    }


} 
