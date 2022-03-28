using System;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    [SerializeField] private PlayerData basicData;
    [SerializeField] private bool useSavedName;
    private float hp;
    private int damageDealt;
    private int killsObtained;
    private int timesDied;
    private string playerName;
    private bool damaged;
    [SerializeField] private int healDelay = 6;
    private float healTimer;
    private Guid playerId;
    [SerializeField] private bool overrideIdForDebug = false;

    public delegate void HandleRespawn(PlayerState player);

    public static event HandleRespawn onRespawn;

    public delegate void HandleNewKill(PlayerState player);

    public static event HandleNewKill onNewKill;

    public float GetHP() => hp;

    // public int GetHP() => hp;

    public int GetDamageDealt() => damageDealt;

    public int GetKillCount() => killsObtained;

    public int GetTimesDied() => timesDied;

    public string GetDisplayName() => playerName;

    public Guid GetPlayerID() => playerId;

    public void SetPlayerID(Guid id)
    {
        if (!overrideIdForDebug) playerId = id;
        else playerId = Guid.NewGuid();
    }

    private void Awake()
    {
        ReadNameFromFile();
    }

    private void Start()
    {
        // healDelay = 6;
        RestartGame();
        if (overrideIdForDebug) playerId = new Guid();
    }

    private void Respawn()
    {
        hp = basicData.GetMaxHealth();
    }

    private void RestartGame()
    {
        hp = basicData.GetMaxHealth();
        damageDealt = 0;
        damaged = false;
        healTimer = 0;
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
        damaged = true;
        healTimer = 0;
        cappedDamage = Mathf.Clamp(damage, 0, (int)hp);
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

    private void Update()
    {
        healTimer = healTimer + Time.deltaTime;
        Debug.Log(damaged);
        //if the player hasn't been damaged for the  delay amount then they are not longer damaged and can heal
        if (healTimer >= healDelay)
        {
            damaged = false;
        }

        //if player hasn't been damaged then heal
        if (!damaged)
        {
            hp = Mathf.Clamp(hp + 0.1f, 0, basicData.GetMaxHealth());
        }
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