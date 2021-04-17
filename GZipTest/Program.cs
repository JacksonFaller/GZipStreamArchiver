using GZipTest.Exceptions;
using GZipTest.Factories;
using GZipTest.Interafaces;
using System;
using System.Configuration;
using System.Reflection;

namespace GZipTest
{
    public class Program
    {
        private static int _threadNumber;
        public const string MaxThreads = nameof(MaxThreads);

        public static int Main(string[] args)
        {
            try
            {
                AssemblyName assemblyName = typeof(Program).Assembly.GetName();
                Console.WriteLine("{0}, Version={1}", assemblyName.Name, assemblyName.Version);
                Log.SetLogger(new ConsoleLogger(true));

                InputParameters inputParams = InputParameters.CreateFromArgs(args);
                ReadConfig();
                Console.WriteLine("Executing operation...");
                Execute(inputParams);
                Console.WriteLine("Done!");

                return 1;
            }
            catch (InvalidModeException ex)
            {
                Log.Error(ex);
                Console.WriteLine(ex.Message);
            }
            catch (MissingParametersException ex)
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
            return 0;
        }

        private static void Execute(InputParameters inputParams)
        {
            using (ICompressionController controller = InitController())
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

        private static void ReadConfig()
        {
            if (int.TryParse(ConfigurationManager.AppSettings[MaxThreads], out int maxThreads))
            {
                _threadNumber = maxThreads;
            }
            else
            {
                _threadNumber = Environment.ProcessorCount;
                Console.WriteLine($"Parameter {MaxThreads} is not set in config, using the processor count {_threadNumber}");
            }
        }

        private static ICompressionController InitController()
        {
            CompressionController controller = new CompressionController(_threadNumber, new CompressorFactory(), new ThreadStorage<int>(_threadNumber));
            return controller;
        }
    }
}
