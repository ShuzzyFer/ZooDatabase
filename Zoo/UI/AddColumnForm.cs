using System;
using System.Windows.Forms;

public class AddColumnForm : Form
{
    private TextBox textBoxColumnName;
    private ComboBox comboBoxDataType;
    private Button addButton;
    private Button cancelButton;

    public string ColumnName => textBoxColumnName.Text.Trim();
    public string DataType => comboBoxDataType.SelectedItem?.ToString() ?? "text";

    public AddColumnForm()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        this.Text = "Добавление столбца";
        this.Size = new System.Drawing.Size(350, 200);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.CenterParent;

        var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(5) };

        // Название столбца
        var labelColumnName = new Label { Text = "Название столбца:", AutoSize = true, Margin = new Padding(0, 0, 0, 3) };
        textBoxColumnName = new TextBox { Width = 300, Margin = new Padding(0, 0, 0, 5) };

        // Тип данных
        var labelDataType = new Label { Text = "Тип данных:", AutoSize = true, Margin = new Padding(0, 0, 0, 3) };
        comboBoxDataType = new ComboBox { Width = 300, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 0, 0, 5) };
        comboBoxDataType.Items.AddRange(new string[] { "text", "integer", "bigint", "real", "decimal", "float", "boolean", "date", "timestamp" });
        comboBoxDataType.SelectedIndex = 0; // Значение по умолчанию: text

        // Кнопки "Добавить" и "Отмена"
        addButton = new Button { Text = "Добавить", Width = 80, Margin = new Padding(5, 0, 5, 0) };
        cancelButton = new Button { Text = "Отмена", Width = 80, Margin = new Padding(0, 0, 0, 0) };
        addButton.Click += (s, e) => { this.DialogResult = DialogResult.OK; this.Close(); };
        cancelButton.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

        var buttonsPanel = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            Margin = new Padding(0, 5, 0, 0)
        };
        buttonsPanel.Controls.Add(cancelButton);
        buttonsPanel.Controls.Add(addButton);

        // Компоновка элементов
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(5), AutoSize = true };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(labelColumnName, 0, 0);
        layout.Controls.Add(textBoxColumnName, 0, 1);
        layout.Controls.Add(labelDataType, 0, 2);
        layout.Controls.Add(comboBoxDataType, 0, 3);
        layout.Controls.Add(buttonsPanel, 0, 4);

        mainPanel.Controls.Add(layout);
        this.Controls.Add(mainPanel);
    }
}