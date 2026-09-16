using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class StageView : MonoBehaviour, IUIViewInitialize
{
    [SerializeField] TextMeshProUGUI nowMapName;
    [SerializeField] TextMeshProUGUI ChallengeStageNum;
    [SerializeField] TextMeshProUGUI stageProcedureText;
    [SerializeField] Image stageProcedureBar;
    [SerializeField] Button challengeButton;
  
    public void InitializeView()
    {
        challengeButton.onClick.AddListener(() => OnChallengeBtnClicked?.Invoke());
    }

    public event Action OnChallengeBtnClicked;

    public void SetName(string nowMapName)
    {
        this.nowMapName.text = nowMapName;
    }
    public void SetChallengeStageNum(int nowStageNum /*, string nextStageName*/)
    {
        this.ChallengeStageNum.text = (nowStageNum + 1).ToString();        
    }   
    public void SetStageProcedureText(int nowStageNum, int totalStageNum)
    {
        this.stageProcedureText.text = $"Stage {nowStageNum}/{totalStageNum}";
    }
    public void SetStageProcedureBar(int nowStageNum, int totalStageNum)
    {
        stageProcedureBar.fillAmount = (float)nowStageNum / totalStageNum;
    }

}
