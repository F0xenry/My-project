using UnityEngine;
using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.qrcode; // Важно для EncodeHintType

public static class PDFGenerator
{
    public static string CreateTicketPDF(int ticketId, string passengerName, int tripId,
                                       int quantity, decimal totalPrice, DateTime tripDate)
    {
        string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ИП Семинаев Билеты");
        Directory.CreateDirectory(folderPath);
        string fileName = $"Билет_{ticketId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
        string fullPath = Path.Combine(folderPath, fileName);

        try
        {
            Document document = new Document(PageSize.A5, 30, 30, 30, 30);
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(fullPath, FileMode.Create));
            document.Open();

            // Путь к системному шрифту Arial для поддержки русского языка
            string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
            BaseFont bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

            iTextSharp.text.Font titleFont = new iTextSharp.text.Font(bf, 16, iTextSharp.text.Font.BOLD);
            iTextSharp.text.Font boldFont = new iTextSharp.text.Font(bf, 12, iTextSharp.text.Font.BOLD);
            iTextSharp.text.Font normalFont = new iTextSharp.text.Font(bf, 12, iTextSharp.text.Font.NORMAL);
            iTextSharp.text.Font italicFont = new iTextSharp.text.Font(bf, 10, iTextSharp.text.Font.ITALIC);

            // Заголовок PDF
            Paragraph header = new Paragraph("ИП Семинаев В.Я.\nМеждугородние перевозки", titleFont);
            header.Alignment = Element.ALIGN_CENTER;
            document.Add(header);
            document.Add(new Paragraph(" "));

            // Таблица с данными
            PdfPTable table = new PdfPTable(2);
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 40, 60 });

            table.AddCell(new Phrase("Номер билета:", boldFont));
            table.AddCell(new Phrase($"#{ticketId}", normalFont));

            table.AddCell(new Phrase("Пассажир:", boldFont));
            table.AddCell(new Phrase(passengerName, normalFont));

            table.AddCell(new Phrase("Дата поездки:", boldFont));
            table.AddCell(new Phrase(tripDate.ToString("dd.MM.yyyy"), normalFont));

            table.AddCell(new Phrase("Количество:", boldFont));
            table.AddCell(new Phrase(quantity.ToString(), normalFont));

            table.AddCell(new Phrase("Сумма:", boldFont));
            table.AddCell(new Phrase($"{totalPrice} ₽", boldFont));

            document.Add(table);
            document.Add(new Paragraph(" "));

            // Безопасная настройка кодировки UTF-8 для QR-кода
            System.Collections.Generic.Dictionary<EncodeHintType, object> hints = new System.Collections.Generic.Dictionary<EncodeHintType, object>();
            hints.Add(EncodeHintType.CHARACTER_SET, "UTF-8");

            string qrText = $"Билет #{ticketId}\n{passengerName}\n{tripDate:dd.MM.yyyy}";

            BarcodeQRCode qr = new BarcodeQRCode(qrText, 1, 1, hints);
            Image qrImage = qr.GetImage();
            qrImage.ScalePercent(120);
            qrImage.Alignment = Element.ALIGN_CENTER;
            document.Add(qrImage);

            // Нижний текст
            Paragraph footer = new Paragraph("\nСохраните этот билет!", italicFont);
            footer.Alignment = Element.ALIGN_CENTER;
            document.Add(footer);

            document.Close();
            writer.Close();

            Debug.Log($"PDF создан успешно: {fullPath}");
            return fullPath; // Возврат значения при успешном завершении
        }
        catch (Exception ex)
        {
            Debug.LogError("Ошибка создания PDF: " + ex.Message);
            return ""; // Возврат пустой строки в случае падения (чтобы избежать CS0161)
        }
    }
}