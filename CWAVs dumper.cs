﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace YATA
{
    public partial class CWAVs_dumper : Form
    {
        public CWAVs_dumper()
        {
            InitializeComponent();
        }

        List<long> source = new List<long>();
        string fileName;
        string fileExtension;
        byte filesizelocation;
        int filesizebuffer = 0;
        string filesizeString;

        private void button1_Click(object sender, EventArgs e)
        {
            clean();
            listBox1.Items.Clear();
            listBox1.Enabled = false;
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            label1.Visible = false;
            string[] filesize = new string[4];
            byte[] magic = new byte[4];
            byte[] CWAVBytes_Little_end = new byte[] { 0x43, 0x57, 0x41, 0x56, 0xFF, 0xFE, 0x40, 0x00 };
            byte[] CWAVBytes_BIG_end = new byte[] { 0x43, 0x57, 0x41, 0x56, 0xFE, 0xFF, 0x40, 0x00 };
            System.IO.FileStream fs = new System.IO.FileStream(Path.GetTempPath() + "snd_dump.bin", FileMode.Open, FileAccess.ReadWrite);
            SearchBytePattern(CWAVBytes_Little_end, fs);
            SearchBytePattern(CWAVBytes_BIG_end, fs);
            if (source.Count == 0)
            {
                button1.Enabled = true;
                label1.Visible = true;
                return;
            }
            foreach (long s in source)
            {
                fileName = "0x" + s.ToString("X");
                fs.Seek(s, SeekOrigin.Begin);
                fs.Read(magic, 0, 4);
                string HexMagic = magic[0].ToString("X2") + magic[1].ToString("X2") + magic[2].ToString("X2") + magic[3].ToString("X2");
                if ((string.Compare(Encoding.ASCII.GetString(magic, 0, 4).ToString(), "CWAV") == 0))
                {
                    fileExtension = "BCWAV";
                    filesizelocation = 0x0C;
                    if (Directory.Exists(Path.GetTempPath() + "DUMP\\" ) == false) 
                    {
                        Directory.CreateDirectory(Path.GetTempPath() + "DUMP\\");
                    }
                }
                fs.Seek(s, SeekOrigin.Begin);
                fs.Seek(filesizelocation, SeekOrigin.Current);//advances from magic to get filesize.
                int i3 = 0;//i3 is to ensure that you're only reading the four needed bytes of file size. This will be ammended as new formats become available.
                for (int time = 0; time <= 3; time++)
                {
                    filesize[i3] = fs.ReadByte().ToString("X2");//X2 is used so that if any bytes get their leading zeroes truncated, it'll add them right back before adding it to the string array.
                    i3++;
                }
                filesizeString = filesize[3] + filesize[2] + filesize[1] + filesize[0];//get the filesize string.
                byte[] bytes = new byte[Convert.ToInt32(filesizeString, 16)];//convert this to read.
                filesizebuffer = int.Parse(filesizeString, System.Globalization.NumberStyles.HexNumber);//sets filesize to byte array size for loading and saving.
                fs.Seek(s, SeekOrigin.Begin);//seek back to read all the data
                fs.Read(bytes, 0, Convert.ToInt32(filesizeString, 16));//read the ripped music data into the buffer
                BinaryWriter binWriter = new BinaryWriter(File.Open(Path.GetTempPath() + "DUMP\\"  + fileName + "." + fileExtension, FileMode.Create));//create the new file with the parameters set earlier.
                binWriter.BaseStream.Write(bytes, 0, bytes.Length);//write the data.
                binWriter.Close();//close the data stream
                Array.Clear(bytes, 0, bytes.Length);//clear the array so no garbage data remains
                bytes = null;//forcefully destroy any data left and make the array become invalid until it is recreated.
            }
            fs.Close();
            string[] files = Directory.GetFiles(Path.GetTempPath() + "DUMP\\" );
            foreach (string file in files)
            {
                listBox1.Items.Add(System.IO.Path.GetFileName(file.ToString()));
            }
            File.WriteAllBytes("vgmstream.exe", Properties.Resources.test);
            File.WriteAllBytes("libg7221_decode.dll", Properties.Resources.libg7221_decode);
            File.WriteAllBytes("libmpg123-0.dll", Properties.Resources.libmpg123_0);
            File.WriteAllBytes("libvorbis.dll", Properties.Resources.libvorbis);
            listBox1.Enabled = true;
            button1.Enabled = true;
            button3.Enabled = true;
            button4.Enabled = true;
        }

        public List<int> SearchBytePattern(byte[] pattern, Stream bytes)
        {

            int patternLength = pattern.Length;
            long totalLength = bytes.Length;
            byte firstMatchByte = pattern[0];
            for (long i = 0; i < totalLength; i++)
            {
                if (firstMatchByte == bytes.ReadByte())
                {
                    bytes.Position--;
                    byte[] match = new byte[patternLength];
                    bytes.Read(match, 0, patternLength);
                    if (match.SequenceEqual<byte>(pattern))
                    {
                        source.Add(bytes.Position - patternLength);
                        i += patternLength - 1;
                    }
                }
                if ((totalLength - bytes.Position) <= patternLength)
                    break;
            }
            bytes.Position = 0;
            return null;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            if ( folderBrowserDialog1.SelectedPath != "")
            {
                string[] files = Directory.GetFiles(Path.GetTempPath() + "DUMP\\");
                foreach (string file in files)
                {
                    System.IO.File.Copy(file, folderBrowserDialog1.SelectedPath +"\\"+ System.IO.Path.GetFileName(file), true);
                }
                MessageBox.Show("Done !");
            } 
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (File.Exists("wav.wav")) File.Delete("wav.wav");
            string[] files = Directory.GetFiles(Path.GetTempPath() + "DUMP\\");
            Process proc = new Process();
            proc.StartInfo.FileName = "vgmstream.exe";
            proc.StartInfo.Arguments = "-o wav.wav " +  files[listBox1.SelectedIndex];
            proc.Start();
            proc.WaitForExit();
            Process.Start("wav.wav");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message + "\r\n \r\n This error may happen when you try to play another cwav before closing the player of the older one"); }
            
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button2.Enabled = true;
        }

        void clean() 
        {
            if (Directory.Exists(Path.GetTempPath() + "DUMP")) Directory.Delete(Path.GetTempPath() + "DUMP", true);
            return;
        }

        private void button4_Click(object sender, EventArgs e)
        {
             folderBrowserDialog1.ShowDialog();
            if (folderBrowserDialog1.SelectedPath == "") return;
            if (!Directory.Exists(Path.GetTempPath() + "DUMP\\Wav"))
            { Directory.CreateDirectory(Path.GetTempPath() + "DUMP\\Wav"); }
            else
            {
                string[] Wavs = Directory.GetFiles(Path.GetTempPath() + "DUMP\\Wav");
                foreach (string Wav in Wavs)
                {
                    System.IO.File.Delete(Wav);
                }
            }
            string[] files = Directory.GetFiles(Path.GetTempPath() + "DUMP\\");
            foreach (string file in files)
            {
                  Process proc = new Process();
                  proc.StartInfo.FileName = "vgmstream.exe";
                  proc.StartInfo.Arguments = "-o " + Path.GetTempPath() + "DUMP\\Wav/" + System.IO.Path.GetFileName(file) + ".wav " + file;
                  proc.Start();
                  proc.WaitForExit();
            }
            string[] ConvertedFiles = Directory.GetFiles(Path.GetTempPath() + "DUMP\\Wav");
            foreach (string ConvertedFile in ConvertedFiles)
            {
                System.IO.File.Copy(ConvertedFile, folderBrowserDialog1.SelectedPath + "\\" + System.IO.Path.GetFileName(ConvertedFile), true);
            }
            MessageBox.Show("Done !");
        }

        private void Frm_closing(object sender, FormClosingEventArgs e)
        {
            clean();
            File.Delete("vgmstream.exe");
            File.Delete("libg7221_decode.dll");
            File.Delete("libmpg123-0.dll");
            File.Delete("libvorbis.dll");
        }
    }
}