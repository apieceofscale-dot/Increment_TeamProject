using UnityEngine;
using System;
using System.Linq;

[DefaultExecutionOrder(-10000)]
public class BootStrapper : MonoBehaviour
{
    /// <summary>
    /// 부트스트랩 시퀀스 종료 체크 여부, 외부 참조 가능
    /// </summary>
    public bool IsBootCompleted { get; private set; }

    private void Awake()
    {
        // 중복 생성 방지
        RunBootSequence();
    }

    private void RunBootSequence()
    {
        IBootStrapper[] targets = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .OfType<IBootStrapper>()
            .OrderBy(target => target.BootOrder)
            .ToArray();

        BootstrapContext context = new BootstrapContext(targets);


        if (!RunPhase(targets, "의존성 주입", target => target.IBootStrapperInject(context))) return;
        if (!RunPhase(targets, "초기화", target => target.IBootStrapperInitialize())) return;

        IsBootCompleted = true;
        Debug.Log($"[BootStrapper] 전체 {targets.Length}개 초기화 완료");
    }

    private bool RunPhase(IBootStrapper[] targets, string phaseName, Action<IBootStrapper> phase)
    {
        foreach (IBootStrapper target in targets)
        {
            try
            {
                phase(target);
            }
            catch (Exception e)
            {
                Debug.LogError($"[BootStrapper] {target.GetType().Name} {phaseName}실패. 부트 시퀀스를 중단합니다.", target as UnityEngine.Object);
                Debug.LogException(e, target as UnityEngine.Object);
                return false;
            }
            Debug.Log($"[BootStrapper] {target.GetType().Name} {phaseName} 완료");

        }
        return true;
    }



}
