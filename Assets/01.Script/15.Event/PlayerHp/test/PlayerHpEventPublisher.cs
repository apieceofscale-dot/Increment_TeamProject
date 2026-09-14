using UnityEngine;

public class PlayerHpEventPublisher : MonoBehaviour
{
    [SerializeField] private PlayerHpEventChannelSO eventChannel;

    [ContextMenu("Send HP Event")]
    private void SendHpEvent()
    {
        PlayerHpEventData data = new PlayerHpEventData(75, 100);

        eventChannel.Raise(data);

        Debug.Log($"HP Event Sent : {data.currentHp} / {data.maxHp}");
    }

}