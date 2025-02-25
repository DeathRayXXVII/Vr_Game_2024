using System.IO;
using UnityEditor;
using UnityEngine;

public class SceneSnapshot
{
    #if UNITY_EDITOR
    [MenuItem("Tools/Capture Scene")]
    static void CaptureScene()
    {
        Camera camera = Camera.current; // Use the main camera or specify another camera

        if (camera == null)
        {
            Debug.LogError("No camera found!");
            return;
        }

        int width = 256;
        int height = 256;

        Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false);

        RenderTexture renderTexture = new RenderTexture(width, height, 24);
        camera.targetTexture = renderTexture;
        camera.Render();

        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture.Apply();

        camera.targetTexture = null;
        RenderTexture.active = null;
        //DestroyImmediate(renderTexture);

        byte[] bytes = texture.EncodeToJPG();
        string fileName = "SceneViewSnapshot.jpg";
        File.WriteAllBytes(Application.dataPath + "/" + fileName, bytes);

        Debug.Log("Scene snapshot saved to " + fileName);
    }
    #endif
}