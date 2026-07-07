using System.Reflection;
using System.Runtime.InteropServices;

// Setting ComVisible to false makes the types in this assembly not visible to COM
// components.  If you need to access a type in this assembly from COM, set the ComVisible
// attribute to true on that type.

[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM.
#if TEST_RELEASE || TEST_DEBUG
[assembly: Guid("d138d36b-61f7-4ed7-ad70-48a8c780abe3")]
#else
[assembly: Guid("bb562cff-e70e-4e8c-b4bd-7791b969878f")] 
#endif

[assembly: AssemblyVersion("3.1.38.*")]    