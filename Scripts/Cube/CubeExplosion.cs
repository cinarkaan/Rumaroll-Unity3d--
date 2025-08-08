using UnityEngine;

public class CubeExplosion : MonoBehaviour
{

    [SerializeField]
    private float _minForce, _maxForce, _radious, _delay;

    private void Start()
    {
        Explode();
    }
    private void Explode ()
    {
        foreach(Transform t in transform)
        {
            var rb = t.GetComponent<Rigidbody>();

            rb.AddExplosionForce(Random.Range(_minForce, _maxForce) , transform.position, _radious, 1f, ForceMode.Force);

            Destroy(t.gameObject, _delay);
        }
    }

}

