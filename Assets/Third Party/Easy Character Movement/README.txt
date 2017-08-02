EASY CHARACTER MOVEMENT
=======================

CHANGE LOG

Version 1.3

	- Improved jump physics. In the new method, we apply a proportional extra jump power (acceleration) to perform variable height jump, this offers a better jump control and removes any floaty jetpack feel.

 	- Added a new maxRiseSpeed property to CharacterMovement component. This helps to limit the maximum rising velocity along y+ axis.

 	- Added tooltips to main components, this helps to tweak its values without the need to look in documents / code.

 	- ECM now belongs to the Scripting/Physics asset store category. This removes the "one license per seat" restriction imposed to editor extensions.


Version 1.2

	- Added a jump tolerance time property to base character controller.
	  This helps to manage how early before hitting the ground you can press jump, and still perform the jump.

	- Added a new Raycast ground detection component.

	- Added a CustomCharacterController example.
	  This shows how to create a custom character controller to perform the movement relative to camera.

	- Fixed a minor bug where some of the controllers were not applying braking drag.


Version 1.1

	- Fixed a minor bug related to demo scene lightmap size.

	- Added a simple demos scene, to faster start.

	- Exposed all ground info from movement component.
	
	- Improved character braking, braking is composed of friction (velocity-dependent drag) and constant deceleration.


VERSION 1.0

	- Initial release.



DISCLAIMER & LEGAL INFORMATION

THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
PARTICULAR PURPOSE.

YOU MAY NOT REDISTRIBUTE THIS SOURCE CODE IN WHOLE OR IN PART
WITHOUT WRITTEN CONSENT FROM THE CONTENT AUTHOR OR COPYRIGHT HOLDER.