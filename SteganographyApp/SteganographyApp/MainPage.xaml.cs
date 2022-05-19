using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Axiom.Encryption;
using System.IO;
using Android.Graphics;
using PCLStorage;
using System.Threading;

namespace SteganographyApp
{
    public partial class MainPage : ContentPage
    {
        Steganograph graph;
        Bitmap FinalImage;
        Bitmap SourceImage;

        public MainPage()
        {
            InitializeComponent();
            HideTextButton.Clicked += (s, e) => { new Thread(HideText).Start(); GetTextButton.IsEnabled = false; HideTextButton.IsEnabled = false; Loading.Source = "Load.gif"; };
            GetTextButton.Clicked += (s, e) => { new Thread(GetText).Start(); GetTextButton.IsEnabled = false; HideTextButton.IsEnabled = false; Loading.Source = "Load.gif"; };
            OpenImageButton.Clicked += OpenImage;
            SaveImageButton.Clicked += SaveImage;
        }

        void HideText()
        {
            FinalImage = graph.HideText(MainText.Text);
            Device.InvokeOnMainThreadAsync(() =>
            {
                SaveImageButton.IsEnabled = true;
                GetTextButton.IsEnabled = true;
                HideTextButton.IsEnabled = true;
                Loading.Source = "Idle.gif";
            });
            return;
        }

        void GetText()
        {
            Device.InvokeOnMainThreadAsync(() =>
            {
                MainText.Text = graph.FindHiddenString();
                GetTextButton.IsEnabled = true;
                HideTextButton.IsEnabled = true;
                Loading.Source = "Idle.gif";
            });
            return;
        }

        async void OpenImage(object s, EventArgs e)
        {
            FileResult result = await FilePicker.PickAsync();

            using (StreamReader sr = new StreamReader(File.Open(result.FullPath, FileMode.Open)))
            {
                BinaryReader binreader = new BinaryReader(sr.BaseStream);
                var allData = ReadAllBytes(binreader);
                BitmapFactory.Options factory = new BitmapFactory.Options();
                factory.InMutable = true;
                SourceImage = BitmapFactory.DecodeByteArray(allData, 0, allData.Length, factory);
            }
            MainImage.Source = (result.FullPath);
            Loading.Source = "Load.gif";
            new Thread(InstatiateSteganograph).Start();
        }

        void InstatiateSteganograph()
        {
            graph = new Steganograph(SourceImage);
            Device.InvokeOnMainThreadAsync(() =>
            {
                HideTextButton.IsEnabled = true;
                GetTextButton.IsEnabled = true;
                Loading.Source = "Idle.gif";
            });
        }

        byte[] ReadAllBytes(BinaryReader reader)
        {
            const int bufferSize = 4096;
            using (var ms = new MemoryStream())
            {
                byte[] buffer = new byte[bufferSize];
                int count;
                while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
                    ms.Write(buffer, 0, count);
                return ms.ToArray();
            }
        }

        async void SaveImage(object s, EventArgs e)
        {
            if (FinalImage == null)
                return;

            await Permissions.RequestAsync<Permissions.StorageWrite>();

            using (var os = new System.IO.FileStream(Android.OS.Environment.ExternalStorageDirectory + "/DCIM/temp.png", System.IO.FileMode.Create))
            {
                FinalImage.Compress(Bitmap.CompressFormat.Png, 100, os);
            }
        }
    }
}
