using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using TMPro;


namespace SzymonPeszek
{
    public class Spawner : MonoBehaviour
    {
        private const float SphereSpawnRate = 0.25f;

        private float _spawnTimer = SphereSpawnRate;

        public const int MAXBalls = 250;

        public static List<SphereGravityManager> spheres = new List<SphereGravityManager>();

        public TextMeshProUGUI counter;
        public GameObject spherePrefab;
        public Camera mainCamera;

        public static int sphereCount => spheres.Count;

        private void Update()
        {
            counter.text = sphereCount.ToString("000");
        }

        private void FixedUpdate()
        {
            _spawnTimer -= Time.fixedDeltaTime;

            if (_spawnTimer <= 0.0f && sphereCount < MAXBalls)
            {
                _spawnTimer = SphereSpawnRate;

                SpawnSphere();
            }
        }

        private void SpawnSphere()
        {
            Instantiate(spherePrefab,
                    mainCamera.ViewportToWorldPoint(new Vector3(Random.Range(0.05f, 0.95f), Random.Range(0.05f, 0.95f),
                        Random.Range(5f, 45f))), new Quaternion(0, 0, 0, 0)).GetComponent<SphereGravityManager>()
                .collisionTurnedOn = true;
        }
    }
}