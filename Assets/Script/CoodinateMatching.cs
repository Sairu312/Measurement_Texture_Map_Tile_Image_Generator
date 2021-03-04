using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;

public class CoodinateMatching : MonoBehaviour
{
    //外部から値をもらう
    public int tileNum;
    public Texture2D finImage;
    public int dotFrequency = 4;

    public bool matchingStart = false;
    public bool coordinateOutputStart = false;
    public Vector2Int imageResolution;
    public Vector2Int displayResolution;
    public Texture2D dotTileImage;
    public int stripeImageNum = 5;
    public Texture2D[] stripeTileImagesX;
    public Texture2D[] stripeTileImagesY;
    public Texture2D[] stripeImagesX;
    public Texture2D[] stripeImagesY;
    public TextAsset coodinateFile;
    public List<string[]> csvDatas = new List<string[]>();
    List<Vector2Int> tileDotCooodinate = new List<Vector2Int>();
    public Vector2Int[] coodinateData;
    private bool[,] judgmentBitMesured;
    private bool[,] judgmentBitMesuredX;
    private bool[,] judgmentBitMesuredY;
    private int[] judgmentBitFlagMesured;
    private List<int[]> judgmentBitSortMesured = new List<int[]>();
    private bool[,] judgmentBitTile;
    private bool[,] judgmentBitTileX;
    private bool[,] judgmentBitTileY;
    private int[] judgmentBitFlagTile;
    private List<int[]> matchList = new List<int[]>();
    public GameObject dotObj;
    public GameObject matchObj;
    public GameObject firstObj;
    private float mag = 0.1f;

    //構造体
    public struct UVdata
    {
        public int index;
        public Vector3 tile;
        public Vector2 imaging;
    }


    // Start is called before the first frame update
    void Start()
    {
        //準備
        CSVRead();
        ReadStripeImages();
        DotImage2Coodinate();
        BitSetUp();
    }

    // Update is called once per frame
    void Update()
    {
        if(matchingStart)
        {
            //計測
            BitMatchingMesuredX();
            BitMatchingMesuredY();
            //タイル
            BitMatchingTileX();
            BitMatchingTileY();
            BitComposition();
            Chageflags();
            MesuredBitSort();
            Matching();
            matchingStart = false;
        }
        if(coordinateOutputStart)
        {
            
            //CoordinateOutput();
            //CoordinateOutputM();

            //MatchOutPut();
            //SoreCoodinate2Uvdata();
            MeshCreate();
            coordinateOutputStart = false;
        }
        
    }

    //準備

    void CSVRead()
    {
        coodinateFile = Resources.Load<TextAsset>("CSV/DotCoordinateFile" + tileNum);
        StringReader reader = new StringReader(coodinateFile.text);
        while(reader.Peek() != -1)
        {
            string line = reader.ReadLine();
            csvDatas.Add(line.Split(','));
        }
        coodinateData = new Vector2Int[csvDatas.Count];
        for(int i = 0; i < coodinateData.Length; i++)
        {
            coodinateData[i] = new Vector2Int(int.Parse(csvDatas[i][0]),int.Parse(csvDatas[i][1]));
        }
        //Debug.Log(coodinateData.Length);
    }

    //改修すべき
    void ReadStripeImages()
    {
        stripeImagesX = new Texture2D[stripeImageNum];
        stripeImagesY = new Texture2D[stripeImageNum];
        stripeTileImagesX = new Texture2D[stripeImageNum];
        stripeTileImagesY = new Texture2D[stripeImageNum];
        dotTileImage = Resources.Load<Texture2D>("TileImages/DotTileImages/1_" + dotFrequency + "/" + tileNum + "_mix_tilie");
        for(int i = 0; i < stripeImageNum; i++)
        {
            int num = (int)Mathf.Pow(2, stripeImageNum + 1 - i);
            stripeImagesX[i] = Resources.Load<Texture2D>("MesuredImages/HullImage/X/" + num + "/" + tileNum);
            stripeImagesY[i] = Resources.Load<Texture2D>("MesuredImages/HullImage/Y/" + num + "/" + tileNum);
            stripeTileImagesX[i] = Resources.Load<Texture2D>("TileImages/StripeTileImages/X/" + num + "/" + tileNum + "_X_" + num + "_1_" + dotFrequency + "_Stripe_tilie");
            stripeTileImagesY[i] = Resources.Load<Texture2D>("TileImages/StripeTileImages/Y/" + num + "/" + tileNum + "_Y_" + num + "_1_" + dotFrequency + "_Stripe_tilie");
        }
   
        imageResolution = new Vector2Int(stripeImagesX[0].width, stripeImagesX[0].height);
        displayResolution = new Vector2Int(stripeTileImagesX[0].width, stripeTileImagesX[0].height);
    }

