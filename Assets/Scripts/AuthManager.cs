using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MySql.Data.MySqlClient;
using BCrypt.Net;
using System.Collections;

public class AuthManager : MonoBehaviour
{
    [Header("Настройки базы данных")]
    private string connectionString = "Server=127.0.0.1;Port=3307;Database=myapp_db;Uid=root;Pwd=;Pooling=false;SslMode=None;AllowPublicKeyRetrieval=True;";

    [Header("Панели")]
    public GameObject loginPanel;
    public GameObject registerPanel;

    [Header("Форма Входа")]
    public TMP_InputField loginField;
    public TMP_InputField passwordField;
    public Button loginButton;
    public TMP_Text loginErrorText;
    public Button goToRegisterButton;

    [Header("Форма Регистрации")]
    public TMP_InputField regLoginField;
    public TMP_InputField regPasswordField;
    public TMP_InputField regConfirmPasswordField;
    public TMP_InputField regFullNameField;
    public TMP_InputField regPhoneField;
    public TMP_InputField regEmailField;
    public Button registerButton;
    public TMP_Text registerErrorText;
    public Button goToLoginButton;

    private void Start()
    {
        ShowLoginPanel();

        // Привязка кнопок
        if (loginButton) loginButton.onClick.AddListener(OnLoginClick);
        if (registerButton) registerButton.onClick.AddListener(OnRegisterClick);
        if (goToRegisterButton) goToRegisterButton.onClick.AddListener(ShowRegisterPanel);
        if (goToLoginButton) goToLoginButton.onClick.AddListener(ShowLoginPanel);

        ClearErrors();
    }

    public void ShowLoginPanel()
    {
        loginPanel.SetActive(true);
        registerPanel.SetActive(false);
        ClearErrors();
    }

    public void ShowRegisterPanel()
    {
        registerPanel.SetActive(true);
        loginPanel.SetActive(false);
        ClearErrors();
    }

    private void ClearErrors()
    {
        if (loginErrorText) loginErrorText.text = "";
        if (registerErrorText) registerErrorText.text = "";
    }

    // ====================== ВХОД ======================
    public void OnLoginClick()
    {
        StartCoroutine(LoginCoroutine());

    }

    private IEnumerator LoginCoroutine()
    {
        loginErrorText.text = "Проверка...";

        bool success = false;
        bool isAdmin = false;
        int userId = 0;

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            try
            {
                conn.Open();

                string query = "SELECT id, password_hash, is_admin FROM users WHERE login = @login LIMIT 1";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@login", loginField.text.Trim());

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string hash = reader.GetString("password_hash");
                            isAdmin = reader.GetBoolean("is_admin");
                            userId = reader.GetInt32("id");

                            if (BCrypt.Net.BCrypt.Verify(passwordField.text, hash))
                            {
                                success = true;
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                loginErrorText.text = "Ошибка подключения к базе данных";
                Debug.LogError("Login error: " + ex.Message);
                yield break;
            }
        }

        // Код после try-catch
        if (success)
        {

            PlayerPrefs.SetInt("CurrentUserID", userId);    // response.user_id должен приходить с сервера
            PlayerPrefs.Save();
            loginErrorText.text = "<color=green>Вход выполнен успешно!</color>";
            Debug.Log("Успешный вход!");
            

            yield return new WaitForSeconds(1f);

            if (isAdmin)
                UnityEngine.SceneManagement.SceneManager.LoadScene("AdminPanel");
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene("ClientScene");
        }
        else
        {
            loginErrorText.text = "Неверный логин или пароль";
        }

        
    }
   

    // ====================== РЕГИСТРАЦИЯ ======================
    public void OnRegisterClick()
    {
        if (string.IsNullOrEmpty(regPasswordField.text) ||
            regPasswordField.text != regConfirmPasswordField.text)
        {
            registerErrorText.text = "Пароли не совпадают!";
            return;
        }

        StartCoroutine(RegisterCoroutine());
    }

    private IEnumerator RegisterCoroutine()
    {
        registerErrorText.text = "Регистрация...";

        bool success = false;

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            try
            {
                conn.Open();

                // Проверка на уникальность
                string checkQuery = "SELECT COUNT(*) FROM users WHERE login = @login OR email = @email OR phone = @phone";
                using (MySqlCommand cmd = new MySqlCommand(checkQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@login", regLoginField.text.Trim());
                    cmd.Parameters.AddWithValue("@email", regEmailField.text.Trim());
                    cmd.Parameters.AddWithValue("@phone", regPhoneField.text.Trim());

                    if ((long)cmd.ExecuteScalar() > 0)
                    {
                        registerErrorText.text = "Логин, email или телефон уже заняты!";
                        yield break;
                    }
                }

                string hash = BCrypt.Net.BCrypt.HashPassword(regPasswordField.text, 12);

                string insertQuery = @"INSERT INTO users 
                    (login, password_hash, full_name, phone, email, is_admin) 
                    VALUES (@login, @hash, @fio, @phone, @email, FALSE)";

                using (MySqlCommand cmd = new MySqlCommand(insertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@login", regLoginField.text.Trim());
                    cmd.Parameters.AddWithValue("@hash", hash);
                    cmd.Parameters.AddWithValue("@fio", regFullNameField.text.Trim());
                    cmd.Parameters.AddWithValue("@phone", regPhoneField.text.Trim());
                    cmd.Parameters.AddWithValue("@email", regEmailField.text.Trim());

                    cmd.ExecuteNonQuery();
                    success = true;
                }
            }
            catch (System.Exception ex)
            {
                registerErrorText.text = "Ошибка при регистрации";
                Debug.LogError("Register error: " + ex.Message);
                yield break;
            }
        }

        // Код после try-catch
        if (success)
        {
            registerErrorText.text = "<color=green>Регистрация прошла успешно!</color>";
            Debug.Log("Регистрация успешна!");

            yield return new WaitForSeconds(1.5f);

            ShowLoginPanel();
            loginField.text = regLoginField.text;   // Подставляем логин для удобства
        }
    }
}