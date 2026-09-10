using System.Collections.Generic;
using UnityEngine;

public class CharacterJobAdvancedment : MonoBehaviour
{
    [SerializeField] private PlayerList playerList;
    public PlayerData CurrentJob { get; private set; }

    public List<PlayerData> GetAvailableJobs()
    {
        List<PlayerData> result = new List<PlayerData>();

        if (playerList == null || playerList.baseList == null)
            return result;

        Dictionary<int, int> counts = new Dictionary<int, int>();

        foreach (PlayerData data in playerList.baseList)
        {
            if (data == null)
                continue;

            counts.TryGetValue(data.id, out int count);
            counts[data.id] = count + 1;
        }

        foreach (PlayerData data in playerList.baseList)
        {
            if (data != null && counts[data.id] == 1)
                result.Add(data);
        }

        return result;
    }

    public bool TryChangeJob(int id)
    {
        foreach (PlayerData data in GetAvailableJobs())
        {
            if (data.id != id)
                continue;

            CurrentJob = data;

            Debug.Log($"직업 변경: {data.displayName} (ID: {data.id})", this);
            return true;
        }
        Debug.LogWarning($"직업 ID {id} | 잘못 입력", this);
        return false;
    }

    public bool TryChangeNextJob()
    {
        List<PlayerData> jobs = GetAvailableJobs();
        if (jobs.Count == 0)
        {
            Debug.LogWarning("선택 가능한 직업 없음. PlayerList 연결", this);
            return false;
        }
        int index = CurrentJob == null ? -1 : jobs.FindIndex(x => x.id == CurrentJob.id);
        return TryChangeJob(jobs[(index + 1) % jobs.Count].id);
    }
}
