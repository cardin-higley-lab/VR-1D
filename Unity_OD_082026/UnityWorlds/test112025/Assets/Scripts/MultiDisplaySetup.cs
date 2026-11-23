using UnityEngine;

public class MultiDisplaySetup : MonoBehaviour
{
    public Camera mainCamera; // The camera for the game

    void Start()
    {
        if (Display.displays.Length > 1)
        {
            // Activate second display
            Display.displays[1].Activate();

            // Move the main camera to Display 1
            mainCamera.targetDisplay = 1;

            // Find all other cameras on Display 0 and disable them
            Camera[] allCameras = Camera.allCameras;
            foreach (Camera cam in allCameras)
            {
                if (cam != mainCamera)
                    cam.gameObject.SetActive(false); // deactivate them
            }

            // Optional: force Display 0 background to black
            Screen.SetResolution(Display.displays[1].renderingWidth, Display.displays[1].renderingHeight, true);
        }
    }
}
