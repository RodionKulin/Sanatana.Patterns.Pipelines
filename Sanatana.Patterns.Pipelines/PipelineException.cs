using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Patterns.Pipelines
{
    public class PipelineException<TInput> : AggregateException
    {
        public TInput PipelineInput { get; set; }

        public PipelineException()
            : base()
        {

        }
        public PipelineException(string message, IEnumerable<Exception> exceptions, TInput input)
            : base(exceptions)
        {
            PipelineInput = input;
        }

    }
}
