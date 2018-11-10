using System;
using System.Collections.Generic;
using System.Text;

namespace GuptaMigration.Models
{
   public class RequestRecord
    {

        public string FunctionName { get; set; }

        public string CalledByFunction { get; set; }

        public string TableName { get; set; }

        public List<string> Operation { get; set; }

        public int OperationLevel { get; set; }

        public int Count { get; set; }

        public string File { get; set; }

    }
}
