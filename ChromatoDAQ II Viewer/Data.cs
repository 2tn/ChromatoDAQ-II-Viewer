using System;

namespace ChromatoDAQ_II_Viewer
{
    public class Data
    {
        public Data(double time, double ch1, double ch2, int input) { Time = time;  Ch1 = ch1; Ch2 = ch2; Input = input; }

        private double time = 0;
        public double Time
        { get { return time; } set { time = value; } }

        private double ch1 = 0;
        public double Ch1
        { get { return ch1; } set { ch1 = value; } }

        private double ch2 = 0;
        public double Ch2
        { get { return ch2; } set { ch2 = value; } }

        private int input = 0;
        public int Input
        { get { return input; } set { input = value; } }
    }
}
