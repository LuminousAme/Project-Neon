using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerDefaultData", menuName = "ProjectNeon/Player")]
public class PlayerData : ScriptableObject
{
    [SerializeField] private int maxHealth = 100;
    public int GetMaxHealth() => maxHealth;
}