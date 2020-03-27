using BlogML.Core.Xml;
using BlogMLConverter.Code;
using BlogMLConverter.Enums;
using BlogMLConverter.Extensions;
using BlogMLConverter.Models;
using BlogMLConverter.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace BlogMLConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            //try
            //{
            //  Check if action is present
            Settings setting = ParseInput(args);
            
            if (setting.ToolAction == ToolAction.Unknown)
            {
                PringUsage();
                return;
            }

            string logPath = String.Format("ConsoleLog-{0}-{1}.log", setting.ToolAction, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
            ConsoleCopy consoleLog = new ConsoleCopy(logPath);

            ConverterML converter = new ConverterML(consoleLog, setting.BlogMLFileName, setting.WRXFileName, setting.SourceImageUrl, setting.DestinationImageUrl);

            //We got Action
            switch (setting.ToolAction)
            {
                case ToolAction.RemoveComments:
                    if (RequiredParametersPrintUsage(ToolAction.RemoveComments, args))
                    {
                        converter.RemoveAllComments(setting.BlogMLFileName);
                        Console.WriteLine("All comments removed from the file : {0}", setting.BlogMLFileName);
                    }
                    break;
                case ToolAction.ExportToWRX:
                    if (RequiredParametersPrintUsage(ToolAction.ExportToWRX, args))
                    {
                        string wrxFileName = converter.GenerateWRXFile();
                        Console.WriteLine("Created WRX format");

                        //Generate ReDirect, SourceQA and TargetQA File
                        converter.GenerateHelperFiles(setting.SourceBaseUrl, setting.TargetBaseUrl, wrxFileName);
                    }
                    break;
                case ToolAction.QATarget:
                    if (RequiredParametersPrintUsage(ToolAction.QATarget, args))
                    {
                        converter.QATarget(setting.QATargetFileName);
                    }
                    break;
                case ToolAction.NewWRXWithOnlyFailedPosts:
                    if (RequiredParametersPrintUsage(ToolAction.NewWRXWithOnlyFailedPosts, args))
                    {
                        string wrxFileName = converter.GenerateWRXFileWithFailedPosts(setting.WRXFileName, setting.QAReportFileName);
                        Console.WriteLine("Successfully created WRX file with only error post names");
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Console.WriteLine("ALL Done");
            Console.ReadLine();

            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex);
            //    Console.WriteLine();
            //    PringUsage();
            //}
            if (consoleLog != null) consoleLog.Dispose();
        }

        private static Settings ParseInput(string[] args)
        {
            const string validArguments = "/Action: /BlogMLFile: /WRXFile: /QASourceFile: /QATargetFile: /QAReportFile: /SourceUrl: /TargetUrl: /SourceImageUrl: /DestinationImageUrl: /NewWRXWithOnlyFailedPosts:";

            Settings setting = new Settings();
            foreach (string s in args)
            {
                if (s.IndexOf(':') == -1)
                    throw new Exception("Arguments not in the format /Key:Valye");

                string[] cmd = s.Split(':');
                string argumentKey = cmd[0].ToUpper();
                string argumentValue = cmd[1];
                if (cmd.Length > 1)
                    for (int i = 2; i < cmd.Length; i++)
                        argumentValue +=  ":" + cmd[i];

                if (!validArguments.ToUpper().Contains(argumentKey))
                {
                    throw new Exception("Unknown parameters passed. Valid values are " + validArguments);
                }

                switch (argumentKey) {
                    case "/ACTION":
                        setting.ToolAction = (ToolAction)Enum.Parse(typeof(ToolAction), argumentValue, false);
                        break;
                    case "/BLOGMLFILE":
                        setting.BlogMLFileName = argumentValue;
                        break;
                    case "/WRXFILE":
                        setting.WRXFileName = argumentValue;
                        break;
                    case "/QASOURCEFILE":
                        setting.QASourceFileName = argumentValue;
                        break;
                    case "/QATARGETFILE":
                        setting.QATargetFileName = argumentValue;
                        break;
                    case "/SOURCEURL":
                        setting.SourceBaseUrl = argumentValue;
                        break;
                    case "/TARGETURL":
                        setting.TargetBaseUrl = argumentValue;
                        break;
                    case "/QAREPORTFILE":
                        setting.QAReportFileName = argumentValue;
                        break;
                    case "/SOURCEIMAGEURL":
                        setting.SourceImageUrl = argumentValue;
                        break;
                    case "/DESTINATIONIMAGEURL":
                        setting.DestinationImageUrl = argumentValue;
                        break;
                }
            }

            return setting;
        }

        private static void PringUsage()
        {
            const string exeName = "BlogMLConverter";

            Console.WriteLine("BlogML to WRX Converter by Enrico Rossini (http://www.puresourcecode.com)");
            Console.WriteLine("This tool helps to convert BlogML to WRX and also helps to generate some QA text files for verifications");
            Console.WriteLine();
            Console.WriteLine("Remove Comments: If  you got lot of spam comments, it's worth removing it.");
            Console.WriteLine("{0} /Action:RemoveComments /BlogMLFile:yourblogmlfile.xml", exeName);
            Console.WriteLine();
            Console.WriteLine("Export To WRX: Will create WordPress Export file (WRX) and also supporting redirect, source and target QA files (for verification).");
            Console.WriteLine("{0} /Action:ExportToWRX /BlogMLFile:yourblogmlfile.xml /SourceUrl:blogs.puresourcecode.com /TargetUrl:puresourcecode.com", exeName);
            Console.WriteLine();
            Console.WriteLine("QA Source. Check whether all the original url are redirection to 301");
            Console.WriteLine("{0} /Action:QASource /QASourceFile:QASourceFile.xml ", exeName);
            Console.WriteLine();
            Console.WriteLine("QA Target. Check whether all the new urls are correct. Will produce a report file with HTTP Status.");
            Console.WriteLine("{0} /Action:QATarget /QATargetFile:QATargetFile.xml ", exeName);
        }

        private static bool RequiredParametersPrintUsage(ToolAction action, string[] args)
        {
            string validArguments;

            switch (action)
            {
                case ToolAction.RemoveComments:
                    validArguments = "/Action: /BlogMLFile:";
                    if (args.Length != 2)
                    {
                        Console.WriteLine("/BlogMLFile: is mandatory");
                        return false;
                    }
                    break;
                case ToolAction.ExportToWRX:
                    validArguments = "/Action: /BlogMLFile: /SourceUrl: /TargetUrl: /SourceImageUrl: /DestinationImageUrl: /WRXFile";
                    if (args.Length != 7)
                    {
                        Console.WriteLine("/BlogMLFile: /SourceUrl: /TargetUrl: are mandatory");
                        return false;
                    }
                    break;
                case ToolAction.QATarget:
                    validArguments = "/Action: /QATargetFile:";
                    if (args.Length != 2)
                    {
                        Console.WriteLine("/QATargetFile: is mandatory");
                        return false;
                    }
                    break;
                case ToolAction.QASource:
                    validArguments = "/Action: /QASourceFile:";
                    if (args.Length != 2)
                    {
                        Console.WriteLine("/QASourceFile: is mandatory");
                        return false;
                    }
                    break;
                case ToolAction.NewWRXWithOnlyFailedPosts:
                    validArguments = "/Action: /WRXFile: /QAReportFile:";
                    if (args.Length != 3)
                    {
                        Console.WriteLine("/WRXFile: /QAReportFile are mandatory");
                        return false;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException("action");
            }

            foreach (string s in args)
            {
                string argumentKey = s.Split(':')[0].ToUpper();
                if (!validArguments.ToUpper().Contains(argumentKey))
                {
                    Console.WriteLine("Unknown parameters passed. Valid values are " + validArguments);
                    return false;
                }
            }

            return true;
        }
    }
}