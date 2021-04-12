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

                Log.SetLogger(new ConsoleLogger()); 
                Controller controller = InitContoller(args);
                Console.WriteLine("Executing operation...");
                controller.ExecuteOperation();

                Console.WriteLine("Done!");
                Console.Read();
            }
            catch (InvalidModeException ex)
            {
                Log.Error(ex);
                Console.WriteLine(ex.Message);
            }
            catch(MissingParametersException ex)
            {
                Log.Error(ex);
                Console.WriteLine("{0}\n{1}", ex.Message, ex.Usage);
            }
            catch (InvalidFormatException ex)
            {
                Log.Error(ex);
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
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
                if (args[0].Equals(Controller.Operation.Compress.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    operation = Controller.Operation.Compress;
                }
                else
                {
                    if (args[0].Equals(Controller.Operation.Decompress.ToString(), StringComparison.OrdinalIgnoreCase))
                        operation = Controller.Operation.Decompress;
                    else
                        throw new InvalidModeException(); // Mode is not compress / decompress
                }

                byte[][] inputBuffer = new byte[_threadNumber][];
                byte[][] outputBuffer = new byte[_threadNumber][];
                EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
                Compressor compressor = new Compressor(waitHandle, inputBuffer, outputBuffer);
                Controller controller = new Controller(operation, args[1], args[2], _threadNumber, 
                    inputBuffer, outputBuffer, compressor, waitHandle);
                compressor.SubscribeToSyncCounterResetEvent(controller);

                return controller;
            }
        }
    }
}
