using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] Image healthImage;
    [SerializeField] PlayerData basicData;
    private void Start()
    {
        SetHealth(basicData.GetMaxHealth());
    }

    public void SetHealth(float hp)
    {
        healthImage.fillAmount = MathUlits.ReMapClamped(0f, basicData.GetMaxHealth(), 0f, 1f, hp);
    }

    public void HardSetHealth(float hp)
    {
        healthImage.fillAmount = MathUlits.ReMapClamped(0f, basicData.GetMaxHealth(), 0f, 1f, hp);
    }
}