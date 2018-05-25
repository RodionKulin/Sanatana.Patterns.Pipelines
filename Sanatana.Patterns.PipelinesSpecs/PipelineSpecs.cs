using NUnit.Framework;
using Sanatana.Patterns.Pipelines;
using SpecsFor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Should;

namespace Sanatana.Patterns.PipelinesSpecs
{
    public class PipelineSpecs
    {
        [TestFixture]
        public class when_executing_pipeline : SpecsFor<Pipeline<string>>
        {
            bool _actionCalled = false;
            string _input = "input";

            [Test]
            public void then_executes_pipeline()
            {
                SUT.Register(Action1);
                PipelineResult result = SUT.Execute(_input, PipelineResult.Success()).Result;

                result.Completed.ShouldBeTrue();
                _actionCalled.ShouldBeTrue();
            }

            public Task<bool> Action1(PipelineContext<string, PipelineResult> context)
            {
                _actionCalled = true;

                context.Input.ShouldEqual(_input);

                return Task.FromResult(true);
            }
        }
        
        [TestFixture]
        public class when_throwing_exception_in_pipeline : SpecsFor<Pipeline<string>>
        {
            bool _rollbackCalled = false;
            string _input = "input";

            [Test]
            public void then_rolls_back_previous_pipeline_steps()
            {
                Pipeline<string> target = new Pipeline<string>();
                SUT.Register(Action1, Action1RollBack);
                SUT.Register(Action2);
                PipelineResult result = SUT.Execute(_input, null).Result;

                result.Completed.ShouldBeFalse();
                result.Messages.ShouldContain("Error");
                _rollbackCalled.ShouldBeTrue();
            }

            public Task<bool> Action1(PipelineContext<string, PipelineResult> context)
            {
                context.Input.ShouldEqual(_input);

                return Task.FromResult(true);
            }
            public Task<bool> Action1RollBack(PipelineContext<string, PipelineResult> context)
            {
                _rollbackCalled = true;

                context.Input.ShouldEqual(_input);

                context.Output = PipelineResult.Error("Error");

                return Task.FromResult(true);
            }
            public Task<bool> Action2(PipelineContext<string, PipelineResult> context)
            {
                throw new Exception();
            }
        }
        
        [TestFixture]
        public class when_registering_steps : SpecsFor<Pipeline<string>>
        {
            [Test]
            public void then_registers_new_module_after_existing()
            {
                SUT.Register(Action1);
                SUT.RegisterAfter(
                  (DelegateModule<string, PipelineResult>)Action1
                  , Action2, null);
                SUT.Modules.Count.ShouldEqual(2);
            }

            public Task<bool> Action1(PipelineContext<string, PipelineResult> context)
            {
                return Task.FromResult(true);
            }
            public Task<bool> Action2(PipelineContext<string, PipelineResult> context)
            {
                return Task.FromResult(true);
            }
        }

        [TestFixture]
        public class when_replacing_steps : SpecsFor<Pipeline<string>>
        {
            [Test]
            public void then_replaces_module()
            {
                SUT.Register(Action1);
                SUT.Register(Action2);

                SUT.Replace((DelegateModule<string, PipelineResult>)Action1
                    , (DelegateModule<string, PipelineResult>)Action2);
                SUT.Modules.Count.ShouldEqual(2);
            }
            
            public Task<bool> Action1(PipelineContext<string, PipelineResult> context)
            {
                return Task.FromResult(true);
            }
            public Task<bool> Action2(PipelineContext<string, PipelineResult> context)
            {
                return Task.FromResult(true);
            }
        }

        [TestFixture]
        public class when_removing_steps : SpecsFor<Pipeline<string>>
        {
            [Test]
            public void then_removes_module()
            {
                SUT.Register(Action1);
                SUT.Register(Action2);

                SUT.Remove(Action2);
                SUT.Modules.Count.ShouldEqual(1);
            }

            public Task<bool> Action1(PipelineContext<string, PipelineResult> context)
            {
                return Task.FromResult(true);
            }
            public Task<bool> Action2(PipelineContext<string, PipelineResult> context)
            {
                return Task.FromResult(true);
            }
        }

    }
}
