using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Drawing.Imaging;
using System.Drawing.Design;

namespace Steganography
{
    public partial class Criptare : Form
    {
        
        public Criptare()
        {
            InitializeComponent();
        }

        private void Criptare_Load(object sender, EventArgs e)
        {
            pictureBox1.Parent = this;
            pictureBox1.SendToBack();
            pictureBox1.BringToFront();

            pictureBox2.Parent = this;
            pictureBox2.SendToBack();
            pictureBox2.BringToFront();
        }

        private Image ResizeImage(Image image, Size newSize)
        {
        	Image newImage = new Bitmap(newSize.Width,newSize.Height);
	
        	using (Graphics GFX = Graphics.FromImage((Bitmap)newImage))
        	{
        		GFX.DrawImage(image,new Rectangle(Point.Empty,newSize));
	        }
	
        	return newImage;
        }

        private void Upload_Image(string path)
        {
           Bitmap image = new Bitmap(path);
        }


        private Image ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);

            using (var graphics = Graphics.FromImage(newImage))
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);

            return newImage;
        }

        private void button1_Click(object sender, EventArgs e)
        {

            OpenFileDialog dlgOpenFileDialog = new OpenFileDialog();
            dlgOpenFileDialog.Filter = "Image Files(*.jpeg;*.bmp;*.png;*.jpg)|*.jpeg;*.bmp;*.png;*.jpg";

            if (dlgOpenFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Image image = Bitmap.FromFile(dlgOpenFileDialog.FileName);
                    Size size= new Size();
                    size.Height = pictureBox1.Size.Height;
                    size.Width = pictureBox1.Size.Width;
                    image = ResizeImage(image, size);
                    pictureBox1.Image = image;
                    
                }
                catch
                {
                    ;
                }
            }
            dlgOpenFileDialog.Dispose();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
         
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image.Save(dialog.FileName+".bmp", ImageFormat.Bmp);
            }           
        }

        private bool verify_pictureBox(PictureBox pictureBox)
        {
            if (pictureBox.Image == null || pictureBox == null)
                return false;
            return true;
        }

        
        private void button3_Click(object sender, EventArgs e)
        {
            if(verify_pictureBox(pictureBox1))
            {
                Bitmap bmp = (Bitmap)pictureBox1.Image;
                Bitmap bmp_res;
                _SteganographyHelper steg = new _SteganographyHelper();

                if (Algorithm.selector == 0)
                {
                    bmp_res = steg.embedText(Algorithm.selector.ToString(), bmp, 0, 0);   //0 for symmetric algorithm 1 for rsa algorithm
                    bmp_res = steg.embedText(Algorithm.sym_alg_selector, bmp, steg.w_stop, steg.h_stop); // type of sym algorithm (DES,3DES,Rijndael)
                    bmp_res = steg.embedText(Algorithm.KeyHex, bmp, steg.w_stop, steg.h_stop); // Key of the alg
                    bmp_res = steg.embedText(Algorithm.IVHex, bmp, steg.w_stop, steg.h_stop);  //IV of the alg
                    bmp_res = steg.embedText(Algorithm.sym_ciphertext, bmp, steg.w_stop, steg.h_stop); //CipherText

                    //MessageBox.Show(Algorithm.selector.ToString());
                    //MessageBox.Show(Algorithm.sym_ciphertext);
                }
                else
                    bmp_res = (Bitmap)pictureBox1.Image;

                if (Algorithm.selector == 1)
                {
                    bmp_res=steg.embedText(Algorithm.selector.ToString(),bmp,0,0); //0 for symmetric algorithm 1 for rsa algorithm
                    bmp_res=steg.embedText(Algorithm.size.ToString(),bmp,steg.w_stop,steg.h_stop); //size of bits
                    bmp_res = steg.embedText(Algorithm.rsa_xml, bmp, steg.w_stop, steg.h_stop); //rsa xml export
                    bmp_res = steg.embedText(Algorithm.rsa_ciphertext, bmp, steg.w_stop, steg.h_stop);
                }
                
                //

                pictureBox2.Image = bmp_res;
                
                button2.Enabled = true;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            RSA_Encryption rsa = new RSA_Encryption();
            rsa.ShowDialog();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Symmetric_Encryption symm_alg = new Symmetric_Encryption();
            symm_alg.ShowDialog();
        }

        private void Criptare_FormClosed(object sender, FormClosedEventArgs e)
        {
            ;
        }

        private void Criptare_FormClosing(object sender, FormClosingEventArgs e)
        {
            ;
        }
    }

    class _SteganographyHelper
    {
        public enum State
        {
            Hiding,
            Filling_With_Zeros
        };
        public int w_stop=0;
        public int h_stop=0;

        public int dec_w_stop=0;
        public int dec_h_stop=0;
        public Bitmap embedText(string text, Bitmap bmp,int w_start,int  h_start)
        {
            // initially, we'll be hiding characters in the image
            State state = State.Hiding;

            // holds the index of the character that is being hidden
            int charIndex = 0;

            // holds the value of the character converted to integer
            int charValue = 0;

            // holds the index of the color element (R or G or B) that is currently being processed
            long pixelElementIndex = 0;

            // holds the number of trailing zeros that have been added when finishing the process
            int zeros = 0;

            // hold pixel elements
            int R = 0, G = 0, B = 0;

            // pass through the rows
            for (int i = h_start; i < bmp.Height; i++)
            {
                // pass through each row
                for (int j = w_start; j < bmp.Width; j++)
                {
                    // holds the pixel that is currently being processed
                    Color pixel = bmp.GetPixel(j, i);

                    // now, clear the least significant bit (LSB) from each pixel element
                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    // for each pixel, pass through its elements (RGB)
                    for (int n = 0; n < 3; n++)
                    {
                        // check if new 8 bits has been processed
                        if (pixelElementIndex % 8 == 0)
                        {
                            // check if the whole process has finished
                            // we can say that it's finished when 8 zeros are added
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            {
                                // apply the last pixel on the image
                                // even if only a part of its elements have been affected
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }

                                w_stop=j+1;
                                h_stop=i;
                                // return the bitmap with the text hidden in
                                return bmp;
                            }

                            // check if all characters has been hidden
                            if (charIndex >= text.Length)
                            {
                                // start adding zeros to mark the end of the text
                                state = State.Filling_With_Zeros;
                            }
                            else
                            {
                                // move to the next character and process again
                                charValue = text[charIndex++];
                            }
                        }

                        // check which pixel element has the turn to hide a bit in its LSB
                        switch (pixelElementIndex % 3)
                        {
                            case 0:
                                {
                                    if (state == State.Hiding)
                                    {
                                        // the rightmost bit in the character will be (charValue % 2)
                                        // to put this value instead of the LSB of the pixel element
                                        // just add it to it
                                        // recall that the LSB of the pixel element had been cleared
                                        // before this operation
                                        R += charValue % 2;

                                        // removes the added rightmost bit of the character
                                        // such that next time we can reach the next one
                                        charValue /= 2;
                                    }
                                } break;
                            case 1:
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2;

                                        charValue /= 2;
                                    }
                                } break;
                            case 2:
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2;

                                        charValue /= 2;
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                } break;
                        }

                        pixelElementIndex++;

                        if (state == State.Filling_With_Zeros)
                        {
                            // increment the value of zeros until it is 8
                            zeros++;
                        }
                    }
                }
            }

            return bmp;
        }

        public string extractText(Bitmap bmp,int w_start,int h_start)
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            // holds the text that will be extracted from the image
            string extractedText = String.Empty;

            // pass through the rows
            for (int i = dec_h_stop; i < bmp.Height; i++)
            {
                // pass through each row
                for (int j = dec_w_stop; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);

                    // for each pixel, pass through its elements (RGB)
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3)
                        {
                            case 0:
                                {
                                    // get the LSB from the pixel element (will be pixel.R % 2)
                                    // then add one bit to the right of the current character
                                    // this can be done by (charValue = charValue * 2)
                                    // replace the added bit (which value is by default 0) with
                                    // the LSB of the pixel element, simply by addition
                                    charValue = charValue * 2 + pixel.R % 2;
                                } break;
                            case 1:
                                {
                                    charValue = charValue * 2 + pixel.G % 2;
                                } break;
                            case 2:
                                {
                                    charValue = charValue * 2 + pixel.B % 2;
                                } break;
                        }

                        colorUnitIndex++;

                        // if 8 bits has been added,
                        // then add the current character to the result text
                        if (colorUnitIndex % 8 == 0)
                        {
                            // reverse? of course, since each time the process occurs
                            // on the right (for simplicity)
                            charValue = reverseBits(charValue);

                            // can only be 0 if it is the stop character (the 8 zeros)
                            if (charValue == 0)
                            {
                                dec_h_stop=i;
                                dec_w_stop=j+1;
                                return extractedText;
                            }

                            // convert the character value from int to char
                            char c = (char)charValue;

                            // add the current character to the result text
                            extractedText += c.ToString();
                        }
                    }
                }
            }

            return extractedText;
        }

        private int reverseBits(int n)
        {
            int result = 0;

            for (int i = 0; i < 8; i++)
            {
                result = result * 2 + n % 2;

                n /= 2;
            }

            return result;
        }
    }

    public static class Algorithm
    {
        //need to transfer
        public static int selector= -1;


        //selector 0 symmetric algorithm

        //need to transfer

        public static string sym_ciphertext;
        public static string IVHex;
        public static string KeyHex;
        public static string sym_alg_selector;

        //
        public static string sym_plaintext;

        //selector 1 rsa algorithm

        //need to transfer
        public static int size;
        public static string rsa_ciphertext;
        public static string rsa_xml;

        public static string rsa_plaintext;
 
    }
}
