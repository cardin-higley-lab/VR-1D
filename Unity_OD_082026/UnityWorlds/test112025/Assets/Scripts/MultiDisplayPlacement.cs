using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class MultiDisplayPlacement : MonoBehaviour
{
    void Awake()
    {
        Application.runInBackground = true;

        // if (Display.displays.Length > 1)
        //    Display.displays[1].Activate();

        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate();
            foreach (var cam in Camera.allCameras)
                cam.targetDisplay = 1;
        }
        else
        {
            foreach (var cam in Camera.allCameras)
                cam.targetDisplay = 0;
}
        var infos = new List<DisplayInfo>();
        Screen.GetDisplayLayout(infos);
        
        if (infos.Count > 1)
        {
            DisplayInfo target = infos[Mathf.Min(1, infos.Count - 1)];
            Screen.MoveMainWindowTo(target, Vector2Int.zero);
        }

        foreach (var cam in Camera.allCameras) cam.targetDisplay = 1;
        foreach (var canvas in FindObjectsOfType<Canvas>()) canvas.targetDisplay = 1;

    }

}
