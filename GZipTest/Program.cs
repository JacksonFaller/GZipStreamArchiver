using System;
using System.Reflection;

namespace GZipTest
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                AssemblyName assemblyName = typeof(Program).Assembly.GetName();
                Console.WriteLine("{0}, Version={1}", assemblyName.Name, assemblyName.Version);

                Controller controller = InitContoller(args);
                Console.WriteLine("Executing operation...");
                controller.ExecuteOperation();

                Console.WriteLine("Done!");
                Console.Read();
            }
            catch (InvalidModeException ex)
            {
                Console.WriteLine(ex.GetType().FullName);
                Console.WriteLine(ex.Message);
            }
            catch(MissingParametersException ex)
            {
                Console.WriteLine(ex.GetType().FullName);
                Console.WriteLine("{0}\n{1}", ex.Message, ex.Usage);
            }
            catch (InvalidFormatException ex)
            {
                Console.WriteLine(ex.GetType().FullName);
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetType().FullName);
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Validate parameters and creates Controller and set compress/decompress mode
        /// </summary>
        /// <param name="args">programm arguments, contains: [mode] [input file] [output file]</param>
        private static Controller InitContoller(string[] args)
        {
            if (args.Length < 3)
            {
                throw new MissingParametersException();
            }
            else
            {
                if (args[0].Equals(Controller.Operation.Compress.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return new Controller(Controller.Operation.Compress, args[1], args[2]);
                }
                else
                {
                    if (args[0].Equals(Controller.Operation.Decompress.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        return new Controller(Controller.Operation.Decompress, args[1], args[2]);
                    }
                    else throw new InvalidModeException(); // Mode is not compress / decompress
                }
            }
        }
    }
}