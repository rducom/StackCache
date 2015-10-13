// <copyright file="LocalLockTest.cs" company="Raphel DUCOM">Copyright Raphel DUCOM ©  2015</copyright>
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using StackCache.Core.Locking;

namespace StackCache.Core.Locking.Tests
{
    /// <summary>This class contains parameterized unit tests for LocalLock</summary>
    [PexClass(typeof(LocalLock))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    public partial class LocalLockTest
    {
        /// <summary>Test stub for Lock(String, TimeSpan, CancellationToken)</summary>
        [PexMethod]
        public Task<IMutexState> LockTest(
            [PexAssumeUnderTest]LocalLock target,
            string key,
            TimeSpan timeout,
            CancellationToken cancellationToken
        )
        {
            Task<IMutexState> result = target.Lock(key, timeout, cancellationToken);
            return result;
            // TODO: add assertions to method LocalLockTest.LockTest(LocalLock, String, TimeSpan, CancellationToken)
        }
    }
}
