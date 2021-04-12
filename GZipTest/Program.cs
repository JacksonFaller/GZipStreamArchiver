using GZipTest.Exceptions;
using System;
using System.Reflection;
using System.Threading;

namespace GZipTest
{
    public class Program
    {
        private static readonly int _threadNumber = 10;

        public static void Main(string[] args)
        {
            try
            {
                AssemblyName assemblyName = typeof(Program).Assembly.GetName();
                Console.WriteLine("{0}, Version={1}", assemblyName.Name, assemblyName.Version);

                Log.SetLogger(new ConsoleLogger());
                InputParameters inputParams = InputParameters.CreateFromArgs(args);
                ICompressionController controller = InitController(inputParams);
                
                Console.WriteLine("Executing operation...");

                Execute(inputParams, controller);

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
                Console.WriteLine("Unexpected error occured");
            }
        }

        private static void Execute(InputParameters inputParams, ICompressionController controller)
        {
            using (IStreamCreator streamCreator = new StreamCreator(inputParams.SourceFile, inputParams.TargetFile))
            {
                if (inputParams.Operation == Operation.Compress)
                {
                    controller.ReadAndInvokeCompress(streamCreator.Source, streamCreator.Target);
                }
                else // target opeartion = decompress
                {
                    controller.ValidateArchive(streamCreator.Source);
                    controller.ReadAndInvokeDecompress(streamCreator.Source, streamCreator.Target);
                }
            }
        }

        private static ICompressionController InitController(InputParameters parameters)
        {
            byte[][] inputBuffer = new byte[_threadNumber][];
            byte[][] outputBuffer = new byte[_threadNumber][];
            EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            Compressor compressor = new Compressor(waitHandle, inputBuffer, outputBuffer);
            CompressionController controller = new CompressionController(_threadNumber, inputBuffer, outputBuffer, compressor, waitHandle);
            compressor.SubscribeToSyncCounterResetEvent(controller);
            return controller;
        }
    }
}
