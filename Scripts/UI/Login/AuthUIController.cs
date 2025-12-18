using UnityEngine;

public class AuthUIController : MonoBehaviour
{
    public GameObject loginPanel;
    public GameObject registerPanel;

    void Start()
    {
        ShowLogin();    // 시작은 로그인 화면
    }

    public void ShowLogin()
    {
        if (loginPanel) loginPanel.SetActive(true);
        if (registerPanel) registerPanel.SetActive(false);
    }

    public void ShowRegister()
    {
        if (loginPanel) loginPanel.SetActive(false);
        if (registerPanel) registerPanel.SetActive(true);
    }
}
