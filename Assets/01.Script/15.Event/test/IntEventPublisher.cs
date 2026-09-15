using UnityEngine;

public class IntEventPublisher : MonoBehaviour
{
    [SerializeField] private IntEventChannelSO eventChannel;

    public void SendEvent(int value1)
    {
        Debug.Log($"event is sent : {value1}");
        eventChannel.Raise(value1);
    }

    [ContextMenu("Send Test Event")]
    private void SendTestEvent()
    {
        SendEvent(75);
    }
}

