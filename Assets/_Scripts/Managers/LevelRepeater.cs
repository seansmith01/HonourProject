using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using com.cyborgAssets.inspectorButtonPro;
using Unity.VisualScripting;
public class LevelRepeater : MonoBehaviour
{
    // Header: Colours
    [Header("Colours")]
    public int ColourIndex;
    public Color[] FogColours;

    // Header: Stuffs
    [Header("Stuffs")]
    [SerializeField] private bool doGPUInstancing;
    [SerializeField] private bool frustrumCulling;
    [SerializeField] private Camera startCamera;
    [SerializeField] private Vector3 repeatAmount;
    public Vector3 RepeatSpacing;

    // Bounds
    [Header("Fog")]
    [SerializeField] private float fogStartDistance;
    [SerializeField] private float fogEndDistance;

    // Level Objects
    [Header("Level Objects")]
    private GameObject level;
    private GameObject levelClonesHolder;

    // Bounds Transforms
    [Header("Bounds Transforms")]
    [SerializeField] private Transform minusBoundsX;
    [SerializeField] private Transform positiveBoundsX;
    [SerializeField] private Transform minusBoundsY;
    [SerializeField] private Transform positiveBoundsY;
    [SerializeField] private Transform minusBoundsZ;
    [SerializeField] private Transform positiveBoundsZ;

    // Start is called before the first frame update
    void Start()
    {
        //if(repeatAmountX==0) { Debug.LogError("RepeatAmountNotSet"); return; }
        //if(repeatAmountY==0) { Debug.LogError("RepeatAmountNotSet"); return; }
        //if(repeatAmountZ==0) { Debug.LogError("RepeatAmountNotSet"); return; }

        GenerateRaycastBounds();
        DestroyLevelHolderInGame();
        RepeatLevel();

        startCamera.backgroundColor = FogColours[ColourIndex];
    }

    [ProButton]
    void RepeatLevel()
    {
        if (levelClonesHolder != null)
        {
            DestroyImmediate(levelClonesHolder);
        }
        levelClonesHolder = new GameObject("LevelClonesHolder");

        //for (int i = 0; i < transform.childCount; i++)
        //{
        //    if (transform.GetChild(i).gameObject.activeSelf)
        //    {
        //        level = transform.GetChild(i).gameObject;
        //    }
        //}
        level = gameObject;



        for (float x = -RepeatSpacing.x * repeatAmount.x; x <= repeatAmount.x * RepeatSpacing.x; x += RepeatSpacing.x)
        {
            for (float y = -RepeatSpacing.y * repeatAmount.y; y <= repeatAmount.y * RepeatSpacing.y; y += RepeatSpacing.y)
            {
                for (float z = -RepeatSpacing.z * repeatAmount.z; z <= repeatAmount.z * RepeatSpacing.z; z += RepeatSpacing.z)
                {

                    if ((x == 0) && (y == 0) && (z == 0))
                    {
                        // continue;
                    }

                    GameObject levelClone = Instantiate(gameObject, new Vector3(x, y, z), Quaternion.identity);
                    Destroy(levelClone.GetComponent<LevelRepeater>());

                    //DisableColliders(levelClone);

                    if (doGPUInstancing)
                    {
                        foreach (Renderer rend in levelClone.GetComponentsInChildren<Renderer>())
                        {
                            rend.sharedMaterial.enableInstancing = true;
                        }
                    }

                    levelClone.name = "levelClone";
                    levelClone.transform.parent = levelClonesHolder.transform;

                    // Delete this instance of the level as its create in the grid
                    // Do this by deleting the children as variables in this scipt are accessed by others
                }
            }
        }
        void DeleteAllChildren()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                Destroy(child.gameObject);

            }
        }
        //level.SetActive(false);
        // transform.eulerAngles = new Vector3(0, 0, 45);
        //float maxRepeatSpacing = GetMaxRepeatSpacing(RepeatSpacing.x, RepeatSpacing.y, RepeatSpacing.z);
        RenderSettings.fogStartDistance = fogStartDistance;
        RenderSettings.fogEndDistance = fogEndDistance;
        RenderSettings.fogColor = FogColours[ColourIndex];
    }

    [ProButton]
    void DestroyLevelHolder()
    {
        if (levelClonesHolder != null)
        {
            DestroyImmediate(levelClonesHolder.gameObject);
        }
    }
    void DestroyLevelHolderInGame()
    {
        if (levelClonesHolder != null)
        {
            DestroyImmediate(levelClonesHolder.gameObject);
        }
    }
    [ProButton]
    void GenerateRaycastBounds()
    {
        Vector3 halfRepeatSpacing = RepeatSpacing / 2;

        minusBoundsX.position = new Vector3(-halfRepeatSpacing.x - 1, 0, 0);
        minusBoundsX.localScale = new Vector3(2, RepeatSpacing.y, RepeatSpacing.z);

        positiveBoundsX.position = new Vector3(halfRepeatSpacing.x + 1, 0, 0);
        positiveBoundsX.localScale = minusBoundsX.localScale;


        minusBoundsY.position = new Vector3(0, -halfRepeatSpacing.y - 1, 0);
        minusBoundsY.localScale = new Vector3(RepeatSpacing.x, 2, RepeatSpacing.z);

        positiveBoundsY.position = new Vector3(0, halfRepeatSpacing.y + 1, 0);
        positiveBoundsY.localScale = minusBoundsY.localScale;


        minusBoundsZ.position = new Vector3(0, 0, -halfRepeatSpacing.z - 1);
        minusBoundsZ.localScale = new Vector3(RepeatSpacing.x, RepeatSpacing.y, 2);

        positiveBoundsZ.position = new Vector3(0, 0, halfRepeatSpacing.z + 1);
        positiveBoundsZ.localScale = minusBoundsZ.localScale;
    }
    [ProButton]
    void SeeBounds()
    {
        ToggleRendererVisibility(minusBoundsX);
        ToggleRendererVisibility(positiveBoundsX);
        ToggleRendererVisibility(minusBoundsY);
        ToggleRendererVisibility(positiveBoundsY);
        ToggleRendererVisibility(minusBoundsZ);
        ToggleRendererVisibility(positiveBoundsZ);
    }
    [ProButton]
    void ToggleFog()
    {
        RenderSettings.fog = !RenderSettings.fog;
    }
    // Helper method to toggle the visibility of a MeshRenderer component
    private void ToggleRendererVisibility(Transform bound)
    {
        if (bound.GetComponent<MeshRenderer>() != null)
        {
            bound.GetComponent<MeshRenderer>().enabled = !bound.GetComponent<MeshRenderer>().enabled;
        }
        else
        {
            Debug.LogWarning("MeshRenderer component is null.");
        }
    }

    private void Update()
    {
        //RenderSettings.fogStartDistance = RepeatSpacing * startDistanceMultiplier;
        //RenderSettings.fogEndDistance = RepeatSpacing * endDistanceMultiplier;


        //RenderSettings.fogDensity = repeatSpacing * (fogMultiplier * 0.01f);
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
    // Enable or disable colliders based on whether it's the first iteration or not
    private void DisableColliders(GameObject levelClone)
    {
        foreach (Collider c in levelClone.GetComponentsInChildren<Collider>())
        {
            Destroy(c);
            //c.enabled = false;
        }
    }
}
