using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class MakeTielImage : MonoBehaviour
{
    public int tileSum;
    public GameObject tileImagePrehub;
    public CoodinateMatching[] cm;
    public Texture2D finalImage;
    public bool createTileImage = false;
    public bool makeTileImgae = false;
    public int dotFrequency = 4;
    public Vector2Int resolution = new Vector2Int(1440, 1440);
    public Camera rendCam;
    // Start is called before the first frame update
    void Start()
    {
        SetUp();
    }

    // Update is called once per frame
    void Update()
    {
        if (createTileImage)
        {
            CreateTileImages();
            createTileImage = false;
        }
        if(makeTileImgae)
        {
            MakeTileImage();
            makeTileImgae = false;
        }

    }

    void SetUp()
    {
        cm = new CoodinateMatching[tileSum];
    }

    void CreateTileImages()
    {
        for (int i = 0; i < tileSum; i++)
        {
            GameObject a = Instantiate(tileImagePrehub,
                       Vector3.zero,
                       Quaternion.identity);
            cm[i] = a.GetComponent<CoodinateMatching>();
            cm[i].tileNum = i;
            //cm[i].tileNum = 1; //テスト用
            cm[i].finImage = finalImage;
            cm[i].dotFrequency = dotFrequency;

        }
    }

     void MakeTileImage()
    {
        RenderTexture rt = new RenderTexture(resolution.x, resolution.y, 24);
        rt.antiAliasing = 8;
        rendCam.targetTexture = rt;
        Texture2D sS = new Texture2D(resolution.x, resolution.y, TextureFormat.RGB24, false);
        rendCam.Render();
        RenderTexture.active = rt;
        sS.ReadPixels(new Rect(0, 0, resolution.x, resolution.y), 0, 0);
        rendCam.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);
        byte[] pngData = sS.EncodeToPNG();
        string filePath = EditorUtility.SaveFilePanel("SaveImage", "/Users/inukaisatoru/Desktop", finalImage.name + "Tile", "png");
        Debug.Log(filePath);
        if (filePath.Length > 0)
        {
            File.WriteAllBytes(filePath, pngData);
        }
    }
}
