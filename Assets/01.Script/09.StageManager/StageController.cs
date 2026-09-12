using UnityEngine;
using System;

public class StageController : MonoBehaviour
{

    private MonsterFacade monsterFacade;
    private StageFactory stageFactory;
    private MonsterSpawner spawner;
    private StageProgressProvider progressProvider;


    private readonly StageStatus status = new StageStatus();

    private MonsterController activeElite;


    public StageProgressInfo Progress => status.ToInfo();
    public EliteProgressInfo EliteProgress => status.ToEliteInfo();


    public void Initialize(MonsterFacade facade, StageFactory factory, StageProgressProvider provider)
    {
        monsterFacade = facade;
        stageFactory = factory;
        progressProvider = provider;

        spawner = GetComponent<MonsterSpawner>();

        if (spawner == null)
            throw new InvalidOperationException("[StageController] Don't have MonsterSpawner in this object.");
        spawner.Initialize(facade);

        enabled = false;

    }


    public void StartStage(int stageId)
    {


        spawner.DespawnAll();
        activeElite = null;

    }
}
