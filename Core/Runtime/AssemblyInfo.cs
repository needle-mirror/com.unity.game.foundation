using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyVersion("0.6.0")]

// Test assemblies
[assembly: InternalsVisibleTo("Unity.GameFoundation.EditorTests")]
[assembly: InternalsVisibleTo("Unity.GameFoundation.RuntimeTests")]

[assembly: InternalsVisibleTo("Unity.GameFoundation.Editor")]

[assembly: InternalsVisibleTo("Unity.GameFoundation.DefaultCatalog")]
[assembly: InternalsVisibleTo("Unity.GameFoundation.DefaultCatalog.Editor")]

[assembly: InternalsVisibleTo("Unity.GameFoundation.DefaultLayers")]
