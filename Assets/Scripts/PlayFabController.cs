using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;

using PlayFab.Json;
using PlayFab.DataModels;
using PlayFab.ProfilesModels;
using System.Collections.Generic;

public class PlayFabController : MonoBehaviour
{
    public static PlayFabController instance;
    public GameObject titleScreen;
    public GameObject gameScreen;
    public Text message;
    public Text coinCounter;

    private void OnEnable()
    {
        if (PlayFabController.instance == null)
        {
            PlayFabController.instance = this;
        }
        else
        {
            if (PlayFabController.instance != this)
            {
                Destroy(this.gameObject);
            }
        }
        DontDestroyOnLoad(this.gameObject);
    }
    
    public void Start()
    {
        // Note: Setting title Id here can be skipped if you have set the value in Editor Extensions already.
        if (string.IsNullOrEmpty(PlayFabSettings.TitleId))
        {
            PlayFabSettings.TitleId = "2BAF4"; // Please change this value to your own titleId from PlayFab Game Manager
        }
    }

    #region Login
    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        var loginRequest = new LoginWithEmailAddressRequest { Email = PlayerInfo.instance.GetUserEmail(), Password = PlayerInfo.instance.GetUserPassword() };
        PlayFabClientAPI.LoginWithEmailAddress(loginRequest, OnLoginSuccess, OnLoginFailure);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        titleScreen.transform.GetChild(0).gameObject.SetActive(true);
        titleScreen.transform.GetChild(1).gameObject.SetActive(false);
        titleScreen.SetActive(false);
        gameScreen.SetActive(true);
        message.text = "";

        GetStats();
    }

    private void OnRegisterFailure(PlayFabError error)
    {
        DisplayErrorMessage(error);
    }

    private void OnLoginFailure(PlayFabError error)
    {
        DisplayErrorMessage(error);
    }

    private void DisplayErrorMessage(PlayFabError error)
    {
        switch (error.Error)
        {
            case PlayFabErrorCode.InvalidEmailOrPassword:
                message.text = "Invalid Email or Password";
                break;
            case PlayFabErrorCode.AccountNotFound:
                message.text = "User Not Found";
                break;
            case PlayFabErrorCode.InvalidParams: // Email格式不符、密碼長度過短等
                message.text = "Invalid Input";
                break;
            case PlayFabErrorCode.EmailAddressNotAvailable:
                message.text = "Email Address Not Available";
                break;
            default:
                message.text = "Unknown Error";
                Debug.LogError(error.GenerateErrorReport());
                Debug.Log("Failed to log in");
                Debug.Log(error.Error);
                break;
        }
    }

    public void OnClickLogin()
    {
        var loginRequest = new LoginWithEmailAddressRequest { Email = PlayerInfo.instance.GetUserEmail(), Password = PlayerInfo.instance.GetUserPassword() };
        PlayFabClientAPI.LoginWithEmailAddress(loginRequest, OnLoginSuccess, OnLoginFailure);
    }

    public void OnClickSignUp()
    {
        var registerRequest = new RegisterPlayFabUserRequest { Email = PlayerInfo.instance.GetUserEmail(), Password = PlayerInfo.instance.GetUserPassword(), Username = PlayerInfo.instance.GetUsername(), DisplayName = PlayerInfo.instance.GetUsername() };
        PlayFabClientAPI.RegisterPlayFabUser(registerRequest, OnRegisterSuccess, OnRegisterFailure);
    }
    #endregion Login

    #region LogOut

    public void OnClickLogOut()
    {
        PlayFabClientAPI.ForgetAllCredentials();
        gameScreen.SetActive(false);
        titleScreen.SetActive(true);
    }

    #endregion LogOut

    #region PlayerStats
    public void SetStats()
    {
        PlayFabClientAPI.UpdatePlayerStatistics
        ( 
            new UpdatePlayerStatisticsRequest 
            {
                // request.Statistics is a list, so multiple StatisticUpdate objects can be defined if required.
                Statistics = new List<StatisticUpdate> 
                {
                    new StatisticUpdate { StatisticName = "PlayerPops", Value = PlayerInfo.instance.GetPlayerPops() }
                }
            },
            result => { coinCounter.text = PlayerInfo.instance.GetPlayerPops().ToString(); },
            error => { Debug.LogError(error.GenerateErrorReport()); }
        );
    }

    void GetStats()
    {
        PlayFabClientAPI.GetPlayerStatistics
        (
            new GetPlayerStatisticsRequest(),
            OnGetStats,
            error => Debug.LogError(error.GenerateErrorReport())
        );
    }

    void OnGetStats(GetPlayerStatisticsResult result)
    {
        foreach (var eachStat in result.Statistics)
        {
            Debug.Log("Received the following Statistics: Statistic (" + eachStat.StatisticName + "): " + eachStat.Value);
            switch(eachStat.StatisticName)
            {
                case "PlayerPops":
                    coinCounter.text = eachStat.Value.ToString();
                    PlayerInfo.instance.SetPlayerPops(eachStat.Value);
                    break;
            }
        }
    }

    /*
    // Build the request object and access the API
    public void StartCloudUpdatePlayerStats()
    {
        // Send a request
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "UpdatePlayerStats", // Arbitrary function name (must exist in your uploaded cloud.js file)
            FunctionParameter = new { PlayerPops = PlayerInfo.instance.GetPlayerPops() }, // The parameter provided to your function
            GeneratePlayStreamEvent = true, // Optional - Shows this event in PlayStream
        }, OnCloudUpdateStats, OnErrorShared);
    }
    // OnCloudHelloWorld defined in the next code block

    private static void OnCloudUpdateStats(ExecuteCloudScriptResult result) 
    {
        // CloudScript returns arbitrary results, so you have to evaluate them one step and one parameter at a time
        Debug.Log(PlayFab.PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer));
        JsonObject jsonResult = (JsonObject)result.FunctionResult;
        object messageValue;
        jsonResult.TryGetValue("messageValue", out messageValue); // note how "messageValue" directly corresponds to the JSON values set in CloudScript
        Debug.Log((string)messageValue);
    }

    private static void OnErrorShared(PlayFabError error)
    {
        Debug.Log(error.GenerateErrorReport());
    }
    */
    #endregion PlayerStats

    public GameObject leaderboardScreen;
    public GameObject listingPrefab;
    public Transform listingContainer;

    #region Leaderboard
    public void GetLeaderboard()
    {
        var requestLeaderboard = new GetLeaderboardRequest { StartPosition = 0, StatisticName = "PlayerPops", MaxResultsCount = 20 }; // 取得前20名的成績
        PlayFabClientAPI.GetLeaderboard(requestLeaderboard, OnGetLeaderboard, OnErrorLeaderboard);
    }

    void OnGetLeaderboard(GetLeaderboardResult result)
    {
        leaderboardScreen.SetActive(true);
        int rank = 1;

        foreach(PlayerLeaderboardEntry player in result.Leaderboard)
        {
            GameObject tempListing = Instantiate(listingPrefab, listingContainer);
            ListingPrefab listing = tempListing.GetComponent<ListingPrefab>();
            listing.playerNameText.text = rank++ + " - " + player.DisplayName;
            listing.playerPopsText.text = player.StatValue.ToString();
        }
    }

    void OnErrorLeaderboard(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }

    public void CloseLeaderboard()
    {
        leaderboardScreen.SetActive(false);
        for(int i = listingContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(listingContainer.GetChild(i).gameObject);
        }
    }
    #endregion Leaderboard
}