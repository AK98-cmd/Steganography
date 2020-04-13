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
using System.IO;
namespace Steganography
{
    public partial class Symmetric_Encryption : Form
    {
        SymmetricAlgorithm mySymmetricAlg;
        ConversionHandler myConverter = new ConversionHandler();

        public Symmetric_Encryption()
        {
            InitializeComponent();
        }

        private void Symmetric_Algorithm_Load(object sender, EventArgs e)
        {

            //Watermark Section

            Watermark watermrk1 = new Watermark(textBoxKey, "Generate the key");
            Watermark watermrk2 = new Watermark(textBoxIV, "Generate the IV");
            Watermark watermrk3 = new Watermark(textBoxPlain, "Type the message to be encrypt");

            //-->

            if(Algorithm.selector==0)
            {
                textBoxIV.Text = Algorithm.IVHex;
                textBoxKey.Text = Algorithm.KeyHex;

                textBoxCipher.Text = Algorithm.sym_ciphertext;
                textBoxCipherHex.Text = myConverter.ByteArrayToHexString(myConverter.StringToByteArray(textBoxCipher.Text));

                textBoxPlain.Text = Algorithm.sym_plaintext;
                textBoxPlainHex.Text = myConverter.ByteArrayToHexString(myConverter.StringToByteArray(textBoxPlain.Text));

                comboBoxCipher.Text = Algorithm.sym_alg_selector;
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBoxCipher.Text)
            {
                case "DES":
                    mySymmetricAlg = DES.Create();
                    break;
                case "3DES":
                    mySymmetricAlg = TripleDES.Create();
                    break;
                case "Rijndael":
                    mySymmetricAlg = Rijndael.Create();
                    break;
            }
            mySymmetricAlg.GenerateIV();
            mySymmetricAlg.GenerateKey();
        }
        
        private void Generate(string cipher)
        {
            switch (comboBoxCipher.Text)
            {
                case "DES":
                    mySymmetricAlg = DES.Create();
                    break;
                case "3DES":
                    mySymmetricAlg = TripleDES.Create();
                    break;
                case "Rijndael":
                    mySymmetricAlg = Rijndael.Create();
                    break;
            }
            mySymmetricAlg.GenerateIV();
            mySymmetricAlg.GenerateKey();
        }

        public byte[] Encrypt(byte[] mess, byte[] key, byte[] iv)
        {
            mySymmetricAlg.Key = key;
            mySymmetricAlg.IV = iv;
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms,mySymmetricAlg.CreateEncryptor(),
            CryptoStreamMode.Write);
            cs.Write(mess, 0, mess.Length);
            cs.Close();
            return ms.ToArray();
        }
        public byte[] Decrypt(byte[] mess, byte[] key, byte[] iv)
        {
            byte[] plaintext = new byte[mess.Length];
            mySymmetricAlg.Key = key;
            mySymmetricAlg.IV = iv;
            MemoryStream ms = new MemoryStream(mess);
            CryptoStream cs = new CryptoStream(ms,mySymmetricAlg.CreateDecryptor(),
            CryptoStreamMode.Read);
            cs.Read(plaintext, 0, mess.Length);
            cs.Close();
            return plaintext;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            byte[] ciphertext = Encrypt(myConverter.StringToByteArray(textBoxPlain.Text),myConverter.HexStringToByteArray(textBoxKey.Text),myConverter.HexStringToByteArray(textBoxIV.Text));
            textBoxCipher.Text = myConverter.ByteArrayToString(ciphertext);
            textBoxCipherHex.Text = myConverter.ByteArrayToHexString(ciphertext);
            textBoxPlainHex.Text = myConverter.ByteArrayToHexString(myConverter.StringToByteArray(textBoxPlain.Text));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            byte[] plaintext = Decrypt(myConverter.HexStringToByteArray(textBoxCipherHex.Text), myConverter.HexStringToByteArray(textBoxKey.Text),myConverter.HexStringToByteArray(textBoxIV.Text));
            textBoxPlain.Text = myConverter.ByteArrayToString(plaintext);
            textBoxPlainHex.Text = myConverter.ByteArrayToHexString(plaintext);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Generate(comboBoxCipher.Text);
            textBoxKey.Text = myConverter.ByteArrayToHexString(mySymmetricAlg.Key);
            textBoxIV.Text = myConverter.ByteArrayToHexString(mySymmetricAlg.IV);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Algorithm.selector = 0;
            Algorithm.IVHex = textBoxIV.Text;
            Algorithm.KeyHex = textBoxKey.Text;
            Algorithm.sym_alg_selector = comboBoxCipher.Text;
            Algorithm.sym_ciphertext = textBoxCipherHex.Text;
            Algorithm.sym_plaintext = textBoxPlain.Text;
        }

        private void textBoxKey_TextChanged(object sender, EventArgs e)
        {
            
        }
    }

        class ConversionHandler
        {   
            public byte[] StringToByteArray(string s)
            {
                return CharArrayToByteArray(s.ToCharArray());
            }
            public byte[] CharArrayToByteArray(char[] array)
            {
                return Encoding.ASCII.GetBytes(array, 0, array.Length);
            }
            public string ByteArrayToString(byte[] array)
            {
                return Encoding.ASCII.GetString(array);
            }
            public string ByteArrayToHexString(byte[] array)
            {
                string s = "";
                int i;
                for (i = 0; i < array.Length; i++)
                {
                    s = s + NibbleToHexString((byte)((array[i] >> 4) & 0x0F)) + NibbleToHexString((byte)(array[i] & 0x0F));
                }
                
                return s;
            }                
            public byte[] HexStringToByteArray(string s)
            {
                byte[] array = new byte[s.Length / 2];
                char[] chararray = s.ToCharArray();
                int i;
                for (i = 0; i < s.Length / 2; i++)
                {
                    array[i] = (byte)(((HexCharToNibble(chararray[2 * i]) << 4) & 0xF0) | ((HexCharToNibble(chararray[2 * i + 1]) & 0x0F)));
                }
                return array;
            }
            public string NibbleToHexString(byte nib)
            {
            
                string s;
                if (nib < 10)
                {
                    s = nib.ToString();
                }
                else
                {
                    char c = (char)(nib + 55);
                    s = c.ToString();
                }
                return s;
            }
            public byte HexCharToNibble(char c)
            {
                byte value = (byte)c;
                if (value < 65)
                {
                    value = (byte)(value - 48);
                }
                else
                {
                    value = (byte)(value - 55);
                }
                return value;
            }
        }
}
