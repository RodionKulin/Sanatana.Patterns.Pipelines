using System;
using System.Threading.Tasks;

namespace Sanatana.Patterns.Pipelines
{
    public interface IPipeline<TInput, TOutput>
    {
        Task<TOutput> Execute(TInput inputModel, TOutput outputModel);
        Task RollBack(PipelineContext<TInput, TOutput> context);
        void Register(Func<PipelineContext<TInput, TOutput>, Task<bool>> rollForwardAction, Func<PipelineContext<TInput, TOutput>, Task<bool>> rollBackAction = null);
        void Register(IPipelineModule<TInput, TOutput> module);
        void RegisterAfter(IPipelineModule<TInput, TOutput> existingModule, Func<PipelineContext<TInput, TOutput>, Task<bool>> rollForwardAction, Func<PipelineContext<TInput, TOutput>, Task<bool>> rollBackAction = null);
        void RegisterAfter(IPipelineModule<TInput, TOutput> existingModule, IPipelineModule<TInput, TOutput> module);
        void RegisterAt(int index, Func<PipelineContext<TInput, TOutput>, Task<bool>> rollForwardAction, Func<PipelineContext<TInput, TOutput>, Task<bool>> rollBackAction = null);
        void RegisterAt(int index, IPipelineModule<TInput, TOutput> module);
        void RegisterBefore(IPipelineModule<TInput, TOutput> existingModule, Func<PipelineContext<TInput, TOutput>, Task<bool>> rollForwardAction, Func<PipelineContext<TInput, TOutput>, Task<bool>> rollBackAction = null);
        void RegisterBefore(IPipelineModule<TInput, TOutput> existingModule, IPipelineModule<TInput, TOutput> module);
        void Remove(Func<PipelineContext<TInput, TOutput>, Task<bool>> rollForwardAction, Func<PipelineContext<TInput, TOutput>, Task<bool>> rollBackAction = null);
        void Remove(IPipelineModule<TInput, TOutput> module);
        void Replace(IPipelineModule<TInput, TOutput> existingModule, IPipelineModule<TInput, TOutput> newModule);
    }
}