    void BitSetUp()
    {
        judgmentBitMesured = new bool[csvDatas.Count, stripeImagesX.Length + stripeImagesY.Length];
        judgmentBitMesuredX = new bool[csvDatas.Count,stripeImagesX.Length];
        judgmentBitMesuredY = new bool[csvDatas.Count, stripeImagesY.Length];
        judgmentBitTile = new bool[tileDotCooodinate.Count, stripeTileImagesX.Length + stripeTileImagesY.Length];
        judgmentBitTileX = new bool[tileDotCooodinate.Count, stripeTileImagesX.Length];
        judgmentBitTileY = new bool[tileDotCooodinate.Count, stripeTileImagesY.Length];
        judgmentBitFlagMesured = new int[csvDatas.Count];
        judgmentBitFlagTile = new int[tileDotCooodinate.Count];
    }




    ///////////////////////////////////////////////
    //測定画像の処理
    //大きい周波数順に右側から並べる　(Ex)2,4,8,16,32,64→111111
    void BitMatchingMesuredX()
    {
        for(int dotNum=0; dotNum < coodinateData.Length; dotNum++)
        {
            for(int stripeNum = 0; stripeNum < stripeImagesX.Length; stripeNum++)
            {
                Color stripeColor = stripeImagesX[stripeNum].GetPixel(coodinateData[dotNum].x, coodinateData[dotNum].y);
                if(stripeColor.r  > 0.5)
                {
                    judgmentBitMesuredX[dotNum,stripeImagesX.Length - stripeNum - 1] = true;
                }
                else
                {
                    judgmentBitMesuredX[dotNum,stripeImagesX.Length - stripeNum -1] = false;
                }
            }
            //PrintJudgmentBit(dotNum, stripeImagesX.Length, judgmentBitMesuredX, nameof(judgmentBitMesuredX));
            //Debug.Log("M:" + coodinateData[dotNum].x + "," + coodinateData[dotNum].y);
        }
    }

    void BitMatchingMesuredY()
    {
        for (int dotNum = 0; dotNum < coodinateData.Length; dotNum++)
        {
            for (int stripeNum = 0; stripeNum < stripeImagesY.Length; stripeNum++)
            {
                Color stripeColor = stripeImagesY[stripeNum].GetPixel(coodinateData[dotNum].x, coodinateData[dotNum].y);
                if (stripeColor.r > 0.5)
                {
                    judgmentBitMesuredY[dotNum, stripeImagesY.Length - stripeNum - 1] = true;
                }
                else
                {
                    judgmentBitMesuredY[dotNum, stripeImagesY.Length - stripeNum - 1] = false;
                }
            }
            //PrintJudgmentBit(dotNum, stripeImagesY.Length, judgmentBitMesuredY, nameof(judgmentBitMesuredY));
        }
    }


    //ビットをデバッグ出力する関数
    void PrintJudgmentBit(int dotNum, int bitNum,bool[,] judgmentBit, string name)
    {
        string tex = "_";
        for (int i = 0; i < bitNum; i++)
        {

            if (judgmentBit[dotNum, i]) tex += 1;
            else tex += 0;
            
        }
        //Debug.Log(name+ dotNum.ToString() + tex);
    }




    ///////////////////////////////////////////////
    //タイル画像の処理
    void DotImage2Coodinate()
    {
        
        for(int y = 0; y < dotTileImage.height; y++)
        {
            for(int x = 0; x < dotTileImage.width; x++)
            {
                Color dotCol = dotTileImage.GetPixel(x, y);
                if(dotCol.r > 0.5)
                {
                    tileDotCooodinate.Add(new Vector2Int(x, y));
                    //Debug.Log("T:" + x + "," + y);
                }
                
            }
        }
    }

