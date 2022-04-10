using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    HUDManager hud;
    public void SetHUD(HUDManager hud) => this.hud = hud;

    [SerializeField] MenuButton[] buttons;

    public void OnClickContinue()
    {
        hud.ChangePauseState(false);
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].UnClick();
            buttons[i].OnStopHover();
        }
    }

    public void OnClickOptions()
    {
        //load the options menu
        if (!SceneManager.GetSceneByBuildIndex(4).isLoaded) SceneManager.LoadScene(4, LoadSceneMode.Additive);
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].UnClick();
            buttons[i].OnStopHover();
        }
    }

    public void OnClickQuit()
    {
        //disconnect from the game
        if (AsyncClient.instance != null) AsyncClient.instance.SendDisconnect();

        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].UnClick();
            buttons[i].OnStopHover();
        }
    }
}
