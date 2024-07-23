using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Connection;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using System;
using FishNet.Managing.Timing;

public class RaycastShooting : NetworkBehaviour
{
    public LayerMask playerLayer;

    public float shootCooldown;
    float shootTimer;

    [SerializeField] LineRenderer lineRenderer;

    [SerializeField] LayerMask rotateToAbleObjectLayer;


    [Header("References")]
    [SerializeField] Transform cameraHolder, gunTip;

    [Header("Gun Settings")]
    [SerializeField] float gunRange;
    [SerializeField] float maxRotateDistance;

    [Header("Line Renderers")]
    [SerializeField] LineRenderer grappleLineRenderer;

    //[Header("Grapple Settings")]
    //[SerializeField] LayerMask whatIsGrappleable;
    //private Vector3 grapplePoint;
    //private Vector3 currentGrapplePosition;
    //private SpringJoint joint;

    [Header("Player Components")]
    private DuplicateManager duplicateManager;
    //private PlayerInput playerInput;
    private LevelRepeater levelRepeater;
    private ServerManager serverManager;

    [Header("Shooting")]
    [SerializeField] GameObject bullet;
    [SerializeField] float lineRendererDuration;
    public LayerMask mask;
    [SerializeField] GameObject impactPointGO;

    private void Awake()
    {
        duplicateManager = GetComponent<DuplicateManager>();
    }
    Transform otherPLayer;
    private void Start()
    {
        levelRepeater = FindFirstObjectByType<LevelRepeater>();
        serverManager = FindFirstObjectByType<ServerManager>();
        SpawnLineRenderers();
        SpawnImpactPoints();
    }
    private void Update()
    {
        if (!base.IsOwner)
            return;

        if (Input.GetButtonDown("Fire1"))
        {
            if (shootTimer <= 0)
            {

                bool hitLocally;
                FireRaycastLocal(cameraHolder.position, cameraHolder.position, gunTip.position, cameraHolder.forward, gunRange, 0f, false, out hitLocally); // local

                if (hitLocally)
                {
                    print("HitLocal");
                }
                else
                {
                    print("NoHitPlayerLocal");
                }
                if (base.IsServer)
                {
                    ShootServer(TimeManager.GetPreciseTick(TimeManager.Tick ), false, false);
                }
                else
                {
                    //PreciseTick pt = base.TimeManager.GetPreciseTick(base.TimeManager.LastPacketTick);
                    PreciseTick pt = base.TimeManager.GetPreciseTick(base.TimeManager.Tick);
                    ShootServer(pt, true, hitLocally);
                }
                //ShootServer(cameraHolder.forward, tttick);
                shootTimer = 0.375f; // need to make on server
            }
        }


        if (shootTimer > 0)
            shootTimer -= Time.deltaTime;

    }
    [ServerRpc(RequireOwnership = false)]
    private void ResetPlayer(Transform player)
    {
        player.GetComponent<CSPMotor>().ResetQueued = true;
        //player.position= new Vector3(0, 0, 0);
        //player.GetComponent<CSPMotor>().enabled = true;
    }
    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        base.TimeManager.OnTick += TimeManager_OnTick;

