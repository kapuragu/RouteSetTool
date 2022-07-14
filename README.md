# RouteSetTool
A tool that quickly decompiles and compiles Fox Engine .frt Fox Route files. Heavily based on youarebritish's work such as FoxKit's Route Builder and FoxLib.

https://github.com/youarebritish/FoxKit

https://github.com/youarebritish/FoxLib

## Usage
Drag a .frt file over the .exe and a decompiled .frt.xml will be generated. A decompiled .frt.xml dragged over the .exe will generate a compiled .frt.

## Batch Options
"RouteSetTool.exe ....frt" will unpack the .frt and create a decompiled .frt.xml.

"RouteSetTool.exe ....frt.xml" will pack the decompiled .frt.xml into a .frt, using version 3, the one used in Metal Gear Solid V: The Phantom Pain and Metal Gear Survive.

"RouteSetTool.exe -version 2 ..." will convert all following output after the number to the specified .frt version. Only supported versions are version 2 (Metal Gear Solid V: Ground Zeroes) and version 3 (Metal Gear Solid V: The Phantom Pain and Metal Gear Survive).

"RouteSetTool.exe -convertevent ..." will convert the Event Types of routes in all following output after the command from Ground Zeroes-only Event Types to Event Types that can be read by The Phantom Pain.

"RouteSetTool.exe -whitelist list.txt ..." will only include routes and routes mentioned by specific routes whitelisted in the .txt file, including known string names and hashes, in all following output after the .txt option.

"RouteSetTool.exe -combine result.frt ..." will combine all following output after the new file name into a new file under that name.
