using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace musicmediaplayerapp

{
    public partial class Form1 : Form
    {
        private List<string> musicFiles;
        private string currentSong;
        private bool isPaused;
        private bool isChangingPosition;
        public Form1()
        {
            InitializeComponent();
            musicFiles = new List<string>();
            isPaused = false;
            isChangingPosition = false;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!isChangingPosition)
            {
                double currentPosition = musicPlayer.Ctlcontrols.currentPosition;
                double duration = 0;

                if (musicPlayer.currentMedia != null)
                {
                    duration = musicPlayer.currentMedia.duration;
                }

                if (currentPosition != null && duration != 0)
                {
                    label2.Text = "Length: " + FormatTime(currentPosition) + "/" + " / " + FormatTime(duration);
                }
                else
                {
                    label2.Text = "No media file is loaded.";
                }
            }
        }
        private void btnLoad_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Multiselect = true;
                openFileDialog.Filter = "MP3 Files|*.mp3"; // Sadece MP3 dosyalarını görüntüle
                openFileDialog.Filter = "All Files|*.*";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (string file in openFileDialog.FileNames)
                    {
                        try
                        {
                            if (IsSupportedFileType(file))
                            {
                                musicFiles.Add(file);
                                listBox1.Items.Add(Path.GetFileName(file));
                            }
                            else
                            {
                                MessageBox.Show($"{file} dosyası desteklenmiyor.");
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Dosya ekleme sırasında bir hata oluştu: {ex.Message}");
                        }
                    }

                    if (musicFiles.Count > 0)
                    {
                        btnStart.Enabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Dosya seçme işlemi sırasında bir hata oluştu: {ex.Message}");
            }
        }

        private bool IsSupportedFileType(string filePath)
        {
            string fileExtension = Path.GetExtension(filePath)?.ToLower();

            if (fileExtension == ".mp3")
            {
                return true;
            }
            else
            {
                MessageBox.Show($"{filePath} dosyası desteklenmiyor.");
                return false;
            }
        }


        private void btnStart_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                if (isPaused)
                {
                    musicPlayer.Ctlcontrols.play();
                    isPaused = false;
                }
                else
                {
                    string selectedFile = musicFiles[listBox1.SelectedIndex];
                    MediaFile mediaFile;

                    switch (Path.GetExtension(selectedFile).ToLower())
                    {
                        case ".mp3":
                            mediaFile = new Mp3File(selectedFile);
                            break;
                        case ".wav":
                            mediaFile = new WavFile(selectedFile);
                            break;
                        default:
                            MessageBox.Show($"{selectedFile} dosyası desteklenmiyor.");
                            return;
                    }

                    musicPlayer.URL = mediaFile.GetFilePath();
                    musicPlayer.Ctlcontrols.play();
                    isPaused = false;
                }
                timer1.Enabled = true;
            }
        
    }

        private void btnPause_Click(object sender, EventArgs e)
        {
            if (isPaused == false)
            {
                musicPlayer.Ctlcontrols.pause();
                isPaused = true;
            }
            else
            {
                musicPlayer.Ctlcontrols.play();
                isPaused = false;
            }

        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            musicPlayer.Ctlcontrols.stop();
            isPaused = false;
            timer1.Enabled = false;

        }

        private string FormatTime(double seconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(seconds);
            return time.ToString(@"mm\:ss");
        }

        private void musicPlayer_PlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {
            if (e.newState == 8)
            {
                int nextIndex = listBox1.SelectedIndex += 1;

                if (nextIndex < musicFiles.Count)
                {
                    listBox1.SelectedIndex = nextIndex;
                    currentSong = musicFiles[nextIndex];
                    musicPlayer.URL = currentSong;
                    musicPlayer.Ctlcontrols.play();
                    isPaused = false;
                }

                else
                {
                    musicPlayer.Ctlcontrols.stop();
                    isPaused = false;
                }
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            musicPlayer.settings.volume = trackBar1.Value;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
    public abstract class MediaFile
    {
        public string FilePath { get; set; }

        public MediaFile(string filePath)
        {
            FilePath = filePath;
        }

        public string GetFilePath()
        {
            return FilePath;
        }
    }

    public class Mp3File : MediaFile
    {
        public Mp3File(string filePath) : base(filePath) { }
    }

    public class WavFile : MediaFile
    {
        public WavFile(string filePath) : base(filePath) { }
    }

}
