using UnityEngine;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

public class SaveData_NI_TTL : MonoBehaviour
{
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
    private byte[] LOW  = new byte[] { 0 };
    private int written = 0;

    private int frameCount = 0;
    private StringBuilder log;
    private string file;

    void Awake()
    {
        Application.runInBackground = true;
        Time.fixedDeltaTime = 1f / 60f;     // FRAME = 60 Hz EXACT
        QualitySettings.vSyncCount = 0;
    }

    void Start()
    {
        // ---- LOG ----
        Directory.CreateDirectory(logFolder);
        file = Path.Combine(logFolder,
            "log_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv");

        log = new StringBuilder();
        log.AppendLine("frameIdx,time,z,speed");

        // ---- NI TASK ----
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

        // LOG
        log.AppendFormat("{0},{1:F6},{2:F6},{3:F6}\n",
            frameCount, t, z, 0f);

        // ---- TTL for THIS frame ----
        // HIGH toute la frame
        DAQmxWriteDigitalLines(task, 1, true, 10.0, false, HIGH, ref written, System.IntPtr.Zero);

        // Fin de frame = LOW
        // (ne dure que quelques microsecondes mais on s'en fout)
        DAQmxWriteDigitalLines(task, 1, true, 10.0, false, LOW, ref written, System.IntPtr.Zero);

        frameCount++;
    }

    void OnApplicationQuit()
    {
        File.WriteAllText(file, log.ToString(), Encoding.UTF8);

        if (task != 0)
        {
            DAQmxStopTask(task);
            DAQmxClearTask(task);
        }
    }
}
