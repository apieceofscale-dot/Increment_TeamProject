using UnityEngine;

public class IntEventSubscriber : MonoBehaviour
{
    [SerializeField] private IntEventChannelSO eventChannel;

    private void OnEnable()
    {
        eventChannel.OnRaised += OnEventRaised;
    }
    private void OnDisable()
    {
        eventChannel.OnRaised -= OnEventRaised;
    }

    private void OnEventRaised(int value1)
    {
        Debug.Log($"event is received : {value1}");
    }

}
