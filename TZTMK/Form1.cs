using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TZTMK
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            SetButtonsStates(false);        // defaultowo
            portNameTextBox.Text = AppObjects.PortName;
            baudRateTextBox.Text = AppObjects.BaudRate;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (AppObjects.PortService.IsPortOpen)
            {
                // jak port jest otwarty to nie otwieramy
                return;
            }

            AppObjects.PortService.OpenPort();

            if (AppObjects.PortService.IsPortOpen)
            {
                SetButtonsStates(true);
                AppObjects.KeyMouseToolkit.CanActivateRemoteControl = true;
                AppObjects.KeyMouseToolkit.KeyActivity += AppObjects.ArduService.KeyTransmission;
                AppObjects.KeyMouseToolkit.MouseMoveActivity += AppObjects.ArduService.MouseMoveTransmission;
                AppObjects.KeyMouseToolkit.MouseButtonActivity += AppObjects.ArduService.MouseButtonTransmission;
                AppObjects.KeyMouseToolkit.MouseWheelActivity += AppObjects.ArduService.MouseWheelTransmission;
            }
        }

        private void SetButtonsStates(bool isEnableToOpen)
        {
            if (isEnableToOpen)
            {
                this.button1.Enabled = false;
                this.button2.Enabled = true;
            }
            else
            {
                this.button1.Enabled = true;
                this.button2.Enabled = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (AppObjects.PortService.IsPortOpen)
            {
                SetButtonsStates(false);
                AppObjects.KeyMouseToolkit.CanActivateRemoteControl = false;
                AppObjects.KeyMouseToolkit.KeyActivity -= AppObjects.ArduService.KeyTransmission;
                AppObjects.KeyMouseToolkit.MouseMoveActivity -= AppObjects.ArduService.MouseMoveTransmission;
                AppObjects.KeyMouseToolkit.MouseButtonActivity -= AppObjects.ArduService.MouseButtonTransmission;
                AppObjects.KeyMouseToolkit.MouseWheelActivity -= AppObjects.ArduService.MouseWheelTransmission;
                AppObjects.PortService.ClosePort();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            var result = MessageBox.Show("Czy na pewno zamknąć aplikację ?", "Potwierdź",
                             MessageBoxButtons.YesNo,
                             MessageBoxIcon.Question);

            e.Cancel = (result == DialogResult.No);
        }
    }
}
