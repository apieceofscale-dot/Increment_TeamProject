using UnityEngine;

public class PlayerHpEventSubscriber : MonoBehaviour
{
    [SerializeField] private PlayerHpEventChannelSO eventChannel;

    private void OnEnable()
    {
        eventChannel.OnRaised += OnHpChanged;
    }
    private void OnDisable()
    {
        eventChannel.OnRaised -= OnHpChanged;
    }
    private void OnHpChanged(PlayerHpEventData data)
    {
        Debug.Log($"HP Event Received : {data.currentHp} / {data.maxHp}");
    }
}
