﻿using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.DependencyInjection
{
    public class DependencyInjectionTestFrameworkExecutor : XunitTestFrameworkExecutor
    {
        private readonly Exception? _exception;
        private readonly IHost? _host;

        public DependencyInjectionTestFrameworkExecutor(IHost? host,
            Exception? exception,
            AssemblyName assemblyName,
            ISourceInformationProvider sourceInformationProvider,
            IMessageSink diagnosticMessageSink)
            : base(assemblyName, sourceInformationProvider, diagnosticMessageSink)
        {
            _host = host;
            _exception = exception;

            if (_host != null) DisposalTracker.Add(_host);
        }

        /// <inheritdoc />
        protected override async void RunTestCases(
            IEnumerable<IXunitTestCase> testCases,
            IMessageSink executionMessageSink,
            ITestFrameworkExecutionOptions executionOptions)
        {
            if (_host == null)
            {
                using var runner = new DependencyInjectionTestAssemblyRunner(null, _exception, TestAssembly,
                    testCases, DiagnosticMessageSink, executionMessageSink, executionOptions);

                await runner.RunAsync();
            }
            else
            {
                await _host.StartAsync();

                using var runner = new DependencyInjectionTestAssemblyRunner(_host.Services, _exception, TestAssembly,
                    testCases, DiagnosticMessageSink, executionMessageSink, executionOptions);

                await runner.RunAsync();

                await _host.StopAsync();
            }
        }
    }
}
