
using UnityEngine;
using System.Text; //for stringbuilder
using System.Collections.Generic; //for List

public class TerrainHeightTexture : MonoBehaviour
{
    #region --- helper ---
    //serializable allows to show in inspector
    [System.Serializable] 
    public enum enumMode
    {
        ClickDent,
        ClickTexture,
        UpdateHeight, 
    }

    [System.Serializable] 
    public class HeightmapParam
    {
        public float depth = 1.0f;
        public float wavesX = 3.0f;
        public float wavesZ = 3.0f;
        public float perlinOffsetX = 0.0f;
        public float perlinOffsetZ = 0.0f;        
    }

    [System.Serializable]
    public class TextureLayerParam
    {
        public int textureIndex = 0;
        public float percentStart = 0.0f;
        public float percentEnd = 0.1f;

        public TextureLayerParam(int textureindex, float percentstart, float percentend)
        {
            this.textureIndex = textureindex;
            this.percentStart = percentstart;
            this.percentEnd = percentend;
        }
        public override string ToString()
        {
            return string.Format("textureIndex={0}, percentHeightStart={1}, percentHeightEnd={2}", textureIndex, percentStart, percentEnd);
        }
    }
    #endregion

    public enumMode Mode = enumMode.ClickDent;
    public int DentSize = 3;
    public float DentDepth = 0.05f;
    public int TextureSize = 3;
    public HeightmapParam heightparam = new HeightmapParam();
    public List<TextureLayerParam> textureparam = new List<TextureLayerParam>();
    private Terrain terr;

