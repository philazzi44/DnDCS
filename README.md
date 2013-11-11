DnDCS
=====

DnD Client-Server

This project aims to solve the shortcomings that some friends and I are experiencing with a previously acquired DnD tool.
The DM can 'reveal' areas of a map that the Players, connected as a Client to the DM's Server instance, can see. The Server
instance has full visibility to the entire map and controls which areas are visible, the size of the on-screen grid (if any), etc.
The Client instance has visibility only what is allowed, and can zoom in/out for the appropriate experience.

Version History

1.0 - 09/22/2013

Initial Version

1.3 - 10/25/2013

Change Log

Client
Click/drag support to scroll the map 
  Mouse wheel and arrow keys still work as well
“Flip View” menu option to rotate the view 270 degrees
Flips X and Y axis, so we can avoid any problems with flipping the resolution. Will look into just Y-axis mirroring at some point
Zoom text centered, and now needs Enter or Left Mouse Click to “commit” the zoom amount. Escape/Right Click cancels, so we don’t accidentally zoom
Zooming should work with CTRL + (Wheel/Up/Down/Plus/Minus)
Zoom no longer needs to generate any duplicate Bitmaps in memory, so overall memory footprint reduced
May work in 32-bit environment as it no longer generates a large bitmap when zooming 

Server
Click/drag support to scroll the map (if the Fog Reveal tool is selected then right click does the scroll)
  Mouse wheel and arrow keys still work as well
Full screen support via F11/Escape or a menu item
Double left click will force the client to center on that spot
  This might mean we can change over to a world where the Server always tells the Clients what they see, so we never scroll “too far” while exploring
Minimap which can be click/dragged, and right-click to alter its color in case the loaded map’s colors make it hard to see
Load Image button on the main view so you don’t have to click on File -> Load anymore
Added support for saving the Fog Reveals that were done on the Server for later re-loading them when the same image is opened
  Trial run for this. May change how it’s done in a way that’s not backwards compatible. Sorry.
Beta functionality for a neat Fog Effect that gradients the edges’ Alpha value. Kudos to Sparks for implementing this
It’s beta because it currently doesn’t work as expected with odd-shaped, or very small, polygons, and doesn’t feel right when things overlap a bit. We can try it, but there’s a menu option to turn it off.

v1.4 - 11/10/2013

- Compiled to “Any CPU” again, from a change to only run on x64 when it used to crash x86.
- Enabled Load Image button in Server on startup
- Client scroll "threshold" dropped to 0, so it shouldn’t “stutter” anymore when trying to scroll a tiny bit
- Server "Center Map" now supports double Right Click as well as existing double Left Click – double right click means we won’t have any mini-pixel reveals
- Client now supports manual "Reconnect" option, to recover from random socket closures – never figured out why the socket disconnects… Client is watching for the disconnect and should give a popup when it happens, so this is just a stopgap
- Client's F11 now properly un-fullscreens
- Changed Saved FogData from X/Y Point data to PNG – saves memory/time in real-world usage. Oops, not backwards compatible!
- Fancy menu - I thought XNA might give us some performance boosts a while ago so started implementing it there, including a cool menu, but then performance in the Win Forms version got good enough so all we have is the cool menu. Requires XNA prereq installed (in Prereq folder)
- Regular win.exe can be run as-is, if you want to ignore the menu.
- Performance tweaks during re-draws geared towards large images
- Broke "Flipped View" option on client, forgot to fix it, sorry
- Web Client beta

v1.5 - ?? (roadmap)

- Flipped View fixes in Win Forms Client
- Flipped View added to Web Client
- Web Client Released