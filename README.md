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

1.4 - 11/10/2013

- Enabled Load Image button in Server on startup
- Client scroll "threshold" dropped to 0
- Server "Center Map" now supports double Right Click as well as existing double Left Click
- Client now supports manual "Reconnect" option, to recover from random socket closures
- Client's F11 now properly un-fullscreens
- Changed Saved FogData from Point[] to PNG. Oops, not backwards compatible!
- Added a fuckin awesome menu that doesn't play nice with Windows Forms lols. Requires XNA prereq installed. 100% for fun.
- Performance tweaks geared towards large images
- Broke "Flipped View" option on client, sorry
- Web Client beta

v1.5 - ??

- Flipped View fixes in Win Forms Client
- Flipped View added to Web Client
- Web Client Released