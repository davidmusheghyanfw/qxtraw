using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class threadtest : MonoBehaviour
{
    // private void Start()
    // {
    //     // Simulate background work
    //     UniTask.Run(async () =>
    //     {
    //         await UniTask.Delay(1000); // Simulate delay in milliseconds
    //
    //         UnityMainThreadDispatcher.Instance().AddJob(() =>
    //         {
    //             // This code runs on the main thread
    //             float balance = PlayerPrefs.GetFloat("Balance_", 0f);
    //             Debug.LogError($"Balance: {balance}");
    //         });
    //     });
    // }
}
