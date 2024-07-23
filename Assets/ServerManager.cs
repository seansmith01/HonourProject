using System.Collections.Generic;
using System.IO;
using FishNet.Object;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ServerManager : NetworkBehaviour
{
    public bool gameHasStarted;
    //public Button startButton;

    [Header("Stats For Game")]
    Dictionary<int, int> scores = new Dictionary<int, int>();
    Dictionary<int, int> shotsHitOnClientButNotOnServer = new Dictionary<int, int>();
    Dictionary<int, int> shotsHitOnServerButNotOnClient = new Dictionary<int, int>();

    [Header("Stats For Testing")]
    Dictionary<int, int> timesWorldWrapped = new Dictionary<int, int>();
    Dictionary<int, int> timesKilledSomeoneFromBehind = new Dictionary<int, int>();


    //Dictionary<int, Dictionary<int, KillInfo>> playerKillInfos = new Dictionary<int, Dictionary<int, KillInfo>>();
    Dictionary<int,  KillInfo> playerKillInfos = new Dictionary<int, KillInfo>();

    [SerializeField] float timerLeft = 10;
    [SerializeField] TextMeshProUGUI scoreboardText;
    [SerializeField] TextMeshProUGUI timerText;

    int playerCountZeroIndexed;

    // outer dictionary is the Player IDs, and the inner dictionary holds the kill distances for each player
    Dictionary<int, Dictionary<int, float>> playerKillDistances = new Dictionary<int, Dictionary<int, float>>();
    Dictionary<int, int> playerLastKillIDs = new Dictionary<int, int>();

    [SerializeField] GameObject endGameUi;
    //[SerializeField] GameObject networkHudCanvas;

    bool allPlayersReady;

    List<PlayerLocalManager> playerLocalManagers = new List<PlayerLocalManager>();

    public void Init(int _playerCountZeroIndexed)
    {
        GameHasStarted();

        endGameUi.SetActive(false);

        //SetGameObjectActiveOnAllClients(networkHudCanvas, false);

        if (!base.IsServer)
        {
            return;
        }

        playerCountZeroIndexed = _playerCountZeroIndexed;

        // Initialize disctionaries with how many players there are
        for (int i = 0; i < _playerCountZeroIndexed; i++)
        {
            scores.Add(i, 0);
            shotsHitOnClientButNotOnServer.Add(i, 0);
            shotsHitOnServerButNotOnClient.Add(i, 0);
            timesWorldWrapped.Add(i, 0);
            timesKilledSomeoneFromBehind.Add(i, 0);
            playerKillDistances.Add(i, new Dictionary<int, float>());
            playerLastKillIDs.Add(i, 0);
        }

        foreach(PlayerLocalManager playerLocalManager in FindObjectsByType<PlayerLocalManager>(FindObjectsSortMode.None))
        {
            playerLocalManagers.Add(playerLocalManager);
        }

    }
    [ObserversRpc]
    void GameHasStarted()
    {
        gameHasStarted = true;
    }
    private void Update()
    {
        if (gameHasStarted)
        {
            if (timerLeft > 0)
            {
                timerLeft -= Time.deltaTime;
                timerText.text = timerLeft.ToString("F0");
            }
        }
        if (!base.IsServer)
        {          
            return;
        }
        if (gameHasStarted)
        {
            InGameUpdate();
        }        
    }
    void InGameUpdate()
    {
        if (!allPlayersReady)
        {
            allPlayersReady = CheckEveryPlayerIsReady();
        }

        if (timerLeft < 0) // Check if timerLeft is less than or equal to 0
        {
            EndGame();
            EndGameClient();
            timerLeft = 0; // Reset timerLeft to 0 after calling EndGame()
        }



        if(Input.GetKeyDown(KeyCode.O))
        {
            for (int i = 0; i < playerLocalManagers.Count; i++)
            {
                playerLocalManagers[i].GetComponent<CSPMotor>().GoToZeroo = true;
               //playerLocalManagers[i].transform.position = new Vector3(transform.position.x, 0, transform.position.z);
               //playerLocalManagers[i].GetComponent<CSPMotor>().enabled = true;
            }
        }
    }
    private bool CheckEveryPlayerIsReady()
    {
        int playersReady = 0;

        for (int i = 0; i < playerLocalManagers.Count; i++)
        {
            if (playerLocalManagers[i].IsReady)
            {
                playersReady++;
            }
        }
        if (playersReady == playerLocalManagers.Count)
        {
            Debug.LogWarning("Evyone READY");

            //for (int i = 0; i < playerLocalManagers.Count; i++)
            //{
            //    if (playerLocalManagers[i].IsServer)
            //    {
            //        playerLocalManagers[i].GetComponent<CSPMotor>().enabled = true;
            //    }
                
            //    //playerLocalManagers[i].gameObject.SetActive(true);
               
            //}

            return true;
        }
        return false;
    }
    private void EndGame()
    {
        endGameUi.SetActive(true);
        // Path to save the CSV file
        string filePath = "data.csv";
        WriteNewLine(filePath);
        ExportDictionaryOfIntsToCSV(scores, filePath, "Score");
        ExportDictionaryOfIntsToCSV(shotsHitOnClientButNotOnServer, filePath, "Shots Hit On Client But Not On Server");
        ExportDictionaryOfIntsToCSV(shotsHitOnServerButNotOnClient, filePath, "Shots Hit On Server But Not On Client");
        ExportDictionaryOfIntsToCSV(timesWorldWrapped, filePath, "Times World Wrapped");

        ExportDictionaryOfKillInfoToCSV(playerKillInfos, filePath);
    }
    [ObserversRpc]
    void EndGameClient()
    {
        endGameUi.SetActive(true);
    }
    public void UpdateShotsHitOnClientButNotOnServer(int ID)
    {
        shotsHitOnClientButNotOnServer[ID]++;
    }
    public void UpdateShotsHitOnServerButNotOnClient(int ID)
    {
        shotsHitOnServerButNotOnClient[ID]++;
    }
    public void UpdateScore(int ID, string wasFromBehind, bool shotWorldWrapped, float shotDistance)
    {
        KillInfo newKillInfo = new KillInfo
        {
            PlayerID = ID,
            ShotWasFromBehind = wasFromBehind,
            ShotDidWorldWrap = shotWorldWrapped,
            ShotDistance = shotDistance
        };
        if (playerKillInfos.Count == 0)
        {
            playerKillInfos.Add(0, newKillInfo);

        }
        else
        {
            playerKillInfos.Add(playerKillInfos.Values.Count + 1, newKillInfo);
        }
        
        scores[ID]++;
        UpdateScoreboard(scores);
    }
    [ObserversRpc]
    private void UpdateScoreboard(Dictionary<int, int> _scores)
    {
        string scoreboardString = "Scoreboard:\n";
        foreach (var kvp in _scores)
        {
            scoreboardString += "Player " + (kvp.Key + 1) + ": " + kvp.Value + "\n";
        }
        scoreboardText.text = scoreboardString;
    }

    public void UpdateTimesWorldWrapped(int ID)
    {
        timesWorldWrapped[ID]++;
    }

    // To get kill distances for a player (OwnerID)
    public Dictionary<int, float> GetKillDistances(int ownerId)
    {
        // Check if the player's kill distances dictionary exists
        if (playerKillDistances.ContainsKey(ownerId))
        {
            // Return the dictionary containing the kill distances for the player
            return playerKillDistances[ownerId];
        }
        else
        {
            // If the player's kill distances dictionary doesn't exist, return an empty dictionary
            return new Dictionary<int, float>();
        }
    }
    static void WriteNewLine(string filePath)
    {
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            // Write CSV header
            writer.WriteLine("");
            writer.WriteLine("New Game");
            writer.WriteLine("");
        }
    }

    static void ExportDictionaryOfIntsToCSV(Dictionary<int, int> dictionary, string filePath, string keyName)
    {
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            // Write CSV header
            writer.WriteLine("Player ID" + keyName);

            // Write dictionary contents
            foreach (var kvp in dictionary)
            {
                writer.WriteLine($"Player:{kvp.Key},{kvp.Value}");
            }
        }
    }

    void ExportDictionaryOfKillInfoToCSV(Dictionary<int, KillInfo> dictionary, string filePath)
    {
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            // Write CSV header
            writer.WriteLine("Ignore,Player ID,ShotWorldWrapped,ShotWasFromBehind, ShootDistance");
            // Write dictionary contents
            foreach (var kvp in dictionary)
            {
               // for (int i = 0; i < kvp.Value.KillID; i++)
               // {
               //     writer.WriteLine($"{kvp.Key},{i},{kvp.Value.ShotDidWorldWrap},{kvp.Value.ShotWasFromBehind},{kvp.Value.ShotDistance}");
               //
               // }

                writer.WriteLine($"{kvp.Key},{kvp.Value.PlayerID},{kvp.Value.ShotDidWorldWrap},{kvp.Value.ShotWasFromBehind},{kvp.Value.ShotDistance}");
            }
        }
    }

    [ObserversRpc]
    private void SetGameObjectActiveOnAllClients(GameObject gameObject, bool setActive)
    {
        gameObject.SetActive(setActive);    
    }
}
public struct KillInfo
{
    public int PlayerID;
    //public int KillID;
    public bool ShotDidWorldWrap;
    public string ShotWasFromBehind;
    public float ShotDistance;
}