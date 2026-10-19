
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Core;
using Unity.Services.Authentication;

public class LoginManager : MonoBehaviour
{
    [Header("Login UI")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_Text messageText;

    [Header("Game Settings")]
    public string gameSceneName = "SampleScene";

    private bool servicesReady = false;
    private bool isProcessing = false;

    private async void Start()
    {
        if (messageText != null)
            messageText.text = "กำลังเชื่อมต่อระบบบัญชี...";

        try
        {
            await UnityServices.InitializeAsync();
            servicesReady = true;

            if (messageText != null)
                messageText.text = "กรุณาเข้าสู่ระบบ";
        }
        catch (Exception e)
        {
            ShowMessage("เชื่อมต่อระบบไม่สำเร็จ กรุณาตรวจสอบอินเทอร์เน็ต");
            Debug.LogException(e);
        }
    }

    public async void Register()
    {
        if (!CanProceed())
            return;

        isProcessing = true;

        try
        {
            await AuthenticationService.Instance
                .SignUpWithUsernamePasswordAsync(
                    usernameInput.text.Trim(),
                    passwordInput.text
                );

            OpenGame();
        }
        catch (AuthenticationException e)
        {
            ShowMessage("สมัครสมาชิกไม่สำเร็จ กรุณาตรวจสอบข้อมูล");
            Debug.LogException(e);
        }
        catch (RequestFailedException e)
        {
            ShowMessage("เชื่อมต่อเซิร์ฟเวอร์ไม่สำเร็จ");
            Debug.LogException(e);
        }
        catch (Exception e)
        {
            ShowMessage("เกิดข้อผิดพลาด กรุณาลองใหม่");
            Debug.LogException(e);
        }
        finally
        {
            isProcessing = false;
        }
    }

    public async void Login()
    {
        if (!CanProceed())
            return;

        isProcessing = true;

        try
        {
            await AuthenticationService.Instance
                .SignInWithUsernamePasswordAsync(
                    usernameInput.text.Trim(),
                    passwordInput.text
                );

            OpenGame();
        }
        catch (AuthenticationException e)
        {
            ShowMessage("เข้าสู่ระบบไม่สำเร็จ กรุณาตรวจสอบบัญชี");
            Debug.LogException(e);
        }
        catch (RequestFailedException e)
        {
            ShowMessage("เชื่อมต่อเซิร์ฟเวอร์ไม่สำเร็จ");
            Debug.LogException(e);
        }
        catch (Exception e)
        {
            ShowMessage("เกิดข้อผิดพลาด กรุณาลองใหม่");
            Debug.LogException(e);
        }
        finally
        {
            isProcessing = false;
        }
    }

    private bool CanProceed()
    {
        if (isProcessing)
            return false;

        if (!servicesReady)
        {
            ShowMessage("ระบบบัญชียังไม่พร้อม กรุณารอสักครู่");
            return false;
        }

        if (usernameInput == null ||
            passwordInput == null ||
            messageText == null)
        {
            Debug.LogError(
                "กรุณากำหนด Username Input, Password Input และ Message Text ใน Inspector"
            );
            return false;
        }

        if (string.IsNullOrWhiteSpace(usernameInput.text) ||
            string.IsNullOrEmpty(passwordInput.text))
        {
            ShowMessage("กรุณากรอก Username และ Password");
            return false;
        }

        if (usernameInput.text.Trim().Length < 3 ||
            passwordInput.text.Length < 8)
        {
            ShowMessage(
                "Username อย่างน้อย 3 ตัว และ Password อย่างน้อย 8 ตัว"
            );
            return false;
        }

        return true;
    }

    private void ShowMessage(string message)
    {
        if (messageText != null)
            messageText.text = message;
    }

    private void OpenGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }
}
