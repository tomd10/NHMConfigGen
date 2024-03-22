using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;
using System.Collections.Specialized;
using System.Security;

namespace NHMConfigGen
{
    internal class Program
    {
        public static void Main()
        {
            const string sep = "@@@@";
        start0:
            List<string> lines = new List<string>();
            lines.Add("Config file for NHM v1.0");
            Console.WriteLine("NHM config generator v1.0");
            
            string choice0 = re("Encrypt cleartext file? [y/N]").ToLower();
            if (choice0 == "y" || choice0 == "yes")
            {
                start2:
                string path = re("Path to source file: ");
                if (File.Exists(path))
                {
                    string text = File.ReadAllText(path);
                    string key = re("AES key (ASCII, 0-16 chars): ");

                    byte[] aesKey = Model.StringToByteArr(key);
                    byte[] ciphertext = AesEncryption.Encrypt(text, aesKey, aesKey);
                    string ciphertextstr = Model.ByteArrToString(ciphertext);

                    string path2 = re("Path to destination file: [" + path + "encr");
                    if (path2 == "") path2 =  path+"encr";

                    File.WriteAllText(path2, ciphertextstr);

                }
                else
                {
                    string choice = re("File does not exist! Repeat? [Y/n]").ToLower();
                    if (choice == "y" || choice == "yes")
                    {
                        goto start2;
                    }
                }
            }
            
            int devCount = 0;
            int condCount = 0;
            startChoice:
            string choice1 = re("Device or condition or end? [d/C/e]").ToLower();
            if (choice1 == "d" || choice1 == "dev" || choice1 == "device")
            {
                //Device
                startDev:
                string choice2 = re("Name [Device" + devCount.ToString() + "]: ");
                if (choice2 == "") choice2 = "Device" + devCount.ToString();

                start3:
                string choice3 = readNe("Hostname: ");

                start10:
                int port = readInt("SSH port number [22]:", 22);
                if (port < 1 || port > 65535)
                {
                    Console.WriteLine("Port number must be between 1 and 65535!");
                    goto start10;
                }

                string choice4 = readStr("Username [admin]: ", "admin");
                string  choice5 = re("Password: ");
                string choice6 = re("Init commands, separate by ####");

                string cmd = "DEVICE"+sep+choice2+sep+choice3+sep+ port + sep + choice4+sep+choice5+sep+choice6;
                Console.WriteLine("Line: " + cmd);

                if (!readBool("OK? [Y/n]", true))
                {
                    goto startDev;
                }
                else
                {
                    lines.Add(cmd);
                    Console.WriteLine(cmd);
                    devCount = devCount + 1;
                    goto startChoice;
                }

            }
            else if (choice1 == "" || choice1 == "c" || choice1 == "cond" || choice1 == "condition")
            {
                startCond:
                string cmd = "";
                Console.WriteLine("0  - Command");
                Console.WriteLine("1  - Display set of words");
                Console.WriteLine("2  - Display set of lines");
                Console.WriteLine("3  - Value equal to");
                Console.WriteLine("4  - Value not equal to");
                Console.WriteLine("5  - Value greater than");
                Console.WriteLine("6  - Value less than");
                Console.WriteLine("7  - Line count equal to");
                Console.WriteLine("8  - Line count NOT equal to");
                Console.WriteLine("9  - Line count greater than");
                Console.WriteLine("10 - Line count less than");
                Console.WriteLine("11 - Word range contains substring");
                Console.WriteLine("12 - Word range doesn't contain substring");
                Console.WriteLine("13 - Line range contains substring");
                Console.WriteLine("14 - Line range doesn't contain substrings");
                int mode = readInt("Mode number [0]: ", 0);
                if (mode < 0 || mode > 14) goto startCond;

                string name = readStr("Condition name [Condition" + condCount.ToString() + "]: ", "Condition" + condCount.ToString());

                if (mode == 0)
                {
                    string commands = re("Enter commands to execute; separate by ####");
                    cmd = "CONDITION" + sep + name + sep + mode.ToString() + sep + commands;
                }
                else if (mode == 1)
                {
                    string command = readCmd();
                    int start, end;
                    readStartEnd(out start, out end);
                    int line = readInt("Line index [0]", 0);

                    string pre = "", post = "", separator = "";
                    bool trimSeparator = true;
                    if (readBool("Extended options? [y/N]", false))
                    {
                        readPrePost(out pre, out post);
                        readExt(out separator, out trimSeparator);
                    }
                    cmd = "CONDITION" + sep + name + sep  + mode.ToString() + sep + command + sep + start + sep + end + sep + line + sep + pre + sep + post + sep + separator + sep + trimSeparator.ToString();
                }
                else if (mode == 2)
                {
                    string command = readCmd();
                    int start, end;
                    readStartEnd(out start, out end);

                    string pre = "", post = "";
                    if (readBool("Extended options? [y/N]", false))
                    {
                        readPrePost(out pre, out post);
                    }
                    cmd = "CONDITION" + sep + name + sep + mode.ToString() + sep + command + sep + start + sep + end + sep + pre + sep + post;
                }
                else if (mode == 3 || mode == 4 || mode == 5 || mode == 6)
                {
                    string command = readCmd();
                    int start = readInt("Start index [0]:", 0);
                    int line = readInt("Line index [0] ", 0);
                    int thr = readIntSigned("Threshold [0]: ");

                    string pre = "", post = "", separator = "";
                    bool trimSeparator = true, trimNonNum=true; 
                    if (readBool("Extended options? [y/N]", false))
                    {
                        readPrePost(out pre, out post);
                        readExt(out separator, out trimSeparator);
                        trimNonNum = readNn();
                    }

                    cmd = "CONDITION" + sep + name + sep + mode.ToString() + sep + command + sep + start + sep + line + sep + thr + sep + pre + sep + post + sep + separator + sep + trimSeparator.ToString() + sep + trimNonNum.ToString();
                }
                else if (mode == 7 || mode == 8 || mode == 9 || mode == 10)
                {
                    string command = readCmd();
                    int thr = readIntSigned("Threshold [0]: ");

                    string pre = "", post = "";
                    if (readBool("Extended options? [y/N]", false))
                    {
                        readPrePost(out pre, out post);

                    }

                    cmd = "CONDITION" + sep + name + sep + mode.ToString() + sep + command + sep + thr.ToString() + sep + pre + sep + post;
                }
                else if (mode == 11 || mode == 12)
                {
                    string command = readCmd();
                    int start, end;
                    readStartEnd(out start, out end);
                    int line = readInt("Line index [0]", 0);
                    string sub = readNe("Substring to search for:");

                    string pre = "", post = "", separator = "";
                    bool trimSeparator = true;
                    if (readBool("Extended options? [y/N]", false))
                    {
                        readPrePost(out pre, out post);
                        readExt(out separator, out trimSeparator);
                    }

                    cmd = "CONDITION" + sep + name + sep + mode.ToString() + sep + command + sep + start.ToString() + sep + end.ToString() + sep + line.ToString() + sep + sub + sep + pre + sep + post + sep + separator + sep + trimSeparator.ToString();
                }
                else if (mode == 13 || mode == 14)
                {
                    string command = readCmd();
                    int start, end;
                    readStartEnd(out start, out end);
                    string sub = readNe("Substring to search for:");

                    string pre = "", post = "";
                    if (readBool("Extended options? [y/N]", false))
                    {
                        readPrePost(out pre, out post);
                    }
                    cmd = "CONDITION" + sep + name + sep + mode.ToString() + sep + command + sep + start.ToString() + sep + end.ToString() + sep + sub + sep + pre + sep + post;
                }


                if (!readBool("OK? [Y/n]", true))
                {
                    goto startChoice;
                }
                else
                {
                    lines.Add(cmd);
                    Console.WriteLine(cmd);
                    condCount = condCount + 1;
                    goto startChoice;
                }

            }
            else if (choice1 == "end" || choice1 == "e")
            {
                if (lines.Count == 0) return;
                string text = String.Join("\n", lines.ToArray());
                string key = re("AES key (ASCII, 0-16 chars): ");

                byte[] aesKey = Model.StringToByteArr(key);
                byte[] ciphertext = AesEncryption.Encrypt(text, aesKey, aesKey);
                string ciphertextstr = Model.ByteArrToString(ciphertext);

                string path2 = readStr("Path to destination file [conf.nhm]: ", "conf.nhm");


                File.WriteAllText(path2, ciphertextstr);
                File.WriteAllText(path2 + "clear", text);
            }
            else
            {
                goto startChoice;
            }


            Console.ReadKey();
        }

