shader_type canvas_item;

const float SQRT_2 = 1.414213562373;

uniform float progress : hint_range(0, 1);
uniform float diamondPixelSize;
uniform vec2 direction = vec2(0.0, 0.0);
uniform bool invert = false;

void fragment() {
    float xFraction = fract(FRAGCOORD.x / diamondPixelSize * 400.0 * SCREEN_PIXEL_SIZE.x);
    float yFraction = fract(FRAGCOORD.y / diamondPixelSize * 240.0 * SCREEN_PIXEL_SIZE.y);
    
    float xDistance = abs(xFraction - 0.5);
    float yDistance = abs(yFraction - 0.5);
    
    vec2 startPos = vec2(0.5) + (direction / 2.0);
    float dist = (xDistance + yDistance + distance(UV * abs(direction), startPos) / SQRT_2) / 2.0;
    
    if (invert) {
        if (dist >= progress) {
            discard;
        }
    } else {
        if (dist <= progress) {
            discard;
        }
    }
    
//    if (xDistance + yDistance + UV.x * abs(direction.x) + UV.y * direction.y > progress * 2.0) {
//        discard;
//    }
}
