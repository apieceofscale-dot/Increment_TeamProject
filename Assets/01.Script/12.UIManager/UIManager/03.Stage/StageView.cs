using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI mapName;
    [SerializeField] TextMeshProUGUI nowStage;
    [SerializeField] TextMeshProUGUI stageProcedureText;
    [SerializeField] Image stageProcedureBar;
    [SerializeField] Button challengeButton;

    public void mapNameChanged(string mapName)
    {
        this.mapName.text = mapName;
    }
    public void nowStageChanged(int nowStageNum)
    {
        this.nowStage.text = nowStageNum.ToString();
    }

    public void NowStageChanged(string stageName)
    {
        this.stageProcedureText.text = stageName;
    }

    public void StageProcedureTextChanged(int stageNum, int totalStageNum)
    {
        this.stageProcedureText.text = $"Stage {stageNum.ToString()}/{totalStageNum.ToString()}";
    }

    public void StageProcedureBarChanged(int nowStageNum, int totalStageNum)
    {
        stageProcedureBar.fillAmount = nowStageNum / totalStageNum;
    }



}
