using UnityEngine;
using System.Runtime.InteropServices;
using System.Text;

public class TestNI : MonoBehaviour
{
    [DllImport("nicaiu")]
    private static extern int DAQmxResetDevice(byte[] deviceName);

    void Start()
    {
        byte[] dev = Encoding.ASCII.GetBytes("Dev1\0");
        int status = DAQmxResetDevice(dev);
        Debug.Log("NI status = " + status);
    }
}
