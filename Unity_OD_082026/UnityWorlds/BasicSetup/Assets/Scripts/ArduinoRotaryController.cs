using UnityEngine;
using System.IO.Ports;
using System.Globalization;

public class ArduinoRotaryController : MonoBehaviour
{
    private SerialPort serial;
    private float originDistance = 0f; // first received distance (zero reference)
    private bool originSet = false;    // whether origin was captured
    private float startZ = 0f;         // world-space Z used as offset base

    void Awake()
    {
        // capture scene start position so we don't snap to 0
        startZ = transform.position.z;
    }

    void Start()
    {
        try
        {
            serial = new SerialPort("COM10", 115200); // Arduino port and baud rate
            serial.NewLine = "\n";                  // read line by line
            serial.ReadTimeout = 50;                // read timeout (ms)
            serial.DtrEnable = true;               // avoid Arduino auto-reset
            serial.Open();

            // clear any stale bytes so first read isn't a big jump
            serial.DiscardInBuffer();
            serial.DiscardOutBuffer();
        }
        catch (System.UnauthorizedAccessException ex) { Debug.LogError("COM in use: " + ex.Message); }
        catch (System.IO.IOException ex)              { Debug.LogError("COM IO error: " + ex.Message); }
        catch (System.Exception ex)
        {
            Debug.LogError($"Serial error: {ex.Message}");
        }
    }

    void Update()
    {
        if (serial == null || !serial.IsOpen) return;

        try
        {
            if (serial.BytesToRead > 0)
            {
                string line = serial.ReadLine().Trim();  // e.g. "time,distance,speed"
                string[] parts = line.Split(',');

                if (parts.Length >= 2 &&
                    float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float distance))
                {
                    if (!originSet)
                    {
                        // set zero reference and also lock the current world Z as offset
                        originDistance = distance;
                        startZ = transform.position.z;   // << crucial line
                        originSet = true;
                        return; // do not move on this very first read
                    }

                    // relative distance since origin
                    float dz = distance - originDistance;

                    // apply keeping the initial offset
                    Vector3 pos = transform.position;
                    pos.z = startZ + dz;
                    transform.position = pos;
                }
            }
        }
        catch { /* ignore sporadic read/timeout */ }
    }

    void OnApplicationQuit()
    {
        if (serial != null && serial.IsOpen) serial.Close(); // close serial on exit
    }
}
