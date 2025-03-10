using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PunchingBag_Respawn : MonoBehaviour
{
    [SerializeField] GameObject spawnObj;
    [SerializeField] Transform parent;
    [SerializeField] PlayerMovement playerMovement;

    private void Start()
    {
        playerMovement.objectPunched += Respawn;
    }

    private void Respawn()
    {
        StartCoroutine(SpawnBag());
    }

    IEnumerator SpawnBag()
    {
        yield return new WaitForSeconds(3f);
        Instantiate(spawnObj, parent);
    }
}
