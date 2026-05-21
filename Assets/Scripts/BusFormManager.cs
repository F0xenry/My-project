using MySql.Data.MySqlClient;
using System;
using UnityEngine;
using TMPro;

public class BusFormManager : MonoBehaviour
{
    // Замените на имя вашей базы данных
    private string connectionString = "Server=127.0.0.1;Port=3307;Database=myapp_db;Uid=root;Pwd=;Pooling=false;SslMode=None;AllowPublicKeyRetrieval=True;";

    [Header("Окно формы")]
    public GameObject busFormWindow;

    [Header("Поля Ввода (UI)")]
    public TMP_InputField busNumberInput; // Для гос. номера (например, А123ВВ 124)
    public TMP_InputField busModelInput;  // Для модели (например, Kia Granbird)

    [Header("Ссылка на менеджер формы рейсов")]
    public TripFormManager tripFormManager; // Чтобы при добавлении автобуса обновлялись списки в форме рейсов

    void Start()
    {
        if (busFormWindow != null) busFormWindow.SetActive(false); // Скрываем при старте
    }

    /// <summary>
    /// Вызывается при нажатии на кнопку "+ Добавить автобус"
    /// </summary>
    public void OpenBusForm()
    {
        busNumberInput.text = "";
        busModelInput.text = "";
        busFormWindow.SetActive(true);
    }

    /// <summary>
    /// Вызывается при нажатии на кнопку "Сохранить" внутри формы автобуса
    /// </summary>
    public void SaveBus()
    {
        // Проверяем, что поля не пустые
        if (string.IsNullOrEmpty(busNumberInput.text) || string.IsNullOrEmpty(busModelInput.text))
        {
            Debug.LogWarning("[BusForm] Заполните все поля ввода!");
            return;
        }

        string busNumber = busNumberInput.text.Trim();
        string busModel = busModelInput.text.Trim();

        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            try
            {
                conn.Open();

                // Запрос на добавление нового автобуса в таблицу buses
                string query = "INSERT INTO buses (bus_number, bus_model) VALUES (@number, @model)";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@number", busNumber);
                    cmd.Parameters.AddWithValue("@model", busModel);

                    cmd.ExecuteNonQuery();
                }

                Debug.Log($"[BusForm] Автобус {busNumber} успешно добавлен в базу данных.");
                busFormWindow.SetActive(false); // Закрываем форму
            }
            catch (Exception ex)
            {
                Debug.LogError("[BusFormError] Ошибка сохранения автобуса: " + ex.Message);
            }
        }
    }

    public void CloseBusForm()
    {
        busFormWindow.SetActive(false);
    }
}
