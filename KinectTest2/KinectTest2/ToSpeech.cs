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
        public int distance;
        public Position position;
        private bool exists;

        public ToSpeech()
        {
            exists = false;
        }
        public ToSpeech(string type, int distance, Position position)
        {
            string[] type_words = type.Split('/');
            this.type = type_words[type_words.Length - 1];
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
            warning.Append(distance);
            warning.Append(" metres ahead to the ");
            warning.Append(position);
            warning.Append(".");

            return warning.ToString();
        }
    }
}