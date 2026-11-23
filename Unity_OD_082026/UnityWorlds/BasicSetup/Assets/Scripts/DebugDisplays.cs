using UnityEngine;

public class DebugDisplays : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Number of displays detected : " + Display.displays.Length);
        for (int i = 0; i < Display.displays.Length; i++)
            Debug.Log("Display " + i + " : " + Display.displays[i].systemWidth + "x" + Display.displays[i].systemHeight);

        // activate displays
        for (int i = 1; i < Display.displays.Length; i++)
            Display.displays[i].Activate();
    }
}
