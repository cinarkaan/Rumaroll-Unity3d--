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

    public List<GameObject> precious { get; private set; }

    public GameObject coinPrefab, diamondPrefab;

    public int _coinCount { get; private set; }
    public int _diamondCount { get; private set; }

    public static bool progress = false;

    private void Awake()
    {
        precious = new List<GameObject>();
        earnedCoin = PlayerPrefs.GetInt("Coin");
        earnedDiamond = PlayerPrefs.GetInt("Diamond");
        EventManager.progress = false;
        StartCoroutine(waitForPlatform());
    }
    private void generateCoins()
    {
        MaterialPropertyBlock CoinMPB = new();
        CoinMPB.SetColor("_ColorBottom", coinPrefab.GetComponent<Renderer>().sharedMaterial.GetColor("_ColorBottom"));
        CoinMPB.SetColor("_ColorTop", coinPrefab.GetComponent<Renderer>().sharedMaterial.GetColor("_ColorTop"));
        UniqueRandomGenerator uniqueRandomGenerator = new UniqueRandomGenerator();
        uniqueRandomGenerator.min = 0;
        uniqueRandomGenerator.max = platformManager.solutionPath.Count;
        uniqueRandomGenerator.count = _coinCount;
        uniqueRandomGenerator.Generate();
        foreach (var item in uniqueRandomGenerator.uniqueRandoms)
        {
            Vector3 pos = new(platformManager.solutionPath[item].x, 1f, platformManager.solutionPath[item].y);
            GameObject coin = Instantiate(coinPrefab, pos, coinPrefab.transform.rotation, transform);
            coin.GetComponent<Renderer>().SetPropertyBlock(CoinMPB);
            precious.Add(coin);
        }
    }
    private void generateDims ()
    {
        MaterialPropertyBlock DiamondMPB = new();
        DiamondMPB.SetColor("_ColorBottom", diamondPrefab.GetComponent<Renderer>().sharedMaterial.GetColor("_ColorBottom"));
        DiamondMPB.SetColor("_ColorTop", diamondPrefab.GetComponent<Renderer>().sharedMaterial.GetColor("_ColorTop"));
        UniqueRandomGenerator uniqueRandomGenerator = new UniqueRandomGenerator();
        uniqueRandomGenerator.min = 0;
        uniqueRandomGenerator.max = platformManager.unSolutionPath.Count;
        uniqueRandomGenerator.count = _diamondCount;
        uniqueRandomGenerator.GenerateBySolutions(platformManager.solutionPath, platformManager.unSolutionPath);

        foreach (var item in uniqueRandomGenerator.uniqueRandoms)
        {
            if ((_diamondCount--) > 0)
            {
                Vector3 pos = new Vector3(platformManager.unSolutionPath[item].x, 0.75f, platformManager.unSolutionPath[item].y);
                GameObject dim = Instantiate(diamondPrefab, pos, Quaternion.Euler(0, 0, 0), transform);
                dim.GetComponent<Renderer>().SetPropertyBlock(DiamondMPB);
                precious.Add(dim);
            }
            else
                break;
        }
        EventManager.progress = true;
        StartCoroutine(Rotate());
    }
    public void checkEarned(GameObject eventObject)
    {
        if (eventObject != null)
        {
            _lootSfx.PlayOneShot(_lootSfx.clip, UIController._volume);
            if (eventObject.layer == 10)
            {
                StartCoroutine(PlayEndDestroy(_collideCoin,eventObject.transform.position));
                earnedCoin++;
                UIController.updateCoinCount(earnedCoin);
                PlayerPrefs.SetInt("Coin", earnedCoin);
            }
            else if (eventObject.layer == 13)
            {
                StartCoroutine(PlayEndDestroy(_collideDiamond, eventObject.transform.position));
                earnedDiamond++;
                UIController.updateDiamondCount(earnedDiamond);
                PlayerPrefs.SetInt("Diamond", earnedDiamond);
            }
            precious.Remove(eventObject);
            Destroy(eventObject);
            PlayerPrefs.Save();
        } 

    }
    private void finalizeEventsCount ()
    {
        switch (platformManager.stage)
        {
            case 4:
                _coinCount = 3;
                _diamondCount = 0;
                break;
            case 5:
                _coinCount = 4;
                _diamondCount = 2;
                break;
            case 6:
                _coinCount = 5;
                _diamondCount = 2;
                break;
            case 7:
                _coinCount = 6;
                _diamondCount = 3;
                break;
            case 8:
                _coinCount = 8;
                _diamondCount = 3;
                break;
            case 9:
                _coinCount = 9;
                _diamondCount = 4;
                break;
            case 10:
                _coinCount = 10;
                _diamondCount = 5;
                break;
            case 11:
                _coinCount = 11;
                _diamondCount = 6;
                break;
            case 12:
                _coinCount = 12;
                _diamondCount = 7;
                break;
            default:
                break;
        }
        generateCoins();
        generateDims();

    }
    private IEnumerator Rotate()
    {
        while (true)
        {
            Quaternion precieousRotation = Quaternion.AngleAxis(360f * Time.deltaTime, Vector3.up) * coinPrefab.transform.rotation;
            precious.ForEach(p => p.transform.rotation = Quaternion.Lerp(p.transform.rotation, precieousRotation * p.transform.rotation, 0.005f));
            yield return null;
        }

    }
    private IEnumerator waitForPlatform ()
    {
        yield return new WaitUntil(() => platformManager.progress);
        finalizeEventsCount();
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

