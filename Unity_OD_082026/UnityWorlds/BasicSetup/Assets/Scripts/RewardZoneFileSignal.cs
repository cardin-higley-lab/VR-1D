using UnityEngine;
using System.IO;

public class RewardZoneFileSignals : MonoBehaviour
{
    [Header("Full folder path (absolute)")]
    public string targetFolder = @"D:\Results\UnityData\RewardSignals";

    [Header("File name to create")]
    public string fileName = "reward_signal.txt";

    private void OnTriggerEnter(Collider other)
    {
        // Vérifie que c'est bien le joueur
        if (!other.CompareTag("Player")) return;

        string fullPath = Path.Combine(targetFolder, fileName);

        try
        {
            // Assure que le folder existe
            if (!Directory.Exists(targetFolder))
                Directory.CreateDirectory(targetFolder);

            // Crée un fichier vide (overwrite si existe)
            using (FileStream fs = File.Create(fullPath))
            {
                // ne rien écrire -> fichier vide
            }

            Debug.Log("Reward file created at: " + fullPath);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error creating reward file: " + e.Message);
        }
    }
}
