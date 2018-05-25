using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Sanatana.Patterns.Pipelines
{    
    public class DelegateModule<TInput, TOutput> : IPipelineModule<TInput, TOutput>
    {
        //properties
        public Func<PipelineContext<TInput, TOutput>, Task<bool>> RollForwardAction { get; set; }
        public Func<PipelineContext<TInput, TOutput>, Task<bool>> RollBackAction { get; set; }



        //init
        protected DelegateModule()
        {
        }

        public DelegateModule(Func<PipelineContext<TInput, TOutput>, Task<bool>> rollForwardAction)
        {
            RollForwardAction = rollForwardAction;
        }

        public DelegateModule(Func<PipelineContext<TInput, TOutput>, Task<bool>> rollForwardAction,
            Func<PipelineContext<TInput, TOutput>, Task<bool>> rollBackAction)
        {
            RollForwardAction = rollForwardAction;
            RollBackAction = rollBackAction;
        }


        //methods
        public virtual Task<bool> RollForward(PipelineContext<TInput, TOutput> context)
        {
            return RollForwardAction(context);
        }

        public virtual Task<bool> RollBack(PipelineContext<TInput, TOutput> context)
        {
            if(RollBackAction == null)
            {
                return Task.FromResult(true);
            }

            return RollBackAction(context);
        }

        public override bool Equals(object obj)
        {
            if(obj == null || (obj is DelegateModule<TInput, TOutput>) == false)
            {
                return false;
            }

            DelegateModule<TInput, TOutput> another = (DelegateModule<TInput, TOutput>)obj;

            bool forwardEquals = RollForwardAction.GetMethodInfo().Name == another.RollForwardAction.GetMethodInfo().Name
                && RollForwardAction.Target == another.RollForwardAction.Target;
            if(forwardEquals == false)
            {
                return false;
            }

            if (RollBackAction == null && another.RollBackAction == null)
            {
                return true;
            }
            else if (RollBackAction == null && another.RollBackAction != null)
            {
                return false;
            }
            else if (RollBackAction != null && another.RollBackAction == null)
            {
                return false;
            }
            else
            {
                bool equals = RollBackAction.GetMethodInfo().Name == another.RollBackAction.GetMethodInfo().Name
                    && RollBackAction.Target == another.RollBackAction.Target;
                return equals;
            }
        }

        public override int GetHashCode()
        {
            int result = RollForwardAction.GetMethodInfo().GetHashCode()
                ^ RollForwardAction.GetType().GetHashCode();
            if (RollForwardAction.Target != null)
                result ^= RuntimeHelpers.GetHashCode(RollForwardAction);

            if (RollBackAction != null)
            {
                result ^= RollBackAction.GetMethodInfo().GetHashCode()
                    ^ RollBackAction.GetType().GetHashCode();
                if (RollBackAction.Target != null)
                    result ^= RuntimeHelpers.GetHashCode(RollBackAction);
            }

            return result;
        }

        public static implicit operator DelegateModule<TInput, TOutput>(
            Func<PipelineContext<TInput, TOutput>, Task<bool>> rollForwardAction)
        {
            return new DelegateModule<TInput, TOutput>()
            {
                RollForwardAction = rollForwardAction
            };
        }
    }
}
