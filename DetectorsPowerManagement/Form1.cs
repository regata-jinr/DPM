using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using OwenioNet;
using OwenioNet.IO;
using OwenioNet.Exceptions;
using OwenioNet.Types;
using OwenioNet.DataConverter.Converter;
using OwenioNet.DataConverter.Types;
using System.Threading;

namespace DetectorsPowerManagement
{
    public partial class Form1 : Form
    {
        private List<PictureBox> pictureBoxList = new List<PictureBox>();
        private List<GroupBox> groupBoxList = new List<GroupBox>();
        private List<Button> buttonOnList = new List<Button>();
        private List<Button> buttonOffList = new List<Button>();
        private List<Bitmap> bmpList = new List<Bitmap>();
        private List<Graphics> gr = new List<Graphics>();
        private Label label = new Label();
        private TextBox textBox = new TextBox();
        private Button button = new Button();
        IOwenProtocolMaster owenProtocol;
        private string parametername = "r.OE";
        private int portNumber = 8;

        public Form1()
        {
            InitializeComponent();
            for (int i = 0; i < 4; i++)
            {
                pictureBoxList.Add(new PictureBox());
                groupBoxList.Add(new GroupBox());
                buttonOnList.Add(new Button());
                buttonOffList.Add(new Button());
                bmpList.Add(new Bitmap(pictureBoxList[i].Size.Width, pictureBoxList[i].Size.Height));
                ((ISupportInitialize)(pictureBoxList[i])).BeginInit();
                groupBoxList[i].SuspendLayout();
            }
            SuspendLayout();
            //Button
            button.Location = new Point(170, 10);
            button.Text = "Set";
            button.AutoSize = true;
            button.Name = "button";
            button.Click += (sender, args) => SetPortNumber();
            //TextBox
            textBox.Location = new Point(130, 12);
            textBox.Name = "textBox";
            textBox.Text = "8";
            textBox.Size = new Size(30, 20);
            //Label
            label.Location = new Point(50, 16);
            label.Name = "label";
            label.Text = "COM port №:";
            label.AutoSize = true;
            // 
            // groupBoxList
            // 
            var names = new[] { "D1", "D5", "D7", "D8" };
            var YShift = 70;
            for (int i = 0; i < 4; i++)
            {
                groupBoxList[i].Controls.Add(pictureBoxList[i]);
                groupBoxList[i].Controls.Add(buttonOnList[i]);
                groupBoxList[i].Controls.Add(buttonOffList[i]);
                groupBoxList[i].Size = new Size(200, 60);
                groupBoxList[i].Location = new Point(10 + ClientSize.Width / 2 - groupBoxList[i].Size.Width / 2, 40 + i * YShift);
                groupBoxList[i].Name = "groupBox" + i.ToString();
                groupBoxList[i].Text = names[i];

                // 
                // buttonOn
                // 

                buttonOnList[i].Location = new Point(20, 20);
                buttonOnList[i].Name = "buttonOn" + i.ToString();
                buttonOnList[i].Size = new Size(50, 20);
                buttonOnList[i].Text = "On";
                buttonOnList[i].UseVisualStyleBackColor = true;
                var vs = i;
                buttonOnList[i].Click += (sender, args) => On(vs, true);

                // 
                // buttonOff
                // 

                buttonOffList[i].Location = new Point(80, 20);
                buttonOffList[i].Name = "buttonOff" + i.ToString();
                buttonOffList[i].Size = new Size(50, 20);
                buttonOffList[i].Text = "Off";
                buttonOffList[i].UseVisualStyleBackColor = true;
                buttonOffList[i].Click += (sender, args) => On(vs, false);

                // 
                // pictureBoxList
                // 

                pictureBoxList[i].Location = new Point(150, 20);
                pictureBoxList[i].Name = "pictureBox" + i.ToString();
                pictureBoxList[i].Size = new Size(25, 25);
                pictureBoxList[i].BackgroundImage = bmpList[i];

                //Graphics
                gr.Add(Graphics.FromImage(pictureBoxList[i].BackgroundImage));
                gr[i].FillEllipse(new SolidBrush(Color.Red), 0, 0, pictureBoxList[i].Size.Width, pictureBoxList[i].Size.Height);
            }
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(285, 320);
            Controls.Add(label);
            Controls.Add(textBox);
            Controls.Add(button);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            for (int i = 0; i < 4; i++)
            {
                Controls.Add(groupBoxList[i]);
                groupBoxList[i].ResumeLayout(false);
                ((ISupportInitialize)(pictureBoxList[i])).EndInit();
            }
            Name = "Form1";
            Text = "DetectorsPowerManagement";
            ResumeLayout(false);
            
        }

        private void On(int i, bool on)
        {
            SolidBrush brush;
            byte[] turnOn;
            if (on)
            {
                brush = new SolidBrush(Color.GreenYellow);
                turnOn = new ConverterFloat(3).Convert(1);
            }
            else
            {
                turnOn = new ConverterFloat(3).Convert(0);
                brush = new SolidBrush(Color.Red);
            }
            gr[i].FillEllipse(brush, 0, 0, pictureBoxList[i].Size.Width, pictureBoxList[i].Size.Height);
            pictureBoxList[i].Refresh();
            try
            {
                owenProtocol.OwenWrite(28 + i, AddressLengthType.Bits8, parametername, turnOn);
            }
            catch
            {
                MessageBox.Show(string.Format("Не могу получить доступ к выходу № {0}.", (13+i).ToString()));
            }
        }

        private void SetPortNumber()
        {
            string text = null;
            try
            {
                text = textBox.Text.ToString();
                portNumber = Int32.Parse(text);
                if (portNumber < 0 || portNumber > 1000)
                {
                    MessageBox.Show(string.Format("Введите число от 0 до 1000, вместо {0}.", text));
                    textBox.Focus();
                    return;
                }
            }
            catch
            {
                MessageBox.Show(string.Format("Введите число больше от 0 до 1000, вместо {0}.", text));
                textBox.Focus();
                return;
            }
            SerialPortAdapter port = new SerialPortAdapter(portNumber, 9600, Parity.None, 8, StopBits.One);
            try
            {
                port.Open();
            }
            catch
            {
                MessageBox.Show(string.Format("Ошибка открытия порта COM{1}: {0}", port.ToString(), portNumber));
                return;
            }
            owenProtocol = OwenProtocolMaster.Create(port);
        }
    }
}