        public static string re(string s)
        {
            Console.WriteLine(s);
            return Console.ReadLine();
        }

        public static int readInt(string s, int def)
        {
        start:
            int ret;
            string choice = re(s);
            if (choice == "") ret = def;
            else
            {
                bool result = int.TryParse(choice, out ret);
                if (result == false || (ret < 0))
                {
                    Console.WriteLine("Wrong input! Enter non-negative integer.");
                    goto start;
                }
            }

            return ret;
        }

        public static int readIntSigned(string s)
        {
        start:
            int ret;
            string choice = re(s);
            if (choice == "") ret = 0;
            else
            {
                bool result = int.TryParse(choice, out ret);
                if (result == false)
                {
                    Console.WriteLine("Wrong input! Enter non-negative integer.");
                    goto start;
                }
            }

            return ret;
        }

        public static string readNe(string s)
        {
            start:
            string choice = re(s);
            if (choice == "")
            {
                Console.WriteLine("Wrong input! Empty strings are not allowed.");
                goto start;
            }

            return choice;
        }

        public static void readPrePost(out string pre, out string post)
        {
            string choice5 = re("Enter pre-commands; separate by ####: ");
            string choice6 = re("Enter post-commands, separate by ####: ");
            pre = choice5;
            post = choice6;
        }

        public static string readStr(string s, string def)
        {
            string choice = re(s);
            if (choice == "") choice = def;
            return choice;
        }

        public static bool readBool(string s, bool def)
        {
            string choice = re(s).ToLower();
            if (choice == "y" || choice == "yes" || choice == "ye")
            {
                return true;
            }
            else if (choice == "n" || choice == "no")
            {
                return false;
            }
            else return def;
        }

        public static void readStartEnd(out int start, out int end)
        {
            start:
            int st = readInt("Start index [0]: ", 0);
            int en = readInt("End index [0]: ", 0);

            if (st > en)
            {
                Console.WriteLine("Start must be less than or equal to end!");
                goto start;
            }

            start = st;
            end = en;
        }

        public static string readCmd()
        {
            return readNe("Command to execute: ");
        }

        public static void readExt(out string separator, out bool trimSeparator)
        {
            separator = readStr("Word separator? [ ]", " ");
            trimSeparator = readBool("Trim separator? [Y/n]", true);
        }

        public static bool readNn()
        {
            return readBool("Trim non-numerical characters? [Y/n]", true);
        }

    }
}
