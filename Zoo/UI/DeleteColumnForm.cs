using System;
using System.Collections.Generic;
using System.Windows.Forms;

public class DeleteColumnForm : Form
{
    private ComboBox comboBoxColumns;
    private Button deleteButton;
    private Button cancelButton;

    public string SelectedColumn => comboBoxColumns.SelectedItem?.ToString();

    public DeleteColumnForm(List<string> columns)
    {
        InitializeComponents(columns);
    }

    private void InitializeComponents(List<string> columns)
    {
        this.Text = "Удаление столбца";
        this.Size = new System.Drawing.Size(350, 150);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.CenterParent;

        var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(5) };

        // Выбор столбца
        var labelColumn = new Label { Text = "Выберите столбец для удаления:", AutoSize = true, Margin = new Padding(0, 0, 0, 3) };
        comboBoxColumns = new ComboBox { Width = 300, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 0, 0, 5) };
        comboBoxColumns.Items.AddRange(columns.ToArray());
        if (comboBoxColumns.Items.Count > 0)
        {
            comboBoxColumns.SelectedIndex = 0; // Выбираем первый столбец по умолчанию
        }

        // Кнопки "Удалить" и "Отмена"
        deleteButton = new Button { Text = "Удалить", Width = 80, Margin = new Padding(5, 0, 5, 0) };
        cancelButton = new Button { Text = "Отмена", Width = 80, Margin = new Padding(0, 0, 0, 0) };
        deleteButton.Click += (s, e) => { this.DialogResult = DialogResult.OK; this.Close(); };
        cancelButton.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

        var buttonsPanel = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            Margin = new Padding(0, 5, 0, 0)
        };
        buttonsPanel.Controls.Add(cancelButton);
        buttonsPanel.Controls.Add(deleteButton);

        // Компоновка элементов
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(5), AutoSize = true };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(labelColumn, 0, 0);
        layout.Controls.Add(comboBoxColumns, 0, 1);
        layout.Controls.Add(buttonsPanel, 0, 2);

        mainPanel.Controls.Add(layout);
        this.Controls.Add(mainPanel);

        // Отключаем кнопку "Удалить", если нет столбцов для удаления
        deleteButton.Enabled = comboBoxColumns.Items.Count > 0;
    }
}