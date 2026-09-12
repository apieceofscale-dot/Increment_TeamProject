using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public sealed class StageFactory : MonoBehaviour, IBootStrapper
{

    public int BootOrder => (int)BootLayer.Factory;

    // Fix after stage prefab finished
    private const string GroundPath = "Grid/Ground";
    private const string BackgroundPath = "Background/Sprite";
    private const string BgmPath = "Bgm";
    private const string PlayerStartPath = "PlayerStart";
    private const string SpawnPointsPath = "MonsterSpawnPoints";
    private const string BossSpawnPath = "BossSpawnPoint";


    private DataManager dataManager;

    // for check reuse
    private Transform currentMap;
    private string currentMapPath;
    private StageMapParts currentParts;


    public void IBootStrapperInject(BootstrapContext context)
    {
        dataManager = context.Get<DataManager>();
    }

    public void IBootStrapperInitialize()
    {
        // It do not create map
        // 'StageFacade' selects what stage to start
    }

    public bool Create(int stageId, out StageBuildResult result)
    {
        result = default;

        //blabla

        return true;
    }
}
