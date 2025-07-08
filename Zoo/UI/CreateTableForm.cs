using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Zoo.Models;

public class CreateTableForm : Form
{
    private TextBox textBoxTableName;
    private List<(TextBox nameTextBox, ComboBox typeComboBox)> _columnControls;
    private TableLayoutPanel columnsTable;

    public string TableName => textBoxTableName.Text.Trim();
    
    public List<ColumnDefinition> Columns =>
        _columnControls.Select(c => new ColumnDefinition(
            c.nameTextBox.Text.Trim(), 
            c.typeComboBox.SelectedItem?.ToString() ?? "varchar(255)"
        )).ToList();

    public CreateTableForm()
    {
        _columnControls = new List<(TextBox, ComboBox)>();
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        this.Text = "Создание таблицы";
        this.Size = new Size(450, 450);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.CenterParent;

        var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

        var labelTableName = new Label { Text = "Название таблицы:", AutoSize = true };
        textBoxTableName = new TextBox { Width = 400 };

        var buttonAddColumn = new Button { Text = "Добавить столбец", Width = 150 };
        buttonAddColumn.Click += (s, e) => AddColumnRow();

        var columnsContainer = new Panel { AutoScroll = true, Width = 400, Height = 250, BorderStyle = BorderStyle.FixedSingle };
        columnsTable = new TableLayoutPanel { AutoSize = true, ColumnCount = 3 };
        columnsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        columnsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        columnsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
        columnsContainer.Controls.Add(columnsTable);

        var createButton = new Button { Text = "Создать", Width = 100, DialogResult = DialogResult.OK };
        var cancelButton = new Button { Text = "Отмена", Width = 100, DialogResult = DialogResult.Cancel };
        
        createButton.Click += CreateButton_Click;

        var buttonsPanel = new TableLayoutPanel
        {
            ColumnCount = 2,
            Dock = DockStyle.Fill,
            ColumnStyles = { new ColumnStyle(SizeType.Percent, 50F), new ColumnStyle(SizeType.Percent, 50F) },
            Controls = { { createButton, 0, 0 }, { cancelButton, 1, 0 } }
        };
        createButton.Anchor = AnchorStyles.Left;
        cancelButton.Anchor = AnchorStyles.Right;

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(10), AutoSize = true };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(labelTableName);
        layout.Controls.Add(textBoxTableName);
        layout.Controls.Add(buttonAddColumn);
        layout.Controls.Add(columnsContainer);
        layout.Controls.Add(buttonsPanel);

        mainPanel.Controls.Add(layout);
        this.Controls.Add(mainPanel);
        this.AcceptButton = createButton;
        this.CancelButton = cancelButton;

        AddColumnRow();
    }

    private void AddColumnRow()
    {
        var nameTextBox = new TextBox { Width = 200 };
        var typeComboBox = new ComboBox { Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
        typeComboBox.Items.AddRange(new string[] { "varchar(255)", "integer", "bigint", "decimal", "float", "boolean", "date", "timestamp", "text" });
        typeComboBox.SelectedIndex = 0;

        var removeButton = new Button { Text = "X", Width = 40 };
        int rowIndex = columnsTable.RowCount;
        columnsTable.RowCount++;
        columnsTable.Controls.Add(nameTextBox, 0, rowIndex);
        columnsTable.Controls.Add(typeComboBox, 1, rowIndex);
        columnsTable.Controls.Add(removeButton, 2, rowIndex);
        
        var controlTuple = (nameTextBox, typeComboBox);
        _columnControls.Add(controlTuple);

        removeButton.Click += (s, e) =>
        {
            columnsTable.Controls.Remove(nameTextBox);
            columnsTable.Controls.Remove(typeComboBox);
            columnsTable.Controls.Remove(removeButton);
            _columnControls.Remove(controlTuple);
            // This is a simplification; a full solution would re-layout the table.
        };
    }

    private void CreateButton_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TableName))
        {
            MessageBox.Show("Название таблицы не может быть пустым!", "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            this.DialogResult = DialogResult.None; // Prevent form from closing
            return;
        }

        if (Columns.Any(c => string.IsNullOrWhiteSpace(c.Name)))
        {
            MessageBox.Show("Название столбца не может быть пустым!", "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            this.DialogResult = DialogResult.None;
            return;
        }

        var columnNames = Columns.Select(c => c.Name.ToLower()).ToList();
        if (columnNames.Distinct().Count() != columnNames.Count)
        {
            MessageBox.Show("Имена столбцов должны быть уникальными!", "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            this.DialogResult = DialogResult.None;
            return;
        }
        
        // Final validation is OK, DialogResult remains OK.
    }
}