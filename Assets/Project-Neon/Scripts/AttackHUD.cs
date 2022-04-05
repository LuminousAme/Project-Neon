using UnityEngine;
using UnityEngine.UI;

public class AttackHUD : MonoBehaviour
{
    [SerializeField] private GameObject center;
    [SerializeField] private GameObject left;
    [SerializeField] private GameObject right;

    private Image rightUI;
    private Image leftUI;
    private Image centerUI;

    private BasicPlayerController player;

    public void SetPlayer(BasicPlayerController p) => player = p;

    private Vector3 startPositionR;
    private Vector3 startPositionL;
    private Vector3 endPositionR;
    private Vector3 endPositionL;
    private float distanceL;
    private float distanceR;

    // Start is called before the first frame update
    private void Start()
    {
        rightUI = right.transform.GetComponent<Image>();
        leftUI = left.transform.GetComponent<Image>();
        centerUI = center.transform.GetComponent<Image>();

        startPositionR = right.transform.localPosition;
        startPositionL = left.transform.localPosition;

        distanceL = Vector3.Distance(startPositionL, centerUI.transform.localPosition);
        distanceR = Vector3.Distance(startPositionR, centerUI.transform.localPosition);

        //endPositionR = new Vector3(startPositionR.x - right.transform.localScale.x * 5.3f, startPositionR.y, 0);
        //endPositionL = new Vector3(startPositionL.x + left.transform.localScale.x * 5.3f, startPositionL.y, 0);
        endPositionR = new Vector3(startPositionR.x - (distanceR * 1.05f) - (right.transform.localScale.x * 2), startPositionR.y, 0);
        endPositionL = new Vector3(startPositionL.x + (distanceL * 1.05f) + (left.transform.localScale.x * 2), startPositionL.y, 0);
    }

    // Update is called once per frame
    private void Update()
    {
        if (player.GetAttackDown())
        {
            //Debug.Log("HUD");
            rightUI.transform.localPosition = Vector3.Lerp(startPositionR, endPositionR, player.GetAttackDownTime() / player.GetHeavyAttackTime());
            leftUI.transform.localPosition = Vector3.Lerp(startPositionL, endPositionL, player.GetAttackDownTime() / player.GetHeavyAttackTime());
            Debug.Log(startPositionR.ToString());
        }
        else
        {
            rightUI.transform.localPosition = startPositionR;
            leftUI.transform.localPosition = startPositionL;
        }
    }
}