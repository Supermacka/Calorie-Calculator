using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CalorieCalculator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("### Calorie Calculator ###");
            Console.WriteLine("Enter ingredients and get the kalorie amount for each.");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("What ingredients do you have?");
            Console.WriteLine("*Press enter on a blank line to stop adding new ingredients*");

            List<string> ingredients = new List<string>();
            List<string> calories = new List<string>();

            int rowNumber = 1;
            string userInput = "";
            Task doThisTask = null;
            do
            {
                // Input of ingredient
                Console.Write($"{rowNumber} - ");
                userInput = Console.ReadLine();
                if (userInput == "")
                {
                    break;
                }
                ingredients.Add(userInput);

                // Get calorie information
                doThisTask = GetCalories(userInput, calories);

                rowNumber++;
            } while (userInput != "");

            // Extra space
            Console.WriteLine();

            // Await 
            if (rowNumber > 1)
            {
                await Task.Run(() => doThisTask);
                CalculateResult(ingredients, calories);
            }
            else
            {
                Console.WriteLine("No ingredient was entered");
            }

            Console.ReadLine();
        }

        public static async Task GetCalories(string userInput, List<string> calories)
        {
            await Task.Run(() =>
            {
                string urlAddress = @"http://google.com/search?q=" + $"{userInput} kcal 100g";

                List<HtmlNode> nodeList = new List<HtmlNode>();
                var nodes = SearchGoogle(urlAddress).DocumentNode.SelectNodes("//div[@class='BNeawe iBp4i AP7Wnd'] //div //div[@class='BNeawe iBp4i AP7Wnd']");
                if (nodes != null)
                {
                    nodeList = nodes.ToList();
                    foreach (HtmlNode node in nodeList)
                    {
                        if (!node.InnerHtml.All(char.IsLetter))
                        {
                            calories.Add(node.InnerHtml.Substring(0, node.InnerHtml.IndexOf(" ")));
                            break;
                        }
                        else
                        {
                            calories.Add("-");
                        }
                    }
                }
                else
                {
                    GetCalories2(userInput, calories);
                }
            });
        }

        public static void GetCalories2(string userInput, List<string> calories)
        {
            string urlAddress = @"http://google.com/search?q=" + $"kcal 100g {userInput}";
            List<HtmlNode> nodeList = new List<HtmlNode>();

            try
            {
                var nodes = SearchGoogle(urlAddress).DocumentNode.SelectNodes("//div[@class='BNeawe uEec3 AP7Wnd']").ElementAt(1); // The HTML has duplicate string formats. We select the right one out of them
                if (nodes != null)
                {
                    nodeList.Add(nodes);
                    foreach (HtmlNode node in nodeList)
                    {
                        if (!node.InnerHtml.All(char.IsLetter))
                        {
                            calories.Add(node.InnerHtml.Substring(0, node.InnerHtml.IndexOf(" ")));
                            break;
                        }
                        else
                        {
                            calories.Add("-");
                        }
                    }
                }
            }
            catch (System.ArgumentOutOfRangeException)
            {
                calories.Add("-");
            }
            catch (System.ArgumentNullException)
            {
                calories.Add("-");
            }
        }

        public static HtmlDocument SearchGoogle(string urlAddress)
        {
            // Need to process to get the real URL of the question.
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            HtmlDocument doc = null;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                // HTML Source Code 
                string htmlData = readStream.ReadToEnd();
                response.Close();
                readStream.Close();

                // Parsing HTML code
                doc = new HtmlDocument();
                doc.LoadHtml(htmlData);
            }
            return doc;
        }

        public static void CalculateResult(List<string> ingredients, List<string> calories)
        {
            // Display columns
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(String.Format("{0,-30}", "Ingridients:"));
            Console.WriteLine(String.Format("{0,-30}", "Calories (per 100g):"));
            Console.ForegroundColor = ConsoleColor.White;

            float calorieResult = 0;
            for (int i = 0; i < ingredients.Count; i++)
            {
                Console.Write($"{ ingredients[i],-30}");
                Console.WriteLine(calories[i] + (calories[i] != "-" ? " kcal" : ""), -30);
                if (calories[i] != "-")
                {
                    calorieResult += float.Parse(calories[i]);
                }
            }
            //Creating dashed line
            for (int i = 0; i < (ingredients[ingredients.Count - 1].Length + 30 + calories[calories.Count - 1].Length); i++)
            {
                Console.Write("-");
            }
            Console.WriteLine();
            Console.Write(String.Format("{0,-30}", "Result:"));
            Console.WriteLine(calorieResult + " kcal", -30);
        }
    }
}
