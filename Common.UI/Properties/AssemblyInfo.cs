using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;

[assembly: AssemblyTitle("TeaTime.Common.UI")]
[assembly: AssemblyDescription("Common user interface parts of the TeaTime product family")]

// equal for all shipped assemblies
[assembly: AssemblyCompany("DiscreteLogics")]
[assembly: AssemblyCopyright("Copyright © DiscreteLogics 2011")]
[assembly: AssemblyTrademark("TeaTime")]

// product
[assembly: AssemblyProduct("TeaTime")]

// todo: fill by release management tools
[assembly: AssemblyConfiguration("Development Build")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

// friends
[assembly: InternalsVisibleTo("DiscreteLogics.TeaTime.Common.UI.Tests")]
[assembly: InternalsVisibleTo("DiscreteLogics.TeaTime.TeaHouse.Tests")]

[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]

