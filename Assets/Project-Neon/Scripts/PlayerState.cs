using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    [SerializeField] private PlayerData basicData;
    [SerializeField] private bool useSavedName;
    private int hp;
    private int damageDealt;
    private int killsObtained;
    private int timesDied;
    private string playerName;
    private int playerId;

    public delegate void HandleRespawn(PlayerState player);
    public static event HandleRespawn onRespawn;

    public delegate void HandleNewKill(PlayerState player);
    public static event HandleNewKill onNewKill;

    public int GetHP() => hp;
    public int GetDamageDealt() => damageDealt;
    public int GetKillCount() => killsObtained;
    public int GetTimesDied() => timesDied;
    public string GetDisplayName() => playerName;
    public int GetPlayerID() => playerId;
    public void SetPlayerID(int id) => playerId = id;

    private void Awake()
    {
        ReadNameFromFile();
    }

    void Start()
    {
        RestartGame();
    }

    void Respawn()
    {
        hp = basicData.GetMaxHealth();
    }

    void RestartGame()
    {
        hp = basicData.GetMaxHealth();
        damageDealt = 0;
        killsObtained = 0;
        timesDied = 0;
        ReadNameFromFile();
    }

    public void ReadNameFromFile()
    {
        playerName = PlayerPrefs.GetString("DisplayName", "Hunter");
    }

    //takes the passed in ammount of damage, and returns true if it killed
    public bool TakeDamage(int damage, out int cappedDamage)
    {
        cappedDamage = Mathf.Clamp(damage, 0, hp);
        hp = Mathf.Clamp(hp - damage, 0, basicData.GetMaxHealth());
        if (hp == 0)
        {
            Respawn();
            onRespawn?.Invoke(this);
            timesDied++;
            return true;
        }

        return false;
    }

    public bool TakeDamage(int damage)
    {
        int useless;
        return TakeDamage(damage, out useless);
    }

    //adds to the total ammount of damage dealth by this player
    public void DealDamage(int damage, bool wasKill = false)
    {
        damageDealt += damage;
        if (wasKill)
        {
            killsObtained++;
            onNewKill?.Invoke(this);
        }
    }

    public int GetBounty()
    {
        return killsObtained * 100 + damageDealt;
    }
}