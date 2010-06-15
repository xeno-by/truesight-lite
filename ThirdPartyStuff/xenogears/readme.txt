This project requires XenoGears source code rather than binaries.
Expected source location: ..\XenoGears (relative to the root of this checkout)
URL to checkout code from: http://xenogears.googlecode.com/svn/trunk/

That's done on purpose to integrate gears' codebase into the project and make it easy to introduce fixes and changes shared between all clients of gears.
Due to the same reason I don't version XenoGears in AssemblyInfo - it ain't exist as an independent binary, rather it is integrated into every client's codebase.