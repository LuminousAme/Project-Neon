using UnityEngine;
using UnityEngine.UI;

public class DashBar : MonoBehaviour
{
    [SerializeField] private GameObject dash1BarOBJ;
    [SerializeField] private GameObject dash2BarOBJ;
    [SerializeField] private Image dash1Bar;

    private Slider dash1BarValue;
    private Slider dash2BarValue;
    [SerializeField] private Image dash2Bar;

    [SerializeField] private PlayerMoveSettings movementSettings;

    private BasicPlayerController player;

    public void SetPlayer(BasicPlayerController p) => player = p;

    // Start is called before the first frame update
    private void Start()
    {
        dash1BarValue = dash1BarOBJ.GetComponent<Slider>();
        dash2BarValue = dash2BarOBJ.GetComponent<Slider>();
        //each bar represents one jump
        dash1BarValue.maxValue = movementSettings.GetDashCooldown();
        dash2BarValue.maxValue = movementSettings.GetDashCooldown();

        dash1BarValue.value = movementSettings.GetDashCooldown();
        dash2BarValue.value = movementSettings.GetDashCooldown();
    }

    // Update is called once per frame
    private void Update()
    {
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
        }
        Debug.Log(player.GetTimeSinceLastDash());
    }
}