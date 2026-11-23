using UnityEngine;
using System.IO;
using System.Text;
using System.IO.Ports;

public class SaveData_FrameLocked : MonoBehaviour
{
    [Header("Frame sync")]
    public int frameHz = 60;             // desired FPS = desired TTL rate

    [Header("Arduino TTL")]
    public string portName = "COM9";
    public int baudRate = 115200;
    public int pulseWidthMs = 5;         // must be < 1000/frameHz (e.g., < 16.7ms at 60 Hz)

    private SerialPort serial;
    private StringBuilder csv;
    private string filePath;

    private float lastZ;
    private float lastTime;

    void Awake()
    {
        // Make FPS deterministic: disable VSync, cap target FPS.
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = Mathf.Max(1, frameHz);
    }

    void Start()
    {

  

        // File init
        string folder = @"D:\Results\UnityData\Logs";
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        filePath = Path.Combine(folder, "log_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv");

        csv = new StringBuilder();
        csv.AppendLine("time,dt,speed,position_z");

        lastZ = transform.position.z;
        lastTime = Time.unscaledTime;

        // Serial init
        try {
            serial = new SerialPort(portName, baudRate);
            serial.NewLine = "\n";
            serial.ReadTimeout = 1;
            serial.WriteTimeout = 1;
            serial.DtrEnable = false; // reduce auto-reset on some boards
            serial.RtsEnable = false;
            serial.Open();
            Debug.Log("Serial opened: " + portName);
        } catch (System.Exception e) {
            Debug.LogError("Serial open failed: " + e.Message);
        }
    }

    void Update()
    {
        // ---- One row per frame ----
        float now = Time.unscaledTime;
        float dt = now - lastTime; if (dt <= 0f) dt = 1e-6f;

        float z = transform.position.z;
        float speed = (z - lastZ) / dt;

        csv.AppendFormat("{0:F6},{1:F6},{2:F6},{3:F6}\n", now, dt, speed, z);

        // ---- One TTL per frame (right after saving) ----
        SendTtlPulse(pulseWidthMs);

        lastZ = z;
        lastTime = now;
    }

    private void SendTtlPulse(int durationMs)
    {
        if (serial == null || !serial.IsOpen) return;
        try { serial.Write("PULSE," + durationMs + "\n"); }
        catch (System.Exception e) { Debug.LogError("Serial write failed: " + e.Message); }
    }

    void OnApplicationQuit()
    {
        File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);
        if (serial != null && serial.IsOpen) serial.Close();
        Debug.Log("CSV saved to: " + filePath);
    }
}
