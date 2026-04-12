NavMeshComponents.Extensions, provides you with ability to create navigation meshes that are generated automatically from your Scene geometry, which allows characters to move intelligently around the game world.
----------------------------------------------------------------------------------

<img width="503" height="278" alt="simple navmesh" src="https://github.com/user-attachments/assets/2bd62165-94f7-46c7-861b-cbf426248030" />
---------------------------------------------------------------------------------------------------

 To use it in your project:
 1.Copy files into your Asset folder (or install as a package).
 2.Create Empty Object in scene root.
 3.Add "Navigation Surface" component to Empty Object and add NavMeshCollecSources2d component after.
 4.Click Rotate Surface to XY (to face surface toward standard 2d camera x-90;y0;z0)
 5.Add "Navigation Modifier" component to scene objects obstacles, override the area.
 6.In "Navigation Surface" hit Bake.
----------------------------------------------------------------------------------------------------
Package Manager: AI Navigation – NavMesh Agent
Unity 2D Pathfinding:
This repo is fork of Unity NavMeshComponents enhanced with Extensions system for 2d Pathfinding and more. 
2D NavMesh
In repo you will find implementation of NavMeshSurface and 2d Extensions for tilemap, sprites and collider2d top down games.
provides you with ability to create navigation meshes that are generated automatically from your Scene geometry, which allows characters to move intelligently around the game world.
How does it works:
-----------------------------------------------------------------------------------------------------
It uses NavMeshSurface as base implementation.
Implements world bound calculation.
Implements source collector of tiles, sprites and 2d colliders
Creates walkable mesh box from world bounds.
Convert tiles, sprites and 2d colliders to sources as NavMeshBuilder would do.
Components & Extensions:
 NavMeshLink
 NavMeshModifier
 NavMeshModifierVolume
 NavMeshSurface:
 NavMeshCollectSources2d
 NavMeshCollectRootSources2d
 NavMeshCacheSources2d
 Utilities
 NavMeshExtensionsProvider.cs
 NavMeshBuilder2d.cs
 NavMeshExtension.cs

 
