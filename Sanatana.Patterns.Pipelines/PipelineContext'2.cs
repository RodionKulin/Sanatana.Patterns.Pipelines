using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Patterns.Pipelines
{
    
    public class PipelineContext<TInput, TOutput>
    {
        public TInput Input { get; set; }
        public TOutput Output { get; set; }
        public int RollBackFromStepIndex { get; set; }
        public List<Exception> Exceptions { get; set; }
    }
}
