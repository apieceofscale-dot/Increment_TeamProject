using UnityEngine;

public class PersistentManagerRoot : MonoBehaviour
{
    private static PersistentManagerRoot instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
