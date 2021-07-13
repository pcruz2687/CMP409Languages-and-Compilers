using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AllanMilne.Ardkit;


namespace CMP409_Coursework
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("An input file must be provided.\n");
                return;
            }

            StreamReader infile = null;
            try
            {
                infile = new StreamReader(args[0]);
            }
            catch (IOException e)
            {
                Console.WriteLine("An I/O error occurred {0:s} file {1:s}.", "opening", args[0]);
                Console.WriteLine(e);
                return;
            }

            PALParser parser = new PALParser();
            parser.Parse(infile);

            foreach (ICompilerError err in parser.Errors)
            {
                Console.WriteLine(err);
            }
            if(parser.Errors.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("{0:d} errors found.", parser.Errors.Count);
            }

            try
            {
                infile.Close();
            }
            catch (IOException e)
            {
                Console.WriteLine("An I/O error occurred {0:s} file {1:s}.", "closing", args[0]);
                Console.WriteLine(e);
                return;
            }

            Console.ReadLine();
        }
    }
}
