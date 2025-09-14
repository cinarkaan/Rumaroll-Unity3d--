using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{

    [Header("The vfx systems after collision events with player")]
    [SerializeField]
    private ParticleSystem _collideCoin, _collideDiamond;

    private int earnedCoin,earnedDiamond;

    [SerializeField]
    private PlatformManager platformManager;

    [SerializeField]
    private UIController UIController;

    [SerializeField]
    private AudioSource _lootSfx;

    public List<GameObject> Precious { get; private set; }

    public GameObject coinPrefab, diamondPrefab;

    public int CoinCount { get; private set; }
    public int DiamondCount { get; private set; }

    public static bool progress = false;

    private void Awake()
    {
        Precious = new List<GameObject>();
        earnedCoin = PlayerPrefs.GetInt("Coin");
        earnedDiamond = PlayerPrefs.GetInt("Diamond");
        EventManager.progress = false;
        StartCoroutine(WaitForPlatform());
    }
    private void GenerateCoins()
    {
        MaterialPropertyBlock CoinMPB = new();
        CoinMPB.SetColor("_ColorBottom", coinPrefab.GetComponent<Renderer>().sharedMaterial.GetColor("_ColorBottom"));
        CoinMPB.SetColor("_ColorTop", coinPrefab.GetComponent<Renderer>().sharedMaterial.GetColor("_ColorTop"));
        UniqueRandomGenerator uniqueRandomGenerator = new UniqueRandomGenerator();
        uniqueRandomGenerator.Min = 0;
        uniqueRandomGenerator.Max = platformManager.SolutionPath.Count;
        uniqueRandomGenerator.Count = CoinCount;
        uniqueRandomGenerator.Generate();
        foreach (var item in uniqueRandomGenerator.UniqueRandoms)
        {
            Vector3 pos = new(platformManager.SolutionPath[item].x, 1f, platformManager.SolutionPath[item].y);
            GameObject coin = Instantiate(coinPrefab, pos, coinPrefab.transform.rotation, transform);
            coin.GetComponent<Renderer>().SetPropertyBlock(CoinMPB);
            Precious.Add(coin);
        }
    }
    private void GenerateDims ()
    {
        MaterialPropertyBlock DiamondMPB = new();
        DiamondMPB.SetColor("_ColorBottom", diamondPrefab.GetComponent<Renderer>().sharedMaterial.GetColor("_ColorBottom"));
        DiamondMPB.SetColor("_ColorTop", diamondPrefab.GetComponent<Renderer>().sharedMaterial.GetColor("_ColorTop"));
        UniqueRandomGenerator uniqueRandomGenerator = new UniqueRandomGenerator();
        uniqueRandomGenerator.Min = 0;
        uniqueRandomGenerator.Max = platformManager.UnSolution.Count;
        uniqueRandomGenerator.Count = DiamondCount;
        uniqueRandomGenerator.GenerateBySolutions(platformManager.SolutionPath, platformManager.UnSolution);

        foreach (var item in uniqueRandomGenerator.UniqueRandoms)
        {
            if ((DiamondCount--) > 0)
            {
                Vector3 pos = new Vector3(platformManager.UnSolution[item].x, 0.75f, platformManager.UnSolution[item].y);
                GameObject dim = Instantiate(diamondPrefab, pos, Quaternion.Euler(0, 0, 0), transform);
                dim.GetComponent<Renderer>().SetPropertyBlock(DiamondMPB);
                Precious.Add(dim);
            }
            else
                break;
        }
        EventManager.progress = true;
    }
    public void CheckEarned(GameObject eventObject)
    {
        if (eventObject != null)
        {
            _lootSfx.PlayOneShot(_lootSfx.clip, UIController._Volume);
            if (eventObject.layer == 10)
            {
                StartCoroutine(PlayEndDestroy(_collideCoin,eventObject.transform.position));
                earnedCoin++;
                UIController.UpdateCoinCount(earnedCoin);
                PlayerPrefs.SetInt("Coin", earnedCoin);
            }
            else if (eventObject.layer == 13)
            {
                StartCoroutine(PlayEndDestroy(_collideDiamond, eventObject.transform.position));
                earnedDiamond++;
                UIController.UpdateDiamondCount(earnedDiamond);
                PlayerPrefs.SetInt("Diamond", earnedDiamond);
            }
            Precious.Remove(eventObject);
            Destroy(eventObject);
            PlayerPrefs.Save();
        } 

    }
    private void FinalizeEventsCount ()
    {
        switch (platformManager.Stage)
        {
            case 4:
                CoinCount = 3;
                DiamondCount = 0;
                break;
            case 5:
                CoinCount = 4;
                DiamondCount = 2;
                break;
            case 6:
                CoinCount = 5;
                DiamondCount = 2;
                break;
            case 7:
                CoinCount = 6;
                DiamondCount = 3;
                break;
            case 8:
                CoinCount = 8;
                DiamondCount = 3;
                break;
            case 9:
                CoinCount = 9;
                DiamondCount = 4;
                break;
            case 10:
                CoinCount = 10;
                DiamondCount = 5;
                break;
            case 11:
                CoinCount = 11;
                DiamondCount = 6;
                break;
            case 12:
                CoinCount = 12;
                DiamondCount = 7;
                break;
            default:
                break;
        }
        GenerateCoins();
        GenerateDims();
        
    }
    private void LateUpdate()
    {
        if (Precious.Count > 0)
        {
            Quaternion precieousRotation = Quaternion.AngleAxis(360f * Time.deltaTime, Vector3.up) * coinPrefab.transform.rotation;
            for (int i = 0; i < Precious.Count; i++)
            {
                var t = Precious[i].transform;
                t.rotation = Quaternion.Lerp(t.rotation, precieousRotation * t.rotation, 0.005f);
            }
        }
    }
    private IEnumerator WaitForPlatform ()
    {
        yield return new WaitUntil(() => platformManager.Progress);
        FinalizeEventsCount();
    }
    private IEnumerator PlayEndDestroy (ParticleSystem vfx, Vector3 position)
    {
        ParticleSystem collideVfx = Instantiate(vfx, position, Quaternion.Euler(-90f, 0f, 0f), transform);
        collideVfx.collision.SetPlane(0, platformManager.transform);
        collideVfx.Play();
        yield return new WaitUntil(() => !collideVfx.isPlaying);
        Destroy(collideVfx.gameObject);
    }

}

