using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using System;
public class ProjectileShooting : NetworkBehaviour
{
    //[SerializeField] private Bullet bulletPrefab;
    //[SerializeField] private float bulletSpeed;
    //[SerializeField] private Transform cameraHolder;

    //private List<Bullet> spawnedBullets = new List<Bullet>();

    //LevelRepeater levelRepeater;
    //float repeatSpacingX, repeatSpacingY, repeatSpacingZ;
    //private void Awake()
    //{
    //    duplicateManager = GetComponent<DuplicateManager>();
    //    levelRepeater = FindFirstObjectByType<LevelRepeater>();
    //    repeatSpacingX = levelRepeater.RepeatSpacing.x;
    //    repeatSpacingY = levelRepeater.RepeatSpacing.y;
    //    repeatSpacingZ = levelRepeater.RepeatSpacing.z;
    //}
    //private void WorldWrap(Transform bullet)
    //{
    //    float halfRepeatSpacingX = repeatSpacingX / 2;
    //    float halfRepeatSpacingY = repeatSpacingY / 2;
    //    float halfRepeatSpacingZ = repeatSpacingZ / 2;

    //    if (bullet.position.x > halfRepeatSpacingX)
    //    {
    //        print("x");
    //        bullet.position -= new Vector3(repeatSpacingX, 0f, 0f);
    //    }
    //    else if (transform.position.x < -halfRepeatSpacingX)
    //    {
    //        bullet.position += new Vector3(repeatSpacingX, 0f, 0f);
    //    }

    //    if (bullet.position.y > halfRepeatSpacingY)
    //    {
    //        bullet.position -= new Vector3(0f, repeatSpacingY, 0f);
    //    }
    //    else if (bullet.position.y < -halfRepeatSpacingY)
    //    {
    //        bullet.position += new Vector3(0f, repeatSpacingY, 0f);
    //    }

    //    if (bullet.position.z > halfRepeatSpacingZ)
    //    {
    //        bullet.position -= new Vector3(0f, 0f, repeatSpacingZ);
    //    }
    //    else if (transform.position.z < -halfRepeatSpacingZ)
    //    {
    //        bullet.position += new Vector3(0f, 0f, repeatSpacingZ);
    //    }
    //}
    //private void Update()
    //{
    //    //foreach (var bullet in spawnedBullets)
    //    //{
    //    //    bullet.BulletTransform.position += bullet.BulletDirection * Time.deltaTime * bulletSpeed;

    //    //    WorldWrap(bullet.BulletTransform);
    //    //}

    //    if (!IsOwner)
    //        return;

    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        Shoot();
            
    //    }
    //}
    //void Shoot()
    //{

    //    int bulletID = UnityEngine.Random.Range(0, 999999);
    //    SpawnBulletLocal(cameraHolder.position, cameraHolder.forward, bulletID, LocalConnection.ClientId);
    //    SpawnBulletServer(cameraHolder.position, cameraHolder.forward, TimeManager.Tick, bulletID, NetworkObject.OwnerId);
    //}
    //DuplicateManager duplicateManager;
    //void SpawnBulletLocal(Vector3 startPos, Vector3 dir, int bulletID, int ownerID)
    //{
    //    Bullet bullet = Instantiate(bulletPrefab, startPos, transform.rotation);
    //    bullet.Initialize(dir, bulletSpeed, bulletID, ownerID);

    //    //for (int i = 0; i < duplicateManager.DuplicateControllers.Count; i++)
    //    //{
    //    //    Vector3 dupStartPos = duplicateManager.DuplicateControllers[i].CameraHolder.position;
    //    //    GameObject dupBullet = Instantiate(bulletPrefab, dupStartPos, transform.rotation);
    //    //    dupBullet.transform.parent = bullet.transform;
    //    //}
    //    //spawnedBullets.Add(new Bullet { BulletTransform = bullet.transform, BulletDirection = dir });

    //}
    //[ServerRpc]
    //void SpawnBulletServer(Vector3 startPos, Vector3 dir, uint startTick, int bulletID, int ownerID)
    //{
    //    SpawnBulletObserver(startPos, dir, startTick, bulletID, ownerID);
    //}
    //[ObserversRpc (ExcludeOwner = true)]
    //void SpawnBulletObserver(Vector3 startPos, Vector3 dir, uint startTick, int bulletID, int ownerID)
    //{
    //    float timeDifference = (float)(TimeManager.Tick - startTick) / TimeManager.TickRate;
    //    //print("time to recieve bullet:" + timeDifference + "s");
    //    Vector3 spawnPos = startPos + dir * bulletSpeed * timeDifference;

    //    Bullet bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
    //    bullet.Initialize(dir, bulletSpeed, bulletID, ownerID);

    //    //DuplicateManager otherPlayerDuplicateManager = null;
    //    ////get duplicate manage of that owner id
    //    //foreach (var dup in FindObjectsByType<DuplicateManager>(FindObjectsSortMode.None))
    //    //{
    //    //    if(dup.GetComponent<NetworkObject>().OwnerId == ownerID)
    //    //    {
    //    //        otherPlayerDuplicateManager = dup;
    //    //    }
    //    //}
    //    //if(otherPlayerDuplicateManager == null) 
    //    //{
    //    //    Debug.LogError("Couldn't find duplicate manager");
    //    //    return;
    //    //}
    //    //for (int i = 0; i < otherPlayerDuplicateManager.DuplicateControllers.Count; i++)
    //    //{
    //    //    Vector3 dupStartPos = otherPlayerDuplicateManager.DuplicateControllers[i].CameraHolder.position;
    //    //    Vector3 dupSpawnPos = dupStartPos + dir * bulletSpeed * timeDifference;
    //    //    GameObject dupBullet = Instantiate(bulletPrefab, dupSpawnPos, transform.rotation);
    //    //    dupBullet.transform.parent = bullet.transform;
    //    //}
    //    //spawnedBullets.Add(new Bullet { BulletTransform = bullet.transform, BulletDirection = dir });
    //}
    ////class Bullet
    ////{
    ////    public Transform BulletTransform;
    ////    public Vector3 BulletDirection;
    ////}
}
