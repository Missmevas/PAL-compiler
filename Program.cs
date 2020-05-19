using System;
using System.Collections.Generic;
using System.IO;



namespace AllanMilne.PALCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            List<ICompilerError> errors = new List<ICompilerError>();

            if (args.Length > 0) //read a single test file
            {
                StreamReader source = new StreamReader(args[0]);
                customParser parser = new customParser();
                PALScanner scanner = new PALScanner();
                parser.Parse(source);

                foreach (CompilerError err in parser.Errors)
                    Console.WriteLine(err);
                Console.WriteLine("\n");
            }
            else 
            {
                Console.WriteLine("Provide the name of the folder containing test suite");
                //String folderName = "C:/Users/Doris/Desktop/PALTests/semantic-errors";
                String folderName = Console.ReadLine();
                Console.WriteLine("\n");

               
                DirectoryInfo directory = new DirectoryInfo(@folderName);
                FileInfo[] Files = directory.GetFiles("*.txt"); //Get files

                foreach (FileInfo file in Files)
                {
                    StreamReader source = new StreamReader(folderName + "/" + file.Name);
                    Console.WriteLine(file.Name);


                    customParser parser = new customParser();
                    PALScanner scanner = new PALScanner();
                    parser.Parse(source);

                    foreach (CompilerError err in parser.Errors)
                        Console.WriteLine(err);
                    Console.WriteLine("\n");
                }
            }

            Console.ReadKey();

            


        }
    }
}
