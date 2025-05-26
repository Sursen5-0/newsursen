using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Common
{
    public static class LogVerifier
    {
        public static void VerifyLog<T>(this Mock<ILogger<T>> logger, LogLevel level, Times times)
        {
            logger.Verify(x =>
                x.Log(level,
                      It.IsAny<EventId>(),
                      It.Is<It.IsAnyType>((_, __) => true),
                      It.IsAny<Exception>(),
                      It.Is<Func<It.IsAnyType, Exception, string>>((_, __) => true)),
                times);
        }
    }
}
