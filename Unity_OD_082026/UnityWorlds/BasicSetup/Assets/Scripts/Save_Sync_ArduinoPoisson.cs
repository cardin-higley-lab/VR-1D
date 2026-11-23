using UnityEngine;
using System.IO;
using System.Text;
using System.IO.Ports;

public class SaveData_ArduinoSync : MonoBehaviour
{
    [Header("Arduino")]
    public string arduinoPort = "COM10";
    public int baudRate = 115200;

    [Header("Save")]
    public string logFolder = @"D:\Results\UnityData\Logs";

    private SerialPort serialArduino;
    private int lastArduinoValue = 0;

    private StreamWriter writer;

    private int frameIdx = 0;

    private float lastZ;
    private float lastT;

    void Awake()
    {
        Application.runInBackground = true;

        Time.fixedDeltaTime = 1f / 60f;

        Application.targetFrameRate = 60;

        QualitySettings.vSyncCount = 0;
    }

    void Start()
    {
        Directory.CreateDirectory(logFolder);

        string fileName =
            Path.Combine(
                logFolder,
                "BehaviorSync_" +
                System.DateTime.Now.ToString("yyyyMMdd_HHmmss") +
                ".csv");

        writer = new StreamWriter(fileName, false, Encoding.UTF8);

        writer.WriteLine(
            "frame,time,z,speedRaw,arduinoValue");

        writer.AutoFlush = false;

        try
        {
            serialArduino =
                new SerialPort(
                    arduinoPort,
                    baudRate);

            serialArduino.ReadTimeout = 1;

            serialArduino.Open();

            Debug.Log("Arduino connected.");
        }
        catch (System.Exception e)
        {
            Debug.LogError(
                "Arduino connection failed: "
                + e.Message);
        }

        lastZ = transform.position.z;
        lastT = Time.unscaledTime;
    }

    void FixedUpdate()
    {
        float t = Time.unscaledTime;

        float z = transform.position.z;

        // Read newest Arduino sample
        if (serialArduino != null &&
            serialArduino.IsOpen)
        {
            try
            {
                while (serialArduino.BytesToRead > 0)
                {
                    string s = serialArduino.ReadLine();

                    lastArduinoValue =
                        int.Parse(s.Trim());
                }
            }
            catch
            {
            }
        }

        float dt = t - lastT;

        float speedRaw = 0f;

        if (dt > 0f)
        {
            speedRaw = (z - lastZ) / dt;
        }

        writer.WriteLine(
            frameIdx + "," +
            t.ToString("F6") + "," +
            z.ToString("F6") + "," +
            speedRaw.ToString("F6") + "," +
            lastArduinoValue);

        if (frameIdx % 300 == 0)
        {
            writer.Flush();
        }

        lastT = t;
        lastZ = z;

        frameIdx++;
    }

    void OnApplicationQuit()
    {
        if (writer != null)
        {
            writer.Flush();
            writer.Close();
        }

        if (serialArduino != null &&
            serialArduino.IsOpen)
        {
            serialArduino.Close();
        }

        Debug.Log("CSV saved.");
    }
}