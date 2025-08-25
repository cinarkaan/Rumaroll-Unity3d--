using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{

    [Header("The vfx systems after collision events with player")]
    [SerializeField]
    private ParticleSystem _collideCoin, _collideDiamond;

    [SerializeField]
    private float rotateSpeed = 60f;

    private int earnedCoin,earnedDiamond;

    [SerializeField]
    private PlatformManager platformManager;

    [SerializeField]
    private UIController UIController;

    [SerializeField]
    private AudioSource _lootSfx;

    public List<GameObject> Precious { get; private set; }

    public GameObject coinPrefab, diamondPrefab;

    public int _CoinCount { get; private set; }
    public int _DiamondCount { get; private set; }

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
        uniqueRandomGenerator.min = 0;
        uniqueRandomGenerator.max = platformManager.SolutionPath.Count;
        uniqueRandomGenerator.count = _CoinCount;
        uniqueRandomGenerator.Generate();
        foreach (var item in uniqueRandomGenerator.uniqueRandoms)
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
        uniqueRandomGenerator.min = 0;
        uniqueRandomGenerator.max = platformManager.UnSolution.Count;
        uniqueRandomGenerator.count = _DiamondCount;
        uniqueRandomGenerator.GenerateBySolutions(platformManager.SolutionPath, platformManager.UnSolution);

        foreach (var item in uniqueRandomGenerator.uniqueRandoms)
        {
            if ((_DiamondCount--) > 0)
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
        StartCoroutine(Rotate());
    }
    public void CheckEarned(GameObject eventObject)
    {
        if (eventObject != null)
        {
            _lootSfx.PlayOneShot(_lootSfx.clip, UIController._volume);
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
                _CoinCount = 3;
                _DiamondCount = 0;
                break;
            case 5:
                _CoinCount = 4;
                _DiamondCount = 2;
                break;
            case 6:
                _CoinCount = 5;
                _DiamondCount = 2;
                break;
            case 7:
                _CoinCount = 6;
                _DiamondCount = 3;
                break;
            case 8:
                _CoinCount = 8;
                _DiamondCount = 3;
                break;
            case 9:
                _CoinCount = 9;
                _DiamondCount = 4;
                break;
            case 10:
                _CoinCount = 10;
                _DiamondCount = 5;
                break;
            case 11:
                _CoinCount = 11;
                _DiamondCount = 6;
                break;
            case 12:
                _CoinCount = 12;
                _DiamondCount = 7;
                break;
            default:
                break;
        }
        GenerateCoins();
        GenerateDims();

    }
    private IEnumerator Rotate()
    {
        while (true)
        {
            Quaternion precieousRotation = Quaternion.AngleAxis(360f * Time.deltaTime, Vector3.up) * coinPrefab.transform.rotation;
            Precious.ForEach(p => p.transform.rotation = Quaternion.Lerp(p.transform.rotation, precieousRotation * p.transform.rotation, 0.005f));
            yield return null;
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