    void BitMatchingTileX()
    {
        for (int dotNum = 0; dotNum < tileDotCooodinate.Count; dotNum++)
        {
            for (int stripeNum = 0; stripeNum < stripeTileImagesX.Length; stripeNum++)
            {
                Color stripeColor = stripeTileImagesX[stripeNum].GetPixel(tileDotCooodinate[dotNum].x, tileDotCooodinate[dotNum].y);
                if (stripeColor.r > 0.5)
                {
                    judgmentBitTileX[dotNum, stripeTileImagesX.Length - stripeNum - 1] = true;
                }
                else
                {
                    judgmentBitTileX[dotNum, stripeTileImagesX.Length - stripeNum - 1] = false;
                }
            }
            //PrintJudgmentBit(dotNum, stripeImagesX.Length, judgmentBitTileX, nameof(judgmentBitTileX));
        }
    }

    void BitMatchingTileY()
    {
        for (int dotNum = 0; dotNum < tileDotCooodinate.Count; dotNum++)
        {
            for (int stripeNum = 0; stripeNum < stripeTileImagesY.Length; stripeNum++)
            {
                Color stripeColor = stripeTileImagesY[stripeNum].GetPixel(tileDotCooodinate[dotNum].x, tileDotCooodinate[dotNum].y);
                if (stripeColor.r > 0.5)
                {
                    judgmentBitTileY[dotNum, stripeTileImagesY.Length - stripeNum - 1] = true;
                }
                else
                {
                    judgmentBitTileY[dotNum, stripeTileImagesY.Length - stripeNum - 1] = false;
                }
            }
            //PrintJudgmentBit(dotNum, stripeImagesY.Length, judgmentBitTileY, nameof(judgmentBitTileY));
        }
    }


    ///////////////////////////////////////////////
    //XYの合成　ビット列をXYの順に並べる
    void BitComposition()
    {
        for(int mesuredDotNum = 0; mesuredDotNum < csvDatas.Count; mesuredDotNum++)
        {
            for(int i = stripeImagesX.Length; i > 0; i--)
            {
                judgmentBitMesured[mesuredDotNum, i - 1] = judgmentBitMesuredX[mesuredDotNum, i - 1];
            }
            for(int j = stripeImagesY.Length; j > 0; j--)
            {
                judgmentBitMesured[mesuredDotNum, j - 1 + stripeImagesX.Length] = judgmentBitMesuredY[mesuredDotNum, j - 1];
            }
            /*
            PrintJudgmentBit(mesuredDotNum,
                            stripeImagesY.Length + stripeImagesX.Length,
                            judgmentBitMesured,
                            nameof(judgmentBitMesured));
            */
        }
        

        for (int tileDotNum = 0; tileDotNum < tileDotCooodinate.Count; tileDotNum++)
        {
            for (int i = stripeTileImagesX.Length; i > 0; i--)
            {
                judgmentBitTile[tileDotNum, i - 1] = judgmentBitTileX[tileDotNum, i - 1];
            }
            for (int j = stripeTileImagesY.Length; j > 0; j--)
            {
                judgmentBitTile[tileDotNum, j - 1 + stripeTileImagesX.Length] = judgmentBitTileY[tileDotNum, j - 1];
            }
            /*
            PrintJudgmentBit(tileDotNum,
                        stripeTileImagesX.Length + stripeTileImagesY.Length,
                        judgmentBitTile,
                        nameof(judgmentBitTile));
            */
        }
    }

    //イント変換 トチ狂ったビット配列をイントに戻す
    void Chageflags()
    {
        int sumLength = stripeImagesX.Length + stripeImagesY.Length;
        for(int i = 0; i < csvDatas.Count; i++)
        {
            judgmentBitFlagMesured[i] = BitArrayToInt(judgmentBitMesured, i);
            //Debug.Log("MesuredFlag:" + Convert.ToString(judgmentBitFlagMesured[i],2));
        }
        for(int i = 0; i < tileDotCooodinate.Count; i++)
        {
            judgmentBitFlagTile[i] = BitArrayToInt(judgmentBitTile, i);
            //Debug.Log("TileFlag:" + Convert.ToString(judgmentBitFlagTile[i],2));
        }
        
    }

    void MesuredBitSort()//同じ計測ビットを持つ要素を排除し，対応関係を保持する
    {
        for(int i = 0; i < csvDatas.Count; i++)
        {
            bool isMatch = false;
            for(int j = 0; j < csvDatas.Count; j++)
            {
                if (i != j && judgmentBitFlagMesured[i] == judgmentBitFlagMesured[j]) isMatch = true;
            }
            if (!isMatch) judgmentBitSortMesured.Add(new int[] { i, judgmentBitFlagMesured[i] });
        }
        foreach(int[] i in judgmentBitSortMesured)
        {
            //Debug.Log("onlyMesuredDot:" + i[0] + "_" + Convert.ToString(i[1], 2));
        }
        //Debug.Log("length:" + judgmentBitSortMesured.Count);
    }

