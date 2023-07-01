shader_type canvas_item;

uniform bool enabled;
uniform vec2 position;
uniform vec2 tile_size;
uniform float width;
uniform float height;

varying flat vec2 cell;

void vertex() {
    if (enabled) {
        cell = round((MODEL_MATRIX * vec4(VERTEX, 0.0, 1.0)).xy - position);
    }
}

float inside_rect(vec2 v, vec2 bottomLeft, vec2 topRight) {
    vec2 s = step(bottomLeft, v) - step(topRight, v);
    return s.x * s.y;   
}

void fragment() {
    if (enabled) {
        vec2 half_size = (vec2(width, height) / 2.0);
        vec2 inv_tile_size = 1.0 / tile_size;

        COLOR = texture(TEXTURE, UV);

        if (inside_rect(cell * inv_tile_size, -half_size, half_size) == 0.0) {
            COLOR.a = 0.2;
        }
    }
}