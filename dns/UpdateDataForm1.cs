﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dns
{
    public partial class UpdateDataForm1 : Form
    {
        private Items form;

        public UpdateDataForm1(Items f)
        {
            InitializeComponent();
            form = f;
            // Запись данных в typeComboBox
            form.SetDataIntoList(typeComboBox);
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void submitButton_Click(object sender, EventArgs e)
        {
            // Вызов метода UpdateData из главной формы
            form.UpdateData(nameTextBox.Text, typeComboBox.SelectedIndex + 1, (int)countTextBox.Value, (int)priceTextBox.Value);
            this.Close();
        }

        private void UpdateDataForm1_Load(object sender, EventArgs e)
        {

        }
    }
}
