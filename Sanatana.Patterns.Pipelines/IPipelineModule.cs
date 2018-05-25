using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Patterns.Pipelines
{
    public interface IPipelineModule<TInput, TOutput>
    {
        Task<bool> RollForward(PipelineContext<TInput, TOutput> context);
        Task<bool> RollBack(PipelineContext<TInput, TOutput> context);
    }
}
