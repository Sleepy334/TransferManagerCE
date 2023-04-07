using System.Reflection;
using System.Runtime.InteropServices;

// In SDK-style projects such as this one, several assembly attributes that were historically
// defined in this file are now automatically added during build and populated with
// values defined in project properties. For details of which attributes are included
// and how to customise this process see: https://aka.ms/assembly-info-properties


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

[assembly: AssemblyVersion("2.3.15.*")]  