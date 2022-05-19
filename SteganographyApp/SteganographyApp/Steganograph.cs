﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Numerics;
using System.Linq.Expressions;
using Android.Graphics;

namespace Axiom.Encryption
{
    class Steganograph
    {
        Bitmap map;

        List<Vector2> Coordinates;

        public Steganograph(Bitmap bitmap)
        {
            map = bitmap;
            Start();
        }

        void Start()
        {
            Coordinates = GetCoordinates(map);
        }

        public Bitmap HideText(string Text)
        {
            //Man muss tuerst alle LSBs entfernen sonst funktioniert das entschlüsseln nicht.
            byte[] bytes = Encoding.ASCII.GetBytes(Text);
            int[] temp = BinaryConverter(bytes);

            foreach (Vector2 v in Coordinates)
                map.SetPixel((int)v.X, (int)v.Y, CalculateColorGreen(new Color(map.GetPixel((int)v.X, (int)v.Y)), 0));

            for (int i = 0; i < temp.Length; i++)
            {
                Vector2 vector = Coordinates[i];
                Color color = new Color(map.GetPixel((int)vector.X, (int)vector.Y));
                //int[] colorBytes = BinaryConverter(color.G);
                //colorBytes[7] = temp[i];
                //byte test = BinaryConverter(colorBytes);

                color = CalculateColorGreen(color, temp[i]);
                map.SetPixel((int)vector.X, (int)vector.Y, color);
            }
            return map;
        }

        //public void HideImage(Image ImageToHide)
        //{

        //}

        //public Image FindHiddenImage()
        //{
        //    Image temp;

        //    return temp;
        //}

        public string FindHiddenString()
        {
            List<byte> temp = new List<byte>();
            List<int> bits = new List<int>();

            foreach (Vector2 vector in Coordinates)
            {
                Color color = new Color(map.GetPixel((int)vector.X, (int)vector.Y));
                byte t = color.G;
                bits.Add(BinaryConverter(color.G)[7]);
            }

            List<List<int>> bytes = ChunkBy(bits, 8);

            foreach (List<int> i in bytes)
            {
                string b = "";
                i.ForEach(x => b += x.ToString());

                try
                {
                    temp.Add(Convert.ToByte(b, 2));
                }
                catch (Exception ex)
                {

                }
            }
            return Encoding.ASCII.GetString(temp.ToArray()) ?? "null";
        }

        //Color CalculateColorGreen(Color color, int bit)
        //{
        //    int[] colorBytes = BinaryConverter(color.G);
        //    colorBytes[7] = bit;
        //    byte test = BinaryConverter(colorBytes);
        //    color = Color.Argb(color.A, color.R, test, color.B);
        //    return color;
        //}

        Color CalculateColorGreen(Color color, int bit)
        {
            string temp = Convert.ToString(color.G, 2);
            temp = temp.PadLeft(8, '0');
            temp = temp.Remove(7);
            temp += bit.ToString();
            return Color.Argb(color.A, color.R, Convert.ToByte(temp, 2), color.B);
        }

        //Color CalculateColorGreen(Color color, int bit)
        //{
        //    int temp = color.G;
        //    temp >>= 1;
        //    temp &= bit;
        //    return Color.Argb(color.A, color.R, temp, color.B);
        //}

        List<Vector2> GetCoordinates(Bitmap image)
        {
            List<Vector2> temp = new List<Vector2>();

            for (int w = 0; w < image.Width; w++)
                for (int h = 0; h < image.Height; h++)
                {
                    temp.Add(new Vector2(w, h));
                }
            return temp;
        }

        int[] BinaryConverter(byte[] array)
        {
            List<int> result = new List<int>();
            foreach (byte b in array)
            {
                char[] temp = Convert.ToString(b, 2).PadLeft(8, '0').ToCharArray();
                Array.ForEach(temp, x => result.Add(Convert.ToInt32(x) - 48));
            }
            return result.ToArray();
        }

        int[] BinaryConverter(byte b)
        {
            List<int> result = new List<int>();
            char[] temp = Convert.ToString(b, 2).PadLeft(8, '0').ToCharArray();
            Array.ForEach(temp, x => result.Add(Convert.ToInt32(x) - 48));
            return result.ToArray();
        }

        byte BinaryConverter(int[] array)
        {
            string temp = "";
            foreach (int i in array)
                temp += i.ToString();

            return Convert.ToByte(temp, 2);
        }

        List<List<T>> ChunkBy<T>(List<T> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
    }
}