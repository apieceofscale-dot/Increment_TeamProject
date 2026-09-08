using UnityEngine;

public class AgentTest : MonoBehaviour
{
    [SerializeField] Transform player;
    Navi2DAgent agent;

    private void Awake()
    {
        agent = GetComponent<Navi2DAgent>();
    }
   
    // Update is called once per frame
    void Update()
    {
        agent.Trace(player.position);
    }
}
