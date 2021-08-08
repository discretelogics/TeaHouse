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

namespace TeaTime.Yahoo.Finance.Downloader_UnitTests.MyToolWindowTest
{
    [TestClass()]
    public class ShowToolWindowTest
    {

        [TestMethod()]
        [Ignore] // setting default options in initialized causes troubles with this test
        public void ValidateToolWindowShown()
        {
            var package = new YahooPackage();
            IVsPackage ivspackage = package;

            // Create a basic service provider
            OleServiceProvider serviceProvider = OleServiceProvider.CreateOleServiceProviderWithBasicServices();

            //Add uishell service that knows how to create a toolwindow
            BaseMock uiShellService = UIShellServiceMock.GetUiShellInstanceCreateToolWin();
            serviceProvider.AddService(typeof(SVsUIShell), uiShellService, false);

            // Site the package
            Assert.AreEqual(0, ivspackage.SetSite(serviceProvider), "SetSite did not return S_OK");

            package.ShowYahooToolWindow();
        }

        [TestMethod()]
        [Ignore] // setting default options in initialized causes troubles with this test
        [ExpectedException(typeof(InvalidOperationException), "Did not throw expected exception when windowframe object was null")]
        public void ShowToolwindowNegativeTest()
        {
            var package = new YahooPackage();
            IVsPackage ivspackage = package;

            // Create a basic service provider
            OleServiceProvider serviceProvider = OleServiceProvider.CreateOleServiceProviderWithBasicServices();

            //Add uishell service that knows how to create a toolwindow
            BaseMock uiShellService = UIShellServiceMock.GetUiShellInstanceCreateToolWinReturnsNull();
            serviceProvider.AddService(typeof(SVsUIShell), uiShellService, false);

            // Site the package
            Assert.AreEqual(0, ivspackage.SetSite(serviceProvider), "SetSite did not return S_OK");

            package.ShowYahooToolWindow();                    
        }
    }
}
