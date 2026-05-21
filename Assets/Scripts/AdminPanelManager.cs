using MySql.Data.MySqlClient;
using System;
using UnityEngine;
using UnityEngine.UI;

public class AdminPanelManager : MonoBehaviour
{
    private string connectionString = "Server=127.0.0.1;Port=3307;Database=myapp_db;Uid=root;Pwd=;Pooling=false;SslMode=None;AllowPublicKeyRetrieval=True;";

    [Header("UI Поля для создания Маршрута")]
    public InputField departureCityField;
    public InputField arrivalCityField;
    public InputField departureStationField;

    [Header("UI Поля для создания Рейса")]
    public InputField routeIdField;
    public InputField departureTimeField; // Формат ввода: ГГГГ-ММ-ДД ЧЧ:ММ:СС
    public InputField totalSeatsField;
    public InputField priceField;

    [Header("UI Уведомления")]
    public Text statusText;

    [Header("Все панели админки")]
    public GameObject[] allPanels; // Сюда в инспекторе скидываем панели: 0 - Рейсы, 1 - Клиенты, 2 - Деньги


    /// <summary>
    /// Метод переключения панелей по ID
    /// </summary>
    public void SwitchPanel(int panelIndex)
    {
        // Циклом проходим по всем панелям: нужную включаем, остальные выключаем
        for (int i = 0; i < allPanels.Length; i++)
        {
            if (allPanels[i] != null)
                allPanels[i].SetActive(i == panelIndex);
        }
    }


    /// <summary>
    /// 3. УДАЛЕНИЕ РЕЙСА
    /// </summary>
    public void DeleteTrip(int tripId)
    {
        using (MySqlConnection conn = new MySqlConnection(connectionString))
        {
            try
            {
                conn.Open();
                string query = "DELETE FROM trips WHERE id = @id";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", tripId);
                    cmd.ExecuteNonQuery();
                    ShowStatus($"<color=orange>Рейс #{tripId} удален.</color>");
                }
            }
            catch (Exception ex)
            {
                ShowStatus("<color=red>Ошибка удаления: " + ex.Message + "</color>");
            }
        }
    }

    private void ShowStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
        Debug.Log(msg);
    }

    private void ClearRouteFields()
    {
        departureCityField.text = ""; arrivalCityField.text = ""; departureStationField.text = "";
    }

    private void ClearTripFields()
    {
        routeIdField.text = ""; departureTimeField.text = ""; totalSeatsField.text = ""; priceField.text = "";
    }
}
