using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.IO.Ports;
using System.Globalization;

public class ArduinoVRLogger : MonoBehaviour
{
    [Header("Arduino")]
    public string arduinoPort = "COM10";
    public int baudRate = 115200;

    [Header("Save")]
    public string logFolder = @"D:\Results\UnityData\Logs";

    [Header("Movement")]
    public float distanceScale = 1.0f;

    private SerialPort serial;

    private StreamWriter behaviorWriter;
    private StreamWriter syncWriter;

    private StringBuilder serialBuffer = new StringBuilder();

    private int frameIdx = 0;

    private bool originSet = false;
    private float originArduinoDistance = 0f;
    private float startZ = 0f;

    private float lastUnityZ = 0f;
    private float lastUnityT = 0f;

    private float latestArduinoTime = 0f;
    private float latestArduinoDistance = 0f;
    private float latestArduinoSpeed = 0f;

    private int latestSyncID = 0;
    private int latestSyncState = 0;
    private int latestLickState = 0;

    private int previousSyncID = 0;
    private int missedSyncCount = 0;

    void Awake()
    {
        Application.runInBackground = true;

        Time.fixedDeltaTime = 1f / 60f;
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;

        startZ = transform.position.z;
    }

    void Start()
    {
        Directory.CreateDirectory(logFolder);

        string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        string behaviorFile = Path.Combine(logFolder, "Behavior60Hz_" + stamp + ".csv");
        string syncFile = Path.Combine(logFolder, "SyncEvents_" + stamp + ".csv");

        behaviorWriter = new StreamWriter(behaviorFile, false, Encoding.UTF8);
        syncWriter = new StreamWriter(syncFile, false, Encoding.UTF8);

        behaviorWriter.WriteLine(
            "frame,unityTime,z,speedUnity,arduinoTime,arduinoDistance,arduinoSpeed,lastSyncID,syncState,lickState,missedSyncCount");

        syncWriter.WriteLine(
            "syncID,unityTimeReceived,arduinoSyncTime,missedBeforeThisPulse");

        behaviorWriter.AutoFlush = false;
        syncWriter.AutoFlush = false;

        try
        {
            serial = new SerialPort(arduinoPort, baudRate);
            serial.NewLine = "\n";
            serial.ReadTimeout = 1;
            serial.DtrEnable = true;

            serial.Open();
            serial.DiscardInBuffer();
            serial.DiscardOutBuffer();

            Debug.Log("Arduino connected on " + arduinoPort);
        }
        catch (Exception e)
        {
            Debug.LogError("Arduino connection failed: " + e.Message);
        }

        lastUnityZ = transform.position.z;
        lastUnityT = Time.unscaledTime;
    }

    void Update()
    {
        ReadArduinoSerial();
    }

    void FixedUpdate()
    {
        float unityTime = Time.unscaledTime;
        float z = transform.position.z;

        float dt = unityTime - lastUnityT;
        float speedUnity = 0f;

        if (dt > 0f)
        {
            speedUnity = (z - lastUnityZ) / dt;
        }

        behaviorWriter.WriteLine(
            frameIdx.ToString() + "," +
            unityTime.ToString("F6", CultureInfo.InvariantCulture) + "," +
            z.ToString("F6", CultureInfo.InvariantCulture) + "," +
            speedUnity.ToString("F6", CultureInfo.InvariantCulture) + "," +
            latestArduinoTime.ToString("F6", CultureInfo.InvariantCulture) + "," +
            latestArduinoDistance.ToString("F6", CultureInfo.InvariantCulture) + "," +
            latestArduinoSpeed.ToString("F6", CultureInfo.InvariantCulture) + "," +
            latestSyncID.ToString() + "," +
            latestSyncState.ToString() + "," +
            latestLickState.ToString() + "," +
            missedSyncCount.ToString());

        if (frameIdx % 300 == 0)
        {
            behaviorWriter.Flush();
            syncWriter.Flush();
        }

        lastUnityT = unityTime;
        lastUnityZ = z;
        frameIdx++;
    }

