using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] Slider healthBar;
    [SerializeField] Image healthImage;
    [SerializeField] PlayerData basicData;
    [SerializeField] float healthBarFadeSpeed = 5f;
    float acutalHealth;
    [SerializeField] Gradient barColor;
    private void Start()
    {
        healthBar = GetComponent<Slider>();
        healthBar.maxValue = basicData.GetMaxHealth();
        healthBar.value = basicData.GetMaxHealth();
        acutalHealth = basicData.GetMaxHealth();
    }

    public void SetHealth(float hp)
    {
        acutalHealth = hp;
    }

    public void HardSetHealth(float hp)
    {
        acutalHealth = hp;
        healthBar.value = hp;
    }

    private void Update()
    {
        //healthBar.value = Mathf.RoundToInt(Mathf.Lerp(healthBar.value, acutalHealth, Time.deltaTime * healthBarFadeSpeed));
        healthBar.value = acutalHealth;
        healthImage.color = barColor.Evaluate(healthBar.normalizedValue);
    }
}