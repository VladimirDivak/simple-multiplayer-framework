using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DVA.SimpleMultiplayerFramework;
using Cysharp.Threading.Tasks;
using DG.Tweening;

public sealed class Test : MonoBehaviour
{
    private async UniTaskVoid Start()
    {
        while (true)
        {
            await transform.DOMoveZ(5, 3).SetRelative().SetEase(Ease.Linear);
            await transform.DOMoveX(5, 3).SetRelative().SetEase(Ease.Linear);
            await transform.DOMoveZ(-5, 3).SetRelative().SetEase(Ease.Linear);
            await transform.DOMoveX(-5, 3).SetRelative().SetEase(Ease.Linear);
        }
    }
}
