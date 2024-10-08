﻿using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.OleDb;
using Excel = Microsoft.Office.Interop.Excel;

namespace dns
{
    public partial class OrdersForm : Form
    {
        const string connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=shopBD.mdb";
        public OleDbConnection myConnection;

        public OrdersForm(string log)
        {
            InitializeComponent();

            this.Text = "Заказы | Вход выполнен: " + log;

            // Подлючение к БД
            myConnection = new OleDbConnection(connectionString);
            myConnection.Open();

            // Заполнение таблицы
            SetRefresh();
        }

        private void OrdersForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            myConnection.Close();
        }

        private void закрытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void шрифтТаблицыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fontDialog1.ShowDialog() == DialogResult.Cancel) return;

            if (fontDialog1.Font.Size >= 15)
            {
                fontDialog1.Font = new Font(fontDialog1.Font.FontFamily, 14);
                MessageBox.Show("Вы выбрали слишком большой размер шрифта, " +
                    "поэтому размер был автоматически установлен на 14", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            dataGridView1.Font = fontDialog1.Font;
            dataGridView2.Font = fontDialog1.Font;
            dataGridView3.Font = fontDialog1.Font;
        }

        private void TableRefresh(DataGridView dgv, string query)
        {
            try
            {
                dgv.Rows.Clear();
                OleDbCommand command = new OleDbCommand(query, myConnection); // Создаю запрос
                OleDbDataReader dbReader = command.ExecuteReader();   // Считываю данные

                // Загрузка данных в таблицу
                while (dbReader.Read())
                    dgv.Rows.Add(dbReader["Код заказа"], dbReader["Товар"], dbReader["Статус"]);

                dbReader.Close();

                dgv.ClearSelection();
                ClearLabels();

                foreach (DataGridViewRow row in dgv.Rows)
                    row.Height = heightBar.Value;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void SetRefresh()
        {
            string query;

            switch (tabControl1.SelectedIndex)
            {
                case 0:
                    query = "SELECT заказы.код_заказа AS [Код заказа], товары.название AS Товар, " +
                    "заказы.статус AS Статус FROM заказы " +
                    "INNER JOIN товары ON заказы.код_товара=товары.код_товара WHERE заказы.статус='активен'";
                    TableRefresh(dataGridView1, query);
                    break;
                case 1:
                    query = "SELECT заказы.код_заказа AS [Код заказа], товары.название AS Товар, " +
                    "заказы.статус AS Статус FROM заказы " +
                    "INNER JOIN товары ON заказы.код_товара=товары.код_товара WHERE заказы.статус='выполнен'";
                    TableRefresh(dataGridView2, query);
                    break;
                case 2:
                    query = "SELECT заказы.код_заказа AS [Код заказа], товары.название AS Товар, " +
                    "заказы.статус AS Статус FROM заказы INNER JOIN товары ON заказы.код_товара=товары.код_товара";
                    TableRefresh(dataGridView3, query);
                    break;
            }
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            SetRefresh();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetRefresh();
        }

        private void GetInfo(DataGridViewRow row)
        {
            string query = $"SELECT клиенты.фамилия AS Фамилия, клиенты.имя AS Имя, " +
                $"клиенты.отчество AS Отчество, клиенты.адрес AS Адрес FROM заказы INNER JOIN клиенты " +
                $"ON заказы.код_клиента=клиенты.код_клиента WHERE заказы.код_заказа={row.Cells[0].Value}";

            OleDbCommand command = new OleDbCommand(query, myConnection);
            OleDbDataReader dbReader = command.ExecuteReader();

            dbReader.Read();
            surnameLabel.Text = dbReader["Фамилия"].ToString();
            nameLabel.Text = dbReader["Имя"].ToString();
            patronymicLabel.Text = dbReader["Отчество"].ToString();
            addressLabel.Text = dbReader["Адрес"].ToString();
            dbReader.Close();

            query = $"SELECT заказы.код_товара AS [Код товара], заказы.количество AS Количество, " +
                $"заказы.вид_доставки AS Доставка, товары.стоимость AS Стоимость, заказы.статус AS Статус, заказы.дата_оформления AS Дата " +
                $"FROM заказы INNER JOIN товары ON заказы.код_товара=товары.код_товара WHERE заказы.код_заказа={row.Cells[0].Value}";

            command = new OleDbCommand(query, myConnection);
            dbReader = command.ExecuteReader();

            dbReader.Read();
            groupBox2.Text = "Заказ от " + dbReader["Дата"].ToString().Split()[0];
            productLabel.Text = dbReader["Код товара"].ToString();
            countLabel.Text = dbReader["Количество"].ToString();
            deliveryLabel.Text = dbReader["Доставка"].ToString();
            priceLabel.Text = (double.Parse(dbReader["Стоимость"].ToString()) * int.Parse(countLabel.Text)).ToString() + "$";
            statusLabel.Text = dbReader["Статус"].ToString();
            dbReader.Close();
        }

        private void ClearLabels()
        {
            surnameLabel.Text = "(нет данных)";
            nameLabel.Text = "(нет данных)";
            patronymicLabel.Text = "(нет данных)";
            addressLabel.Text = "(нет данных)";
            productLabel.Text = "(нет данных)";
            countLabel.Text = "(нет данных)";
            deliveryLabel.Text = "(нет данных)";
            priceLabel.Text = "0$";
            statusLabel.Text = "(нет данных)";
            groupBox2.Text = "Заказ";
        }

        private void statusLabel_TextChanged(object sender, EventArgs e)
        {
            if (statusLabel.Text == "активен")
            {
                statusLabel.Font = new Font(statusLabel.Font, FontStyle.Bold);
                statusLabel.ForeColor = Color.IndianRed;
                setStatusButton.Visible = true;
                return;
            }
            if (statusLabel.Text == "выполнен")
            {
                statusLabel.Font = new Font(statusLabel.Font, FontStyle.Bold);
                statusLabel.ForeColor = Color.OliveDrab;
                setStatusButton.Visible = false;
                return;
            }
            statusLabel.Font = new Font(statusLabel.Font, FontStyle.Regular);
            statusLabel.ForeColor = SystemColors.ControlText;
            setStatusButton.Visible = false;
        }

        private void bindingNavigatorDelete_Click(object sender, EventArgs e)
        {
            if (CurrentTable.SelectedRows.Count == 0)
            {
                MessageBox.Show("Заказ не выбран", "Действие невозможно",
                   MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("Вы действительно хотите удалить заказ?", "Подтверждение действия",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;

            DataGridView dgv = CurrentTable;
            string query = $"DELETE FROM заказы WHERE код_заказа={dgv.CurrentRow.Cells[0].Value}";
            QueriesClass.ApplyQuery_ReturnNone(myConnection, query);
            SetRefresh();
        }

        private void setStatusButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Заказ выполнен?", "Подтверждение действия",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;

            string query = $"UPDATE заказы SET статус='выполнен' WHERE код_заказа={CurrentTable.CurrentRow.Cells[0].Value}";
            QueriesClass.ApplyQuery_ReturnNone(myConnection, query);

            SetRefresh();
        }

        private DataGridView CurrentTable
        {
            get
            {
                switch (tabControl1.SelectedIndex)
                {
                    case 1:
                        return dataGridView2;
                    case 2:
                        return dataGridView3;
                }
                return dataGridView1;
            }
        }

        private void посикToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentTable.ClearSelection();
            SearchForm sf = new SearchForm(CurrentTable);
            sf.ShowDialog();
        }

        private void bindingNavigatorAddNewItem_Click(object sender, EventArgs e)
        {
            myConnection.Close();

            AddOrderForm addOrderForm = new AddOrderForm();

            DialogResult result = addOrderForm.ShowDialog();
            if (result == DialogResult.No || result == DialogResult.Abort)
            {
                myConnection.Open();
                return;
            }

            myConnection.Open();
            SetRefresh();
        }

        private void dataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (CurrentTable.SelectedRows.Count == 0)
            {
                ClearLabels();
                return;
            }
            GetInfo(CurrentTable.SelectedRows[0]);
        }

        private void heightBar_Scroll(object sender, EventArgs e)
        {
            sizeLabel.Text = heightBar.Value.ToString();
            foreach (DataGridViewRow row in CurrentTable.Rows)
            {
                row.Height = heightBar.Value;
            }
        }

        private void экспортВExcelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportToExcel();
        }

        private void ExportToExcel()
        {
            try
            {
                Excel.Application exApp = new Excel.Application();
                exApp.Visible = true;
                exApp.Workbooks.Add();
                Excel.Worksheet workSheet = exApp.ActiveSheet;


                switch(tabControl1.SelectedIndex)
                {
                    case 0:
                        workSheet.Cells[1, 1] = "Список активных заказов";
                        break;
                    case 1:
                        workSheet.Cells[1, 1] = "Список выполненных заказов";
                        break;
                    case 2:
                        workSheet.Cells[1, 1] = "Список всех заказов";
                        break;
                }

                workSheet.Range["A1", "C1"].Merge();
                workSheet.Range["A1", "A1"].HorizontalAlignment = Excel.Constants.xlCenter;
                workSheet.Range["A1", "A1"].Font.Size = 14;

                for (int i = 0; i < CurrentTable.Columns.Count; i++)
                {
                    workSheet.Cells[2, i + 1] = CurrentTable.Columns[i].HeaderText;

                    for (int j = 0; j < CurrentTable.Rows.Count; j++)
                    {
                        workSheet.Cells[j + 3, i + 1] = CurrentTable.Rows[j].Cells[i].Value.ToString();
                    }
                }
                workSheet.Range["A2", "C2"].Font.Bold = true;
                workSheet.UsedRange.Borders.Weight = 2;
                exApp.Columns.AutoFit();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void searchString_TextChanged(object sender, EventArgs e)
        {
            if (searchString.Text.Length < 1)
            {
                CurrentTable.ClearSelection();
                return;
            }

            foreach (DataGridViewRow row in CurrentTable.Rows)
            {
                string concat = "";
                foreach (DataGridViewCell cell in row.Cells) concat += cell.Value.ToString();
                concat = concat.ToLower();
                if (concat.Contains(searchString.Text.ToLower()))
                {

                    CurrentTable.FirstDisplayedScrollingRowIndex = row.Index;
                    CurrentTable.ClearSelection();
                    row.Selected = true;
                }
            }
        }
    }
}
