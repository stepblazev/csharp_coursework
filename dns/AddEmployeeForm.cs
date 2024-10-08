﻿using System;
using System.Windows.Forms;

namespace dns
{
    public partial class AddEmployeeForm : Form
    {
        private readonly EmployeesForm parentForm;
        private readonly string action;

        public AddEmployeeForm(EmployeesForm f, string ac)
        {
            InitializeComponent();

            parentForm = f;
            action = ac;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void submitButton_Click(object sender, EventArgs e)
        {
            if (surnameTextBox.Text.Length < 1 || nameTextBox.Text.Length < 1 ||
                patronymicTextBox.Text.Length < 1 || adressTextBox.Text.Length < 1 ||
                phoneTextBox.Text.Length < 1 || educationTextBox.Text.Length < 1 ||
                passportTextBox.Text.Length < 1 || positionTextBox.Text.Length < 1)
            {
                MessageBox.Show("Проверьте введённые данные", "Действие невозможно",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string surname = surnameTextBox.Text;
            string name = nameTextBox.Text;
            string patronymic = patronymicTextBox.Text;

            string query = $"SELECT * FROM сотрудники WHERE фамилия='{surname}' AND имя='{name}' AND отчество='{patronymic}'";
            if (action=="adding" && QueriesClass.HasLinks(parentForm.myConnection, query))
            {
                MessageBox.Show("Сотрудник с таким именен уже существует", "Повторите попытку",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string date = dateTimePicker.Value.ToString();
            string adress = adressTextBox.Text;
            string passport = passportTextBox.Text;
            string education = educationTextBox.Text;
            string phone = phoneTextBox.Text;
            string position = positionTextBox.Text;

            switch (action)
            {
                case "adding":
                    query = $"INSERT INTO сотрудники (фамилия, имя, отчество, дата_рождения, образование, должность," +
                        $" дата_приема, адрес, телефон, паспорт) " +
                        $"VALUES ('{surname}', '{name}', '{patronymic}', '{date}', '{education}', " +
                        $"'{position}', '{DateTime.Now.ToString("dd.MM.yyyy")}', '{adress}', '{phone}', '{passport}')";
                    QueriesClass.ApplyQuery_ReturnNone(parentForm.myConnection, query);
                    break;
                case "updating":
                    query = $"UPDATE сотрудники SET адрес='{adress}',дата_рождения='{date}', " +
                        $"телефон='{phone}', должность='{position}', образование='{education}', паспорт='{passport}' " +
                        $"WHERE фамилия='{surname}' AND имя='{name}' and отчество='{patronymic}'";
                    QueriesClass.ApplyQuery_ReturnNone(parentForm.myConnection, query);
                    break;
            }

            FillProgressBar();
            parentForm.TableRefresh();

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
    }
}
