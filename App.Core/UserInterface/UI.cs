using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.UserInterface
{

    public struct Vector2
    {
        public Vector2(Int32 x, Int32 y)
        {
            this.X = x;
            this.Y = y;
        }
        public Int32 X;
        public Int32 Y;
    }


    public class UI
    {
        private UInt16 Background;
        private Vector2 Position;
        private String[] Texts = [];
        private String[] Floating = [];
        private Object ToObject = null;



        public static UI DIALOG(UInt16 background)
        {

            return new UI() { Background = background };
        }


        public UI POSITION(Int32 x, Int32 y)
        {
            this.Position = new Vector2(x, y);
            return this;
        }

        public UI TEXT(String[] strings)
        {
            this.Texts = strings;
            return this;
        }
        public UI FLOATING(String[] strings)
        {
            this.Floating = strings;
            return this;
        }

        public UI TO(Object @object)
        {
            this.ToObject = @object;
            return this;
        }

        public void WAITCLOSE()
        {

        }


        public void SEND()
        {
            Console.WriteLine(this);
        }



        public static String GIF(string name, string resourceUri, Int32 x, Int32 y)
        {
            return $"gif {name},{resourceUri},{x},{y}";
        }
        public static String IMAGE(string name, string resourceUri, Int32 x, Int32 y)
        {
            return $"image {name},{resourceUri},{x},{y}";
        }
        public static String BUTTTON(string name, string resourceUri, Int32 x, Int32 y)
        {
            return $"button {name},{resourceUri},{x},{y}";
        }


    }
}
