using UnityEngine;
using System.IO;
using System.Text;

public class SaveData_FixedFrame : MonoBehaviour
{
    private StringBuilder csvBuilder;
    private float lastZ = 0f;
    private float speed = 0f;

    private float dt = 0.02f;        // 고정된 dt
    private string filePath;

    void Start()
    {
        string folder = @"D:\Results\UnityData\Logs";
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        filePath = folder + "/log_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";

        csvBuilder = new StringBuilder();
        csvBuilder.AppendLine("time,dt,speed,position_z");

        lastZ = transform.position.z;
    }

    void FixedUpdate()
    {
        float now = Time.time;               // 실제 시간
        float currentZ = transform.position.z;

        speed = (currentZ - lastZ) / dt;     // 고정 dt 기반 속도 계산

        csvBuilder.AppendLine(
            now.ToString("F6") + "," +
            dt.ToString("F6") + "," +
            speed.ToString("F6") + "," +
            currentZ.ToString("F6")
        );

        lastZ = currentZ;
    }

    void OnApplicationQuit()
    {
        File.WriteAllText(filePath, csvBuilder.ToString());
        Debug.Log("CSV saved to: " + filePath);
    }
}