    private void Start()
    {
        terr = this.GetComponent<Terrain>();

        if (textureparam.Count == 0)
        {
            textureparam.Add(new TextureLayerParam(0, 0.0f, 0.3f)); //dirt
            textureparam.Add(new TextureLayerParam(1, 0.3f, 0.6f)); //grass
            textureparam.Add(new TextureLayerParam(2, 0.6f, 1.0f)); //snow
            textureparam.Add(new TextureLayerParam(3, 1.0f, 1.0f)); //red
        }

        StringBuilder sb1 = new StringBuilder();
        sb1.Append("[terrainData]");
        sb1.Append(string.Format(" TERRAIN: size.x={0}, size.z={1}, size.y={2} ", terr.terrainData.size.x, terr.terrainData.size.z, terr.terrainData.size.y));
        sb1.Append(string.Format(" | HEIGHTMAP: heightmapWidth={0}, heightmapHeight={1} ", terr.terrainData.heightmapWidth, terr.terrainData.heightmapHeight));
        sb1.Append(string.Format(" | ALPHAMAP: alphamapWidth={0}, alphamapHeight={1}, alphamapLayers={2} ", terr.terrainData.alphamapWidth, terr.terrainData.alphamapHeight, terr.terrainData.alphamapLayers));
        Debug.Log(sb1.ToString());

    }
    private void Update()
    {
        switch (Mode)
        {
            case enumMode.ClickDent:
                ClickDent();
                break;
            case enumMode.UpdateHeight:
                GenerateHeight();
                break;
            case enumMode.ClickTexture:
                ClickTexture();
                break;
        }        
    }
    private void ClickDent()
    {
        if (Input.GetMouseButtonDown(0) == false)
        {
            return;
        }

        //cast ray from camera into scene
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //hit gameobject?
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit) == true)
        {
            //hit terrain gameobject?
            if (hit.transform.gameObject.name == "Terrain")
            {
                //convert hit.point to heightmap coordinate
                float percentX = (hit.point.x - terr.transform.position.x) / (float)terr.terrainData.size.x;    //value from 0.0 to 1.0
                float percentZ = (hit.point.z - terr.transform.position.z) / (float)terr.terrainData.size.z;
                int heightmapX = (int)(percentX * terr.terrainData.heightmapWidth);     //multiply into heightmap coord range
                int heightmapZ = (int)(percentZ * terr.terrainData.heightmapHeight);

                //get terrain's heightmap
                float[,] heights = terr.terrainData.GetHeights(0, 0, terr.terrainData.heightmapWidth, terr.terrainData.heightmapHeight);

                //dent the heightmap
                for (int areaX = heightmapX - DentSize; areaX < heightmapX + DentSize; areaX++)
                {
                    for (int areaZ = heightmapZ - DentSize; areaZ < heightmapZ + DentSize; areaZ++)
                    {
                        int X = Mathf.Clamp(areaX, 0, terr.terrainData.heightmapWidth);
                        int Z = Mathf.Clamp(areaZ, 0, terr.terrainData.heightmapHeight);

                        //Note: flip the coordinates!!
                        heights[Z, X] -= DentDepth;
                    }
                }

                //set terrain's heightmap
                terr.terrainData.SetHeights(0, 0, heights);
            }
        }
    }
    private void ClickTexture()
    {
        if (Input.GetMouseButtonDown(0) == false)
        {
            return;
        }

        //cast ray from camera into scene
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //hit gameobject?
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit) == true)
        {
            //hit terrain gameobject?
            if (hit.transform.gameObject.name == "Terrain")
            {
                //convert hit.point to heightmap coordinate
                float percentX = (hit.point.x - terr.transform.position.x) / (float)terr.terrainData.size.x;    //value from 0.0 to 1.0
                float percentZ = (hit.point.z - terr.transform.position.z) / (float)terr.terrainData.size.z;
                int alphamapX = (int)(percentX * terr.terrainData.alphamapWidth);     //multiply into alphamap coord range
                int alphamapZ = (int)(percentZ * terr.terrainData.alphamapHeight);

                //get alphamap
                float[,,] alphamap = terr.terrainData.GetAlphamaps(0, 0, terr.terrainData.alphamapWidth, terr.terrainData.alphamapHeight);

                //set the alpha of "last" texture layer at coord
                for (int areaX = alphamapX - TextureSize; areaX < alphamapX + TextureSize; areaX++)
                {
                    for (int areaZ = alphamapZ - TextureSize; areaZ < alphamapZ + TextureSize; areaZ++)
                    {
                        int X = Mathf.Clamp(areaX, 0, terr.terrainData.alphamapWidth);
                        int Z = Mathf.Clamp(areaZ, 0, terr.terrainData.alphamapHeight);

                        //Note: flip the coordinates!! alpha of last texture at coord
                        for (int i = 0; i < terr.terrainData.alphamapLayers; i++)
                        {
                            if (i == terr.terrainData.alphamapLayers - 1)
                            {
                                alphamap[Z, X, i] = 1.0f;
                            }
                            else
                            {
                                alphamap[Z, X, i] = 0.0f;
                            }
                        }
                    }
                }

                //set alphamap
                terr.terrainData.SetAlphamaps(0, 0, alphamap);
            }
        }
    }
    public void GenerateHeight()
    {
        //Note: use PerlinNoise algorithm to generate 'smoothly random' changing heights

        if (terr == null)
        {
            terr = this.GetComponent<Terrain>();
        }

        float[,] heightmap = new float[terr.terrainData.heightmapWidth, terr.terrainData.heightmapHeight];

        for (int w = 0; w < terr.terrainData.heightmapWidth; w++)
        {
            for (int h = 0; h < terr.terrainData.heightmapHeight; h++)
            {
                //Note:
                //  w / (float)heightmapWidth   equals a value from 0.0 to 1.0f. need float or will only get zeros
                //  wavesX                      equals how many waves along the terrain X axis
                //  perlinOffsetX               equals how far along the X axis the perlin algorithm returns a 'smooth random' height

                float perlinCoordX = ((w / (float)terr.terrainData.heightmapWidth) * heightparam.wavesX) + heightparam.perlinOffsetX;
                float perlinCoordZ = ((h / (float)terr.terrainData.heightmapHeight) * heightparam.wavesZ) + heightparam.perlinOffsetZ;

                float perlinHeight = Mathf.PerlinNoise(perlinCoordX, perlinCoordZ);

                heightmap[w, h] = perlinHeight * heightparam.depth;   //set the heightmap coord height
            }
        }

        //set the terrain heightmap
        terr.terrainData.SetHeights(0, 0, heightmap);
    }        
    public void GenerateTexture()
    {
        //Note: texture alpha based on terrain height (percent of total possible height)

        if (terr == null)
        {
            terr = this.GetComponent<Terrain>();
        }

        //get alphamap
        float[,,] alphamap = terr.terrainData.GetAlphamaps(0, 0, terr.terrainData.alphamapWidth, terr.terrainData.alphamapHeight);

        for (int w = 0; w < alphamap.GetLength(0); w++)
        {
            for (int h = 0; h < alphamap.GetLength(1); h++)
            {
                //percent height terrain at coord
                float terrainHeightAt = terr.terrainData.GetHeight(h, w);
                float percentTerrainHeight = terrainHeightAt / terr.terrainData.size.y;

                //alpha of each texture layer (at coord)
                for (int t = 0; t < textureparam.Count; t++)
                {
                    if (percentTerrainHeight >= textureparam[t].percentStart && percentTerrainHeight < textureparam[t].percentEnd)
                    {
                        alphamap[w, h, t] = 1.0f;
                    }
                    else
                    {
                        alphamap[w, h, t] = 0.0f;
                    }
                }
            }
        }

        //set alphamap
        terr.terrainData.SetAlphamaps(0, 0, alphamap);
    }
}
