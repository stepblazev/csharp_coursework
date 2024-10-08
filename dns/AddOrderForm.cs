﻿using System;
using System.Windows.Forms;
using System.Data.OleDb;

namespace dns
{
    public partial class AddOrderForm : Form
    {
        const string connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=shopBD.mdb";
        public OleDbConnection myConnection;

        private bool isSucessful = false;

        public AddOrderForm()
        {
            InitializeComponent();

            // Подлючение к БД
            myConnection = new OleDbConnection(connectionString);
            myConnection.Open();

            UpdateClients();
            UpdateItems();
            deliveryComboBox.SelectedIndex = 0;
        }

        private void UpdateClients()
        {
            clientComboBox.Items.Clear();
            try
            {
                string query = $"SELECT фамилия, имя, отчество FROM клиенты";
                OleDbCommand command = new OleDbCommand(query, myConnection);
                OleDbDataReader dbReader = command.ExecuteReader();

                while (dbReader.Read())
                    clientComboBox.Items.Add($"{dbReader["фамилия"]} {dbReader["имя"]} {dbReader["отчество"]}");
                dbReader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateItems()
        {
            QueriesClass.SetDataIntoList(myConnection, itemComboBox, "название", "товары");
        }

        private void addClientButton_Click(object sender, EventArgs e)
        {
            AddClientForm addClientForm = new AddClientForm(myConnection, "adding");

            DialogResult result = addClientForm.ShowDialog();
            if (result == DialogResult.Abort || result == DialogResult.No)
                return;

            MessageBox.Show("Клиент добавлен!", "Операция успешна", MessageBoxButtons.OK, MessageBoxIcon.Information);
            UpdateClients();

            clientComboBox.SelectedIndex = clientComboBox.Items.Count - 1;
        }

        private void AddOrderForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            myConnection.Close();
        }

        private void submitButton_Click(object sender, EventArgs e)
        {
            if (clientComboBox.SelectedIndex < 0 ||
                itemComboBox.SelectedIndex < 0 ||
                countTextBox.Value < 1 ||
                deliveryComboBox.SelectedIndex < 0)
            {
                MessageBox.Show("Проверьте введенные данные", "Действие невозможно",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ВНИМАНИЕ!!! split сработает корректно только в том случае, если в фамилии, имени и отчестве
            // не будет пробелов (пробелы между фамилией, именем и отчеством не считаются).

            // Получение ID клиента
            string[] client = clientComboBox.Text.Split();
            string query = $"SELECT код_клиента FROM клиенты WHERE фамилия='{client[0]}' " +
                $"AND имя='{client[1]}' AND отчество='{client[2]}'";
            string clientID = QueriesClass.ApplyQuery_Return(myConnection, query);

            // Получение ID товара
            query = $"SELECT код_товара FROM товары WHERE название='{itemComboBox.SelectedItem}'";
            string itemID = QueriesClass.ApplyQuery_Return(myConnection, query);

            // Запись данных в базу данных
            query = $"INSERT INTO заказы (код_клиента, дата_оформления, код_товара, количество, статус, вид_доставки) " +
                $"VALUES ({clientID}, '{DateTime.Now.ToString("dd.MM.yyyy")}', {itemID}, {countTextBox.Value}, " +
                $"'активен', '{deliveryComboBox.SelectedItem}')";
            QueriesClass.ApplyQuery_ReturnNone(myConnection, query);

            FillProgressBar();

            isSucessful = true;
            this.Close();
        }

        private void FillProgressBar()
        {
            progressBar1.Visible = true;
            for (int i = 0; i <= 100; i++)
            {
                System.Threading.Thread.Sleep(10);
                progressBar1.Value = i;
            }
        }

        private void AddOrderForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isSucessful) this.DialogResult = DialogResult.OK;
            else this.DialogResult = DialogResult.No;
        }
    }
}
