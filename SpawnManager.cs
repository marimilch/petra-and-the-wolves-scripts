using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public float timeBetweenSingleSpawn = 0.25f;
    public float minDistance = 4.0f;
    public float maxDistance = 8.0f;
    public GameObject enemyPrefab;
    public ParticleSystem spawnAnim;
    public AudioClip nextWaveSound;

    private int waveNumber = 0;
    private bool wait = false;
    private Vector3[] fan;
    private GameObject player;
    private AudioSource sound;

    private int remoteI = 0;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
        sound = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (CountEnemies() < 1 && !wait)
        {
            wait = true;
            ++waveNumber;
            StartCoroutine(WaitForNextWave());
        }
    }

    private int CountEnemies()
    {
        return GameObject.FindGameObjectsWithTag("Enemy").Length;
    }

    IEnumerator WaitForNextWave()
    {
        sound.PlayOneShot(nextWaveSound);
        yield return new WaitForSeconds(nextWaveSound.length);
        SpawnNextWave();
    }

    void SpawnNextWave()
    {
        CreateWigglyVectorFan(
            waveNumber, FlattenTo(player.transform.position, 0.5f)
        );


        for(int i = 0; i < fan.Length; ++i)
        {
            Debug.Log("Start Coroutine.");

            remoteI = i; //i have no better solution for this :/
            StartCoroutine(
                "DelayedSpawnEnemy"
            );
        }
    }

    IEnumerator DelayedSpawnEnemy()
    {
        Debug.Log("Wait Spawn");
        int i = remoteI;

        yield return new WaitForSeconds(i * timeBetweenSingleSpawn);
        Debug.Log("Spawn");
        SpawnEnemyAt(fan[i]);
        wait = false;
    }

    private Vector3 FlattenTo(Vector3 vec, float y)
    {
        return new Vector3(vec.x, y, vec.z);
    }

    void CreateWigglyVectorFan(int parts, Vector3 origin)
    {
        if (parts <= 0)
        {
            fan = new Vector3[0];
            return;
        }

        fan = new Vector3[parts];
        float angle = (2*Mathf.PI) / parts;

        for(int i = 0; i < parts; ++i)
        {
            float mag = Random.Range(minDistance, maxDistance);
            float x = Mathf.Cos(angle*i) * mag;
            float z = Mathf.Sin(angle*i) * mag;
            fan[i] = new Vector3(x, 0, z) + origin;
        }
    }

    void SpawnEnemyAt(Vector3 pos)
    {
        Instantiate(spawnAnim, pos, spawnAnim.transform.rotation);
        Instantiate(enemyPrefab, pos, enemyPrefab.transform.rotation);
    }
}
