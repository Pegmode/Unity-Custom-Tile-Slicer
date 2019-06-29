using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class SpriteTileSlicerWindow : EditorWindow
{
    //styling
    //GUIStyle headerStyle = new GUIStyle();

    //WorkProperites
    private Texture2D spriteTexture;
    private Color testColor;
    private int tileSize = 8; //assuming sqr
    private string debugString = "debug";
    private Tile debugTile;
    private string[,] tilePosTable;
    private string path;
    private Vector2 PreviewScroll;

    private string tileFolderFilepath = "Assets/";
    private string tilePaletteFilepath = "Assets/";




    [MenuItem("Window/Sprite Slicer")]
    public static void ShowWindow()
    {
        EditorWindow window = (SpriteTileSlicerWindow)EditorWindow.GetWindow(typeof(SpriteTileSlicerWindow), true, "Custom Tiled Sprite Slicer");
        window.minSize = new Vector2(300,600);
        window.maxSize = new Vector2(300, 600);
    }

    private void OnGUI()
    {
        GUILayout.Label("Preview");
        PreviewScroll = EditorGUILayout.BeginScrollView(PreviewScroll, GUILayout.MinWidth(290), GUILayout.MinHeight(250));
        GUILayout.Box(spriteTexture, GUILayout.Width(290), GUILayout.Height(250));
        EditorGUILayout.EndScrollView();
        EditorGUILayout.IntField("TileSize:", tileSize);
        if (GUILayout.Button("Splice"))
        {
            Debug.Log("Splicing...");
   
            spliceTexture();
            createTiles();
            createTilePallet();
        }
        if (GUILayout.Button("Reset"))
        {
            spriteTexture = null;
        }

        spriteTexture = (Texture2D)EditorGUILayout.ObjectField("Texture Selection:", spriteTexture, typeof(Texture2D), true); //may need to disable scene objs
        GUILayout.BeginHorizontal();
        EditorGUILayout.TextField("Tile Folder",tileFolderFilepath);
        if (GUILayout.Button("Change"))
        {
            tileFolderFilepath = EditorUtility.SaveFolderPanel("Tile save location", "", "");
            tileFolderFilepath = tileFolderFilepath.Substring(tileFolderFilepath.LastIndexOf("Asset"));
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        EditorGUILayout.TextField("TilePalette save location", tilePaletteFilepath);
        if (GUILayout.Button("Change"))
        {
            tilePaletteFilepath = EditorUtility.SaveFolderPanel("Tile palette save location", "", "");
            tilePaletteFilepath = tilePaletteFilepath.Substring(tilePaletteFilepath.LastIndexOf("Assets"));
        }
        GUILayout.EndHorizontal();
        if (GUILayout.Button("debug"))
        {
            Debug.Log("Running dbug...");
            debug();
        }
        GUILayout.Box(debugString);
        debugTile = (Tile)EditorGUILayout.ObjectField("DEBUGDILE", debugTile, typeof(Tile), true);
        //GUILayout.Box(debugSprite.packed.ToString());
        //GUILayout.Box(AssetDatabase.GetAssetPath(debugTile));
        //GUILayout.Box((string)tilePosTable[36,24]);

    }
    private void loadTexture(string filePath)
    {
        //TextureImporter ti = new TextureImporter();
    }

    private void debug()
    {
        createTestPalette();
    }

    private void createTestPalette()
    {
        GameObject testObj = new GameObject();
        testObj.AddComponent(typeof(Grid));
        GameObject testChild = new GameObject();
        testChild.AddComponent(typeof(Tilemap));
        testChild.AddComponent(typeof(TilemapRenderer));
        testChild.name = "Layer1";
        testChild.transform.parent = testObj.transform;

        Tilemap tilemap = testChild.GetComponent(typeof(Tilemap)) as Tilemap;
        tilemap.SetTile(Vector3Int.zero,debugTile);


        GridPalette testPallet = ScriptableObject.CreateInstance<GridPalette>();
        testPallet.name = "palette Settings";


        //string pathUntrimmed = EditorUtility.SaveFolderPanel("Save Tile", "", "");
        // string path = pathUntrimmed.Substring(pathUntrimmed.LastIndexOf("Unity Editor Test"));
        Object prefab = PrefabUtility.SaveAsPrefabAsset(testObj, "Assets/tiles/AATesto.prefab");
        AssetDatabase.AddObjectToAsset(testPallet, prefab);
        AssetDatabase.SaveAssets();
    }


    //Get Sprites
    //string pathstr = AssetDatabase.GetAssetPath(debugSprite);
    //Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(pathstr).OfType<Sprite>().ToArray();


    private void createTilePallet()
    {
        GameObject baseObj = new GameObject();
        baseObj.AddComponent(typeof(Grid));
        GameObject layer1 = new GameObject();
        layer1.AddComponent(typeof(Tilemap));
        layer1.AddComponent(typeof(TilemapRenderer));
        layer1.name = "Layer1";
        layer1.transform.parent = baseObj.transform;

        Tilemap tilemap = layer1.GetComponent(typeof(Tilemap)) as Tilemap;
        for (int x = 0; x < tilePosTable.GetLength(0);x++)
        {
            for (int y = 0; y < tilePosTable.GetLength(1);y++)
            {

                Tile tile = AssetDatabase.LoadAssetAtPath(path + tilePosTable[x,y] + ".asset",typeof(Tile)) as Tile;
                tilemap.SetTile(new Vector3Int(x,y,0),tile);
            }
        }
        GridPalette pallet = ScriptableObject.CreateInstance<GridPalette>();
        pallet.name = "Pallet Settings";

        Object prefab = PrefabUtility.SaveAsPrefabAsset(baseObj, "Assets/tiles/AATesto.prefab");//change later
        AssetDatabase.AddObjectToAsset(pallet, prefab);
        AssetDatabase.SaveAssets();
    }

    private void createTiles()
    {
        string pathUntrimmed = EditorUtility.SaveFolderPanel("Save Tile","","");
        path = pathUntrimmed.Substring(pathUntrimmed.LastIndexOf("Asset"));
        string pathstr = AssetDatabase.GetAssetPath(spriteTexture);
        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(pathstr).OfType<Sprite>().ToArray();
     

        foreach(Sprite sprite in sprites)
        {
            //Tile tile = new Tile(); //outdated?
            Tile tile = (Tile)ScriptableObject.CreateInstance(typeof(Tile));
            tile.sprite = sprite;
            AssetDatabase.CreateAsset(tile, path + sprite.name + ".asset");
        }
    }

    private void spliceTexture()
    {
        TextureImporter tImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(spriteTexture)) as TextureImporter;
        tImporter.isReadable = true; 
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(spriteTexture), ImportAssetOptions.ForceUpdate);//Update spriteTexture with isReadable so that we have access to it in spriteTexture

        tilePosTable = new string[spriteTexture.width/tileSize,spriteTexture.height/tileSize];

        List<SpriteMetaData> spriteMDBuffer = new List<SpriteMetaData>();
        for (int i = 0; i < spriteTexture.width; i += tileSize)//x
        {
            for (int j = 0; j < spriteTexture.height; j += tileSize)//y
            {
                if (!isAlreadyScanned(i, j))
                {
                    spliceAtCoord(i, j, spriteMDBuffer);
                }
            }
        }
        tImporter.spritePixelsPerUnit = (int)tileSize;
        tImporter.filterMode = FilterMode.Point;
        tImporter.spriteImportMode = SpriteImportMode.Multiple;
        tImporter.spritesheet = spriteMDBuffer.ToArray();
        tImporter.isReadable = false;//Save memory by not loading texture twice when not using scripts
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(spriteTexture), ImportAssetOptions.ForceUpdate);
    }

    //private bool isAlreadyScanned(int x, int y)
    //{
    //    Color[] currentPos = spriteTexture.GetPixels(x, y, tileSize, tileSize);

    //    for (int i = 0; i <= spriteTexture.width - tileSize; i += tileSize) //first rect
    //    {
    //        for (int j = 0; j < y; j += tileSize)
    //        {

    //            Color[] scannedPos = spriteTexture.GetPixels(i, j, tileSize, tileSize);
    //            if (isColSeqEqual(currentPos, scannedPos))
    //            {
    //                tilePosTable[x / tileSize, y / tileSize] = spriteTexture.name + i + "x" + j;//keep track of name for later
    //                return true;
    //            }
    //        }
    //    }
    //    for (int j = 0; j < y; j += tileSize) //2nd rect 
    //    {
    //            Color[] scannedPos = spriteTexture.GetPixels(x, j, tileSize, tileSize);
    //            if (isColSeqEqual(currentPos, scannedPos))
    //            {
    //                tilePosTable[x / tileSize, y / tileSize] = spriteTexture.name + x + "x" + j;//keep track of name for later
    //                return true;
    //            }
    //    }
    //    tilePosTable[x / tileSize, y / tileSize] = spriteTexture.name + x + "x" + y;//keep track of name for later
    //    return false;
    //}

    private bool isAlreadyScanned(int x, int y)
    {
        Color[] currentPos = spriteTexture.GetPixels(x, y, tileSize, tileSize);

        for (int i = 0; i < x; i += tileSize) //first rect
        {
            for (int j = 0; j <= spriteTexture.height - tileSize; j += tileSize)
            {

                Color[] scannedPos = spriteTexture.GetPixels(i, j, tileSize, tileSize);
                if (isColSeqEqual(currentPos, scannedPos))
                {
                    tilePosTable[x / tileSize, y / tileSize] = spriteTexture.name + i + "x" + j;//keep track of name for later
                    return true;
                }
            }
        }
        for (int j = 0; j < y; j += tileSize) //2nd rect 
        {
            Color[] scannedPos = spriteTexture.GetPixels(x, j, tileSize, tileSize);
            if (isColSeqEqual(currentPos, scannedPos))
            {
                tilePosTable[x / tileSize, y / tileSize] = spriteTexture.name + x + "x" + j;//keep track of name for later
                return true;
            }
        }
        tilePosTable[x / tileSize, y / tileSize] = spriteTexture.name + x + "x" + y;//keep track of name for later
        return false;
    }

    private bool isColSeqEqual(Color[] seq1, Color[] seq2)
    {
        if (seq1.Length != seq2.Length)
        {
            return false;
        }
        for (int i = 0; i < seq1.Length; i++)
        {
            if (seq1[i].a == 0 && seq2[i].a == 0)//transperent
            {
            }

            else if (seq1[i] != seq2[i])
            {
                return false;
            }
        }

        return true;
    }

    private void spliceAtCoord(int x, int y, List<SpriteMetaData> spriteMDBuffer)
    {
        SpriteMetaData spriteMD = new SpriteMetaData();
        spriteMD.alignment = 9;//maybe have a GUI field to change these under dropdown
        spriteMD.name = spriteTexture.name + x + "x" + y;
        spriteMD.pivot = new Vector2(0.5f, 0.5f);
        spriteMD.rect = new Rect(x, y, tileSize, tileSize);
        spriteMDBuffer.Add(spriteMD);
    }


}