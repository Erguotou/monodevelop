﻿//
// DotNetCoreSdkVersionTests.cs
//
// Author:
//       Matt Ward <matt.ward@xamarin.com>
//
// Copyright (c) 2017 Xamarin Inc. (http://xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using NUnit.Framework;

namespace MonoDevelop.DotNetCore.Tests
{
	[TestFixture]
	class DotNetCoreSdkVersionTests
	{
		[TestCase ("1.0.0-preview5-004460", 4460)]
		[TestCase ("1.0.0-preview2-003156", 3156)]
		[TestCase ("1.0.0-preview2-1-003177", 3177)]
		[TestCase ("1.0.0-rc3-004530", 4530)]
		[TestCase ("1.0.0-rc4-4771", 4771)]
		public void ValidBuildVersions (string sdkVersion, int expectedBuildVersion)
		{
			int buildVersion = -1;
			bool result = DotNetCoreSdkVersion.TryGetBuildVersion (sdkVersion, out buildVersion);

			Assert.AreEqual (expectedBuildVersion, buildVersion);
			Assert.IsTrue (result);
		}

		[TestCase ("")]
		[TestCase (null)]
		[TestCase ("1")]
		public void InvalidBuildVersions (string sdkVersion)
		{
			int buildVersion = -1;
			bool result = DotNetCoreSdkVersion.TryGetBuildVersion (sdkVersion, out buildVersion);

			Assert.IsFalse (result);
		}
	}
}