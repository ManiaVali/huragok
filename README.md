# Huragok
Huragok is a helper program for extracting and converting data from the Halo engine into formats other programs can understand.

## Features & Examples

### Can serialize any tag to either JSON or YAML
`huragok serialize --tag "C:\Program Files (x86)\Steam\steamapps\common\HREK\tags\levels\solo\m52\bitmaps\concrete\concrete_b_diffuse.bitmap"`

`huragok -s yaml serialize --tag "C:\Program Files (x86)\Steam\steamapps\common\HREK\tags\levels\solo\m52\shaders\fire_self_illum.shader"`

### Can export bitmaps
`huragok export bitmap --tags "C:\Program Files (x86)\Steam\steamapps\common\HREK\tags\levels\solo\m52\bitmaps\concrete\concrete_floor_smooth_a.bitmap" --out-dir $Env:USERPROFILE\Desktop`

### Can automatically convert cubemaps
`huragok export bitmap --tags "C:\Program Files (x86)\Steam\steamapps\common\HREK\tags\levels\solo\m52\bitmaps\cubemaps\cubemap_city_a.bitmap" --cubemap-layout equirectangular --out-dir $Env:USERPROFILE\Desktop`

### Can automatically fix normal maps (recomputes missing Z channel), and/or convert them to OpenGL normal maps
`huragok export bitmap --tags "C:\Program Files (x86)\Steam\steamapps\common\HREK\tags\levels\solo\m52\bitmaps\terrain\rocks_zen\zenrock_reflection_normal.bitmap" --normal-fix --normal-flip-green --out-dir $Env:USERPROFILE\Desktop`

### Can export render_models
`huragok export render-model --tags "C:\Program Files (x86)\Steam\steamapps\common\HREK\tags\objects\vehicles\covenant\phantom\phantom.render_model" --out-dir $Env:USERPROFILE\Desktop`

### Can automatically convert to different units such as Blam, JMS, or Metric. Converts to metric by default, but can also extract in the original Blam world units.
`huragok export render-model --tags "C:\Program Files (x86)\Steam\steamapps\common\HREK\tags\objects\props\covenant\antennae_comm\antennae_comm.render_model" --coordinate-system jms --out-dir $Env:USERPROFILE\Desktop`

`huragok export render-model --tags "C:\Program Files (x86)\Steam\steamapps\common\HREK\tags\objects\props\covenant\antennae_comm\antennae_comm.render_model" --coordinate-system blam --out-dir $Env:USERPROFILE\Desktop`

### Can export to GLB and OBJ as well
`huragok export render-model --tags "C:\Program Files (x86)\Steam\steamapps\common\HREK\tags\objects\props\covenant\battery\battery.render_model" --model-format glb --out-dir $Env:USERPROFILE\Desktop`

`huragok export render-model --tags "C:\Program Files (x86)\Steam\steamapps\common\HREK\tags\objects\props\covenant\battery\battery.render_model" --model-format obj --out-dir $Env:USERPROFILE\Desktop`

### Can export sounds
`huragok export sound --tags "C:\Program Files (x86)\Steam\steamapps\common\HREK\tags\sound\game_sfx\ui\shield_depleted\deplete\loop.sound" --out-dir $Env:USERPROFILE\Desktop`

### All types of exports can also be done in bulk, by specifying several tag files
`huragok export render-model --tags "C:\Program Files (x86)\Steam\steamapps\common\HREK\tags\objects\vehicles\covenant\phantom\phantom.render_model" "C:\Program Files (x86)\Steam\steamapps\common\HREK\tags\objects\vehicles\covenant\banshee\banshee.render_model" --out-dir $Env:USERPROFILE\Desktop`

### By specifying a directory (including subdirectories with --recurse)
`huragok export sound --directory "C:\Program Files (x86)\Steam\steamapps\common\HREK\tags\sound\device_machines" --recurse --out-dir $Env:USERPROFILE\Desktop`

### Or even using a text file full of tags
`huragok export bitmap --from-file "Z:\some_bitmaps.txt" --out-dir $Env:USERPROFILE\Desktop`

## Credits & Attributions
- ManiaVali -- primary developer.
- ILoveAGoodCrisp -- general guidance on using ManagedBlam and creator of [Foundry](https://github.com/ILoveAGoodCrisp/Foundry), the code of which I studied.
- Gravemind2401 -- creator of [Reclaimer](https://github.com/Gravemind2401/Reclaimer), the code of which I studied.

## To do, for now.
- [x] Add option for batch exporting of bitmaps
- [x] Add option for batch exporting of render models
- [x] Add marker(group) support to render models
- [ ] Add vertex colors support to render models
- [ ] Add functionality for exporting scenario structure BSP
- [ ] Add functionality to preview bitmaps
- [x] Add functionality to preview sound tags
- [x] Add option to sound exporter to transcode to formats other than OGG
- [x] Add functionality to preview sound_looping tags
- [x] Add functionality for exporting sound tags
- [ ] Add documentation to publicly exposed classes and members.
- [x] Replace each tags bespoke meta commands with a proper serialization system
- [x] Expand shader command to more accurately expose functions and internal data
- [x] Add support for reading functions
- [x] Unify model and render-model intermediate formats
