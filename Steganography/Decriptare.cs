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
    public partial class Decriptare : Form
    {

        RSACryptoServiceProvider myrsa = new RSACryptoServiceProvider();
        SymmetricAlgorithm mySymmetricAlg;
        ConversionHandler myConverter = new ConversionHandler();

        private void sym_algorithm_dec(string sym_alg,string key,string IV,string ciph_txt)
        {
            switch (sym_alg)
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


            mySymmetricAlg.IV = myConverter.HexStringToByteArray(IV);
            mySymmetricAlg.Key = myConverter.HexStringToByteArray(key);
            byte[] plaintext = Decrypt(myConverter.HexStringToByteArray(ciph_txt), mySymmetricAlg.Key, mySymmetricAlg.IV);

            textBox1.Text = myConverter.ByteArrayToString(plaintext);

        }

        public byte[] Decrypt(byte[] mess, byte[] key, byte[] iv)
        {
            byte[] plaintext = new byte[mess.Length];
            mySymmetricAlg.Key = key;
            mySymmetricAlg.IV = iv;
            MemoryStream ms = new MemoryStream(mess);
            CryptoStream cs = new CryptoStream(ms, mySymmetricAlg.CreateDecryptor(),CryptoStreamMode.Read);
            cs.Read(plaintext, 0, mess.Length);
            cs.Close();
            return plaintext;
            //myConverter.HexStringToByteArray
        }

        public Decriptare()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlgOpenFileDialog = new OpenFileDialog();
            dlgOpenFileDialog.Filter = "Image Files(*.jpeg;*.bmp;*.png;*.jpg)|*.jpeg;*.bmp;*.png;*.jpg";

            if (dlgOpenFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Image image = Bitmap.FromFile(dlgOpenFileDialog.FileName);
                    pictureBox1.Image = image;
                }
                catch
                {

                }
            }
            dlgOpenFileDialog.Dispose();
        }

        private bool verify_pictureBox(PictureBox pictureBox)
        {
            if (pictureBox.Image == null || pictureBox == null)
                return false;
            return true;
        }

        public void rsa_dec(int size,string cipher_txt,string xml)
        {
            myrsa = new RSACryptoServiceProvider(size);
            myrsa.FromXmlString(xml);
            byte[] ciphertext = myConverter.StringToByteArray(cipher_txt);
            byte[] plain = myrsa.Decrypt(ciphertext, true);
            textBox1.Text = myConverter.ByteArrayToString(plain);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (verify_pictureBox(pictureBox1))
            {
                String message, message1, message2, message3;
                Bitmap bmp = (Bitmap)pictureBox1.Image;
                Bitmap bmp_res;
                _SteganographyHelper steg = new _SteganographyHelper();

                int selector;
                string sym_alg_sel;
                string key_hex;
                string IV_hex;
                string chipertxt;

                int size;
                string xml;
                string ciphertxt_rsa;

                selector = Convert.ToInt32(steg.extractText(bmp, 0, 0)); //0 for symmetric algorithm 1 for rsa algorithm
                
                if (selector == 0)
                {
                    sym_alg_sel = steg.extractText(bmp, steg.dec_w_stop, steg.dec_h_stop);  // type of sym algorithm (DES,3DES,Rijndael)
                    key_hex = steg.extractText(bmp, steg.dec_w_stop, steg.dec_h_stop); // Key of the alg
                    IV_hex = steg.extractText(bmp, steg.dec_w_stop, steg.dec_h_stop); //IV of the alg
                    chipertxt = steg.extractText(bmp, steg.dec_w_stop, steg.dec_h_stop);  //CipherText
                    
                    //MessageBox.Show(chipertxt);
                    //MessageBox.Show(key_hex);
                    //MessageBox.Show(IV_hex);
                    //MessageBox.Show(sym_alg_sel);
                    sym_algorithm_dec(sym_alg_sel, key_hex, IV_hex, chipertxt);

                }
                else
                    bmp_res = (Bitmap)pictureBox1.Image;

                if (selector == 1)
                {
                    size = Convert.ToInt32(steg.extractText(bmp, steg.dec_w_stop, steg.dec_h_stop));
                    xml = steg.extractText(bmp, steg.dec_w_stop, steg.dec_h_stop);
                    ciphertxt_rsa = steg.extractText(bmp, steg.w_stop, steg.dec_h_stop);

                    
                }           
            }
        }

        private void Decriptare_Load(object sender, EventArgs e)
        {
            pictureBox1.Parent = this;
            pictureBox1.SendToBack();
            pictureBox1.BringToFront();

            
        }
    }
}
