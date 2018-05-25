using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Patterns.Pipelines
{
    public class Pipeline<TInput, TOutput> : IPipeline<TInput, TOutput>
    {
        //properties
        public List<IPipelineModule<TInput, TOutput>> Modules { get; set; }



        //init
        public Pipeline()
        {
            Modules = new List<IPipelineModule<TInput, TOutput>>();            
        }



        //register
        public virtual void Register(Func<PipelineContext<TInput, TOutput>, Task<bool>> rollForwardAction
            , Func<PipelineContext<TInput, TOutput>, Task<bool>> rollBackAction = null)
        {
            var newModule = new DelegateModule<TInput, TOutput>(rollForwardAction, rollBackAction);
            Register(newModule);
        }

        public virtual void Register(IPipelineModule<TInput, TOutput> module)
        {
            Modules.Add(module);
        }

        public virtual void RegisterAt(int index
            , Func<PipelineContext<TInput, TOutput>, Task<bool>> rollForwardAction
            , Func<PipelineContext<TInput, TOutput>, Task<bool>> rollBackAction = null)
        {
            var newModule = new DelegateModule<TInput, TOutput>(rollForwardAction, rollBackAction);
            RegisterAt(index, newModule);
        }
        
        public virtual void RegisterAt(int index, IPipelineModule<TInput, TOutput> module)
        {
            Modules.Insert(index, module);
        }

        public virtual void RegisterBefore(IPipelineModule<TInput, TOutput> existingModule
            , Func<PipelineContext<TInput, TOutput>, Task<bool>> rollForwardAction
            , Func<PipelineContext<TInput, TOutput>, Task<bool>> rollBackAction = null)
        {
            var newModule = new DelegateModule<TInput, TOutput>(rollForwardAction, rollBackAction);
            RegisterBefore(existingModule, newModule);
        }

        public virtual void RegisterBefore(IPipelineModule<TInput, TOutput> existingModule
            , IPipelineModule<TInput, TOutput> module)
        {
            int existingIndex = Modules.IndexOf(existingModule);
            if (existingIndex == -1)
            {
                throw new KeyNotFoundException("Provided existing module was not found. Can not register before not registered module.");
            }

            RegisterAt(existingIndex, module);
        }

        public virtual void RegisterAfter(IPipelineModule<TInput, TOutput> existingModule
            , Func<PipelineContext<TInput, TOutput>, Task<bool>> rollForwardAction
            , Func<PipelineContext<TInput, TOutput>, Task<bool>> rollBackAction = null)
        {
            var newModule = new DelegateModule<TInput, TOutput>(rollForwardAction, rollBackAction);
            RegisterAfter(existingModule, newModule);
        }

        public virtual void RegisterAfter(IPipelineModule<TInput, TOutput> existingModule
            , IPipelineModule<TInput, TOutput> module)
        {
            int existingIndex = Modules.IndexOf(existingModule);
            if (existingIndex == -1)
            {
                throw new KeyNotFoundException("Provided existing module was not found. Can not register after not registered module.");
            }

            RegisterAt(existingIndex + 1, module);
        }

        public virtual void Remove(Func<PipelineContext<TInput, TOutput>, Task<bool>> rollForwardAction
            , Func<PipelineContext<TInput, TOutput>, Task<bool>> rollBackAction = null)
        {
            var existingModule = new DelegateModule<TInput, TOutput>(rollForwardAction, rollBackAction);
            Remove(existingModule);
        }

        public virtual void Remove(IPipelineModule<TInput, TOutput> module)
        {
            int index = Modules.IndexOf(module);
            if (index == -1)
            {
                throw new KeyNotFoundException("Provided existing module was not found. Can not remove not registered module.");
            }

            Modules.RemoveAt(index);
        }
        
        public virtual void Replace(IPipelineModule<TInput, TOutput> existingModule
            , IPipelineModule<TInput, TOutput> newModule)
        {
            int index = Modules.IndexOf(existingModule);
            if (index == -1)
            {
                throw new KeyNotFoundException("Provided existing module was not found. Can not replace not registered module.");
            }

            Modules.RemoveAt(index);
            Modules.Insert(index, newModule);
        }



        //execute
        public virtual async Task<TOutput> Execute(TInput inputModel, TOutput outputModel)
        {
            var context = new PipelineContext<TInput, TOutput>()
            {
                Completed = true,
                Input = inputModel,
                RollBackFromStepIndex = -1,
                Exceptions = null,
                Output = outputModel
            };
            
            try
            {
                foreach (IPipelineModule<TInput, TOutput> module in Modules)
                {
                    context.Completed = await module.RollForward(context).ConfigureAwait(false);
                    if (context.Completed == false)
                    {
                        break;
                    }
                    context.RollBackFromStepIndex++;
                }
            }
            catch (Exception ex)
            {
                context.Completed = false;

                if (context.Exceptions == null)
                {
                    context.Exceptions = new List<Exception>();
                }
                context.Exceptions.Add(ex);
            }

            if(context.Completed == false)
            {
                //start rollback from last successfuly completed step.
                await RollBack(context).ConfigureAwait(false);
            }
            
            return context.Output;
        }

        public virtual async Task RollBack(PipelineContext<TInput, TOutput> context)
        {
            for (int i = context.RollBackFromStepIndex; i >= 0; i--)
            {
                try
                {
                    await Modules[i].RollBack(context).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    if (context.Exceptions == null)
                    {
                        context.Exceptions = new List<Exception>();
                    }
                    context.Exceptions.Add(ex);
                }
            }
        }


    }
}
