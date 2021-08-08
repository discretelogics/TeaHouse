using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("TeaTime.Common")]
[assembly: AssemblyDescription("Common parts of the TeaTime product family")]

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

[assembly: InternalsVisibleTo("DiscreteLogics.TeaTime.Common.Tests")]