    //マッチング
    void Matching()
    {
        int cont = 0;
        foreach(int[] i in judgmentBitSortMesured)
        {
            for(int j = 0; j < judgmentBitFlagTile.Length; j++)
            {
                Debug.Log(j);
                if (i[1] == judgmentBitFlagTile[j])
                {
                    matchList.Add(new int[] { i[0], j ,cont});
                    cont += 1;
                }
            }
        }
        foreach (int[] i in matchList)
        {
            //Debug.Log("onlyMesuredDot:" + i[0] + "_" + i[1]);
            //Debug.Log("MatchTile:(" + tileDotCooodinate[i[1]].x + "," + tileDotCooodinate[i[1]].y + ")");
            //Debug.Log("MatchMesured:(" + coodinateData[i[0]].x + "," + coodinateData[i[0]].y + ")");
            
        }
        //Debug.Log("length:" + matchList.Count);
    }

    ///////////////////////////////////////////////
    //見やすいように球として出力する．そのうち使わなくなる
    void CoordinateOutput()
    {
        float mag = 1f;
        for (int i = 0; i < coodinateData.Length; i++)
        {
            bool isMatch = true;
            foreach (int[] j in matchList)
            {
                if (i == j[0])
                {
                    Instantiate(matchObj,
                                new Vector3(coodinateData[i].x * mag, -coodinateData[i].y * mag, 0),
                                Quaternion.identity);
                    isMatch = false;
                }
                
            }
            if(isMatch)
            {
                Instantiate(dotObj,
                            new Vector3(coodinateData[i].x * mag, -coodinateData[i].y * mag, 0),
                            Quaternion.identity);
            }
            
        }
    }

    void CoordinateOutputM()
    {
        float mag = 1f;
        for (int i = 0; i < tileDotCooodinate.Count; i++)
        {
            bool isMatch = true;

            foreach (int[] j in matchList)
            {
                if (i == j[1])
                {
                    Instantiate(matchObj,
                                new Vector3(tileDotCooodinate[i].x * mag, tileDotCooodinate[i].y * mag, 0),
                                Quaternion.identity);
                    Debug.Log("vec:" + tileDotCooodinate[i].x + "," + tileDotCooodinate[i].y);
                    isMatch = false;
                }
            }
            if (isMatch)
            {
                Instantiate(dotObj,
                            new Vector3(tileDotCooodinate[i].x * mag, tileDotCooodinate[i].y * mag, 0),
                            Quaternion.identity);
            
            }

        }
    }

    ////////////////////////
    //メッシュ生成
    void MeshCreate()
    {
        int perfectNum = 0;
        int vertNum = 0;
        int fpix = 4;
        int mineX1 = 0;
        int mineY1 = 0;
        int mineXY = 0;
        Mesh mesh = new Mesh();
        List<Vector3> vert = new List<Vector3>();
        List<Vector2> uv = new List<Vector2>();
        List<int> triangle = new List<int>();
        foreach (int[] i in matchList)
        {
            vertNum += 1;
            Vector2Int mine = new Vector2Int(tileDotCooodinate[i[1]].x, tileDotCooodinate[i[1]].y);
            uv.Add(new Vector2((float)coodinateData[i[0]].x / (float)imageResolution.x * mag, (float)coodinateData[i[0]].y / (float)imageResolution.y * mag));
            vert.Add(new Vector3(tileDotCooodinate[i[1]].x, tileDotCooodinate[i[1]].y, 0));
            bool isPerfect = true;
            int cnt = 0;
            foreach(int[] j in matchList)
            {
                if(mine.x + fpix == tileDotCooodinate[j[1]].x && mine.y == tileDotCooodinate[j[1]].y)
                {
                    mineX1 = j[2];
                }
                else
                {
                    cnt += 1;
                }
                if (cnt == matchList.Count)
                {
                    Debug.Log(cnt);
                    isPerfect = false;
                }
            }
            cnt = 0;
            foreach (int[] j in matchList)
            {
                if (mine.x == tileDotCooodinate[j[1]].x && mine.y + fpix == tileDotCooodinate[j[1]].y)
                {

                    mineY1 = j[2];
                }
                else
                {
                    cnt += 1;
                }
                if (cnt == matchList.Count) isPerfect = false;
            }
            cnt = 0;
            foreach (int[] j in matchList)
            {
                if (mine.x + fpix == tileDotCooodinate[j[1]].x && mine.y + fpix == tileDotCooodinate[j[1]].y)
                {

                    mineXY = j[2];
                }
                else
                {
                    cnt += 1;
                }
                if (cnt == matchList.Count) isPerfect = false;
            }
            if(isPerfect)
            {
                perfectNum += 1;
                //Debug.Log(mine.x + "," + mine.y);
                //Debug.Log(i[2] + "," + mineY1 + "," + mineXY + "," + i[2] + "," + mineXY + "," + mineX1);
                int[] tmp = new int[]{ i[2], mineY1, mineXY, i[2], mineXY, mineX1 };
                triangle.AddRange(tmp);
            }
        }
        //Debug.Log("matchNum" + matchList.Count);
        //Debug.Log(vertNum + "," + perfectNum);
        mesh.vertices = vert.ToArray();
        mesh.triangles = triangle.ToArray();
        mesh.uv = uv.ToArray();
        GetComponent<Renderer>().material.mainTexture = finImage;
        MeshFilter mf = GetComponent<MeshFilter>();
        mf.sharedMesh = mesh;
    }
    

    


