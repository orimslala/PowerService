using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerServiceChallenge
{
    public class MissingLogFileConfigurationException : Exception
    {
        public MissingLogFileConfigurationException(string val) : base(val) { }
    }

    public class InvalidFilePathException : Exception
    {
        public InvalidFilePathException(string val) : base(val) { }
    }

    public class InvalidIntervalException : Exception
    {
        public InvalidIntervalException(string val) : base(val) { }
    }
}
