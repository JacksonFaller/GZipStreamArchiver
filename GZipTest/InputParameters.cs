using GZipTest.Exceptions;
using System;

namespace GZipTest
{
    public class InputParameters
    {
        public string SourceFile { get; set; }
        public string TargetFile { get; set; }
        public Operation Operation { get; set; }

        /// <summary>
        /// Validate parameters and creates Controller and set compress/decompress mode
        /// </summary>
        /// <param name="args">programm arguments, contains: [mode] [input file] [output file]</param>
        public static InputParameters CreateFromArgs(string[] args)
        {
            if (args.Length < 3)
            {
                throw new MissingParametersException();
            }
            else
            {
                var parameters = new InputParameters
                {
                    Operation = GetOperation(args[0]),
                    SourceFile = args[1],
                    TargetFile = args[2]
                };
                
                return parameters;
            }
        }

        public static Operation GetOperation(string operationName)
        {
            if(Enum.TryParse(operationName, true, out Operation operation))
                return operation;

            throw new InvalidModeException();
        }
    }
}
