using UnityEngine;

[CreateAssetMenu(fileName = "PlayerHpEventChannel",
                 menuName = "EventChannel/Player HP Event Channel")]
public class PlayerHpEventChannelSO : EventChannelSO<PlayerHpEventData>
{

}
public struct PlayerHpEventData
{
    public int currentHp;
    public int maxHp;

    public PlayerHpEventData(int currentHp, int maxHp)
    {
        this.currentHp = currentHp;
        this.maxHp = maxHp;
    }
}