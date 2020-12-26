module Colors

type RgbaColor = uint32

let inline r(color: RgbaColor) = byte (color >>> 24)
let inline g(color: RgbaColor) = byte (color >>> 16)
let inline b(color: RgbaColor) = byte (color >>> 8)
let inline a(color: RgbaColor) = byte color

/// <summary>
/// Constructs the <see cref="RgbaColor" /> from R, G, B and alpha
/// components.
/// </summary>
let inline rgbaColor (r: byte) (g: byte) (b: byte) (a: byte): RgbaColor =
    (uint32 r <<< 24) ||| (uint32 g <<< 16) ||| (uint32 b <<< 8) ||| (uint32 a)

let inline mixColors colorA colorB mixRatio =
    let mixByteValues (v1:byte) (v2: byte): byte =
        let v1Float = float v1
        let mixedFloat = (float v2 - v1Float) * mixRatio + v1Float
        byte mixedFloat

    match mixRatio with
    | 0. -> colorA
    | 1. -> colorB
    | _ ->
        rgbaColor
            (mixByteValues (r colorA) (r colorB))
            (mixByteValues (g colorA) (g colorB))
            (mixByteValues (b colorA) (b colorB))
            (mixByteValues (a colorA) (a colorB))

let inline toHex(color: RgbaColor): string =
    let alpha = a color
    match alpha with
    | 0xffuy -> sprintf "#%02x%02x%02x" (r color) (g color) (b color)
    | _ -> sprintf "#%02x%02x%02x%02x" alpha (r color) (g color) (b color)
