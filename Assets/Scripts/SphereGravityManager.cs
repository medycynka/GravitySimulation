using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SzymonPeszek
{
    [RequireComponent(typeof(Rigidbody))]
    public class SphereGravityManager : MonoBehaviour
    {
        private const float MaxSphereMass = 50f;

        private SphereCollider[] _sphereColliders;
        private float _timeToTurnCollisionOn = 0.5f;
        private bool _explode = true;
        private Vector3 _explodeForce;
        private int _consumedSpheres;

        public float currentRadius = 1;
        public float currentMass = 1;
        public Rigidbody rb;
        public float gravityZonePower = 0.6674f;
        [Range(1f, 2f)] public float dragValue = 1.05f;
        public GameObject spherePrefab;
        public float explodePower = 250f;
        public bool collisionTurnedOn;


        private void Start()
        {
            _sphereColliders = GetComponents<SphereCollider>();
            _sphereColliders[0].enabled = collisionTurnedOn;
            _sphereColliders[1].enabled = collisionTurnedOn;
            rb.mass = currentMass;
        }

        private void OnEnable()
        {
            if (Spawner.spheres == null)
            {
                Spawner.spheres = new List<SphereGravityManager>();
            }

            Spawner.spheres.Add(this);
        }

        private void OnDisable()
        {
            Spawner.spheres.Remove(this);
        }

        private void FixedUpdate()
        {
            rb.velocity /= dragValue;

            if (!collisionTurnedOn)
            {
                _timeToTurnCollisionOn -= Time.fixedDeltaTime;

                if (_timeToTurnCollisionOn <= 0.0f)
                {
                    // or make coroutine
                    collisionTurnedOn = true;
                    _sphereColliders[0].enabled = collisionTurnedOn;
                    _sphereColliders[1].enabled = collisionTurnedOn;
                }
                else
                {
                    rb.AddForce(_explodeForce);
                }
            }

            if (currentMass >= MaxSphereMass && Spawner.sphereCount < Spawner.MAXBalls && _explode)
            {
                Explode();
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            if (Spawner.sphereCount < Spawner.MAXBalls)
            {
                SphereGravityManager otherSphereGravity = other.collider.GetComponent<SphereGravityManager>();

                // If script exists, either check if this object currentMass is bigger or if equal, check if this object is older
                if (otherSphereGravity != null)
                {
                    if (otherSphereGravity.currentMass < currentMass ||
                        (otherSphereGravity.currentMass == currentMass &&
                         otherSphereGravity.GetInstanceID() > GetInstanceID()))
                    {
                        _consumedSpheres++;
                        rb.mass += otherSphereGravity.currentMass;
                        currentMass = rb.mass;
                        currentRadius = Mathf.Sqrt(currentMass);
                        transform.localScale = currentRadius * Vector3.one;

                        Destroy(other.collider.gameObject);
                    }
                }
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.attachedRigidbody && collisionTurnedOn)
            {
                Vector3 gravityDirection = transform.position - other.transform.position;
                float dst = gravityDirection.magnitude;

                if (dst == 0f)
                {
                    return;
                }

                gravityDirection.Normalize();

                float gravityForceMagnitude = gravityZonePower * (rb.mass * other.attachedRigidbody.mass) / (dst * dst);
                Vector3 gravityForce = gravityDirection.normalized * gravityForceMagnitude;
                
                if (Spawner.sphereCount >= Spawner.MAXBalls)
                {
                    gravityForce *= -1f;
                }

                other.attachedRigidbody.AddForce(gravityForce);
            }
        }

        private void Explode()
        {
            _explode = false;
            _sphereColliders[0].enabled = false;
            _sphereColliders[1].enabled = false;
            collisionTurnedOn = false;
            rb.mass = 1;
            currentMass = 1;
            currentRadius = 1;
            transform.localScale = Vector3.one;
            _explodeForce = new Vector3(Random.Range(-explodePower, explodePower),
                Random.Range(-explodePower, explodePower),
                Random.Range(-explodePower, explodePower));
            rb.AddForce(_explodeForce);
            Vector3 spawnPosition = transform.position;

            for (int i = 0; i < _consumedSpheres; i++)
            {
                if (Spawner.sphereCount < Spawner.MAXBalls)
                {
                    SphereGravityManager newSphereGravity =
                        Instantiate(spherePrefab, spawnPosition, Quaternion.identity)
                            .GetComponent<SphereGravityManager>();
                    newSphereGravity._explodeForce = new Vector3(Random.Range(-explodePower, explodePower),
                        Random.Range(-explodePower, explodePower),
                        Random.Range(-explodePower, explodePower));
                    newSphereGravity.rb.AddForce(newSphereGravity._explodeForce);
                }
                else
                {
                    break;
                }
            }

            _consumedSpheres = 0;
        }
    }
}