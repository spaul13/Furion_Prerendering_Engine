# Furion_Prerendering_Engine

First, download the Viking village scene from the Unity asset store (https://assetstore.unity.com/packages/essentials/tutorial-projects/viking-village-29140)

Then download 360 Panorama Capture Asset (https://assetstore.unity.com/packages/tools/camera/360-panorama-capture-38755) from unity Asset store.

360 Panorama Capture might be deprecated. Hence find the asset from https://github.com/mjycom/Furion_Prerender_Engine.git


After importing both these assets, one can easily find these from inside Assets Under Project tab 

Drag and Drop the viking village demo scene into the Hierarchy

Create an empty GameObject named "Capture Panorama" and add the CapturePanorama script (can be found inside 360 Panorama capture) to it 

Create another empty Gameobject name it as "ProbeCamera".

Add Rigidbody component to it and uncheck Use Gravity


