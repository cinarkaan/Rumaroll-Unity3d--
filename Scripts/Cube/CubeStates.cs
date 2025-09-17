using UnityEngine;

public class CubeStates : MonoBehaviour
{

    [Header("For Explosive")]
    [Tooltip("For Explosive Effect")]
    [SerializeField]
    private float MinForce, MaxForce, Radious, Delay;

    [Header("For Blade Cutters")]
    [Tooltip("For Cutter Effect")]
    [SerializeField]
    private float CutterForce, CutterTorque;

    [Header("For Spike,Cutter, Bulldozer")]
    [Tooltip("For Scatter Effect")]
    [SerializeField]
    private float ScatterForce, ScatterTorque;

    public State CubeState;

    public void Start()
    {
        if (CubeState == State.Explosive)
            Explode();
        else if (CubeState == State.Cutter)
            Cutter();
        else
            Scatter();
    }
    private void Explode ()
    {
        foreach(Transform t in transform)
        {
            var rb = t.GetComponent<Rigidbody>();

            rb.AddExplosionForce(Random.Range(MinForce, MaxForce) , transform.position, Radious, 1f, ForceMode.Force);

            rb.AddTorque(Random.rotation.eulerAngles * 0.75f, ForceMode.Impulse);
        }
        Destroy(gameObject, Delay);
    }
    private void Cutter()
    {
        foreach (Transform t in transform)
        {
            var rb = t.GetComponent<Rigidbody>();

            rb.AddForce(Random.insideUnitSphere.normalized * CutterForce, ForceMode.Force);

            rb.AddTorque(Random.insideUnitSphere.normalized * CutterTorque, ForceMode.Force);
        }
        Destroy(gameObject, Delay);
    }
    private void Scatter ()
    {
        foreach (Transform t in transform)
        {
            var rb = t.GetComponent<Rigidbody>();

            rb.AddForce(Random.insideUnitCircle.normalized * CutterForce, ForceMode.Force);

            rb.AddTorque(Random.insideUnitCircle.normalized * CutterTorque, ForceMode.Force);
        }
        Destroy(gameObject, Delay);
    }
}

public enum State
{
    Cutter,
    Explosive,
    Scatter
}


