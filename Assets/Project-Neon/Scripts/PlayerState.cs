using System;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    [SerializeField] private PlayerData basicData;
    private bool useSavedName;
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
    public static float hpRegenRate = 1f;

    public delegate void HandleRespawn(PlayerState player);

    public static event HandleRespawn onRespawn;

    public delegate void HandleNewKill(PlayerState player);

    public static event HandleNewKill onNewKill;

    public delegate void HandleLocalHPChange(PlayerState player);

    public static event HandleLocalHPChange onHPChange;

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

    public void SetUseName(bool useSavedName, string name)
    {
        this.useSavedName = useSavedName;
        if (!this.useSavedName) playerName = name;
        else ReadNameFromFile();
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
        Debug.Log("Restarted");
    }

    public void ReadNameFromFile()
    {
        if (useSavedName) playerName = PlayerPrefs.GetString("DisplayName", "Hunter");
    }

    //takes the passed in ammount of damage, and returns true if it killed
    public bool TakeDamage(int damage, out int cappedDamage, Vector3 hitpos)
    {
        damaged = true;
        healTimer = 0;
        float scale = (damage == 10) ? 1 : 2;

        cappedDamage = Mathf.Clamp(damage, 0, (int)hp);
        hp = Mathf.Clamp(hp - damage, 0, basicData.GetMaxHealth());
        if (hp == 0)
        {
            Respawn();
            onRespawn?.Invoke(this);
            timesDied++;

            if (AsyncClient.instance != null) AsyncClient.instance.UpdateHP(playerId, hp, hitpos, scale);

            return true;
        }

        if (AsyncClient.instance != null) AsyncClient.instance.UpdateHP(playerId, hp, hitpos, scale);

        return false;
    }

    public bool TakeDamage(int damage, Vector3 hitpos)
    {
        int useless;
        return TakeDamage(damage, out useless, hitpos);
    }

    private void Update()
    {
        healTimer = healTimer + Time.deltaTime;
        //Debug.Log(damaged);
        //if the player hasn't been damaged for the  delay amount then they are not longer damaged and can heal
        if (healTimer >= healDelay)
        {
            damaged = false;
        }

        //if player hasn't been damaged then heal
        if (!damaged)
        {
            //uncomment this when we figure out how to make it work with netcode
            //hp = Mathf.Clamp(hp + hpRegenRate * Time.deltaTime, 0, basicData.GetMaxHealth());
        }
        //if (Input.GetKeyDown(KeyCode.F4))
        //{
        //    TakeDamage(10, new Vector3(1000f, 1000f, 1000f));
        //}
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

        //update the other clients with our score
        if (AsyncClient.instance != null) AsyncClient.instance.UpdateScore(playerId, killsObtained, damageDealt);
    }

    public int GetBounty()
    {
        return killsObtained * 100 + damageDealt;
    }

    public void RemoteUpdateBounty(int newKillCount, int damageDealt)
    {
        killsObtained = newKillCount;
        this.damageDealt = damageDealt;
    }

    public void RemoteUpdateHP(float newHP)
    {
        if (hp < newHP && newHP > 95f) onRespawn?.Invoke(this);
        hp = newHP;
    }
}