        tttick = TimeManager.Tick;
    }   
    public override void OnStopNetwork()
    {
        base.OnStopNetwork();
        if (base.TimeManager != null)
            base.TimeManager.OnTick -= TimeManager_OnTick;
    }
    uint tttick;
    private void TimeManager_OnTick()
    {
        if (!base.IsServer)
        {
            tttick++;
        }
    }
    [SerializeField] GameObject fakecolllllllllllll;

    [ServerRpc(RequireOwnership = false)]
    private void ShootServer(PreciseTick tick, bool shotFromClient, bool hitLocallyFromClient)
    {
        //Debug.LogWarning("current tick:" + (int)TimeManager.Tick);
        //Debug.LogWarning("sent tick:" + (int)tick);
        //tick -= 5;

        

        var tickDiff = TimeManager.Tick - tick.Tick;
        //Debug.LogWarning("tick diff:" + tickDiff);
        //tick -= tickDiff;
        //rewind this and every other play to that tick i think
        PlayerCollisionRollbackRaycasts playerCollisionRollbackRaycasts = GetComponent<PlayerCollisionRollbackRaycasts>();
        // position player was when shot
        Vector3 rewindPos = Vector3.zero;
        Vector3 rewindDir = Vector3.zero;
        Vector3 rewindCameraPos = Vector3.zero;
        Vector3 rewindGunTip = Vector3.zero;




        //for (int i = playerCollisionRollbackRaycasts.PastStates.Count - 1; i > 0; i--)
        for (int i = 0; i < playerCollisionRollbackRaycasts.PastStates.Count; i++)
        {
            if (playerCollisionRollbackRaycasts.PastStates[i].Tick == (int)tick.Tick)
            {
                rewindPos = playerCollisionRollbackRaycasts.PastStates[i].Position;
                rewindDir = playerCollisionRollbackRaycasts.PastStates[i].Direction;
                rewindCameraPos = playerCollisionRollbackRaycasts.PastStates[i].CameraHolderPos;
                rewindGunTip = playerCollisionRollbackRaycasts.PastStates[i].GunTipPosition;
                break;

            }
        }
        if (rewindPos == Vector3.zero)
        {
            rewindPos = transform.position;
            rewindDir = cameraHolder.forward;
            rewindCameraPos = cameraHolder.position;
            rewindGunTip = gunTip.position;
            Debug.LogError("Couldn't find past state of this player with same tick or player is at 0,0,0");
            //return;
        }
    // When the server recives that a player has shot, it rolls everything back to the tick they shot from
   // https://fish-networking.gitbook.io/docs/manual/guides/lag-compensation/states
        foreach (var playerRollback in FindObjectsByType<PlayerCollisionRollbackRaycasts>(FindObjectsSortMode.None))
        {
            //is here for debugging
            // where it should be (with mesh renderer turned on for debugging)
            //playerRollback.DisableFakeCollider();
        }
        foreach (var playerRollback in FindObjectsByType<PlayerCollisionRollbackRaycasts>(FindObjectsSortMode.None))
        {
            Vector3 otherPlayerRewindPos = Vector3.zero;
            Quaternion otherPlayerRewindRot = Quaternion.identity;
            for (int i = 0; i < playerRollback.PastStates.Count; i++)
            {
                //if (playerRollback.PastStates[i].Tick == (int)TimeManager.Tick - 1)
                if (playerRollback.PastStates[i].Tick == (int)tick.Tick)
                {
                    var asdsadasd = TimeManager.Tick;
                    otherPlayerRewindPos = playerRollback.PastStates[i].Position;
                    otherPlayerRewindRot = playerRollback.PastStates[i].Rotation;
                    break;
                }
            }
            if (otherPlayerRewindPos == Vector3.zero)
            {
                Debug.LogError("Couldn't find past state of other player with same tick or player is at 0,0,0");
                return;
            }
            playerRollback.EnableFakeCollider(otherPlayerRewindPos, otherPlayerRewindRot);
        }
        if(shotFromClient && hitLocallyFromClient)
        {
            serverManager.UpdateShotsHitOnClientButNotOnServer(OwnerId);
        }


        FireRaycastServer(rewindCameraPos, rewindDir, gunRange, 0f, false, shotFromClient, hitLocallyFromClient);

        foreach (var playerRollback in FindObjectsByType<PlayerCollisionRollbackRaycasts>(FindObjectsSortMode.None))
        {
            // where it should be normally (with mesh renderer turned off)
            playerRollback.DisableFakeCollider();
        }
        // then update clients
        ShootObsver(rewindCameraPos, rewindCameraPos, rewindGunTip, rewindDir);
    }
    [ObserversRpc (ExcludeOwner = true)]
    private void ShootObsver(Vector3 rayStartPos, Vector3 cameraHolderPos, Vector3 gunTipPos, Vector3 direction)
    {
        bool noOneCares;
        FireRaycastLocal(rayStartPos, cameraHolderPos, gunTipPos, direction, gunRange, 0f, true,  out noOneCares);
    }
    // need fire local that only ignores self layer
    void FireRaycastLocal(Vector3 rayStartPos, Vector3 cameraHolderPos, Vector3 gunTipPos, Vector3 direction, float rayRange, float totalRayDistance, bool sentFromServer, out bool hitLocally)
    {

        // Get the spacing values for repeated objects
        float repeatSpacingX = levelRepeater.RepeatSpacing.x;
        float repeatSpacingY = levelRepeater.RepeatSpacing.y;
        float repeatSpacingZ = levelRepeater.RepeatSpacing.z;

        // Initialize a RaycastHit variable
        RaycastHit hit;

        // Create a LayerMask and set it to everything except the object's own layer
        int thisPlayerLayer = LayerMask.GetMask("Player" + NetworkObject.OwnerId);
       // int player0 = LayerMask.GetMask("Player0");
       // int player1 = LayerMask.GetMask("Player1");
       // int player2 = LayerMask.GetMask("Player2");
       // int player3 = LayerMask.GetMask("Player3");
       //
       // // Combine the layer masks using the bitwise OR operator
       // int layerMaskInt = player0 | player1 | player2 | player3;
        // Perform a raycast from the startRayPos in the camera's forward direction
        // Handle when the ray hits something
        if (Physics.Raycast(rayStartPos, direction, out hit, rayRange, ~thisPlayerLayer))
        {
            // Handle when the ray hits a player (where a player was)
            if (hit.transform.CompareTag("FakeCollider"))
            {
                //Instantiate(fakecolllllllllllll, hit.transform.position, hit.transform.rotation);
                totalRayDistance += hit.distance;
                DrawLasersAndImpactParticle(cameraHolderPos, gunTipPos, direction * totalRayDistance, true, hit.normal);

                if (!sentFromServer && !IsHost)
                {
                    print("HITFUCKINGLOCALFUCKIGNFUDCKJC");
                    //serverManager.UpdateLocalClientHits(OwnerId);
                    hitLocally = true;
                    return;

                }

            }
            // Handle when the ray hits a bounds trigger
            else if (hit.transform.CompareTag("BoundsTrigger"))
            {
                // Calculate the new position for the next ray
                Vector3 newRayOffset = Vector3.zero;

                switch (hit.transform.name)
                {
                    case "-BoundsX":
                        newRayOffset = new Vector3(repeatSpacingX, 0, 0); //print("-x"); 
                        break;                                                         //
                    case "BoundsX":
                        newRayOffset = new Vector3(-repeatSpacingX, 0, 0); //print("x"); 
                        break;                                                         //
                    case "-BoundsY":
                        newRayOffset = new Vector3(0, repeatSpacingY, 0); //print("-y"); 
                        break;                                                         //
                    case "BoundsY":
                        newRayOffset = new Vector3(0, -repeatSpacingY, 0); //print("y"); 
                        break;                                                         //
                    case "-BoundsZ":
                        newRayOffset = new Vector3(0, 0, repeatSpacingZ); //print("-z"); 
                        break;                                                         //
                    case "BoundsZ":
                        newRayOffset = new Vector3(0, 0, -repeatSpacingZ); //print("z"); 
                        break;
                    default:
                        Debug.LogError("The player has hit an unidentified bound");
                        break;
                }
                float remainingDist = rayRange - Vector3.Distance(hit.point, rayStartPos);

                Vector3 newRayPos = hit.point + newRayOffset;
                Debug.DrawLine(rayStartPos, hit.point, Color.green, 5f);

                float newTotalRayDistance = totalRayDistance + hit.distance;
                // Recursively fire a new ray from the adjusted position
                FireRaycastLocal(newRayPos, cameraHolderPos, gunTipPos, direction, remainingDist, newTotalRayDistance, sentFromServer, out hitLocally);
            }
            // Handle when the ray hits a wall
            else
            {
                totalRayDistance += hit.distance;
                DrawLasersAndImpactParticle(cameraHolderPos, gunTipPos, direction * totalRayDistance, true, hit.normal);
                //Debug.DrawLine(rayStartPos, hit.point, Color.green, 5f);
                
                
            }
        }
        // Handle when the ray doesn't hit anything
        else
        {
            //Debug.DrawRay(rayStartPos, direction * rayRange, Color.green, 5f);
            // Ray missed everything so it's laser range is the max possible
            DrawLasersAndImpactParticle(cameraHolderPos, gunTipPos, direction * gunRange, false, Vector3.zero);            
        }

        hitLocally = false;

    }
    private void FireRaycastServer(Vector3 rayStartPos, Vector3 direction, float rayRange, float totalRayDistance, bool shotHasWorldWrapped, bool shotFromClient, bool shotHitLocallyOnClient)
    {
        // Get the spacing values for repeated objects
        float repeatSpacingX = levelRepeater.RepeatSpacing.x;
        float repeatSpacingY = levelRepeater.RepeatSpacing.y;
        float repeatSpacingZ = levelRepeater.RepeatSpacing.z;

        // Initialize a RaycastHit variable
        RaycastHit hit;

        // Create a LayerMask and set it to everything except the object's own layer
        //int thisPlayerLayer = LayerMask.GetMask("Player" + NetworkObject.OwnerId);

        //WE NEED TO ONLY BE ABLE TO HIT FAKE COLLIDERS
        // NEED TO IGNORE ALL OF THESE MASKS BECAUSE OF THE CHARACTER CONTROLLER COLLIDER
        int player0 = LayerMask.GetMask("Player0");
        int player1 = LayerMask.GetMask("Player1");
        int player2 = LayerMask.GetMask("Player2");
        int player3 = LayerMask.GetMask("Player3");
        int myFakeCollider = LayerMask.GetMask("FakeCollider" + OwnerId);

        // Combine the layer masks using the bitwise OR operator
        int layerMaskInt = player0 | player1 | player2 | player3 | myFakeCollider;
        // Perform a raycast from the startRayPos in the camera's forward direction
        // Handle when the ray hits something
        if (Physics.Raycast(rayStartPos, direction, out hit, rayRange, ~layerMaskInt))
        {
            if(hit.collider.gameObject.name == "Player: 0")
            {
                Debug.Log("ok we hit a" + hit.collider.gameObject, hit.collider.gameObject);

            }
            // Handle when the ray hits a player (where a player was)
            if (hit.transform.CompareTag("FakeCollider"))
            {
                GameObject fakeColliderPlayerOwner = hit.transform.GetComponent<FakeCollider>().PlayerOwner;

                // If the player has hit ANOTHER player, not a world wrapped shot on themself
                if (fakeColliderPlayerOwner != gameObject)
                {
                    // THIS IS JUST FOR TESTING SO U CANT SHOOT HOST
                    if(fakeColliderPlayerOwner.GetComponent<PlayerLocalManager>().OwnerId == 0)
                    {
                        //return;
                    }


                    ResetPlayer(fakeColliderPlayerOwner.transform);

                    // check if they shot them from behind (for testing)
                    string hitFromBehind = HitFromBehind(direction, hit);
                    // update the total shot distance (for testing)
                    totalRayDistance += hit.distance;

                    //Debug.Log("SERVER HIT");

                    
                    if(shotFromClient && !shotHitLocallyOnClient)
                    {
                        serverManager.UpdateShotsHitOnServerButNotOnClient(OwnerId);                        
                    }
                    serverManager.UpdateScore(OwnerId, hitFromBehind, shotHasWorldWrapped, totalRayDistance);


                    Debug.DrawLine(rayStartPos, hit.point, Color.red, 20f);
                }
                

            }
            // Check if the hit was from behind
            // Handle when the ray hits a bounds trigger
            else if (hit.transform.CompareTag("BoundsTrigger"))
            {
                // Calculate the new position for the next ray
                Vector3 newRayOffset = Vector3.zero;
                float remainingDist = rayRange - Vector3.Distance(hit.point, rayStartPos);

                switch (hit.transform.name)
                {
                    case "-BoundsX":
                        newRayOffset = new Vector3(repeatSpacingX, 0, 0); //print("-x"); 
                        break;                                                         //
                    case "BoundsX":
                        newRayOffset = new Vector3(-repeatSpacingX, 0, 0); //print("x"); 
                        break;                                                         //
                    case "-BoundsY":
                        newRayOffset = new Vector3(0, repeatSpacingY, 0); //print("-y"); 
                        break;                                                         //
                    case "BoundsY":
                        newRayOffset = new Vector3(0, -repeatSpacingY, 0); //print("y"); 
                        break;                                                         //
                    case "-BoundsZ":
                        newRayOffset = new Vector3(0, 0, repeatSpacingZ); //print("-z"); 
                        break;                                                         //
                    case "BoundsZ":
                        newRayOffset = new Vector3(0, 0, -repeatSpacingZ); //print("z"); 
                        break;
                    default:
                        Debug.LogError("The player has hit an unidentified bound");
                        break;
                }

                Vector3 newRayPos = hit.point + newRayOffset;
                Debug.DrawLine(rayStartPos, hit.point, Color.black, 5f);

                float newTotalRayDistance = totalRayDistance + hit.distance;
                // Recursively fire a new ray from the adjusted position
                FireRaycastServer(newRayPos, direction, remainingDist, newTotalRayDistance, true, shotFromClient, shotHitLocallyOnClient);
            }
            // Handle when the ray hits a wall
            else
            {
                Debug.DrawLine(rayStartPos, hit.point, Color.green, 5f);
            }
        }
        // Handle when the ray doesn't hit anything
        else
        {
            Debug.DrawRay(rayStartPos, direction * rayRange, Color.blue, 5f);
        }
    }

    private string HitFromBehind(Vector3 direction, RaycastHit hit)
    {
        Vector3 rayDirection = direction.normalized;
        float dotProduct = Vector3.Dot(rayDirection, hit.transform.forward);
        if (dotProduct < 0)
        {
            //Debug.Log("Hit from behind!");
            // Do something when hit from behind
            return "Front";
        }
        else
        {
            return "Behind";

            // Do something when hit from front or side
            //Debug.Log("Hit from front or side!");
        }
        //Debug.LogWarning(dotProduct);
    }

    void DrawLasersAndImpactParticle(Vector3 cameraHolderPosition, Vector3 gunTipPosition, Vector3 hitOffset, bool hitSomething, Vector3 hitNormal)
    {
        LineRenderer shootLineRender = GetPooledLineRenderer();
        shootLineRender.gameObject.SetActive(true);
        //shootLineRender.transform.parent = transform;

        // How it should be but neet to pass through
        shootLineRender.SetPosition(0, gunTipPosition);
        shootLineRender.SetPosition(1, cameraHolderPosition + hitOffset);
        
        StartCoroutine(DisableLineRender(shootLineRender, lineRendererDuration)); // disable after x secs

        if (hitSomething)
        {
            GameObject impactInstance = GetPooledImpactPoint();
            impactInstance.SetActive(true);
            impactInstance.transform.position = cameraHolderPosition + hitOffset + (hitNormal / 2f);
            StartCoroutine(DisableImpactPoint(impactInstance, lineRendererDuration)); // disable after x secs
        }

        for (int i = 0; i < duplicateManager.DuplicateControllers.Count; i++)
        {
            


            LineRenderer dupShootLineRender = GetPooledLineRenderer();
            dupShootLineRender.gameObject.SetActive(true);

            Transform dupGunTip = duplicateManager.DuplicateControllers[i].GunTip;
            Transform dupCamHolder = duplicateManager.DuplicateControllers[i].CameraHolder;
            dupShootLineRender.SetPosition(0, dupGunTip.position);
            dupShootLineRender.SetPosition(1, dupCamHolder.position + hitOffset);
            StartCoroutine(DisableLineRender(dupShootLineRender, lineRendererDuration)); // disable after x secs

            if (hitSomething)
            {
                GameObject impactInstance = GetPooledImpactPoint();

                Vector3 impacePos= dupCamHolder.position + hitOffset + (hitNormal / 2f);

                //If duplicate impact point is further than 100 away from centre, don't spawn it to save on performance. impact point from player is always spawned above
                if(Vector3.Distance(impacePos, Vector3.zero) > 50)
                {
                    continue;
                }
                impactInstance.SetActive(true);
                impactInstance.transform.position = impacePos;
                


                StartCoroutine(DisableImpactPoint(impactInstance, lineRendererDuration)); // disable after x secs
            }
        }
    }

    List<LineRenderer> lineRenderers = new List<LineRenderer>();
    List<GameObject> impactPoints = new List<GameObject>();

    void SpawnImpactPoints()
    {
        GameObject holder = Instantiate(new GameObject("ImpactPoints Holder"));
        for (int i = 0; i < 10 * duplicateManager.DuplicateControllers.Count; i++)
        {
            GameObject ip = Instantiate(impactPointGO);
            ip.gameObject.SetActive(false);
            ip.transform.parent = holder.transform;
            impactPoints.Add(ip);
        }
    }
    void SpawnLineRenderers()
    {
        GameObject holder = Instantiate(new GameObject("Line Renderer Holder"));
        for (int i = 0; i < 10 * duplicateManager.DuplicateControllers.Count; i++)
        {
            LineRenderer lr = Instantiate(lineRenderer);
            lr.gameObject.SetActive(false);
            lr.transform.parent = holder.transform;
            lineRenderers.Add(lr);
        }
    }
    private GameObject GetPooledImpactPoint()
    {
       
        // Cycle through the list of pooled bullets
        for (int i = 0; i < impactPoints.Count; i++)
        {
            // If bullet is not active, return it
            if (!impactPoints[i].gameObject.activeInHierarchy)
            {
                return impactPoints[i];
            }
        }

        // If all bullets are active, reuse the oldest one and move it to the end of the list
        GameObject oldestImpactPoint = impactPoints[0];
        impactPoints.RemoveAt(0);
        impactPoints.Add(oldestImpactPoint);
        return oldestImpactPoint;
    }
    
    private LineRenderer GetPooledLineRenderer()
    {
        // Cycle through the list of pooled bullets
        for (int i = 0; i < lineRenderers.Count; i++)
        {
            // If bullet is not active, return it
            if (!lineRenderers[i].gameObject.activeInHierarchy)
            {
                return lineRenderers[i];
            }
        }

        // If all bullets are active, reuse the oldest one and move it to the end of the list
        LineRenderer oldestLineRenderer = lineRenderers[0];
        lineRenderers.RemoveAt(0);
        lineRenderers.Add(oldestLineRenderer);
        return oldestLineRenderer;
    }
    IEnumerator DisableImpactPoint(GameObject ip, float duration)
    {
        yield return new WaitForSeconds(duration);
        ip.SetActive(false);
    }
    IEnumerator DisableLineRender(LineRenderer lr, float duration)
    {
        lr.widthMultiplier = 1;
        float time = 0f;
        
        while (time < 1)
        {
            time += Time.deltaTime / 0.25f;
            lr.widthMultiplier = Mathf.Lerp(1, 0, time);
            yield return null;
        }
        yield return new WaitForSeconds(duration);

        lr.gameObject.SetActive(false);
    }
}