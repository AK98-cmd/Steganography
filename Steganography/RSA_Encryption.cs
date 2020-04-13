using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Security.Cryptography;
using System.Runtime.InteropServices;

namespace Steganography
{
    public partial class RSA_Encryption : Form
    {

        ConversionHandler myConverter = new ConversionHandler();
        RSACryptoServiceProvider myrsa = new RSACryptoServiceProvider();
        RSACryptoServiceProvider _myrsa;
        string xml_str1;
        string xml_str2;
        int Size;
        public RSA_Encryption()
        {
            InitializeComponent();
        }

        private void Encrypt()
        {
            myrsa = new RSACryptoServiceProvider(Size);
            byte[] plain = myConverter.StringToByteArray(textBoxPlain.Text);
            byte[] ciphertext = myrsa.Encrypt(plain, true);
            textBoxCipher.Text = myConverter.ByteArrayToString(ciphertext);
            textBoxCipherHex.Text = myConverter.ByteArrayToHexString(ciphertext);
        }

        private void textBoxKey_TextChanged(object sender, EventArgs e)
        {
           
        }

        private void RSA_Encryption_Load(object sender, EventArgs e)
        {
            Size = 1024;
            Watermark watermrk = new Watermark(textBoxPlain,"Type the message to be encrypt");

            if (Algorithm.selector == 1)
            {
                comboBox1.Text = Algorithm.size.ToString();
                textBoxPlain.Text = Algorithm.rsa_plaintext;
                textBoxPlainHex.Text = myConverter.ByteArrayToHexString(myConverter.StringToByteArray(textBoxPlain.Text));

                textBoxCipher.Text = Algorithm.rsa_ciphertext;
                textBoxCipherHex.Text = myConverter.ByteArrayToHexString(myConverter.StringToByteArray(textBoxCipher.Text));
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Encrypt();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Size = Convert.ToInt32(comboBox1.Text);
        }

        private void textBoxPlain_TextChanged(object sender, EventArgs e)
        {
            if(textBoxPlain.Text.Length>0)
            {
                textBoxPlainHex.Text = myConverter.ByteArrayToHexString(myConverter.StringToByteArray(textBoxPlain.Text));
            }
        }

        private void Decrypt()
        {
            myrsa = new RSACryptoServiceProvider(Convert.ToInt32(comboBox1.Text));
            byte[] ciphertext = myConverter.StringToByteArray(textBoxCipher.Text);
            byte[] plain = myrsa.Decrypt(ciphertext, true);
            textBoxPlain.Text = myConverter.ByteArrayToString(plain);
            textBoxPlainHex.Text = myConverter.ByteArrayToHexString(plain);
            MessageBox.Show(myConverter.ByteArrayToString(plain));
        }
        private void button3_Click(object sender, EventArgs e)
        {
            Decrypt();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Algorithm.selector = 1;
            Algorithm.size = Convert.ToInt32(comboBox1.Text);
            Algorithm.rsa_xml = myrsa.ToXmlString(true);
            Algorithm.rsa_ciphertext = textBoxCipher.Text;
            Algorithm.rsa_plaintext = textBoxPlain.Text;
        }
    }

    public class Watermark
    {
        // Within your class or scoped in a more appropriate location:
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        // In your constructor or somewhere more suitable:

        public Watermark(TextBox txtBox,string msg)
        {
            SendMessage(txtBox.Handle, 0x1501, 1, msg);
        }
    }
    public class WatermarkTextBox : TextBox
    {
        /// <summary>
        /// The text that will be presented as the watermak hint
        /// </summary>
        private string _watermarkText = "Type here";
        /// <summary>
        /// Gets or Sets the text that will be presented as the watermak hint
        /// </summary>
        public string WatermarkText
        {
            get { return _watermarkText; }
            set { _watermarkText = value; }
        }

        /// <summary>
        /// Whether watermark effect is enabled or not
        /// </summary>
        private bool _watermarkActive = true;
        /// <summary>
        /// Gets or Sets whether watermark effect is enabled or not
        /// </summary>
        public bool WatermarkActive
        {
            get { return _watermarkActive; }
            set { _watermarkActive = value; }
        }

        /// <summary>
        /// Create a new TextBox that supports watermak hint
        /// </summary>
        public WatermarkTextBox()
        {
            this._watermarkActive = true;
            this.Text = _watermarkText;
            this.ForeColor = Color.Gray;

            GotFocus += (source, e) =>
            {
                RemoveWatermak();
            };

            LostFocus += (source, e) =>
            {
                ApplyWatermark();
            };

        }

        /// <summary>
        /// Remove watermark from the textbox
        /// </summary>
        public void RemoveWatermak()
        {
            if (this._watermarkActive)
            {
                this._watermarkActive = false;
                this.Text = "";
                this.ForeColor = Color.Black;
            }
        }

        /// <summary>
        /// Applywatermak immediately
        /// </summary>
        public void ApplyWatermark()
        {
            if (!this._watermarkActive && string.IsNullOrEmpty(this.Text)
                || ForeColor == Color.Gray)
            {
                this._watermarkActive = true;
                this.Text = _watermarkText;
                this.ForeColor = Color.Gray;
            }
        }

    }/// <summary>
}
