using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class SingleDisplaySetup : MonoBehaviour
{
    void Awake()
    {
        Application.runInBackground = true;

        // 🔹 모든 카메라와 UI를 기본 디스플레이(0)로 설정
        foreach (var cam in Camera.allCameras)
            cam.targetDisplay = 0;

        foreach (var canvas in FindObjectsOfType<Canvas>())
            canvas.targetDisplay = 0;

        // 🔹 보조 디스플레이 관련 코드 제거
        // Display.displays[1].Activate() 등 호출 금지
    }
}
