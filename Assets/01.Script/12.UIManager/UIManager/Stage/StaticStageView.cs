using TMPro;
using UnityEngine;

public class StaticStageView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI mapName;
    [SerializeField] TextMeshProUGUI nowStage;
    [SerializeField] TextMeshProUGUI stageProcedure;    

    public void mapNameChanged(string mapName)
    {
        this.mapName.text = mapName;
    }

    public void NowStageChanged(string stageName)
    {
        this.stageProcedure.text = stageName;
    }

    public void StageProcedureChanged(int stageNum, int totalStageNum)
    {
        this.stageProcedure.text = $"Stage {stageNum.ToString()}/{totalStageNum.ToString()}";
    }



}
