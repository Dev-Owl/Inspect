using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InspectCodeHTML
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    PrintHelp();
                }
                else
                {
                    string output = "", input = "";
                    try
                    {
                        foreach (string argument in args)
                        {
                            if (argument.StartsWith("/") && argument.Contains("="))
                            {
                                switch (char.ToLower(argument[1]))
                                {
                                    case 'i':
                                        {
                                            input = GetPath(argument);
                                        } break;
                                    case 'o':
                                        {
                                            output = GetPath(argument);
                                        } break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        PrintHelp();
                    }
                    if (String.IsNullOrEmpty(output))
                    {
                        output = input.Replace(System.IO.Path.GetFileName(input), "InspectCodeHTML.html");
                    }

                    HTMLCreator creator = new HTMLCreator(input, output);
                    creator.Run();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Internal error: " + ex.ToString());
            }

            WaitOnAnyKey();
        }

        static void PrintHelp()
        {
            Console.WriteLine("Missing or invalid arguments, please see the help below");
            Console.WriteLine("-------------------------------------------------------");
            Console.WriteLine(@"/i Input path to the file. Example /i=C:\Result.xml");
            Console.WriteLine(@"/o Output path of the HTML file. Example /o=C:\Result.html");
            Console.WriteLine(@"If /o is not set the input directory is used");
            Console.WriteLine("-------------------------------------------------------");

        }

        static string GetPath(string RawValue)
        {
            return RawValue.Substring(RawValue.IndexOf('=') + 1).Replace("\"", "");
        }

        static void WaitOnAnyKey(string Message="Press any Key to continue")
        {
            Console.WriteLine(Message);
            Console.ReadLine();
        }
    }
}
