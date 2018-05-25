using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Patterns.Pipelines
{
    public class PipelineResult<T> : PipelineResult
    {
        //properties
        public T Data { get; set; }



        //init
        public PipelineResult()
        {
        }

        public PipelineResult(bool result, List<string> messages = null)
        {
            Completed = result;
            Messages = messages ?? new List<string>();
        }

        public new static PipelineResult<T> Error(string message)
        {
            return new PipelineResult<T>()
            {
                Completed = false,
                Messages = new List<string> { message }
            };
        }

        public new static PipelineResult<T> Error(List<string> messages)
        {
            return new PipelineResult<T>()
            {
                Completed = false,
                Messages = messages
            };
        }

        public static PipelineResult<T> Success(T data)
        {
            return new PipelineResult<T>()
            {
                Data = data,
                Completed = true
            };
        }
    }
}
