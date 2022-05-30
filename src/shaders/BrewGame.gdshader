shader_type canvas_item;

uniform vec4 burntColor: hint_color = vec4(0.3398, 0.23047, 0.10156, 0);

const vec4 beerc = vec4(0.8f, 0.647059f, 0.407843f, 1.0f);
const vec4 diff = vec4(vec3(0.01f), 0.0f);

bool lt(vec4 v1, vec4 v2) {
	bool c1 = v1.r < v2.r;
	bool c2 = v1.g < v2.g;
	bool c3 = v1.b < v2.b;
	return c1 && c2 && c3;
}

void fragment() {
	//read from texture
	COLOR = texture(TEXTURE, UV); 
	
	//Check for the beer color
	float isbeer = float(lt((beerc - diff), COLOR) && lt(COLOR, (beerc + diff)));
	COLOR -= burntColor * isbeer;
}
