using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using TestHelper;

namespace BufferedLoggerAnalyzer.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {

        [TestMethod]
        public void TestBufferedAttributeWillBeSuggestedForStaticFunctionWithFunctionNameAttribute()
        {
            var test = @"
    using Microsoft.Extensions.Logging;
    using Microsoft.Azure.WebJobs;

    namespace WebApplication
    {
        public static class Handler
        {
            [FunctionName(""Entrypoint"")]
            public static async Task Entrypoint([HttpTrigger] HttpRequest request, ILogger logger)
            {
                logger.LogInformation(""Received request"");
            }
        }
    }";
            var lines = test.Split(Environment.NewLine);
            var diagnosticLine = lines.Single(line => line.Contains("ILogger"));
            var column = diagnosticLine.IndexOf("logger") + 1;
            var row = Array.IndexOf(lines, diagnosticLine) + 1;
            var expected = new DiagnosticResult
            {
                Id = "BufferedLoggerAnalyzer",
                Message = "ILogger is used without [Buffered] attribute",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", row, column) }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
    using Microsoft.Extensions.Logging;
    using Microsoft.Azure.WebJobs;

    namespace WebApplication
    {
        public static class Handler
        {
            [FunctionName(""Entrypoint"")]
            public static async Task Entrypoint([HttpTrigger] HttpRequest request, [Buffered] ILogger logger)
            {
                logger.LogInformation(""Received request"");
            }
        }
    }";
            VerifyCSharpFix(test, fixtest);
        }

        [TestMethod]
        public void TestBufferedAttributeWillNotBeSuggestedForNonStaticFunctionWithFunctionNameAttribute()
        {
            var test = @"
    using Microsoft.Extensions.Logging;
    using Microsoft.Azure.WebJobs;

    namespace WebApplication
    {
        public static class Handler
        {
            [FunctionName(""Entrypoint"")]
            public async Task Entrypoint([HttpTrigger] HttpRequest request, ILogger logger)
            {
                logger.LogInformation(""Received request"");
            }
        }
    }";
            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void TestBufferedAttributeWillNotBeSuggestedForStaticFunctionWithoutFunctionNameAttribute()
        {
            var test = @"
    using Microsoft.Extensions.Logging;
    using Microsoft.Azure.WebJobs;

    namespace WebApplication
    {
        public static class Handler
        {
            public static async Task Entrypoint([HttpTrigger] HttpRequest request, ILogger logger)
            {
                logger.LogInformation(""Received request"");
            }
        }
    }";
            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void TestBufferedAttributeWillNotBeSuggestedForConstructor()
        {
            var test = @"
    using Microsoft.Extensions.Logging;
    using Microsoft.Azure.WebJobs;

    namespace WebApplication
    {
        public class Handler
        {
            public Handler([HttpTrigger] HttpRequest request, ILogger logger)
            {
                logger.LogInformation(""Received request"");
            }
        }
    }";
            VerifyCSharpDiagnostic(test);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new BufferedLoggerAnalyzerCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new BufferedLoggerAnalyzer();
        }
    }
}
