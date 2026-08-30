using UnityEngine;
using System;
using System.Collections.Generic;

[DefaultExecutionOrder(-10000)]
public class BootStrapper : MonoBehaviour
{
    [Header("ÃÊ±âÈ­ ¸Å´ÏÀú ¸ñ·Ï")]
    [Tooltip("ÀÇÁ¸¼º ±ÔÄ¢ ¸ÂÃç À§¿¡¼­ºÎÅÍ µî·Ï ¿ä¸Á")]
    [SerializeField]
    private List<MonoBehaviour> bootTargets = new List<MonoBehaviour>();

    // ÃÊ±âÈ­ ¼º°ø ¸Å´ÏÀúµé
    private readonly List<IBootStrapper> initializedTargets = new List<IBootStrapper>();

    /// <summary>
    /// ºÎÆ®½ºÆ®·¦ ½ÃÄö½º Á¾·á Ã¼Å© ¿©ºÎ, ¿ÜºÎ ÂüÁ¶ °¡´É
    /// </summary>
    public bool IsBootCompleted { get; private set; }

    // Áßº¹ È£Ãâ °¨Áö
    private bool currentStepCallbackInvoked;


    private void Awake()
    {
        // Áßº¹ Á¦°Å
        if (FindObjectsByType<BootStrapper>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        InitializeStep(0);
    }

    /// <summary>
    /// ¸Å´ÏÀú ÃÊ±âÈ­ ½ÃÀÛ, Äİ¹é È£Ãâ ½ÃÁ¡¿¡ ´ÙÀ½ ¸Å´ÏÀú·Î ³Ñ¾î°¨
    /// </summary>
    /// <param name="index"></param>
    private void InitializeStep(int index)
    {
        // ÀüºÎ ³¡³ª¸é completed true·Î ¸¸µé°í Á¾·á
        if (index >= bootTargets.Count)
        {
            IsBootCompleted = true;
            Debug.Log($"[BootStrapper] ÀüÃ¼ {initializedTargets.Count}°³ ÃÊ±âÈ­ ¿Ï·á");
            return;
        }

        MonoBehaviour target = bootTargets[index];

        // ºó ÀÎ½ºÆåÅÍ ½½·Ô °Ç³Ê¶Ù±â
        if (target == null)
        {
            Debug.LogError($"[BootStrapper] {index}ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿? ï¿½ï¿½ï¿½ï¿½");
            InitializeStep(index + 1);
            return;
        }

        // ÀÎÅÍÆäÀÌ½º ±¸Çö ¾È µÈ ÄÉÀÌ½º °Ç³Ê¶Ù±â
        if (target is not IBootStrapper bootStrapper)
        {
            Debug.LogError($"[BootStrapper] '{target.name}'ï¿½ï¿½ IBootStrapperï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½Ê¾Ò½ï¿½ï¿½Ï´ï¿½. ï¿½Ê±ï¿½È­ï¿½ï¿½ ï¿½Ç³Ê¶İ´Ï´ï¿½.");
            InitializeStep(index + 1);
            return;
        }

        currentStepCallbackInvoked = false;

        BootstrapContext context = new BootstrapContext(
            onStepCompleted: () => HandleStepCompleted(target, bootStrapper, index)
        );

        try
        {
            bootStrapper.IBootStrapperInitialize(context);
        }
        catch
        {
            Debug.LogError("ÃÊ±âÈ­ Áß ¿¹¿Ü ¹ß»ıÇÏ¿© Áß´Ü");
            return;
        }

    }
    private void HandleStepCompleted(MonoBehaviour target, IBootStrapper bootStrapper, int index)
    {
        if (currentStepCallbackInvoked)
        {
            Debug.Log("µÎ ¹ø ÀÌ»ó È£ÃâÇß½À´Ï´Ù. È®ÀÎ ÇÊ¿ä");
            return;
        }
        currentStepCallbackInvoked = true;

        initializedTargets.Add(bootStrapper);
        Debug.Log($"[BootStrapper] ({index + 1}/{bootTargets.Count}) ÃÊ±âÈ­ ¿Ï·á");


        InitializeStep(index + 1);
    }

}
