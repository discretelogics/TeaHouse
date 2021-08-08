/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Collections;
using System.Text;
using System.Reflection;
using Microsoft.VsSDK.UnitTestLibrary;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VSSDK.Tools.VsIdeTesting;
using SharpTestsEx;

namespace TeaTime.Yahoo.Finance.Downloader_UnitTests.MyToolWindowTest
{
    /// <summary>
    ///This is a test class for MyToolWindowTest and is intended
    ///to contain all MyToolWindowTest Unit Tests
    ///</summary>
    [TestClass]
    public class MyToolWindowTest
    {
        /// <summary>
        ///MyToolWindow Constructor test
        ///</summary>
        [TestMethod]
        [Ignore] // fails sometimes with XamlParse - ComException
        public void ToolWindowConstructorTest()
        {
            YahooBrowserPane target = new YahooBrowserPane();
            Assert.IsNotNull(target, "Failed to create an instance of MyToolWindow");            
            target.Content.Should().Not.Be.Null();            
        }

        /// <summary>
        ///Verify the Content property is valid.
        ///</summary>
        [TestMethod]
        [Ignore] // fails sometimes with XamlParse - ComException
        public void WindowPropertyTest()
        {
            YahooBrowserPane target = new YahooBrowserPane();
            Assert.IsNotNull(target.Content, "Content property was null");
        }

    }
}
