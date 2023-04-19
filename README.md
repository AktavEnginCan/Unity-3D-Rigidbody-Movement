# 1. Description 
This repository is about my "Movement" Script for Unity! This implements a **1. & 3. Person-Movement** in your game.

## 1.1. Features
- Transform player to direction
  - run
  - jump
- Rotate player & camera
  - Zoom camera (3. Person)
  - Prevent camera culling (3. Person)
  
## 1.2. Used Components
- Rigidbody
- Capsule Collider

## 1.3. Used Assets
- [Gridbox Prototype Materials](https://assetstore.unity.com/packages/2d/textures-materials/gridbox-prototype-materials-129127)

# 2. Installation
## 2.1. Sample Scene
To test the sample scene in Unity, import the [project files](https://github.com/Engin1999/Unity-3D-Rigidbody-Movement/tree/main/3D%20Rigidbody%20Movement).

##  2.2. C# Script
If you only need the ["Movement"-Script](https://github.com/Engin1999/Unity-3D-Rigidbody-Movement/blob/main/3D%20Rigidbody%20Movement/Assets/Scripts/Movement.cs), follow the steps below.

### Editor settings
1) Have one main camera in scene
2) Instantiate empty object //Transform position = Player spawn position
   - Name: "Player" (not nessesary)
   - Add ["Movement"-Script](https://github.com/Engin1999/Unity-3D-Rigidbody-Movement/blob/main/3D%20Rigidbody%20Movement/Assets/Scripts/Movement.cs) as component

# 3. How to Use
Change script parameters as you wish.
