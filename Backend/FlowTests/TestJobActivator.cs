using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowTests
{
    public class TestJobActivator : JobActivator
    {
        private readonly IServiceProvider _serviceProvider;

        public TestJobActivator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override object ActivateJob(Type jobType)
        {
            return _serviceProvider.GetRequiredService(jobType);
        }

        public override JobActivatorScope BeginScope(JobActivatorContext context)
        {
            return new HangfireScope(_serviceProvider.CreateScope());
        }

        private class HangfireScope : JobActivatorScope
        {
            private readonly IServiceScope _scope;

            public HangfireScope(IServiceScope scope)
            {
                _scope = scope;
            }

            public override object Resolve(Type type)
            {
                return _scope.ServiceProvider.GetRequiredService(type);
            }

            public override void DisposeScope()
            {
                _scope.Dispose();
            }
        }
    }
}