    void MatchOutPut()
    {
        float magT = 0.3f;
        float magM = 0.01f;
        for(int i = 0; i < matchList.Count; i++)
        {
            float x = (float)i / (float)matchList.Count;
            GameObject a = Instantiate(matchObj,
                        new Vector3(tileDotCooodinate[matchList[i][1]].x * magT - 180, tileDotCooodinate[matchList[i][1]].y * magT - 270, 0),
                        Quaternion.identity);
            a.GetComponent<Renderer>().material.color = new Color(x, 0f, 0f, 1f);


            GameObject b = Instantiate(matchObj,
                        new Vector3(coodinateData[matchList[i][0]].x * magM, -coodinateData[matchList[i][0]].y * magM, 0),
                        Quaternion.identity);
            b.GetComponent<Renderer>().material.color = new Color(x,0f,0f, 1f);

            Debug.Log("Tile:(" + (tileDotCooodinate[matchList[i][1]].x - coodinateData[matchList[i][0]].x) + "," + (tileDotCooodinate[matchList[i][1]].y - coodinateData[matchList[i][0]].y) +  ")");
        }
    }




    /*
    void  SoreCoodinate2Uvdata()
    {
        Mesh mesh = new Mesh();
        Vector3[] meshVert = new Vector3[matchList.Count];
        Vector2[] meshUV = new Vector2[matchList.Count];
        UVdata[] uvDatas = new UVdata[matchList.Count];
        for(int i = 0; i < matchList.Count; i++)
        {
            uvDatas[i].tile = new Vector3(tileDotCooodinate[matchList[i][1]].x, tileDotCooodinate[matchList[i][1]].y,0);
            uvDatas[i].imaging = new Vector2(coodinateData[matchList[i][0]].x, coodinateData[matchList[i][0]].y);
            uvDatas[i].index = i;
        }
        List<int> triangles = new List<int>();
        mesh.triangles = triangles.ToArray();

    }
    */



    /*
    bool CompareBitArray(bool[,] a, bool[,] b, int aIndex, int bIndex)
    {
        bool isEqual = true;
        for (int bit = 0; bit < stripeImagesX.Length + stripeImagesY.Length; bit++)
        {
            if (a[aIndex, bit] != b[bIndex, bit]) isEqual = false;
        }
        return isEqual;
    }

    bool CompareBitSideArray(bool[,] a, bool[] b, int aIndex)
    {
        bool isEqual = true;
        for (int bit = 0; bit < stripeImagesX.Length + stripeImagesY.Length; bit++)
        {
            if (a[aIndex, bit] != b[bit]) isEqual = false;
        }
        return isEqual;
    }

    bool CompareBit(bool[] a, bool[] b)
    {
        bool isEqual = true;
        for (int bit = 0; bit < stripeImagesX.Length + stripeImagesY.Length; bit++)
        {
            if (a[bit] != b[bit]) isEqual = false;
        }
        return isEqual;
    }
    */

    int BitArrayToInt(bool[,]a, int index)
    {
        int flag = 0;
        int sum = stripeImagesX.Length + stripeImagesY.Length;
        for (int i = 0; i < sum; i++)
        {
            if (a[index, sum - i - 1]) flag += (int)Mathf.Pow(2, i);
        }
        return flag;
    }
}
