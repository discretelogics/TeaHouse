using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("TeaHouse")]
[assembly: AssemblyDescription("TimeSeries Management Tool")]

// equal for all shipped assemblies
[assembly: AssemblyCompany("DiscreteLogics")]
[assembly: AssemblyCopyright("Copyright © DiscreteLogics 2011")]
[assembly: AssemblyTrademark("TeaTime")]

// product
[assembly: AssemblyProduct("TeaBlend")]

// todo: fill by release management tools
[assembly: AssemblyConfiguration("Development Build")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

// friends
[assembly: InternalsVisibleTo("DiscreteLogics.TeaTime.TeaHouse.Tests")]
[assembly: InternalsVisibleTo("DiscreteLogics.TeaHouse.TestApplication")]
                            