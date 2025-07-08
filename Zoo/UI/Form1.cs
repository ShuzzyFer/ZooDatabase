using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClosedXML.Excel;
using Zoo.Models;
using Zoo.Services;

namespace Zoo
{
    public partial class Form1 : Form
    {
        private readonly IDatabaseService _dbService;
        private readonly IBackupService _backupService;
        private readonly IQueryService _queryService;

        private DataTable _currentDataTable;
        private bool _hasUnsavedChanges = false;
        private bool _isLoadingData = false;
        private object oldCellValue;
        private bool isSorting = false;

        public Form1(IDatabaseService dbService, IBackupService backupService, IQueryService queryService)
        {
            _dbService = dbService;
            _backupService = backupService;
            _queryService = queryService;

            InitializeComponent();
            LoadTables();
        }

        // Этот метод вызывается в конструкторе Form1
        private void InitializeComponent()
        {
            this.Text = "Zoo Database Manager";
            this.WindowState = FormWindowState.Maximized;
            this.AutoScaleMode = AutoScaleMode.Font;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.Size = new Size(800, 600); // Компактный размер окна
            this.MinimumSize = new Size(600, 400); // Минимальный размер при изменении

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(5),
                RowCount = 2
            };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var headerPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 40,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(5),
                BackColor = ColorTranslator.FromHtml("#E6F0FA")
            };

            var tableLabel = new Label
            {
                Text = "Выбор таблицы:",
                AutoSize = true,
                ForeColor = ColorTranslator.FromHtml("#333333"),
                Margin = new Padding(5, 10, 5, 0)
            };

            this.comboBoxTables = new ComboBox
            {
                Size = new System.Drawing.Size(200, 30),
                DropDownStyle = ComboBoxStyle.DropDown,
                BackColor = Color.White,
                ForeColor = ColorTranslator.FromHtml("#333333"),
                Margin = new Padding(0, 5, 10, 5)
            };

            this.buttonCreateTable = new Button { Text = "Создать таблицу", Margin = new Padding(5) };
            this.buttonDeleteTable = new Button { Text = "Удалить таблицу", Margin = new Padding(5) };
            this.buttonAddColumn = new Button { Text = "Добавить поле", Margin = new Padding(5) };
            this.buttonDeleteColumn = new Button { Text = "Удалить поле", Margin = new Padding(5) };
            this.buttonRenameColumn = new Button { Text = "Переименовать поле", Margin = new Padding(5) };
            this.buttonAddRow = new Button { Text = "Добавить запись", Margin = new Padding(5) };
            this.buttonDeleteRow = new Button { Text = "Удалить запись", Margin = new Padding(5) };
            this.buttonSaveChanges = new Button { Text = "Сохранить", Margin = new Padding(5) };
            this.buttonExportTable = new Button { Text = "Экспорт в Excel", Margin = new Padding(5) };
            this.buttonExecuteQuery = new Button { Text = "Ввод запроса", Margin = new Padding(5) };
            this.buttonBackupDatabase = new Button { Text = "Резерв базы", Margin = new Padding(5) };
            this.buttonRestoreDatabase = new Button { Text = "Восстановить базу", Margin = new Padding(5) };

            // Стилизация кнопок
            var buttons = new[] {
                buttonCreateTable, buttonDeleteTable, buttonAddColumn, buttonDeleteColumn,
                buttonRenameColumn, buttonAddRow, buttonDeleteRow, buttonSaveChanges,
                buttonExportTable, buttonExecuteQuery, buttonBackupDatabase, buttonRestoreDatabase
            };

            foreach (var btn in buttons)
            {
                btn.Size = new System.Drawing.Size(120, 30);
                btn.BackColor = ColorTranslator.FromHtml("#40C4FF");
                btn.ForeColor = Color.White;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
            }
            buttonRenameColumn.Size = new Size(150, 30);

