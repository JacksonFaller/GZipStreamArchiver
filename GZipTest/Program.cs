using System;
using System.Reflection;
using System.Threading;

namespace GZipTest
{
    class Program
    {
        private static readonly int _threadNumber = 10;
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
                Controller.Operation operation;
                Enum.TryParse(args[0], true, out operation);

                if(operation != 0)
                {
                    byte[][] inputBuffer = new byte[_threadNumber][];
                    byte[][] outputBuffer = new byte[_threadNumber][];
                    EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
                    Compressor compressor = new Compressor(waitHandle, inputBuffer, outputBuffer);
                    Controller controller = new Controller(operation, args[1], args[2], _threadNumber, 
                        inputBuffer, outputBuffer, compressor, waitHandle);
                    compressor.SubscribeToSyncCounterResetEvent(controller);

                    return controller;
                }
                else throw new InvalidModeException(); // Mode is not compress / decompress
            }
        }
    }
}