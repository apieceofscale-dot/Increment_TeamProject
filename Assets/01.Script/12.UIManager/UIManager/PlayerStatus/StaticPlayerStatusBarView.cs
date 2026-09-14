using TMPro;
using UnityEngine;

public class StaticPlayerStatusBarView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI level;
    [SerializeField] TextMeshProUGUI job;

    public void LevelChagned(int level)
    {
        this.level.text = level.ToString();
    }
    public void JobChanged(string jobName)
    {
        this.job.text = jobName;
    }
}