    void ReadArduinoSerial()
    {
        if (serial == null || !serial.IsOpen) return;

        try
        {
            string incoming = serial.ReadExisting();

            if (string.IsNullOrEmpty(incoming))
            {
                return;
            }

            serialBuffer.Append(incoming);

            while (true)
            {
                string bufferString = serialBuffer.ToString();
                int newlineIndex = bufferString.IndexOf('\n');

                if (newlineIndex < 0)
                {
                    break;
                }

                string line = bufferString.Substring(0, newlineIndex).Trim();
                serialBuffer.Remove(0, newlineIndex + 1);

                if (line.Length == 0)
                {
                    continue;
                }

                string[] parts = line.Split(',');

                if (parts.Length < 1)
                {
                    continue;
                }

                if (parts[0] == "POS")
                {
                    ParsePositionLine(parts);
                }
                else if (parts[0] == "SYNC")
                {
                    ParseSyncLine(parts);
                }
                else
                {
                    Debug.LogWarning("Unknown Arduino line: " + line);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Serial read error: " + e.Message);
        }
    }

    void ParsePositionLine(string[] parts)
    {
        // Expected:
        // POS,arduinoTime,distance,speed,lastSyncID,syncState,lickState

        if (parts.Length < 7)
        {
            Debug.LogWarning("Bad POS line, not enough fields: " + string.Join("|", parts));
            return;
        }

        bool okTime = float.TryParse(
            parts[1],
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out float arduinoTime);

        bool okDistance = float.TryParse(
            parts[2],
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out float arduinoDistance);

        bool okSpeed = float.TryParse(
            parts[3],
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out float arduinoSpeed);

        bool okSyncID = int.TryParse(parts[4], out int syncID);
        bool okSyncState = int.TryParse(parts[5], out int syncState);
        bool okLickState = int.TryParse(parts[6], out int lickState);

        if (!okTime || !okDistance || !okSpeed || !okSyncID || !okSyncState || !okLickState)
        {
            Debug.LogWarning(
                "Failed to parse POS line: " +
                string.Join("|", parts));
            return;
        }

        latestArduinoTime = arduinoTime;
        latestArduinoDistance = arduinoDistance;
        latestArduinoSpeed = arduinoSpeed;

        latestSyncID = syncID;
        latestSyncState = syncState;
        latestLickState = lickState;

        if (!originSet)
        {
            originArduinoDistance = arduinoDistance;
            startZ = transform.position.z;
            originSet = true;
            return;
        }

        float dz = (arduinoDistance - originArduinoDistance) * distanceScale;

        Vector3 pos = transform.position;
        pos.z = startZ + dz;
        transform.position = pos;
    }

    void ParseSyncLine(string[] parts)
    {
        // Expected:
        // SYNC,syncID,arduinoSyncTime

        if (parts.Length < 3)
        {
            Debug.LogWarning("Bad SYNC line, not enough fields: " + string.Join("|", parts));
            return;
        }

        bool okID = int.TryParse(parts[1], out int syncID);

        bool okTime = float.TryParse(
            parts[2],
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out float arduinoSyncTime);

        if (!okID || !okTime)
        {
            Debug.LogWarning(
                "Failed to parse SYNC line: " +
                string.Join("|", parts));
            return;
        }

        int missedBeforeThisPulse = 0;

        if (previousSyncID > 0 && syncID != previousSyncID + 1)
        {
            missedBeforeThisPulse = syncID - previousSyncID - 1;

            if (missedBeforeThisPulse > 0)
            {
                missedSyncCount += missedBeforeThisPulse;

                Debug.LogWarning(
                    "Missed SYNC IDs. Previous = " +
                    previousSyncID +
                    ", current = " +
                    syncID +
                    ", missed = " +
                    missedBeforeThisPulse);
            }
        }

        previousSyncID = syncID;

        float unityTimeReceived = Time.unscaledTime;

        latestSyncID = syncID;

        syncWriter.WriteLine(
            syncID.ToString() + "," +
            unityTimeReceived.ToString("F6", CultureInfo.InvariantCulture) + "," +
            arduinoSyncTime.ToString("F6", CultureInfo.InvariantCulture) + "," +
            missedBeforeThisPulse.ToString());
    }

    void OnApplicationQuit()
    {
        if (behaviorWriter != null)
        {
            behaviorWriter.Flush();
            behaviorWriter.Close();
        }

        if (syncWriter != null)
        {
            syncWriter.Flush();
            syncWriter.Close();
        }

        if (serial != null && serial.IsOpen)
        {
            serial.Close();
        }

        Debug.Log("Behavior and sync CSV saved. Total missed sync IDs detected by Unity = " + missedSyncCount);
    }
}