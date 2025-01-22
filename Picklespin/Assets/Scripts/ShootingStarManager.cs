using UnityEngine;
using System.Collections;

public class ShootingStarManager : MonoBehaviour
{
    [SerializeField] private GameObject shootingStarPrefab;
    [SerializeField] private float minSpawnDelay = 5f;
    [SerializeField] private float maxSpawnDelay = 15f;
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float travelTime = 2f;
    [SerializeField] private float fadeOutDuration = 1f;
    [SerializeField] private float minSpeed = 30f;
    [SerializeField] private float maxSpeed = 60f;
    [SerializeField] private float skyCenterHeight = 50f;
    [SerializeField] private float horizontalRange = 50f;
    [SerializeField] private float verticalRange = 10f;

    private TrailRenderer trail;
    private Material starMaterial;
    private Color baseColor;
    private Transform cam;

    private void Start()
    {
        trail = shootingStarPrefab.GetComponent<TrailRenderer>();
        starMaterial = trail.material;
        baseColor = starMaterial.color;
        cam = Camera.main.transform;
        shootingStarPrefab.SetActive(false);
        StartCoroutine(StarRoutine());
    }

    private IEnumerator StarRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minSpawnDelay, maxSpawnDelay));
            trail.Clear();
            starMaterial.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0f);
            Vector3 c = cam.position;
            Vector3 startPos = new Vector3(c.x, skyCenterHeight, c.z) + new Vector3(Random.Range(-horizontalRange, horizontalRange), Random.Range(-verticalRange, verticalRange), 0f);
            shootingStarPrefab.transform.position = startPos;
            Vector3 dir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 0f), Random.Range(-0.5f, 0.5f)).normalized;
            float speed = Random.Range(minSpeed, maxSpeed);
            shootingStarPrefab.SetActive(true);

            float t = 0f;
            float total = fadeInDuration + travelTime + fadeOutDuration;

            while (t < total)
            {
                t += Time.deltaTime;
                if (t < fadeInDuration)
                {
                    float a = Mathf.Lerp(0f, baseColor.a, t / fadeInDuration);
                    starMaterial.color = new Color(baseColor.r, baseColor.g, baseColor.b, a);
                }
                else if (t < fadeInDuration + travelTime)
                {
                    starMaterial.color = baseColor;
                }
                else
                {
                    float d = (t - fadeInDuration - travelTime) / fadeOutDuration;
                    float a = Mathf.Lerp(baseColor.a, 0f, d);
                    starMaterial.color = new Color(baseColor.r, baseColor.g, baseColor.b, a);
                }
                shootingStarPrefab.transform.Translate(dir * speed * Time.deltaTime, Space.World);
                yield return null;
            }
            shootingStarPrefab.SetActive(false);
        }
    }
}
