# RetroScreen
RetroScreen is a fast and lightweight solution that allows you to achieve a low-resolution pixelated effect. Just drag and drop the script to the main camera and it will work! You can also specify the rate of camera render to emulate low FPS without changing the actual frame rate of the game.

## Examples
![Demo scene](https://github.com/Mufalder/unity-retroscreen/raw/main/screen1.png)
*Screenshot from the demo scene included in the project.*

![Chronicles](https://github.com/Mufalder/unity-retroscreen/raw/main/screen2.png)
*This is the screenshot from our small prototype Chronicles for the game jam. This project was using early versions of this tool (This game actually gave birth to this asset).*

![Alveole](https://github.com/Mufalder/unity-retroscreen/raw/main/screen3.png)
*This is the screenshot from the game [Alveole](https://store.steampowered.com/app/988930/Alveole/). In this case, we only needed to limit the frame rate without changing the actual game FPS because it was impossible for the consoles and could have a negative effect on the inputs.*

## How does it work?
When you press play, the script creates a new camera that will render only a specific layer. Then it also creates a quad mesh in front of this camera and renders the render texture from the main camera to it. So you got two cameras, the main camera or “Render Camera”, which actually is disabled and only renders to the texture, and the second camera “Display Camera” that renders quad with texture. It may be difficult at first glance, but it's actually easier than it sounds.

## External links
[Unity Asset Store](https://assetstore.unity.com/packages/vfx/shaders/fullscreen-camera-effects/retroscreen-210736)
