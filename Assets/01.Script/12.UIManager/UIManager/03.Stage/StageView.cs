using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class StageView : MonoBehaviour, IUIViewInitialize
{
    [SerializeField] TextMeshProUGUI mapName;
    [SerializeField] TextMeshProUGUI ChallengeStageNum;
    [SerializeField] TextMeshProUGUI stageProcedureText;
    [SerializeField] Image stageProcedureBar;
    [SerializeField] Button challengeButton;
    [SerializeField] TextMeshProUGUI timeleft;
    [SerializeField] TextMeshProUGUI monsterLeft;
  
    public void InitializeView()
    {
        challengeButton.onClick.AddListener(() => OnChallengeBtnClicked?.Invoke());
    }

    public event Action OnChallengeBtnClicked;

    public void SetName(string nowMapName)
    {
        this.mapName.text = nowMapName;
    }
    public void SetChallengeStageNum(string nextStageName)
    {
        ChallengeStageNum.text = nextStageName ?? string.Empty;
    }   
    public void SetStageProcedureText(int nowStageNum, int totalStageNum)
    {
        this.stageProcedureText.text = $"Stage {nowStageNum}/{totalStageNum}";
    }
    public void SetStageProcedureBar(int nowStageNum, int totalStageNum)
    {
        stageProcedureBar.fillAmount = (float)nowStageNum / totalStageNum;
    }
    public void SetChallengeAvailable(bool available)
    {
        challengeButton.interactable = available;
    }



    //아래 2개는 시간 남으면 하기.
    public void SetTimeLeft(string timeLeft)
    {
        this.timeleft.text = timeLeft;
    }

    public void Monsterleft(int monsterLeft)
    {
        this.monsterLeft.text = monsterLeft.ToString();
    }

}
