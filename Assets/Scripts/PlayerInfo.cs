using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    public static PlayerInfo instance;

    private void OnEnable()
    {
        if (PlayerInfo.instance == null)
        {
            PlayerInfo.instance = this;
        }
        else
        {
            if (PlayerInfo.instance != this)
            {
                Destroy(this.gameObject);
            }
        }
        DontDestroyOnLoad(this.gameObject);
    }

    private string userEmail;
    private string userPassword;
    private string username;

    public string GetUserEmail() => userEmail;
    public string GetUserPassword() => userPassword;
    public string GetUsername() => username;

    public void SetUserEmail(string emailIn) => userEmail = emailIn;
    public void SetUserPassword(string passwordIn) => userPassword = passwordIn;
    public void SetUsername(string usernameIn) => username = usernameIn;

    private int playerPops;
    public int GetPlayerPops() => playerPops;
    public int SetPlayerPops(int popsIn) => playerPops = popsIn;
    public void IncreasePops() => playerPops++;
}
