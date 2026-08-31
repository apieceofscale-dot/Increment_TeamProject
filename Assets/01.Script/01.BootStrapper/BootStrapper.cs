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
        if (FindObjectsByType<BootStrapper>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        RunBootSequence();
    }

    private void RunBootSequence()
    {
        IBootStrapper[] targets = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .OfType<IBootStrapper>()
            .OrderBy(target => target.BootOrder)
            .ToArray();

        foreach (var target in targets)
        {
            try
            {
                target.IBootStrapperInitialize();
            }
            catch (Exception e)
            {
                // 두 번째 인자로 target을 넘기면 콘솔에서 클릭 시 해당 오브젝트가 선택된다.
                Debug.LogError($"[BootStrapper] {target.GetType().Name} 초기화 실패. 부트 시퀀스를 중단합니다.", target as UnityEngine.Object);
                Debug.LogException(e, target as UnityEngine.Object);
                return;
            }
            Debug.Log($"[BootStrapper] {target.GetType().Name} 초기화 완료");

        }
        IsBootCompleted = true;
        Debug.Log($"[BootStrapper] 전체 {targets.Length}개 초기화 완료");
    }



}
