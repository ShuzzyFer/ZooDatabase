using System;
using System.Collections.Generic;
using System.Windows.Forms;

public class AddRowForm : Form
{
    private Dictionary<string, string> columnTypes;
    private Dictionary<string, TextBox> textBoxes;
    private Dictionary<string, string> values;

    public Dictionary<string, string> Values => values;

    public AddRowForm(Dictionary<string, string> columnTypes)
    {
        this.columnTypes = columnTypes;
        textBoxes = new Dictionary<string, TextBox>();
        values = new Dictionary<string, string>();
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        this.Text = "Добавление записи";
        this.Size = new System.Drawing.Size(400, 300); // Уменьшаем высоту до 300 пикселей
        this.FormBorderStyle = FormBorderStyle.FixedSingle; // Фиксированный размер окна
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.CenterParent;

        var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(5) };

        // Панель с прокруткой для полей
        var fieldsPanel = new Panel
        {
            AutoScroll = true,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 5)
        };

        // TableLayoutPanel для вертикального расположения полей
        var fieldsLayout = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 2,
            Dock = DockStyle.Top,
            Padding = new Padding(0)
        };
        fieldsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F)); // Название столбца
        fieldsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F)); // Поле ввода

        // Добавляем поля для ввода
        int rowIndex = 0;
        foreach (var column in columnTypes.Keys)
        {
            if (column.ToLower() == "id") continue; // Пропускаем столбец id

            fieldsLayout.RowCount++;
            fieldsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Метка с названием столбца
            var label = new Label
            {
                Text = $"{column} ({columnTypes[column]}):",
                AutoSize = true,
                Margin = new Padding(0, 5, 5, 5)
            };

            // Поле для ввода
            var textBox = new TextBox
            {
                Width = 200,
                Margin = new Padding(0, 5, 0, 5)
            };
            textBoxes[column] = textBox;

            fieldsLayout.Controls.Add(label, 0, rowIndex);
            fieldsLayout.Controls.Add(textBox, 1, rowIndex);
            rowIndex++;
        }

        fieldsPanel.Controls.Add(fieldsLayout);

        // Кнопки "OK" и "Cancel"
        var okButton = new Button { Text = "OK", Width = 80, Margin = new Padding(5, 0, 5, 0) };
        var cancelButton = new Button { Text = "Cancel", Width = 80, Margin = new Padding(0, 0, 0, 0) };
        okButton.Click += OkButton_Click;
        cancelButton.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

        var buttonsPanel = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Bottom,
            Margin = new Padding(0, 5, 0, 0)
        };
        buttonsPanel.Controls.Add(cancelButton);
        buttonsPanel.Controls.Add(okButton);

        // Компоновка элементов
        mainPanel.Controls.Add(fieldsPanel);
        mainPanel.Controls.Add(buttonsPanel);
        this.Controls.Add(mainPanel);
    }

    private void OkButton_Click(object sender, EventArgs e)
    {
        foreach (var column in columnTypes.Keys)
        {
            if (column.ToLower() == "id") continue;

            string value = textBoxes[column].Text.Trim();
            string dataType = columnTypes[column].ToLower();

            // Валидация введённых данных
            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    if (dataType.Contains("integer") || dataType.Contains("bigint"))
                    {
                        int.Parse(value); // Проверяем, что это целое число
                    }
                    else if (dataType.Contains("real") || dataType.Contains("decimal") || dataType.Contains("float"))
                    {
                        decimal.Parse(value); // Проверяем, что это число с плавающей точкой
                    }
                    else if (dataType.Contains("boolean"))
                    {
                        if (!value.Equals("true", StringComparison.OrdinalIgnoreCase) && !value.Equals("false", StringComparison.OrdinalIgnoreCase))
                        {
                            throw new FormatException("Ожидается значение true или false.");
                        }
                    }
                    else if (dataType.Contains("date") || dataType.Contains("timestamp"))
                    {
                        DateTime.Parse(value); // Проверяем, что это дата
                    }
                    // Для текстовых типов (text) и других валидация не требуется
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка в поле '{column}': {ex.Message}\nОжидается тип данных: {dataType}");
                    return;
                }
            }

            values[column] = value;
        }

        this.DialogResult = DialogResult.OK;
        this.Close();
    }
}