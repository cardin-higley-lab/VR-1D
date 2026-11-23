using UnityEngine;

[ExecuteInEditMode]
public class FlipCameraVertical : MonoBehaviour
{
    private Camera cam;
    private Matrix4x4 originalMatrix;
    private bool flipped = false;

    void Start()
    {
        cam = GetComponent<Camera>();
        originalMatrix = cam.projectionMatrix;
        Flip();
    }

    void Flip()
    {
        Matrix4x4 mat = cam.projectionMatrix;
        mat *= Matrix4x4.Scale(new Vector3(1, -1, 1)); // Y축 반전
        cam.projectionMatrix = mat;
        flipped = true;
    }

    void OnDisable()
    {
        // 원래대로 복원
        if (flipped && cam != null)
        {
            cam.projectionMatrix = originalMatrix;
        }
    }
}
