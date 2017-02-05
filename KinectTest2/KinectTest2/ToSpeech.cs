using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;

namespace KinectTest2
{
    public enum Position
    {
        Left,
        Right,
        Center
    }

    public class ToSpeech
    {
        private static int SYNTH_RATE = 2;
        private static int SYNTH_VOL = 100;

        public string type;
        public double distance;
        public Position position;
        private bool exists;

        public ToSpeech()
        {
            exists = false;
        }
        public ToSpeech(string type, double distance, Position position)
        {
            string[] type_words = type.Split('/');
            this.type = type_words[1];
            this.distance = distance;
            this.position = position;
            this.exists = true;
        }


        public override string ToString()
        {
            return base.ToString() + " " + this.type + " " + this.distance + " " + this.position;
        }

        public void WarnUserOfObstruction()
        {
            if (!exists)
            {
                return;
            }

            Console.WriteLine("About to speak.");

            SpeechSynthesizer synthesizer = new SpeechSynthesizer();
            synthesizer.Volume = SYNTH_VOL;
            synthesizer.Rate = SYNTH_RATE;

            // Speak() or SpeakAsynch() ?
            string warning = ConstructWarning();
            synthesizer.Speak(warning);
        }

        private string ConstructWarning()
        {
            StringBuilder warning = new StringBuilder();
            warning.Append("There is a ");
            warning.Append(type);
            warning.Append(" about ");
            warning.Append(RoundToSignificantDigits(distance, 2));
            warning.Append(" metres ahead to the ");
            warning.Append(position);
            warning.Append(".");

            return warning.ToString();
        }

        private static double RoundToSignificantDigits(double d, int digits)
        {
            if (d == 0)
                return 0;

            double scale = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(d))) + 1);
            return scale * Math.Round(d / scale, digits);
        }
    }
}