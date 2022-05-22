using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
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
        }

        public Bitmap HideText(string Text)
        {
            //Man muss zuerst alle LSBs entfernen sonst funktioniert das entschlüsseln nicht.
            byte[] bytes = Encoding.ASCII.GetBytes(Text);
            int[] temp = BinaryConverter(bytes);
            Coordinates = GetCoordinates(map, temp.Length);

            for (int i = 0; i < Coordinates.Count; i++)
            {
                Vector2 vector = Coordinates[i];
                Color color = new Color(map.GetPixel((int)vector.X, (int)vector.Y));

                if (temp.Length > i)
                {
                    color = CalculateColorGreen(color, temp[i]);
                    map.SetPixel((int)vector.X, (int)vector.Y, color);
                }
                else
                {
                    color = CalculateColorGreen(color, 0);
                    map.SetPixel((int)vector.X, (int)vector.Y, color);
                }
            }

            string bits = Convert.ToString(temp.Length, 2).PadLeft(32, '0');
            for (int i = 0; i < 32; i++)
            {
                Vector2 v = Coordinates[i];
                map.SetPixel((int)v.X, (int)v.Y, CalculateColorBlue(new Color(map.GetPixel((int)v.X, (int)v.Y)), bits.ElementAt(i) - '0'));
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
            int lenght = GetLenght();
            Coordinates = GetCoordinates(map, lenght);

            for (int i = 0; i < lenght; i++)
            {
                Vector2 v = Coordinates[i];
                Color color = new Color(map.GetPixel((int)v.X, (int)v.Y));
                byte t = color.G;
                bits.Add((int)Convert.ToString(color.G, 2).Last() - '0');
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

        int GetLenght()
        {
            string temp = "";
            Coordinates = GetCoordinates(map, 32);
            for(int i = 0; i < 32; i++)
            {
                Vector2 v = Coordinates[i];
                temp += Convert.ToString(new Color(map.GetPixel((int)v.X, (int)v.Y)).B, 2).Last();
            }
            return Convert.ToInt32(temp, 2);
        }

        Color CalculateColorGreen(Color color, int bit)
        {
            string temp = Convert.ToString(color.G, 2);
            temp = temp.PadLeft(8, '0')
                       .Remove(7);

            temp += bit.ToString();
            return Color.Argb(color.A, color.R, Convert.ToByte(temp, 2), color.B);
        }

        Color CalculateColorBlue(Color color, int bit)
        {
            string temp = Convert.ToString(color.B, 2);
            temp = temp.PadLeft(8, '0')
                       .Remove(7);

            temp += bit.ToString();
            return Color.Argb(color.A, color.R, color.G, Convert.ToByte(temp, 2));
        }

        Color CalculateColorRed(Color color, int bit)
        {
            string temp = Convert.ToString(color.R, 2);
            temp = temp.PadLeft(8, '0')
                       .Remove(7);

            temp += bit.ToString();
            return Color.Argb(color.A, Convert.ToByte(temp, 2), color.G, color.B);
        }

        //Color CalculateColorGreen(Color color, int bit)
        //{
        //    int temp = color.G;
        //    temp >>= 1;
        //    temp &= bit;
        //    return Color.Argb(color.A, color.R, temp, color.B);
        //}

        List<Vector2> GetCoordinates(Bitmap image, int lenght)
        {
            List<Vector2> temp = new List<Vector2>();

            for (int w = 0; w < image.Width; w++)
                for (int h = 0; h < image.Height; h++)
                {
                    temp.Add(new Vector2(w, h));

                    if (temp.Count >= lenght)
                        break;
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
