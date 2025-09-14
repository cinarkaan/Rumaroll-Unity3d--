using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlatformManager : ExceptionalPlatform
{
    [Header("Grid Settings")]
    [Range(4, 13)]
    public int Stage = 5;    

    [SerializeField]
    private GameMapController gameMapController;

    [SerializeField]
    private ParticleSystem Clue;

    public ParticleSystem Clue_ => Clue;

    private void Awake()
    {
        Prefabs[2].transform.position = new Vector3(6, 0.98f, 6);

        Stage = PlayerPrefs.GetInt("Stage");

        _Stage = Stage;

        InitializeWeather(Random.Range(0, 3));

        RandomMaterialSelection();

        InitializeEnvironment();

        InitializeSolution();

        CreateGrid();

        PlaceFlag();
    }
    private void LateUpdate()
    {
        if (Progress && gameMapController.currentIndex == 1)
        {
            ParallelFrustumCulling();
            FrustumCullingForColorfuls();
            AllActivated = false;
        }
        else
        {
            if (Progress)
            {
                Graphics.DrawMeshInstanced(TileMesh, 0, TileMat, Tile);
                Graphics.DrawMeshInstanced(FrameMesh, 0, FrameMat, Frame);
                Graphics.DrawMeshInstanced(SurfacesMesh, 0, SurfacesMat, Surface);
                for (int i = 0; i < Renderers.Count && !AllActivated; i++)
                    if (!Renderers[i].enabled) Renderers[i].enabled = true;
                AllActivated = true;
            }
        }
    }
    protected override void RandomMaterialSelection()
    {
        HashSet<Object> selected = new();

        AllMaterials = Resources.LoadAll("Lit/GradientLit", typeof(Material)).ToList();
  
        while (selected.Count < 6)
            selected.Add(AllMaterials[Random.Range(0, AllMaterials.Count)]);

        var Selected = selected.ToArray();

        Prefabs[2].transform.GetChild(5).GetComponent<Renderer>().sharedMaterial = (Material)Selected[0];
        Prefabs[2].transform.GetChild(1).GetComponent<Renderer>().sharedMaterial = (Material)Selected[1];
        Prefabs[2].transform.GetChild(2).GetComponent<Renderer>().sharedMaterial = (Material)Selected[2];
        Prefabs[2].transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = (Material)Selected[3];
        Prefabs[2].transform.GetChild(3).GetComponent<Renderer>().sharedMaterial = (Material)Selected[4];
        Prefabs[2].transform.GetChild(4).GetComponent<Renderer>().sharedMaterial = (Material)Selected[5];

        Prefabs[2].GetComponent<RollingCubeController>().LinearToGamma();

        AllMaterials = Selected.ToList();

        selected.Clear();
    }
    protected override void InitializeSolution ()
    {
        Vector2Int goal = new(6 + Stage, 6 + Stage);
        SolutionPath = GenerateSolutionPath(new Vector2Int(6, 6), goal);

        CubeSimulator cubeSim = new();
        GridTiles[new Vector2Int(6, 6)] = new PlatformTile((Material)AllMaterials[0], true);

        for (int i = 1; i < SolutionPath.Count; i++)
        {
            Vector2Int prev = SolutionPath[i - 1];
            Vector2Int current = SolutionPath[i];
            Vector3 moveDir = new Vector3(current.x - prev.x, 0, current.y - prev.y);

            cubeSim.Roll(moveDir);
            int bottomFace = cubeSim.faceIndices[0];
            GridTiles[SolutionPath[i]] = new PlatformTile((Material)AllMaterials[bottomFace], true);
        }
    }
    protected override void CreateGrid()
    {
        MaterialPropertyBlock mpb = new();
        for (int x = 0; x < 12 + Stage; x++)
        {
            for (int z = 0; z < 12 + Stage; z++)
            {
                if (x < 6 || z < 6 || x > 6 + Stage || z > 6 + Stage)
                {
                    Tile.Add(Matrix4x4.TRS(new Vector3(x, -0.1f, z), Prefabs[0].transform.localRotation, new Vector3(1f, 0.4f, 1f)));
                    Frame.Add(Matrix4x4.TRS(new Vector3(x, -0.4f, z), Prefabs[0].transform.GetChild(1).localRotation, Prefabs[0].transform.GetChild(1).localScale));
                    Surface.Add(Matrix4x4.TRS(new Vector3(x, -0.4f, z), Quaternion.Euler(new Vector3(-90f, 0, 0)), Prefabs[0].transform.GetChild(0).localScale));
                }
                else
                {
                    Tile.Add(Matrix4x4.TRS(new Vector3(x, 0.28f, z), Prefabs[0].transform.localRotation, new Vector3(1f, 0.4f, 1f)));
                    Frame.Add(Matrix4x4.TRS(new Vector3(x, -0.0105f, z), Prefabs[0].transform.GetChild(1).localRotation, Prefabs[0].transform.GetChild(1).localScale));
                    GameObject colorfulTile = Instantiate(Prefabs[0].transform.GetChild(0).gameObject, new Vector3(x, -0.0105f, z), Quaternion.Euler(-90f, 0f, 0f), transform);
                    Vector2Int pos = new(x, z);
                    if (GridTiles.ContainsKey(pos))
                    {
                        mpb.SetColor("_ColorBottom", AdjustColorAccordingToTile(pos)._bottom.gamma);
                        mpb.SetColor("_ColorTop", AdjustColorAccordingToTile(pos)._top.gamma);
                        colorfulTile.GetComponent<Renderer>().SetPropertyBlock(mpb);
                        colorfulTile.AddComponent<ColorfulTile>();
                        GridTiles[pos].tile = colorfulTile;
                        GridTiles[pos].OnSolution = true;
                        colorfulTile.name = "OnSolution";

                    }
                    else
                    {
                        mpb.SetColor("_ColorBottom", ((Material)AllMaterials[Random.Range(0, AllMaterials.Count)]).GetColor("_ColorBottom").gamma);
                        mpb.SetColor("_ColorTop", ((Material)AllMaterials[Random.Range(0, AllMaterials.Count)]).GetColor("_ColorTop").gamma);
                        colorfulTile.GetComponent<Renderer>().SetPropertyBlock(mpb);
                        GridTiles[pos] = new PlatformTile(colorfulTile.GetComponent<Renderer>().sharedMaterial, false);
                        GridTiles[pos].tile = colorfulTile;
                        colorfulTile.name = "UnSolution";
                    }
                }
                mpb.Clear();
            }
        }
        SolutionPath = GridTiles.Where(value => value.Value.OnSolution).Select(key => key.Key).ToList();
        UnSolution = GridTiles.Where(value => !value.Value.OnSolution).Select(key => key.Key).ToList();
        Renderers = GridTiles.Select(value => value.Value.tile.GetComponent<Renderer>()).ToList();
    }
    protected override void InitializeWeather(int status)
    {
        if (PlayerPrefs.GetInt("Vfx") == 0) return;

        var shape = Weather[status].shape;
        shape.enabled = true;

        Vector3 pos = Vector3.zero;
        if (status == 0 || status == 2)
        {
            shape.scale = new Vector3(Stage + 12f, Stage + 12f, 1f);
            pos = new Vector3((Stage + 12) / 2f, 2.5f, (Stage + 12) / 2f);
        }
        else if (status == 1)
        {
            shape.scale = new Vector3(Stage + 12f, 1f, Stage + 12f);
            pos = new Vector3((Stage + 12) / 2f, 1.8f, (Stage + 12) / 2f);
        }


        Weather[status].transform.position = pos;
        ParticleSystem weather = Instantiate(Weather[status], pos, Quaternion.Euler(0f, 0f, 0f), transform);
        weather.name = "Weather";

    }
    public override void CreateDynamics()
    {
        // If the stage is even , it can be placed both vertical and horizontal. It will be implemented at the Stage 6

        // One cell that is surrounded the center of blade. At the Stage 10 will be inmplemented

        // According to the stage , it can be placed each region but it must not be conflict with spike. At the odd stage will be implemented (7,9)
        if (Stage % 2 == 0)
        {
            int middlePoint = Stage / 2 + 6;
            int lastPoint = Stage + 6;
            switch (Stage)
            {
                case 6:
                    for (int x = 6; x <= lastPoint; x++)
                        if (SolutionPath.Contains(new Vector2Int(x, middlePoint)) && DynamicPath.Count < 2)
                            DynamicPath.Add(new Vector2Int(x, middlePoint));
                    for (int y = 6; y <= lastPoint; y++)
                        if (SolutionPath.Contains(new Vector2Int(middlePoint, y)) && DynamicPath.Count < 4)
                            DynamicPath.Add(new Vector2Int(middlePoint, y));
                    break;
                case 8:
                    for (int x = 6; x <= lastPoint; x++)
                        if (SolutionPath.Contains(new Vector2Int(x, middlePoint)) && DynamicPath.Count < 3)
                            DynamicPath.Add(new Vector2Int(x, middlePoint));
                    for (int y = 6; y <= lastPoint; y++)
                        if (SolutionPath.Contains(new Vector2Int(middlePoint, y)) && DynamicPath.Count < 6)
                            DynamicPath.Add(new Vector2Int(middlePoint, y));
                    break;
                case 10:
                    List<GameObject> placed = GameObject.Find("ObstacleManager").GetComponent<ObstacleManager>().Blade.ToList();
                    while (placed.Count > 0)
                    {
                        Vector2Int pos = new Vector2Int((int)placed[0].transform.position.x, (int)placed[0].transform.position.z);
                        if (SolutionPath.Contains(pos + Vector2Int.left))
                            DynamicPath.Add(pos + Vector2Int.left);
                        if (SolutionPath.Contains(pos + Vector2Int.right))
                            DynamicPath.Add(pos + Vector2Int.right);
                        if (SolutionPath.Contains(pos + Vector2Int.up))
                            DynamicPath.Add(pos + Vector2Int.up);
                        if (SolutionPath.Contains(pos + Vector2Int.down))
                            DynamicPath.Add(pos + Vector2Int.down);
                        placed.Remove(placed[0]);
                    }
                    break;
                case 12:
                    for (int x = 6; x <= lastPoint; x++)
                        if (SolutionPath.Contains(new Vector2Int(x, middlePoint)) && DynamicPath.Count < 4)
                            DynamicPath.Add(new Vector2Int(x, middlePoint));
                    for (int y = 6; y <= lastPoint; y++)
                        if (SolutionPath.Contains(new Vector2Int(middlePoint, y)) && DynamicPath.Count < 7)
                            DynamicPath.Add(new Vector2Int(middlePoint, y));
                    break;
                default:
                    return;
            }
            if (PlayerPrefs.GetInt("Vfx") == 1)
                DynamicPath.Where(d => GridTiles[d].tile.GetComponent<ColorfulTile>() != null).ToList().ForEach(d => GridTiles[d].tile.GetComponent<ColorfulTile>().AddSmokeVfx(AdjustColorAccordingToTile(d),Smoke_Burst));
            LaunchDynamicPath();
        }
        else
        {
            List<GameObject> referanced;
            switch (Stage)
            {
                case 7:
                    referanced = GameObject.Find("ObstacleManager").GetComponent<ObstacleManager>().Spikes.ToList();
                    while (referanced.Count > 0)
                    {
                        int regionCount = 1;
                        Vector2Int pos = new Vector2Int((int)referanced[0].transform.position.x, (int)referanced[0].transform.position.z);
                        if (SolutionPath.Contains(pos + Vector2Int.left) && regionCount > 0)
                        {
                            DynamicPath.Add(pos + Vector2Int.left);
                            regionCount--;
                        }
                        if (SolutionPath.Contains(pos + Vector2Int.right) && regionCount > 0)
                        {
                            DynamicPath.Add(pos + Vector2Int.right);
                            regionCount--;
                        }
                        if (SolutionPath.Contains(pos + Vector2Int.up) && regionCount > 0)
                        {
                            DynamicPath.Add(pos + Vector2Int.up);
                            regionCount--;
                        }
                        if (SolutionPath.Contains(pos + Vector2Int.down) && regionCount > 0)
                        {
                            DynamicPath.Add(pos + Vector2Int.down);
                            regionCount--;
                        }
                        referanced.Remove(referanced[0]);
                    }
                    break;
                case 9:
                    referanced = GameObject.Find("ObstacleManager").GetComponent<ObstacleManager>().Blade.ToList();
                    while (referanced.Count > 0)
                    {
                        Vector2Int pos = new Vector2Int((int)referanced[0].transform.position.x, (int)referanced[0].transform.position.z);
                        if (SolutionPath.Contains(pos + Vector2Int.left))
                            DynamicPath.Add(pos + Vector2Int.left);
                        if (SolutionPath.Contains(pos + Vector2Int.right))
                            DynamicPath.Add(pos + Vector2Int.right);
                        if (SolutionPath.Contains(pos + Vector2Int.up))
                            DynamicPath.Add(pos + Vector2Int.up);
                        if (SolutionPath.Contains(pos + Vector2Int.down))
                            DynamicPath.Add(pos + Vector2Int.down);
                        referanced.Remove(referanced[0]);
                    }
                    break;
                case 11:
                    referanced = GameObject.Find("ObstacleManager").GetComponent<ObstacleManager>().Blade.ToList();
                    while (referanced.Count > 0)
                    {
                        Vector2Int pos = new Vector2Int((int)referanced[0].transform.position.x, (int)referanced[0].transform.position.z);
                        if (SolutionPath.Contains(pos + Vector2Int.left))
                            DynamicPath.Add(pos + Vector2Int.left);
                        if (SolutionPath.Contains(pos + Vector2Int.right))
                            DynamicPath.Add(pos + Vector2Int.right);
                        if (SolutionPath.Contains(pos + Vector2Int.up))
                            DynamicPath.Add(pos + Vector2Int.up);
                        if (SolutionPath.Contains(pos + Vector2Int.down))
                            DynamicPath.Add(pos + Vector2Int.down);
                        referanced.Remove(referanced[0]);
                    }
                    break;
                default:
                    return;
            }
            if (PlayerPrefs.GetInt("Vfx") == 1)
                DynamicPath.Where(d => GridTiles[d].tile.GetComponent<ColorfulTile>() != null).ToList().ForEach(d => GridTiles[d].tile.GetComponent<ColorfulTile>().AddSmokeVfx(AdjustColorAccordingToTile(d),Smoke_Burst));
            LaunchDynamicPath();
        }
    }
    private void LaunchDynamicPath()
    {
        foreach (var pos in DynamicPath)
            GridTiles[pos].tile.GetComponent<ColorfulTile>().RepeatColor((Material)AllMaterials[Random.Range(0, AllMaterials.Count)], GridTiles[pos].material);
    }
    public void AdjustColorOfClue (Vector2Int pos)
    {
        Gradient _arrows = new Gradient();
        Gradient _glow = new Gradient();

        var _arrowsColorModule = Clue.colorOverLifetime;
        var glowColorModule = Clue.transform.GetChild(0).GetComponent<ParticleSystem>().colorOverLifetime;

        _arrows.colorKeys = new GradientColorKey[]
        {
            new GradientColorKey(AdjustColorAccordingToTile(pos)._bottom,0f),
            new GradientColorKey(AdjustColorAccordingToTile(pos)._top , 1f)
        };

        _glow.colorKeys = new GradientColorKey[]
        {
            new GradientColorKey(AdjustColorAccordingToTile(pos)._bottom, 0f),
            new GradientColorKey(AdjustColorAccordingToTile(pos)._top, 1f)
        };

        _arrowsColorModule.color = new ParticleSystem.MinMaxGradient(_arrows);
        glowColorModule.color = new ParticleSystem.MinMaxGradient(_glow);
    }
}
