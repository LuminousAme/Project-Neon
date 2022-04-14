using UnityEngine;
using UnityEngine.UI;

public class DashBar : MonoBehaviour
{
    [SerializeField] private GameObject dash1BarOBJ;
    [SerializeField] private GameObject dash2BarOBJ;

    private Image dash1BarValue;
    float value1;
    private Image dash2BarValue;
    float value2;

    [SerializeField] private PlayerMoveSettings movementSettings;

    private BasicPlayerController player;

    public void SetPlayer(BasicPlayerController p) => player = p;

    // Start is called before the first frame update
    private void Start()
    {
        dash1BarValue = dash1BarOBJ.transform.GetChild(0).GetComponent<Image>();
        dash2BarValue = dash2BarOBJ.transform.GetChild(0).GetComponent<Image>();
        //each bar represents one jump
        value1 = 1f;
        value2 = 1f;

        dash1BarValue.fillAmount = movementSettings.GetDashCooldown();
        dash2BarValue.fillAmount = movementSettings.GetDashCooldown();
    }

    // Update is called once per frame
    private void Update()
    {
        value1 = MathUlits.ReMapClamped(movementSettings.GetDashCooldown(), 0f, 0f, 1f, player.GetDashTotalCooldown());
        value2 = MathUlits.ReMapClamped(2f * movementSettings.GetDashCooldown(), movementSettings.GetDashCooldown(), 0f, 1f, player.GetDashTotalCooldown());

        dash1BarValue.fillAmount = value1;
        dash2BarValue.fillAmount = value2;

        /*
        if (player.GetNumOfDashesTaken() == 0)
        {
            value1 = 1f;
            value2 = 2f;
        }
        else if (player.GetNumOfDashesTaken() == 1)
        {
            totalValue = MathUlits.ReMapClamped(movementSettings.GetDashCooldown(), 0f, 1f, 2f, player.GetTimeSinceLastDashAtIndex(0));
        }
        else if (player.GetNumOfDashesTaken() == 2)
        {
            totalValue = MathUlits.ReMapClamped(movementSettings.GetDashCooldown(), 0f, 0f, 1f, player.GetTimeSinceLastDashAtIndex(1));
        }*/

        /*
        if (player.GetNumOfDashesTaken() == 1)
        {
            //one dash used
           // dash1BarValue.value = 0;
            dash1BarValue.value = player.GetTimeSinceLastDash();

        }
        else if (player.GetNumOfDashesTaken() == 2)
        {
            dash2BarValue.value = 0;
        }
        else
        {
            dash1BarValue.value = movementSettings.GetDashCooldown();
            dash2BarValue.value = movementSettings.GetDashCooldown();
            //   player.GetTimeSinceLastDash();
        }*/
        //Debug.Log(player.GetTimeSinceLastDash());
    }
}