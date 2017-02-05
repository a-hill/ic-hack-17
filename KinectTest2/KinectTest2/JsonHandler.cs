using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Newtonsoft.Json;
using System.Web.Helpers;
using System.Net.Http;

namespace KinectTest2
{

    public static class JsonHandler
    {
        public static void jsonHandlerAndFilterAndSpeak(string text)
        {
            //string text = System.IO.File.ReadAllText(@"C:\Users\ahmer\Documents\ic-hack-17\json.txt");

            var client = new HttpClient();

            var reqCotent = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>("img", "this is a block of text"),
            });



            // print the json string
            System.Console.WriteLine("Contents {0}", text);

            // parse the json into data
            dynamic data = Json.Decode(text);

            // get the most reliable result and fill a toSpeech object
            ToSpeech result = bestResult(data);
            Console.WriteLine("result=");
            Console.WriteLine(result);

            // do a test print
            // Console.WriteLine("if this works it will print a class");
            // Console.WriteLine(data.images[0].classifiers[0].classes[0]["class"]);

            result.WarnUserOfObstruction();

            // all done, wait for key press
            System.Console.ReadLine();
        }

        public static ToSpeech bestResult(dynamic data)
        {
            ToSpeech result;

            // to to get images[0]
            try
            {
                dynamic classes = data.images[0].classifiers[0].classes;

                // finally, filter classes: choose classObj with non-null type and highest score
                // 1) filter out the null type-hierachies. 2) choose one with highest score
                decimal highest = -1;
                string typeOfHighest = "";
                foreach (dynamic claz in classes)
                {
                    decimal parsed = claz.score;
                    //int.TryParse(claz.score, out parsed);
                    if (parsed > highest && claz.type_hierarchy != null)
                    {
                        highest = parsed;
                        typeOfHighest = claz.type_hierarchy;
                    }
                }

                // now, construct a ToSpeech
                result = new ToSpeech(typeOfHighest, 0, Position.Left); // we only know type rn

            }
            catch (Exception e) // this would be better unhandled here and instead caught by caller function
            {
                Console.WriteLine(e);
                result = new ToSpeech(); // return an empty object
            }

            return result;
        }

    }
}