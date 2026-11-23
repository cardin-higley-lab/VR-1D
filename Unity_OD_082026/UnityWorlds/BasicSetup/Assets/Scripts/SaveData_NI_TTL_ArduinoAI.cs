using UnityEngine;
using System.IO;
using System.Text;
using System.IO.Ports;
using System.Runtime.InteropServices;

public class SaveData_NI_TTL_ArduinoAI : MonoBehaviour
{
    // ========== NI (TTL OUTPUT) ==========
    public string device = "Dev1";
    public string line = "port0/line0";
    public string logFolder = @"D:\Results\UnityData\Logs";

    [DllImport("nicaiu")]
    private static extern int DAQmxCreateTask(byte[] taskName, ref ulong taskHandle);

    [DllImport("nicaiu")]
    private static extern int DAQmxCreateDOChan(
        ulong taskHandle, byte[] lines, byte[] names, int grouping);

    [DllImport("nicaiu")]
    private static extern int DAQmxStartTask(ulong taskHandle);

    [DllImport("nicaiu")]
    private static extern int DAQmxWriteDigitalLines(
        ulong taskHandle, int samples, bool autoStart, double timeout,
        bool layout, byte[] data, ref int written, System.IntPtr reserved);

    [DllImport("nicaiu")]
    private static extern int DAQmxStopTask(ulong taskHandle);

    [DllImport("nicaiu")]
    private static extern int DAQmxClearTask(ulong taskHandle);

    private ulong task = 0;
    private byte[] HIGH = new byte[] { 1 };
    private byte[] LOW = new byte[] { 0 };
    private int written = 0;

    private int frameCount = 0;
    private StringBuilder log;
    private string file;


    // ========== ARDUINO (ANALOG IN) ==========
    public string arduinoPort = "COM10";
    public int baudRate = 115200;

    private SerialPort serialArduino;
    private int lastAnalogValue = 0;


    // ========== SPEED MEMORY ==========
    private float lastZ = 0f;
    private float lastT = 0f;



    void Awake()
    {
        Application.runInBackground = true;
        Time.fixedDeltaTime = 1f / 60f;
        QualitySettings.vSyncCount = 0;
    }



    void Start()
    {
        // ---- LOG ----
        Directory.CreateDirectory(logFolder);
        file = Path.Combine(logFolder,
            "log_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv");

        log = new StringBuilder();
        log.AppendLine("frameIdx,time,z,speedRaw,lickArduino");


        // ---- ARDUINO ----
        try
        {
            serialArduino = new SerialPort(arduinoPort, baudRate);
            serialArduino.ReadTimeout = 5;
            serialArduino.Open();
            Debug.Log("Arduino connected on " + arduinoPort);
        }
        catch
        {
            Debug.LogError("FAILED TO OPEN ARDUINO PORT " + arduinoPort);
        }


        // ---- NI ----
        byte[] tname = Encoding.ASCII.GetBytes("unityTTL\0");
        if (DAQmxCreateTask(tname, ref task) != 0)
            Debug.LogError("Error: CreateTask");

        string phys = device + "/" + line;
        byte[] lines = Encoding.ASCII.GetBytes(phys + "\0");
        byte[] names = Encoding.ASCII.GetBytes("\0");

        if (DAQmxCreateDOChan(task, lines, names, 0) != 0)
            Debug.LogError("Error: CreateDOChan");

        if (DAQmxStartTask(task) != 0)
            Debug.LogError("Error: StartTask");

        Debug.Log("TTL OUTPUT READY on " + phys);
    }



    void FixedUpdate()
    {
        if (task == 0) return;

        float t = Time.unscaledTime;
        float z = transform.position.z;

        // ===== READ ARDUINO =====
        if (serialArduino != null && serialArduino.IsOpen && serialArduino.BytesToRead > 0)
        {
            try
            {
                string s = serialArduino.ReadLine();
                lastAnalogValue = int.Parse(s);   // 0–1023
            }
            catch { }
        }

        // ===== SPEED RAW =====
        float dt = t - lastT;
        float dz = z - lastZ;
        float speedRaw = 0f;

        if (dt > 0f) speedRaw = dz / dt;

        lastT = t;
        lastZ = z;

        // ===== LOG =====
        log.AppendFormat("{0},{1:F6},{2:F6},{3:F6},{4}\n",
            frameCount, t, z, speedRaw, lastAnalogValue);

        // ===== TTL PULSE =====
        DAQmxWriteDigitalLines(task, 1, true, 10.0, false,
            HIGH, ref written, System.IntPtr.Zero);

        DAQmxWriteDigitalLines(task, 1, true, 10.0, false,
            LOW, ref written, System.IntPtr.Zero);

        frameCount++;
    }



    void OnApplicationQuit()
    {
        File.WriteAllText(file, log.ToString(), Encoding.UTF8);

        if (serialArduino != null && serialArduino.IsOpen)
            serialArduino.Close();

        if (task != 0)
        {
            DAQmxStopTask(task);
            DAQmxClearTask(task);
        }
    }
}
