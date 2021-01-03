# Interactions using Unity XR Interaction Toolkit

*Note : This read-me is written as a logbook and might be a bit heavy to load due to some GIFs.*

These past years I have tried a few different frameworks for VR interaction, **SteamVR** at it's start and then **VRTK 3.3** ([you can check my Attack On Titans tribute game using it here.](https://github.com/Hydeo/Attack-On-Titans-VR)).

Unity came up with its own take with the **[XR Interaction Toolkit](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@0.9/manual/index.html)** and this little repo is my first attempt using it.
VR development is still at its beginning but we can feel that we are reaching the point of inflection. Unity's XR Interaction Toolkit is definitely a step in the right direction, handling all the different hardware out there is a lot easier today than it was yesterday (and it was hell).
OpenXR should start being implemented in Unity's toolkit soon, I cannot wait to see how this will improve multiplatform développement.

That being said, my objective for this project was to build "basic" interactions with Unity's new toolkit to see how easy it would be. I have to say that I was quite pleased. 
Basic VR rig and mouvement were a breeze to implement (I went for continuous mouvement in this project) and the XR Toolkit comes with some great exemples.

Then I started creating some interactable props :

Very simple levers : 

![](Gif/LeverInteraction.gif)

A "realistic" gun interaction let's say :

![](Gif/GunInteraction-compressed.gif)

And because Half-Life : Alyx changed our perception of qualitative VR Interactions, I wanted to add a kind of Gravity Glove.

![](Gif/GravityGlove-compressed.gif)

[This repo](https://github.com/Frank01001/Unity---HL-A-Gravity-Gloves-Basic-Implementation) made by @[Frank01001](https://github.com/Frank01001) was really helpful, I mostly used the the [parabola calculation helper](https://github.com/Frank01001/Unity---HL-A-Gravity-Gloves-Basic-Implementation/blob/master/Game%20Assets/Scripts/ComplementarCalculations.cs). 
Ported all of that to the Unity's XR Toolkit and added some targeting feature to make it more usable. 
Lot of fun working on that feature.

As usual I would love to polish all of that for hours and hours and add plenty of features but that would get out of scope.

## Conclusion
I'm very happy with this journey into Unity development I started a few years back (~2017/2018), I getting more confortable with it and with each experiment I manage to dive on some of its toolkit (URP and ShaderGraph are the main ones for this project) while doing whatever VR development I fancy at this particular moment.
