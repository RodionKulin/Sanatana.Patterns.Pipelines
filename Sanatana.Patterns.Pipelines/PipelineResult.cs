using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Patterns.Pipelines
{
    public class PipelineResult
    {
        //properties
        public bool Completed { get; set; }
        public List<string> Messages { get; set; }



        //init
        public PipelineResult()
        {
        }

        public PipelineResult(bool result, List<string> messages = null)
        {
            Completed = result;
            Messages = messages ?? new List<string>();
        }

        public static PipelineResult Error(string message)
        {
            return new PipelineResult()
            {
                Completed = false,
                Messages = new List<string> { message }
            };
        }

        public static PipelineResult Error(List<string> messages)
        {
            return new PipelineResult()
            {
                Completed = false,
                Messages = messages
            };
        }

        public static PipelineResult Success()
        {
            return new PipelineResult()
            {
                Completed = true
            };
        }
    }

    
}
