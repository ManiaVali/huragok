# Huragok
Huragok is a helper program for extracting and converting data from the Halo engine into formats other programs can understand.

## Quick Start
- Download the [latest release](https://github.com/ManiaVali/huragok/releases) for the engine version you are working with.<br>Each game has its own version at this time.
- Once downloaded, extract the archive and then open a command prompt in the folder containing Huragok.exe.
- Run a supported command; such as `huragok.exe --help` to view the supported commands.<br>You can find examples of supported commands in the "Features & Examples" section.

## Features & Examples
<details>
    <summary><b>Show section</b></summary>

### Can serialize any tag to either JSON or YAML
```powershell
huragok serialize --tag "C:\Program Files (x86)\Steam\steamapps\common\HREK\tags\levels\solo\m52\bitmaps\concrete\concrete_b_diffuse.bitmap"
```
<details>
    <summary><b>Show example output</b></summary>

```json
{
  "show bitmap": "unsupported custom type: BitmapGroup (TagFieldCustomToolCommand)",
  "Usage": 0,
  "Flags": {
    "bitmap is TILED": false,
    "use less blurry bump map": false,
    "dither when compressing": false,
    "generate random sprites": false,
    "using tag_interop and tag_resource": false,
    "alpha channel stores TRANSPARENCY": false,
    "preserve alpha channel in mipmaps for ALPHA TEST": false,
    "only use on demand": false,
    "generate tight bounds": false,
    "tight bounds from alpha channel": false,
    "can be sampled": false,
    "bitmap is double sized": false,
    "bitmap is triple sized": false
  },
  "sprite spacing": 4,
  "bump map height": 5,
  ... etc ...
```
</details>


```powershell
huragok --serialization-format yaml serialize --tag "C:\Program Files (x86)\Steam\steamapps\common\HREK\tags\levels\solo\m52\shaders\fire_self_illum.shader"
```
<details>
    <summary><b>Show example output</b></summary>

```yaml
render_method:
- definition: shaders\shader.render_method_definition
  reference: 
  options:
  - short: 2
  - short: -1
  - short: -1
  - short: -1
  - short: 4
  - short: -1
  - short: 3
  - short: -1
  - short: -1
  - short: -1
  - short: -1
  - short: -1
  parameters:
  - parameter name: albedo_color
    parameter type: 5
    bitmap:
  ... etc ...
```
</details>

### Can export bitmaps
```powershell
huragok export bitmap --tags "C:\Program Files (x86)\Steam\steamapps\common\HREK\tags\levels\solo\m52\bitmaps\concrete\concrete_floor_smooth_a.bitmap" --out-dir C:\Users\user\Desktop
```
<details>
    <summary><b>Show example output</b></summary>

<table>
<tr>
<td align="center">
    <b><i>concrete_floor_smooth_a.bitmap</i></b>
</td>
</tr>
<tr>
<td>
<img src="https://maniavali.com/wp-content/uploads/2026/05/concrete_floor_smooth_a.png" width="100%">
</td>
</tr>
</table>
</details>

### Can automatically convert cubemaps
```powershell
huragok export bitmap --tags "C:\Program Files (x86)\Steam\steamapps\common\HREK\tags\levels\solo\m52\bitmaps\cubemaps\cubemap_city_a.bitmap" --cubemap-layout equirectangular --out-dir C:\Users\user\Desktop
```

<details>
    <summary><b>Show example output</b></summary>

<table>
<tr>
<td align="center">
    <b>Without <code>--cubemap-layout equirectangular</code></b><br>
    <i>Uses engine-specific cubemap layout; not easy to use elsewhere.</i>
</td>
<td align="center">
    <b>With <code>--cubemap-layout equirectangular</code></b><br>
    <i>Uses a standardized layout; can be used anywhere!</i>
</td>
</tr>
<tr>
<td>
<img src="https://maniavali.com/wp-content/uploads/2026/05/m52_cubemap_club.png" width="100%">
</td>
<td>
<img src="https://maniavali.com/wp-content/uploads/2026/05/m52_cubemap_club-1.png" width="100%">
</td>
</tr>
</table>

</details>

### Can automatically fix normal maps (recomputes missing Z channel), and/or convert them to OpenGL normal maps
```powershell
huragok export bitmap --tags "C:\Program Files (x86)\Steam\steamapps\common\HREK\tags\levels\solo\m52\bitmaps\terrain\rocks_zen\zenrock_reflection_normal.bitmap" --normal-fix --normal-flip-green --out-dir C:\Users\user\Desktop
```

<details>
    <summary><b>Show example output</b></summary>

<table>
<tr>
<td align="center">
    <b>Without <code>--normal-fix</code></b><br>
    <i>Not usable with missing Z channel!</i>
</td>
<td align="center">
    <b>With <code>--normal-fix</code></b><br>
    <i>Fully usable in any software!</i>
</td>
</tr>
<tr>
<td>
<img src="https://media.discordapp.net/attachments/1502577765101342912/1502578994497720362/fountain_carvings_zen_bump.png?ex=6a02337c&is=6a00e1fc&hm=391d1dff0ba8dc83379ad83dd508bb8422767f4a35a55306ef3d5e2d4da0198b&=&format=webp&quality=lossless&width=849&height=849" width="100%">
</td>
<td>
<img src="https://media.discordapp.net/attachments/1502577765101342912/1502578994036605028/fountain_carvings_zen_bump.png?ex=6a02337c&is=6a00e1fc&hm=9bad0530b0cf237299a3b77ba753a034dbb6e05a15a6bf50cee4b364438ba37f&=&format=webp&quality=lossless&width=849&height=849" width="100%">
</td>
</tr>
</table>

</details>

### Can export render_models
```powershell
huragok export render-model --tags "C:\Program Files (x86)\Steam\steamapps\common\HREK\tags\objects\vehicles\covenant\phantom\phantom.render_model" --out-dir C:\Users\user\Desktop
```
<details>
    <summary><b>Show example output</b></summary>

<table>
<tr>
<td align="center">
    <b><i>phantom.render_model</i></b>
</td>
</tr>
<tr>
<td>
<img src="https://media.discordapp.net/attachments/1502577765101342912/1502577765592072324/Screenshot_20260509_003333.png?ex=6a023257&is=6a00e0d7&hm=8463ad34fcc3e2d708ea2edf701b1a7d8861087858f83efbc23a3912f3d0cc89&=&format=webp&quality=lossless&width=1372&height=849" width="100%">
</td>
</tr>
</table>
</details>

#### Also supports
- Exporting in different coordinate systems with `--coordinate-system (blam, jms, or metric)`
- Exporting to different models formats with `--model-format (glb, obj, or fbx)`

### Can export sounds
```powershell
huragok export sound --tags "C:\Program Files (x86)\Steam\steamapps\common\HREK\tags\sound\game_sfx\ui\shield_depleted\deplete\loop.sound" --out-dir C:\Users\user\Desktop
```
<details>
    <summary><b>Show example output</b></summary>

<table>
    <tbody>
        <tr>
            <td>
                <p><b><i>exit_hp_lp1.sound</i></b></p>
                <audio controls="1" controlslist="nofullscreen noremoteplayback" src="https://cdn.discordapp.com/attachments/1502577765101342912/1502578469257609346/exit_hp_lp1.ogg?ex=6a0232ff&is=6a00e17f&hm=4df02735e766639da85416589fe81d62b24066e4be7aa7e095cf59c2d522ed9a&">Your browser does not support the audio tag.</audio>
            </td>
            <td>
                <p><b><i>exit_hp_lp2.sound</i></b></p>
                <audio controls="1" controlslist="nofullscreen noremoteplayback" src="https://cdn.discordapp.com/attachments/1502577765101342912/1502578469693821000/exit_hp_lp2.ogg?ex=6a0232ff&is=6a00e17f&hm=eba3d5d52431fec39bf0388e2b7f3eb301f5859bdefad70dd73759eba990f016&">Your browser does not support the audio tag.</audio>
            </td>
        </tr>
        <tr>
            <td>
                <p><b><i>exit_hp_lp3.sound</i></b></p>
                <audio controls="1" controlslist="nofullscreen noremoteplayback" src="https://cdn.discordapp.com/attachments/1502577765101342912/1502578470017040465/exit_hp_lp3.ogg?ex=6a0232ff&is=6a00e17f&hm=c3ce293944a288626716a55b0fb28015d8c1472b24349c887c32ba7760fe101f&">Your browser does not support the audio tag.</audio>
            </td>
            <td>
                <p><b><i>exit_hp_lp4.sound</i></b></p>
                <audio controls="1" controlslist="nofullscreen noremoteplayback" src="https://cdn.discordapp.com/attachments/1502577765101342912/1502578470545395732/exit_hp_lp4.ogg?ex=6a0232ff&is=6a00e17f&hm=bd1d4d1fe6eacf225532c629766dca5cbfdb536269d0c27449774b65450db4a9&">Your browser does not support the audio tag.</audio>
            </td>
        </tr>
    </tbody>
</table>

</details>



### All types of exports can also be done in bulk, by specifying several tag files
```powershell
huragok export render-model --tags "C:\Program Files (x86)\Steam\steamapps\common\HREK\tags\objects\vehicles\covenant\phantom\phantom.render_model" "C:\Program Files (x86)\Steam\steamapps\common\HREK\tags\objects\vehicles\covenant\banshee\banshee.render_model" --out-dir C:\Users\user\Desktop
```

### By specifying a directory (including subdirectories with `--recurse`)
```powershell
huragok export sound --directory "C:\Program Files (x86)\Steam\steamapps\common\HREK\tags\sound\device_machines" --recurse --out-dir C:\Users\user\Desktop
```

### Or even using a text file full of tags
```powershell
huragok export bitmap --from-file "Z:\some_bitmaps.txt" --out-dir C:\Users\user\Desktop
```
</details>

## Credits & Thanks
- [**ManiaVali**](https://github.com/ManiaVali)
    - Primary developer.
- [**ILoveAGoodCrisp**](https://github.com/ILoveAGoodCrisp)
    - General guidance on using ManagedBlam.
    - Creator of [Foundry](https://github.com/ILoveAGoodCrisp/Foundry), the code of which I studied.
- [**Gravemind2401**](https://github.com/Gravemind2401)
    - Creator of [Reclaimer](https://github.com/Gravemind2401/Reclaimer), the code of which I studied.
- The_Heavynator
    - Early testing

## Attributions
This project includes code, libraries, and/or assets from:
- [**SharpGLTF**](https://github.com/vpenades/SharpGLTF)
- [**Fmod5Sharp**](https://github.com/SamboyCoding/Fmod5Sharp)
- [**YamlDotNet**](https://github.com/aaubry/yamldotnet)
- [**NVorbis**](https://github.com/NVorbis/NVorbis)
- [**NAudio**](https://github.com/naudio/naudio)
- [**FFMpegCore**](https://github.com/rosenbjerg/FFMpegCore)