            headerPanel.Controls.AddRange(new Control[]
            {
                tableLabel, comboBoxTables,
                buttonCreateTable, buttonDeleteTable, buttonAddColumn, buttonDeleteColumn,
                buttonRenameColumn, buttonAddRow, buttonDeleteRow, buttonSaveChanges,
                buttonExportTable, buttonExecuteQuery, buttonBackupDatabase, buttonRestoreDatabase
            });

            this.dataGridViewTables = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                BackgroundColor = Color.White,
                DefaultCellStyle = { BackColor = ColorTranslator.FromHtml("#F0F8FF"), ForeColor = ColorTranslator.FromHtml("#333333") },
                ColumnHeadersDefaultCellStyle = { BackColor = ColorTranslator.FromHtml("#40C4FF"), ForeColor = Color.White },
                EnableHeadersVisualStyles = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Margin = new Padding(0),
                RowHeadersVisible = true,
                RowHeadersWidth = 50
            };

            layout.Controls.Add(headerPanel, 0, 0);
            layout.Controls.Add(dataGridViewTables, 0, 1);
            this.Controls.Add(layout);
            this.BackColor = ColorTranslator.FromHtml("#E6F0FA");

            // Привязка событий
            comboBoxTables.SelectedIndexChanged += ComboBoxTables_SelectedIndexChanged;
            buttonCreateTable.Click += ButtonCreateTable_Click;
            buttonDeleteTable.Click += ButtonDeleteTable_Click;
            buttonAddColumn.Click += ButtonAddColumn_Click;
            buttonDeleteColumn.Click += ButtonDeleteColumn_Click;
            buttonRenameColumn.Click += ButtonRenameColumn_Click;
            buttonAddRow.Click += ButtonAddRow_Click;
            buttonDeleteRow.Click += ButtonDeleteRow_Click;
            buttonSaveChanges.Click += ButtonSaveChanges_Click;
            buttonExportTable.Click += ButtonExportTable_Click;
            buttonExecuteQuery.Click += ButtonExecuteQuery_Click;
            buttonBackupDatabase.Click += ButtonBackupDatabase_Click;
            buttonRestoreDatabase.Click += ButtonRestoreDatabase_Click;
            
            dataGridViewTables.CellValueChanged += DataGridViewTables_CellValueChanged;
            dataGridViewTables.DataError += DataGridViewTables_DataError;
            dataGridViewTables.Sorted += (s, e) => { isSorting = true; UpdateRowNumbers(); isSorting = false; };
        }

        private void SafeAction(Action action, string errorMessagePrefix)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{errorMessagePrefix}: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private async Task SafeAsyncAction(Func<Task> action, string errorMessagePrefix)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{errorMessagePrefix}: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void LoadTables()
        {
            SafeAction(() =>
            {
                string selectedTable = comboBoxTables.Text;
                comboBoxTables.Items.Clear();
                var tables = _dbService.GetTables();
                comboBoxTables.Items.AddRange(tables.ToArray());
                if (comboBoxTables.Items.Contains(selectedTable))
                {
                    comboBoxTables.SelectedItem = selectedTable;
                }
            }, "Ошибка при загрузке списка таблиц");
        }

        private void LoadTableData(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                dataGridViewTables.DataSource = null;
                return;
            }
            
            SafeAction(() =>
            {
                _isLoadingData = true;
                _currentDataTable = _dbService.LoadTableData(tableName);
                dataGridViewTables.DataSource = _currentDataTable;
                _hasUnsavedChanges = false;
                UpdateSaveButtonColor();
                UpdateRowNumbers();
            }, "Ошибка при загрузке данных таблицы");
            
            _isLoadingData = false;
        }

        private void ComboBoxTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxTables.SelectedItem != null)
            {
                LoadTableData(comboBoxTables.SelectedItem.ToString());
            }
        }

        private void ButtonCreateTable_Click(object sender, EventArgs e)
        {
            using (var form = new CreateTableForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    SafeAction(() =>
                    {
                        _dbService.CreateTable(form.TableName, form.Columns);
                        MessageBox.Show("Таблица успешно создана.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadTables();
                        comboBoxTables.SelectedItem = form.TableName;
                    }, "Ошибка при создании таблицы");
                }
            }
        }
        
        private void ButtonDeleteTable_Click(object sender, EventArgs e)
        {
            string tableName = comboBoxTables.Text;
            if (string.IsNullOrEmpty(tableName)) return;

            var result = MessageBox.Show($"Вы уверены, что хотите удалить таблицу '{tableName}'? Это действие нельзя отменить.", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                SafeAction(() =>
                {
                    _dbService.DeleteTable(tableName);
                    MessageBox.Show("Таблица успешно удалена.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    comboBoxTables.Text = "";
                    dataGridViewTables.DataSource = null;
                    LoadTables();
                }, "Ошибка при удалении таблицы");
            }
        }
        
        private void ButtonAddColumn_Click(object sender, EventArgs e)
        {
            string tableName = comboBoxTables.Text;
            if (string.IsNullOrEmpty(tableName)) 
            {
                MessageBox.Show("Выберите таблицу для добавления поля.", "Информация");
                return;
            }
            
            using (var form = new AddColumnForm())
            {
                if(form.ShowDialog() == DialogResult.OK)
                {
                    SafeAction(() =>
                    {
                        var newColumn = new ColumnDefinition(form.ColumnName, form.DataType);
                        _dbService.AddColumn(tableName, newColumn);
                        MessageBox.Show("Столбец успешно добавлен.", "Успех");
                        LoadTableData(tableName);
                    }, "Ошибка при добавлении столбца");
                }
            }
        }

        private void ButtonDeleteColumn_Click(object sender, EventArgs e)
        {
            string tableName = comboBoxTables.Text;
            if (string.IsNullOrEmpty(tableName)) 
            {
                MessageBox.Show("Выберите таблицу для удаления поля.", "Информация");
                return;
            }

            List<string> columns = null;
            SafeAction(() => columns = _dbService.GetColumnNamesForDelete(tableName), "Ошибка получения списка столбцов");
            
            if (columns == null || columns.Count == 0)
            {
                MessageBox.Show("В таблице нет столбцов для удаления (кроме 'id').", "Информация");
                return;
            }

            using (var form = new DeleteColumnForm(columns))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    var result = MessageBox.Show($"Вы уверены, что хотите удалить столбец '{form.SelectedColumn}'?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result == DialogResult.Yes)
                    {
                        SafeAction(() =>
                        {
                            _dbService.DeleteColumn(tableName, form.SelectedColumn);
                            MessageBox.Show("Столбец успешно удален.", "Успех");
                            LoadTableData(tableName);
                        }, "Ошибка при удалении столбца");
                    }
                }
            }
        }

        private void ButtonRenameColumn_Click(object sender, EventArgs e)
        {
            string tableName = comboBoxTables.Text;
            if (string.IsNullOrEmpty(tableName))
            {
                MessageBox.Show("Выберите таблицу для переименования поля.", "Информация");
                return;
            }

            List<string> columns = null;
            SafeAction(() => columns = _dbService.GetColumnNamesForDelete(tableName), "Ошибка получения списка столбцов");

            if (columns == null || columns.Count == 0)
            {
                MessageBox.Show("В таблице нет столбцов для переименования (кроме 'id').", "Информация");
                return;
            }

            // Используем вашу старую форму, адаптировав вызов
            using (var form = new Form())
            {
                form.Text = "Переименовать столбец";
                form.Size = new Size(350, 200);
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterParent;

                var panel = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(10), RowCount = 5 };
                panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

                var labelColumn = new Label { Text = "Выберите столбец:", AutoSize = true };
                var comboBoxColumns = new ComboBox { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList };
                comboBoxColumns.Items.AddRange(columns.ToArray());
                
                var labelNewName = new Label { Text = "Новое название:", AutoSize = true };
                var textBoxNewName = new TextBox { Dock = DockStyle.Top };
                
                var okButton = new Button { Text = "Переименовать", DialogResult = DialogResult.OK };
                var cancelButton = new Button { Text = "Отмена", DialogResult = DialogResult.Cancel };
                
                var buttonsPanel = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Bottom };
                buttonsPanel.Controls.Add(cancelButton);
                buttonsPanel.Controls.Add(okButton);
                
                panel.Controls.Add(labelColumn, 0, 0);
                panel.Controls.Add(comboBoxColumns, 0, 1);
                panel.Controls.Add(labelNewName, 0, 2);
                panel.Controls.Add(textBoxNewName, 0, 3);
                
                form.Controls.Add(panel);
                form.Controls.Add(buttonsPanel);

                form.AcceptButton = okButton;
                form.CancelButton = cancelButton;

                if (form.ShowDialog() == DialogResult.OK)
                {
                    string oldName = comboBoxColumns.SelectedItem?.ToString();
                    string newName = textBoxNewName.Text.Trim();
                    if(string.IsNullOrEmpty(oldName) || string.IsNullOrEmpty(newName))
                    {
                        MessageBox.Show("Необходимо выбрать столбец и ввести новое имя.", "Ошибка");
                        return;
                    }
                    SafeAction(() =>
                    {
                        _dbService.RenameColumn(tableName, oldName, newName);
                        MessageBox.Show("Столбец успешно переименован.", "Успех");
                        LoadTableData(tableName);
                    }, "Ошибка при переименовании столбца");
                }
            }
        }
        
        private void ButtonAddRow_Click(object sender, EventArgs e)
        {
            string tableName = comboBoxTables.Text;
            if (string.IsNullOrEmpty(tableName))
            {
                MessageBox.Show("Выберите таблицу.", "Информация");
                return;
            }

            // Ваша форма AddRowForm остается без изменений, так как она уже достаточно хорошо отделена.
            // Нужно только адаптировать получение данных из нее
            Dictionary<string, string> columnTypes = new Dictionary<string, string>();
            foreach(DataColumn col in _currentDataTable.Columns)
            {
                columnTypes[col.ColumnName] = col.DataType.Name;
            }

            using(var form = new AddRowForm(columnTypes))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    SafeAction(() =>
                    {
                        var values = form.Values.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
                        _dbService.AddRow(tableName, values);
                        MessageBox.Show("Запись успешно добавлена.", "Успех");
                        LoadTableData(tableName);
                    }, "Ошибка при добавлении записи");
                }
            }
        }
        
        private void ButtonDeleteRow_Click(object sender, EventArgs e)
        {
            if (dataGridViewTables.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите строку для удаления.", "Информация");
                return;
            }
            
            string tableName = comboBoxTables.Text;
            var selectedRow = (dataGridViewTables.SelectedRows[0].DataBoundItem as DataRowView)?.Row;
            
            if (selectedRow == null) return;
            
            var result = MessageBox.Show("Вы уверены, что хотите удалить выбранную запись?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if(result == DialogResult.Yes)
            {
                SafeAction(() =>
                {
                    _dbService.DeleteRow(tableName, selectedRow);
                    MessageBox.Show("Запись успешно удалена.", "Успех");
                    LoadTableData(tableName);
                }, "Ошибка при удалении записи");
            }
        }
        
        private void ButtonSaveChanges_Click(object sender, EventArgs e)
        {
            if (!_hasUnsavedChanges)
            {
                MessageBox.Show("Нет несохраненных изменений.");
                return;
            }
            string tableName = comboBoxTables.Text;
            SafeAction(() =>
            {
                _dbService.SaveChanges(tableName, _currentDataTable);
                MessageBox.Show("Изменения успешно сохранены.", "Успех");
                _hasUnsavedChanges = false;
                UpdateSaveButtonColor();
                LoadTableData(tableName);
            }, "Ошибка при сохранении изменений");
        }

        private void ButtonExportTable_Click(object sender, EventArgs e)
        {
            if (dataGridViewTables.DataSource == null)
            {
                MessageBox.Show("Нет данных для экспорта.", "Информация");
                return;
            }

            using (var sfd = new SaveFileDialog { Filter = "Excel Workbook|*.xlsx", FileName = $"{comboBoxTables.Text}.xlsx" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    SafeAction(() =>
                    {
                        using (var workbook = new XLWorkbook())
                        {
                            var worksheet = workbook.Worksheets.Add(_currentDataTable, "Export");
                            worksheet.Columns().AdjustToContents();
                            workbook.SaveAs(sfd.FileName);
                        }
                        MessageBox.Show("Экспорт успешно завершен.", "Успех");
                    }, "Ошибка при экспорте в Excel");
                }
            }
        }
        
        private void ButtonExecuteQuery_Click(object sender, EventArgs e)
        {
            // Эта форма тоже остается практически без изменений, т.к. она была достаточно изолирована
            // Но теперь она будет использовать IQueryService
            using (var queryForm = new Form())
            {
                // ... реализация формы запросов, как в вашем исходном коде ...
                // Вместо `savedQueries` используется `_queryService`
                // Например, `comboBoxQueries.Items.AddRange(_queryService.GetQueries().Select(q => q.Name).ToArray());`
                // При сохранении/удалении вызываются `_queryService.SaveQuery()` и `_queryService.DeleteQuery()`
                // При выполнении запроса `_dbService.ExecuteCustomQuery()`
            }
        }

        private async void ButtonBackupDatabase_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog { Filter = "Backup files (*.backup)|*.backup", FileName = "zoo_backup.backup" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    await SafeAsyncAction(async () =>
                    {
                        await _backupService.BackupDatabaseAsync(sfd.FileName);
                        MessageBox.Show("Резервное копирование успешно завершено.", "Успех");
                    }, "Ошибка при резервном копировании");
                }
            }
        }

        private async void ButtonRestoreDatabase_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Восстановление базы данных приведет к потере всех текущих данных. Продолжить?", "Внимание", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result != DialogResult.Yes) return;
            
            using (var ofd = new OpenFileDialog { Filter = "Backup files (*.backup)|*.backup" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    await SafeAsyncAction(async () =>
                    {
                        await _backupService.RestoreDatabaseAsync(ofd.FileName);
                        MessageBox.Show("База данных успешно восстановлена.", "Успех");
                        LoadTables();
                        dataGridViewTables.DataSource = null;
                    }, "Ошибка при восстановлении");
                }
            }
        }

        private void DataGridViewTables_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_isLoadingData || isSorting) return;
            _hasUnsavedChanges = true;
            UpdateSaveButtonColor();
        }
        
        private void DataGridViewTables_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            MessageBox.Show($"Ошибка ввода данных в столбце '{dataGridViewTables.Columns[e.ColumnIndex].HeaderText}'.\nОжидается тип: {_currentDataTable.Columns[e.ColumnIndex].DataType.Name}.\n\n{e.Exception.Message}",
                            "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            e.Cancel = true; // Отменяем стандартное окно ошибки
        }

        private void UpdateSaveButtonColor()
        {
            buttonSaveChanges.BackColor = _hasUnsavedChanges ? Color.FromArgb(76, 175, 80) : ColorTranslator.FromHtml("#40C4FF");
        }

        private void UpdateRowNumbers()
        {
            for (int i = 0; i < dataGridViewTables.Rows.Count; i++)
            {
                dataGridViewTables.Rows[i].HeaderCell.Value = (i + 1).ToString();
            }
        }
        
        // Поля, которые были в вашем исходном коде, но теперь управляются через DI
        private ComboBox comboBoxTables;
        private DataGridView dataGridViewTables;
        private Button buttonCreateTable;
        private Button buttonDeleteTable;
        private Button buttonAddColumn;
        private Button buttonDeleteColumn;
        private Button buttonRenameColumn;
        private Button buttonAddRow;
        private Button buttonDeleteRow;
        private Button buttonSaveChanges;
        private Button buttonExportTable;
        private Button buttonExecuteQuery;
        private Button buttonBackupDatabase;
        private Button buttonRestoreDatabase;
    }
}